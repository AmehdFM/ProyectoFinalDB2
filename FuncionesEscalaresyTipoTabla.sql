-- Función escalar que devuelve el total de lotes disponibles en una etapa
create or alter function fnTotalLotesDisponibles(@etapaid int)
returns int
as
begin
    declare @totalLotes int
    select @totalLotes = count(*) from Lote
    where EtapaID = @etapaid and Estado = 'Disponible'
    return @totalLotes
end
go

-- Función escalar que calcula el valor total de un lote según su área y precio por vara
create or alter function fnValorLote(@loteid int)
returns float
as
begin
    declare @area float, @preciovara float, @valorLote float
    select @area = Area from Lote where LoteID = @loteid
    select @preciovara = PrecioVara from Etapa where EtapaID = (select EtapaID from Lote where LoteID = @loteid)
    set @valorLote = @area * @preciovara
    return @valorLote
end
go

-- Función escalar que calcula la cantidad de años de un crédito a partir de la fecha de la venta y la fecha de vencimiento
create or alter function fnAniosCredito(@ventaid int)
returns int
as
begin
    declare @fechaVenta datetime, @fechaVencimiento datetime
    declare @anosCredito int
    select @fechaVenta = Fecha from Venta where VentaID = @ventaid
    select @fechaVencimiento = (select max(FechaVencimiento) from PlanPago where VentaID = @ventaid)
    set @anosCredito = datediff(year, @fechaVenta, @fechaVencimiento)
    return @anosCredito
end
go

-- Función escalar que devuelve el saldo pendiente de un cliente en base a sus pagos realizados
create or alter function fnSaldoPendiente(@clienteid int)
returns float
as
begin
    declare @saldoPendiente float
    select @saldoPendiente = sum(Monto) - sum(MontoPagado) 
    from Venta v
    join PlanPago pp on v.VentaID = pp.VentaID
    join Pago p on pp.PlanPagoID = p.PlanPagoID
    where v.ClienteID = @clienteid
    return @saldoPendiente
end
go

-- Función escalar que calcula la cantidad de pagos pendientes de un cliente
create or alter function fnPagosPendientes(@clienteid int)
returns int
as
begin
    declare @pagosPendientes int
    select @pagosPendientes = count(*) from PlanPago
    where VentaID in (select VentaID from Venta where ClienteID = @clienteid) 
    and Estado = 'Pendiente'
    return @pagosPendientes
end
go

-- Función tipo tabla que devuelve todos los lotes disponibles en una etapa
create or alter function fnLotesDisponiblesPorEtapa(@etapaid int)
returns table
as
return
(
    select LoteID, Area, Catastro, Matricula from Lote
    where EtapaID = @etapaid and Estado = 'Disponible'
)
go

-- Función tipo tabla que devuelve todos los lotes vendidos
create or alter function fnLotesVendidos()
returns table
as
return
(
    select LoteID, ClienteID, Fecha from Venta v
    join Lote l on v.LoteID = l.LoteID
)
go

-- Función tipo tabla que devuelve el historial de pagos de un cliente
create or alter function fnHistorialPagos(@clienteid int)
returns table
as
return
(
    select p.PagoID, pp.CuotaNumero, p.MontoPagado, p.FechaPago, p.TipoPago
    from Pago p
    join PlanPago pp on p.PlanPagoID = pp.PlanPagoID
    join Venta v on pp.VentaID = v.VentaID
    where v.ClienteID = @clienteid
)
go

-- Función tipo tabla que devuelve los clientes que tienen pagos pendientes
create or alter function fnClientesConPagosPendientes()
returns table
as
return
(
    select v.ClienteID, c.Nombre, sum(pp.Monto) - sum(p.MontoPagado) as SaldoPendiente
    from Venta v
    join Cliente c on v.ClienteID = c.ClienteID
    join PlanPago pp on v.VentaID = pp.VentaID
    join Pago p on pp.PlanPagoID = p.PlanPagoID
    group by v.ClienteID, c.Nombre
    having sum(pp.Monto) - sum(p.MontoPagado) > 0
)
go

-- Función tipo tabla que devuelve los gastos realizados en un proyecto
create or alter function fnGastosProyecto(@proyectoID int)
returns table
as
return
(
    select GastoID, Concepto, Monto, Fecha from Gastos
    where ProyectoID = @proyectoID
)
go