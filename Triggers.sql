-- Trigger para Actualizar la información del lote cuando se inserte una venta

CREATE OR ALTER TRIGGER tr_ActualizarEstadoLote
ON Venta
AFTER INSERT
AS
BEGIN
    UPDATE Lote SET Estado = 'Vendido' FROM Lote l
    INNER JOIN inserted i ON l.LoteID = i.LoteID

END
GO

--Generar la factura automáticamente
CREATE OR ALTER TRIGGER tr_GenerarFacturaAutomatica ON Pago AFTER INSERT
AS
BEGIN

    INSERT INTO Factura (PagoID, FechaEmision)
    SELECT PagoID, GETDATE() FROM inserted

END
GO

--Actualizar plan pago
CREATE OR ALTER TRIGGER actualizarPlanPago ON Pago AFTER INSERT
AS
BEGIN
    UPDATE PlanPago SET Estado = 'Pagado' FROM PlanPago p
    INNER JOIN inserted i ON p.PlanPagoID = i.PlanPagoID;
END
GO


--Esta función crea por completo las cuotas a pagar por el cliente
CREATE OR ALTER FUNCTION dbo.f_GenerarCuotas (@MontoFinanciar FLOAT, @TasaAnual FLOAT, @PlazoAnios INT)
RETURNS @Amortizacion TABLE (CuotaNumero INT, FechaVencimiento DATETIME, MontoCuota FLOAT, Capital FLOAT, Interes FLOAT)
AS
BEGIN
    DECLARE @CuotaTotal INT = @PlazoAnios * 12
    DECLARE @MontoMensual FLOAT = @MontoFinanciar / @CuotaTotal
    DECLARE @InteresMensual FLOAT = (@MontoFinanciar * (@TasaAnual / 100)) / 12
    DECLARE @Contador INT = 1

    WHILE @Contador <= @CuotaTotal
    BEGIN
        INSERT INTO @Amortizacion (CuotaNumero, FechaVencimiento, MontoCuota, Capital, Interes)
        VALUES (
            @Contador, 
            DATEADD(MONTH, @Contador, GETDATE()),
            (@MontoMensual + @InteresMensual), 
            @MontoMensual, 
            @InteresMensual
        )
        SET @Contador = @Contador + 1
    END
    RETURN
END
GO

-- Triger para generar el plan de pago luego de ejecutarse una venta
CREATE OR ALTER TRIGGER tr_GenerarPlanPago ON Venta AFTER INSERT
AS
BEGIN

    DECLARE @VentaID INT, 
            @LoteID INT,
            @Tipo VARCHAR(10), 
            @Prima FLOAT, 
            @InteresVenta FLOAT, 
            @Plazo INT,
            @ValorLote FLOAT;

    DECLARE cr_Ventas CURSOR FOR 
    SELECT VentaID, LoteID, Tipo, ISNULL(Prima, 0), Interes, Plazo 
    FROM inserted;

    OPEN cr_Ventas;
    FETCH NEXT FROM cr_Ventas INTO @VentaID, @LoteID, @Tipo, @Prima, @InteresVenta, @Plazo;

    WHILE @@FETCH_STATUS = 0
    BEGIN

        IF @Tipo = 'Crédito'
        BEGIN

            SELECT @ValorLote = (L.Area * E.PrecioVara)
            FROM Lote L
            INNER JOIN Bloque B ON L.BloqueID = B.BloqueID
            INNER JOIN Etapa E ON B.EtapaID = E.EtapaID
            WHERE L.LoteID = @LoteID;

            DECLARE @MontoFinanciar FLOAT = @ValorLote - @Prima;

            INSERT INTO PlanPago (VentaID, CuotaNumero, FechaVencimiento, Monto, Capital, Interes, Estado)
            SELECT 
                @VentaID, 
                f.CuotaNumero, 
                f.FechaVencimiento, 
                f.MontoCuota, 
                f.Capital, 
                f.Interes, 
                'Pendiente'
            FROM dbo.f_GenerarCuotas(@MontoFinanciar, @InteresVenta, @Plazo) f;
        END

        FETCH NEXT FROM cr_Ventas INTO @VentaID, @LoteID, @Tipo, @Prima, @InteresVenta, @Plazo;
    END

    CLOSE cr_Ventas;
    DEALLOCATE cr_Ventas;
END
GO

CREATE TRIGGER tr_ValidarEliminarLote ON Lote FOR DELETE 
AS
BEGIN
    IF EXISTS (SELECT 1 FROM deleted WHERE Estado = 'Vendido')
    BEGIN
        ROLLBACK TRANSACTION
    END
END
GO


CREATE TRIGGER tr_PrevenirCambioVenta ON Venta FOR UPDATE
AS
BEGIN
    IF UPDATE(Plazo) OR UPDATE(Interes) OR UPDATE(LoteID)
    BEGIN
        ROLLBACK TRANSACTION;
    END
END
GO

CREATE TRIGGER tr_ProteccionProyectos ON Proyecto FOR DELETE
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Etapa WHERE ProyectoID IN (SELECT ProyectoID FROM deleted))
    BEGIN
        ROLLBACK TRANSACTION
    END
END
GO

CREATE TRIGGER tr_ReversarEstadoLote
ON Venta
AFTER DELETE
AS
BEGIN
    -- Al eliminar la venta, el lote vuelve a estar disponible para el público
    UPDATE Lote
    SET Estado = 'Disponible'
    FROM Lote l
    INNER JOIN deleted d ON l.LoteID = d.LoteID;
END
GO

CREATE TRIGGER tr_ReversarPagoPlan
ON Pago
AFTER DELETE
AS
BEGIN
    -- Si se borra un pago, la cuota en el plan vuelve a quedar pendiente
    UPDATE PlanPago
    SET Estado = 'Pendiente'
    FROM PlanPago p
    INNER JOIN deleted d ON p.PlanPagoID = d.PlanPagoID;
END
GO

CREATE TRIGGER tr_LimpiarPlanVenta
ON Venta
AFTER DELETE
AS
BEGIN
    -- Elimina todas las cuotas proyectadas de la venta que fue borrada
    DELETE FROM PlanPago
    WHERE VentaID IN (SELECT VentaID FROM deleted);
END
GO