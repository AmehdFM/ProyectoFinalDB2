-- Procedimiento almacenado para registrar una venta con manejo transaccional
create or alter procedure spRegistrarVenta
    @clienteid int, 
    @loteid int, 
    @empleadoid int, 
    @tipoVenta varchar(10), 
    @prima float, 
    @plazo int, 
    @interes float
as
begin
    begin transaction
    declare @ventaID int, @err int = 0

    -- Insertar venta
    insert into Venta (LoteID, ClienteID, EmpleadoID, Tipo, Prima, Plazo, Interes)
    values (@loteid, @clienteid, @empleadoid, @tipoVenta, @prima, @plazo, @interes)
    
    if @@ERROR != 0 
    begin
        set @err = -1
        rollback
        return
    end

    -- Obtener el ID de la venta insertada
    select @ventaID = scope_identity()

    -- Insertar plan de pago (con cuotas iniciales)
    declare @cuotaNumero int = 1
    declare @montoCuota float = (@prima + (select Area from Lote where LoteID = @loteid) * @interes) / @plazo
    while @cuotaNumero <= @plazo
    begin
        insert into PlanPago (VentaID, CuotaNumero, FechaVencimiento, Monto, Capital, Interes)
        values (@ventaID, @cuotaNumero, dateadd(month, @cuotaNumero, getdate()), @montoCuota, @montoCuota * 0.8, @montoCuota * 0.2)

        if @@ERROR != 0 
        begin
            set @err = -1
            rollback
            return
        end

        set @cuotaNumero = @cuotaNumero + 1
    end

    -- Actualizar el estado del lote
    update Lote set Estado = 'Vendido' where LoteID = @loteid

    if @@ERROR != 0
    begin
        set @err = -1
        rollback
        return
    end

    -- Commit si todo fue exitoso
    if @err = 0
        commit
    else
        rollback
end
go

-- Procedimiento almacenado para registrar un pago
create or alter procedure spRegistrarPago
    @ventaID int, 
    @montoPagado float, 
    @empleadoID int, 
    @tipoPago varchar(20), 
    @cuotaNumero int
as
begin
    begin transaction
    declare @pagoID int, @planPagoID int, @err int = 0

    -- Obtener el Plan de Pago correspondiente
    select @planPagoID = PlanPagoID from PlanPago 
    where VentaID = @ventaID and CuotaNumero = @cuotaNumero

    if @@ERROR != 0 or @planPagoID is null
    begin
        set @err = -1
        rollback
        return
    end

    -- Insertar el pago
    insert into Pago (VentaID, PlanPagoID, EmpleadoID, MontoPagado, TipoPago, FechaPago)
    values (@ventaID, @planPagoID, @empleadoID, @montoPagado, @tipoPago, getdate())
    
    if @@ERROR != 0
    begin
        set @err = -1
        rollback
        return
    end

    -- Actualizar el estado de la cuota
    update PlanPago set Estado = 'Pagado' where PlanPagoID = @planPagoID

    if @@ERROR != 0
    begin
        set @err = -1
        rollback
        return
    end

    -- Commit si todo fue exitoso
    if @err = 0
        commit
    else
        rollback
end
go

-- Procedimiento almacenado para registrar un gasto en un proyecto
create or alter procedure spRegistrarGasto
    @proyectoID int,
    @concepto varchar(100),
    @monto float
as
begin
    begin transaction
    declare @gastoID int, @err int = 0

    -- Insertar el gasto
    insert into Gastos (ProyectoID, Concepto, Monto)
    values (@proyectoID, @concepto, @monto)
    
    if @@ERROR != 0
    begin
        set @err = -1
        rollback
        return
    end

    -- Actualizar el estado financiero del proyecto (si es necesario)
    -- Este paso depende de los requisitos adicionales, si existen

    if @@ERROR != 0
    begin
        set @err = -1
        rollback
        return
    end

    -- Commit si todo fue exitoso
    if @err = 0
        commit
    else
        rollback
end
go