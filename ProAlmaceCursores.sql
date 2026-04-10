--spRegistrarVenta
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
    declare @ventaID int, @err int = 0, @etapaID int, @cuotaNumero int, @montoCuota float
    declare @fechaVencimiento datetime, @capital float, @interesCuota float

    -- Obtener el EtapaID desde la tabla Bloque
    select @etapaID = EtapaID from Bloque where BloqueID = (select BloqueID from Lote where LoteID = @loteid)

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

    -- Inicializar cursor para recorrer las cuotas
    declare cuota_cursor cursor for
    select top(@plazo) row_number() over (order by (select null)) as CuotaNumero
    from master.dbo.spt_values -- Dummy table for row numbers (replace with a suitable table)
    
    open cuota_cursor
    fetch next from cuota_cursor into @cuotaNumero

    while @@FETCH_STATUS = 0
    begin
        set @montoCuota = (@prima + (select Area from Lote where LoteID = @loteid) * @interes) / @plazo
        set @capital = @montoCuota * 0.8
        set @interesCuota = @montoCuota * 0.2
        set @fechaVencimiento = dateadd(month, @cuotaNumero, getdate())

        -- Insertar plan de pago para la cuota
        insert into PlanPago (VentaID, CuotaNumero, FechaVencimiento, Monto, Capital, Interes)
        values (@ventaID, @cuotaNumero, @fechaVencimiento, @montoCuota, @capital, @interesCuota)

        if @@ERROR != 0 
        begin
            set @err = -1
            rollback
            return
        end

        fetch next from cuota_cursor into @cuotaNumero
    end

    close cuota_cursor
    deallocate cuota_cursor

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
    declare @pagoID int, @planPagoID int, @err int = 0, @estadoPago varchar(20)

    -- Obtener el Plan de Pago correspondiente
    select @planPagoID = pp.PlanPagoID from PlanPago pp
    join Venta v on pp.VentaID = v.VentaID
    where v.VentaID = @ventaID and pp.CuotaNumero = @cuotaNumero

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

    -- Usamos cursor para actualizar los pagos y estado
    declare pago_cursor cursor for
    select Estado from PlanPago where PlanPagoID = @planPagoID
    
    open pago_cursor
    fetch next from pago_cursor into @estadoPago

    while @@FETCH_STATUS = 0
    begin
        if @estadoPago != 'Pagado'
        begin
            update PlanPago set Estado = 'Pagado' where PlanPagoID = @planPagoID
        end
        fetch next from pago_cursor into @estadoPago
    end

    close pago_cursor
    deallocate pago_cursor

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

    -- Usar un cursor para actualizar el estado del gasto en el proyecto
    declare gasto_cursor cursor for
    select ProyectoID from Gastos where GastoID = scope_identity()
    
    open gasto_cursor
    fetch next from gasto_cursor into @proyectoID

    while @@FETCH_STATUS = 0
    begin
        -- Realizar cualquier operación adicional con el gasto registrado
        fetch next from gasto_cursor into @proyectoID
    end

    close gasto_cursor
    deallocate gasto_cursor

    -- Commit si todo fue exitoso
    if @err = 0
        commit
    else
        rollback
end
go