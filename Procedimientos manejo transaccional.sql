--1)Registrar Venta con Plan de Pago y Actualización de Lote
--Cuando un cliente compra un lote, se deben insertar la venta, 
--generar el plan de pagos y marcar el lote como vendido. 
--Todo esto debe hacerse en una transacción para asegurar que
--no queden registros inconsistentes si ocurre un error.

create or alter procedure spRegistrarVenta
    @LoteID int, @ClienteID int, @EmpleadoID int, @AvalID int = null,
    @BeneficiarioID int = null, @TipoPago varchar(10), @Prima float = 0,
    @Plazo int, @Interes float
as
begin

    begin transaction

    -- el insert disparará automáticamente los triggers tr_ActualizarEstadoLote y tr_GenerarPlanPago
    insert into Venta (LoteID, ClienteID, EmpleadoID, AvalID, BeneficiarioID, Tipo, Prima, Plazo, Interes)
    values (@LoteID, @ClienteID, @EmpleadoID, @AvalID, @BeneficiarioID, @TipoPago, @Prima, @Plazo, @Interes);

    -- validamos si ocurrió un error durante el insert o en la ejecución de los triggers
    if @@error <> 0
    begin
        rollback transaction
    end
    else
    begin
        commit transaction;
    end
end
go

--2) Registrar Pago de una Cuota con Factura
--Cuando se recibe un pago, se debe registrar el pago, actualizar el plan de pago,
--generar la factura y asociar todo a la cuenta bancaria correspondiente. Se maneja
--transacción para que todo se guarde correctamente o se deshaga en caso de error.
create or alter procedure spRegistrarPago
    @VentaID int, @PlanPagoID int, @EmpleadoID int, @CuentaID int,
    @MontoPagado float, @TipoPago varchar(20)
as
begin
    set nocount on;
    begin transaction;

    declare @CapitalPagado float, @InteresPagado float;
    
    -- obtenemos el desglose requerido del plan de pagos antes de insertar el registro
    select @CapitalPagado = Capital, @InteresPagado = Interes 
    from PlanPago where PlanPagoID = @PlanPagoID;

    insert into Pago (VentaID, PlanPagoID, EmpleadoID, CuentaID, MontoPagado, CapitalPagado, InteresPagado, TipoPago)
    values (@VentaID, @PlanPagoID, @EmpleadoID, @CuentaID, @MontoPagado, @CapitalPagado, @InteresPagado, @TipoPago);

    if @@error <> 0
    begin
        rollback transaction
    end
    else
    begin
        commit transaction
    end
end
go

--3)Registrar Depósito en Caja para la Cuenta de un Proyecto
--Al final del día, el cajero deposita los pagos recibidos en la cuenta bancaria
--de la etapa correspondiente. Todo debe hacerse de forma transaccional para no perder información de los depósitos.

create or alter procedure dbo.spDepositoCaja
    @EmpleadoID int,
    @CuentaID int,
    @MontoTotal float,
    @NumeroReferencia varchar(50)
as
begin


    -- iniciamos la transacción explícita
    begin transaction

    -- insertamos el registro del depósito en la tabla correspondiente
    insert into DepositoCaja (EmpleadoID, CuentaID, MontoTotal, NumeroReferencia)
    values (@EmpleadoID, @CuentaID, @MontoTotal, @NumeroReferencia)

    -- validamos si ocurrió algún error durante la inserción
    if @@error <> 0
    begin
        -- si hay error, deshacemos la operación para no perder trazabilidad
        rollback transaction
    end
    else
    begin
        -- si todo es correcto, confirmamos el registro
        commit transaction
    end
end
go
