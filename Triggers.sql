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
create or alter trigger tr_GenerarPlanPago
on Venta
after insert
as
begin
     
    declare @VentaID int, @LoteID int, @Plazo int, @Interes float, @Prima float
    declare @MontoTotal float, @MontoFinanciar float
    declare @tipo varchar(20)
     
    
   
    -- obtenemos los datos de la venta recién insertada
    select 
        @VentaID = VentaID, 
        @LoteID = LoteID, 
        @Plazo = Plazo, 
        @Interes = Interes, 
        @Prima = Prima, 
        @tipo = tipo
    from inserted

    set @MontoTotal = dbo.fnValorLote(@LoteID)

    set @MontoFinanciar = @MontoTotal - ISNULL(@Prima, 0)

    if (@tipo = 'credito')
    BEGIN


    insert into PlanPago (VentaID, CuotaNumero, FechaVencimiento, Monto, Capital, Interes, Estado)
    select 
        @VentaID, 
        CuotaNumero, 
        FechaVencimiento, 
        MontoCuota, 
        Capital, 
        Interes, 
        'Pendiente'
    from dbo.f_GenerarCuotas(@MontoFinanciar, @Interes, @Plazo)
    end
    else
    begin
        insert into PlanPago (VentaID, CuotaNumero, FechaVencimiento, Monto, Capital, Interes, Estado)
        values(@VentaID, 0, NULL, @MontoFinanciar, @MontoFinanciar, 0, 'pendiente')
    end
end
go

CREATE OR ALTER TRIGGER tr_ValidarEliminarLote ON Lote FOR DELETE 
AS
BEGIN
    IF EXISTS (SELECT 1 FROM deleted WHERE Estado = 'Vendido')
    BEGIN
        ROLLBACK TRANSACTION
    END
END
GO


CREATE OR ALTER TRIGGER tr_PrevenirCambioVenta ON Venta FOR UPDATE
AS
BEGIN
    IF UPDATE(Plazo) OR UPDATE(Interes) OR UPDATE(LoteID)
    BEGIN
        ROLLBACK TRANSACTION;
    END
END
GO

CREATE OR ALTER TRIGGER tr_ProteccionProyectos ON Proyecto FOR DELETE
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Etapa WHERE ProyectoID IN (SELECT ProyectoID FROM deleted))
    BEGIN
        ROLLBACK TRANSACTION
    END
END
GO

CREATE OR ALTER TRIGGER tr_ReversarEstadoLote
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

CREATE OR ALTER TRIGGER tr_ReversarPagoPlan
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

CREATE OR ALTER TRIGGER tr_LimpiarPlanVenta
ON Venta
AFTER DELETE
AS
BEGIN
    -- Elimina todas las cuotas proyectadas de la venta que fue borrada
    DELETE FROM PlanPago
    WHERE VentaID IN (SELECT VentaID FROM deleted);
END
GO
