--1)Registrar Venta con Plan de Pago y Actualización de Lote
--Cuando un cliente compra un lote, se deben insertar la venta, 
--generar el plan de pagos y marcar el lote como vendido. 
--Todo esto debe hacerse en una transacción para asegurar que
--no queden registros inconsistentes si ocurre un error.

CREATE OR ALTER PROCEDURE spRegistrarVenta
    @LoteID INT,
    @ClienteID INT,
    @EmpleadoID INT,
    @AvalID INT = NULL,
    @BeneficiarioID INT = NULL,
    @TipoPago VARCHAR(10),
    @Prima FLOAT = 0,
    @Plazo INT,
    @Interes FLOAT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @VentaID INT;
    DECLARE @Cuota FLOAT;
    DECLARE @Mes INT;
    DECLARE @Error INT = 0;

    BEGIN TRANSACTION;
	
    BEGIN TRY
        -- Insertar la venta
        INSERT INTO Venta (LoteID, ClienteID, EmpleadoID, AvalID, BeneficiarioID, Tipo, Prima, Plazo, Interes)
        VALUES (@LoteID, @ClienteID, @EmpleadoID, @AvalID, @BeneficiarioID, @TipoPago, @Prima, @Plazo, @Interes);

        SET @VentaID = SCOPE_IDENTITY();

        -- Marcar lote como vendido
        UPDATE Lote
        SET Estado = 'Vendido'
        WHERE LoteID = @LoteID;

        -- Generar plan de pagos mensual
        SET @Cuota = (@Prima + (@Plazo * @Interes)) / @Plazo;
        SET @Mes = 1;

        WHILE @Mes <= @Plazo
        BEGIN
            INSERT INTO PlanPago (VentaID, CuotaNumero, FechaVencimiento, Monto, Capital, Interes, Estado)
            VALUES (@VentaID, @Mes, DATEADD(MONTH, @Mes, GETDATE()), @Cuota, @Cuota - (@Cuota*@Interes), @Cuota*@Interes, 'Pendiente');
            SET @Mes = @Mes + 1;
        END

        COMMIT TRANSACTION;
        SELECT 'Venta registrada con éxito' AS Mensaje, @VentaID AS VentaID;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT ERROR_MESSAGE() AS Error;
    END CATCH
END;
GO

--2) Registrar Pago de una Cuota con Factura
--Cuando se recibe un pago, se debe registrar el pago, actualizar el plan de pago,
--generar la factura y asociar todo a la cuenta bancaria correspondiente. Se maneja
--transacción para que todo se guarde correctamente o se deshaga en caso de error.

CREATE OR ALTER PROCEDURE spRegistrarPago
    @VentaID INT,
    @PlanPagoID INT,
    @EmpleadoID INT,
    @CuentaID INT,
    @MontoPagado FLOAT,
    @TipoPago VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Capital FLOAT;
    DECLARE @Interes FLOAT;
    DECLARE @PagoID INT;

    BEGIN TRANSACTION;

    BEGIN TRY
        -- Calcular capital e interés pagado
        SELECT @Capital = Capital, @Interes = Interes
        FROM PlanPago
        WHERE PlanPagoID = @PlanPagoID;

        -- Insertar pago
        INSERT INTO Pago (VentaID, PlanPagoID, EmpleadoID, CuentaID, MontoPagado, CapitalPagado, InteresPagado, TipoPago)
        VALUES (@VentaID, @PlanPagoID, @EmpleadoID, @CuentaID, @MontoPagado, @Capital, @Interes, @TipoPago);

        SET @PagoID = SCOPE_IDENTITY();

        -- Actualizar estado del plan de pago
        UPDATE PlanPago
        SET Estado = 'Pagado'
        WHERE PlanPagoID = @PlanPagoID;

        -- Generar factura
        INSERT INTO Factura (PagoID)
        VALUES (@PagoID);

        COMMIT TRANSACTION;
        SELECT 'Pago registrado con éxito' AS Mensaje, @PagoID AS PagoID;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT ERROR_MESSAGE() AS Error;
    END CATCH
END;
GO

--3)Registrar Depósito en Caja para la Cuenta de un Proyecto
--Al final del día, el cajero deposita los pagos recibidos en la cuenta bancaria
--de la etapa correspondiente. Todo debe hacerse de forma transaccional para no perder información de los depósitos.

create or alter procedure spdepositocaja
    @empleadoid int,
    @cuentaid int,
    @montototal float,
    @numeroreferencia varchar(50)
as
begin
    set nocount on;
    begin transaction;

    begin try
        insert into depositocaja (empleadoid, cuentaid, montototal, numeroreferencia)
        values (@empleadoid, @cuentaid, @montototal, @numeroreferencia);

        commit transaction;
        select 'depósito registrado con éxito' as mensaje;
    end try
    begin catch
        rollback transaction;
        select error_message() as error;
    end catch
end;
go