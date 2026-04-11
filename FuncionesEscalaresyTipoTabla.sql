-- FunciÃ³n escalar que devuelve el total de lotes disponibles en una etapa
create or alter function dbo.fnTotalLotesDisponibles(@etapaid int)
returns int
as
begin
declare @totalLotes int
select @totalLotes = count(*) from Lote l
inner join Bloque b on l.BloqueID = b.BloqueID
where b.EtapaID = @etapaid and l.Estado = 'Disponible'
return @totalLotes
end
go

create or alter function dbo.fnValorLote(@loteid int)
returns float
as
begin
declare @area float, @preciovara float, @valorLote float
declare @esquina bit, @parque bit, @callecerrada bit

select 
    @area = Area, 
    @esquina = Esquina, 
    @parque = Parque, 
    @callecerrada = CalleCerrada 
from Lote 
where LoteID = @loteid

select @preciovara = e.PrecioVara 
from Etapa e
inner join Bloque b on e.EtapaID = b.EtapaID
inner join Lote l on b.BloqueID = l.BloqueID
where l.LoteID = @loteid

-- cÃ¡lculo base: Ã¡rea multiplicada por el precio por vara de la etapa
set @valorLote = @area * @preciovara


if @esquina = 1 set @valorLote = @valorLote * 1.10
if @parque = 1 set @valorLote = @valorLote * 1.05
if @callecerrada = 1 set @valorLote = @valorLote * 1.05

return @valorLote
end
go

-- FunciÃ³n escalar que calcula la cantidad de aÃ±os de un crÃ©dito a partir de la fecha de la venta y la fecha de vencimiento
create or alter function dbo.fnEsClienteMoroso(@clienteid int)
returns bit
as
begin
declare @moroso bit

if exists (
    select 1 
    from PlanPago pp
    inner join Venta v on pp.VentaID = v.VentaID
    where v.ClienteID = @clienteid 
    and pp.Estado = 'Pendiente' 
    and pp.FechaVencimiento < getdate()
)
    set @moroso = 1
else
    set @moroso = 0 

return @moroso
end
go

-- FunciÃ³n escalar que devuelve el saldo pendiente de un cliente en base a sus pagos realizados
create or alter function dbo.fnSaldoPendiente(@clienteid int)
returns float
as
begin
declare @totalDeuda float, @totalPagado float

select @totalDeuda = sum(pp.Monto) 
from PlanPago pp
inner join Venta v on pp.VentaID = v.VentaID
where v.ClienteID = @clienteid


select @totalPagado = sum(p.MontoPagado)
from Pago p
inner join Venta v on p.VentaID = v.VentaID
where v.ClienteID = @clienteid

return isnull(@totalDeuda, 0) - isnull(@totalPagado, 0)
end
go

-- FunciÃ³n escalar que calcula la cantidad de pagos pendientes de un cliente
create or alter function dbo.fnpagospendientes(@clienteid int)
returns int
as
begin
declare @pagospendientes int

select @pagospendientes = count(*) 
from PlanPago pp
inner join Venta v on pp.VentaID = v.VentaID
where v.ClienteID = @clienteid and pp.Estado = 'Pendiente'

return isnull(@pagospendientes, 0)
end
go

-- FunciÃ³n tipo tabla que devuelve todos los lotes disponibles en una etapa
create or alter function dbo.fnLotesDisponiblesPorEtapa(@etapaid int)
returns table
as
return
(
    select l.LoteID, l.Area, l.Catastro, l.Matricula 
    from Lote l
    inner join Bloque b on l.BloqueID = b.BloqueID
    where b.EtapaID = @etapaid and l.Estado = 'Disponible'
)
go

-- FunciÃ³n tipo tabla que devuelve todos los lotes vendidos
create or alter function dbo.fnResumenVentasPorEmpleado()
returns table
as
return
(
    select 
        e.Nombre as Empleado, 
        count(v.VentaID) as CantidadVentas, 
        sum(dbo.fnValorLote(v.LoteID)) as TotalVendido
    from Empleado e
    inner join Venta v on e.EmpleadoID = v.EmpleadoID
    group by e.Nombre
)
go

-- FunciÃ³n tipo tabla que devuelve el historial de pagos de un cliente
create or alter function dbo.fnHistorialPagos(@ClienteID int)
returns table
as
return
(
    select 
        p.PagoID, 
        pp.CuotaNumero, 
        p.MontoPagado, 
        p.CapitalPagado, 
        p.InteresPagado, 
        p.FechaPago, 
        p.TipoPago
    from Pago p
    inner join PlanPago pp on p.PlanPagoID = pp.PlanPagoID
    inner join Venta v on pp.VentaID = v.VentaID
    where v.ClienteID = @ClienteID
)
go

-- FunciÃ³n tipo tabla que devuelve los clientes que tienen pagos pendientes
create or alter function dbo.fnClientesConPagosPendientes()
returns table
as
return
(
    select 
        v.ClienteID, 
        c.Nombre, 
        sum(isnull(pp.TotalMonto, 0) - isnull(p.TotalPagado, 0)) as SaldoPendiente
    from Venta v
    inner join Cliente c on v.ClienteID = c.ClienteID
    inner join (
        select VentaID, sum(Monto) as TotalMonto 
        from PlanPago group by VentaID
    ) pp on v.VentaID = pp.VentaID
    left join (
        select VentaID, sum(MontoPagado) as TotalPagado 
        from Pago group by VentaID
    ) p on v.VentaID = p.VentaID
    group by v.ClienteID, c.Nombre
    having sum(isnull(pp.TotalMonto, 0) - isnull(p.TotalPagado, 0)) > 0
)
go

-- FunciÃ³n tipo tabla que devuelve los gastos realizados en un proyecto
create or alter function dbo.fnGastosProyecto(@ProyectoID int)
returns table
as
return
(
    select 
        g.GastoID, 
        p.Nombre as NombreProyecto, 
        g.Concepto, 
        g.Monto, 
        g.Fecha 
    from Gastos g
    inner join Proyecto p on g.ProyectoID = p.ProyectoID
    where g.ProyectoID = @ProyectoID
)
go

create or alter function fn_ObtenerEtapasPorProyecto(@ProyectoID int)
returns table
as
return (
    select
        Nombre as 'Nombre',
        PrecioVara as 'Precio x Metro',
        Interes as 'Interés (%)',
        AreaVerde * 100 as 'Área Verde (%)'
    from Etapa
    where ProyectoID = @ProyectoID
)
go

create or alter function fn_ObtenerBloquesPorProyecto(@ProyectoID int)
returns table
as
return (
    select
        E.Nombre as 'Etapa',
        B.NumeroBloque as 'Número de Bloque'
    from Bloque B
    join Etapa E on B.EtapaID = E.EtapaID
    where E.ProyectoID = @ProyectoID
)
go

create or alter function fn_ObtenerLotesPorProyecto(@ProyectoID int)
returns table
as
return (
    select
        L.LoteID,
        E.Nombre as 'Etapa',
        B.NumeroBloque as 'Bloque',
        L.Numero as 'Número de Lote',
        L.Area as 'Área (v²)',
        L.Catastro as 'Catastro',
        L.Matricula as 'Matrícula',
        L.Colindancias as 'Colindancias',
        case when L.Esquina = 1 then 'Sí' else 'No' end as 'Esquina',
        case when L.Parque = 1 then 'Sí' else 'No' end as 'Parque',
        case when L.CalleCerrada = 1 then 'Sí' else 'No' end as 'Calle Cerrada',
        L.Estado as 'Estado'
    from Lote L
    join Bloque B on L.BloqueID = B.BloqueID
    join Etapa E on B.EtapaID = E.EtapaID
    where E.ProyectoID = @ProyectoID
)
go

