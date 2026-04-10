------------------------------- VISTas ------------------------------------------

create or alter view vLotesDisponibles
as
	select 
		l.LoteID,
		l.Numero as NumeroLote,
		l.Area,
		l.Catastro,
		l.Matricula,
		l.Esquina,
		l.Parque,
		l.CalleCerrada,
		b.NumeroBloque,
		e.Nombre as Etapa,
		e.PrecioVara,
		p.Nombre as Proyecto,
		p.Departamento,
		p.Municipio
	from Lote l
	inner join Bloque b on l.BloqueID = b.BloqueID
	inner join Etapa e on b.EtapaID = e.EtapaID
	inner join Proyecto p on p.ProyectoID = p.ProyectoID
	where l.Estado = 'Disponible'
go

create or alter view vResumenEtapa
as
	select 
		p.Nombre as Proyecto,
		e.EtapaID,
		e.Nombre as Etapa,
		e.PrecioVara,
		e.Interes,
		count(l.LoteID) as TotalLotes,
		sum(case when l.Estado = 'Disponible' then 1 else 0 end) as LotesDisponibles,
		sum(case when l.Estado = 'Vendido' then 1 else 0 end) as LotesVendidos,
		round(sum(l.Area), 2) as AreaTotal,
		round(sum(case when l.Estado = 'Disponible' then l.Area else 0 end), 2) as AreaDisponible
	from Etapa e
	inner join Proyecto p on e.ProyectoID = p.ProyectoID 
	inner join Bloque b on b.EtapaID = e.EtapaID
	inner join Lote l on l.BloqueID = b.BloqueID
	group by p.Nombre, e.EtapaID, e.Nombre, e.PrecioVara, e.Interes
go


create or alter view vVentasPorEmpleado 
as
	select 
		em.EmpleadoID,
		em.Nombre as Empleado,
		em.Cargo,
		count(v.VentaID) as TotalVentas,
		sum(case when v.Tipo = 'Contado' then 1 else 0 end) as VentasContado,
		sum(case when v.Tipo = 'Credito' then 1 else 0 end) as VentasCredito,
		min(v.Fecha) as PrimeraVenta,
		max(v.Fecha) as UltimaVenta
	from Empleado em
	LEFT JOIN Venta v on em.EmpleadoID = v.EmpleadoID
	group by em.EmpleadoID, em.Nombre, em.Cargo
go

create or alter view vCuotasPendientes 
as
	select 
		pp.PlanPagoID,
		pp.VentaID,
		pp.CuotaNumero,
		pp.FechaVencimiento,
		pp.Monto,
		pp.Capital,
		pp.Interes,
		datediff(day, pp.FechaVencimiento, getdate()) as DiasRetraso,
		c.Nombre as Cliente,
		c.Telefono,
		l.Numero as NumeroLote,
		b.NumeroBloque,
		e.Nombre as Etapa,
		p.Nombre as Proyecto
	from PlanPago pp
	inner join Venta v on pp.VentaID = v.VentaID
	inner join Cliente c on v.ClienteID = c.ClienteID
	inner join Lote l on v.LoteID = l.LoteID
	inner join Bloque b on l.BloqueID = b.BloqueID
	inner join Etapa e on b.EtapaID = e.EtapaID
	inner join Proyecto p on e.ProyectoID = p.ProyectoID
	where pp.Estado = 'Pendiente'
go

create or alter view vEstadoCuentaCliente
as
	select 
		c.ClienteID,
		c.Nombre as Cliente,
		c.DNI,
		c.Telefono,
		v.VentaID,
		p.Nombre as Proyecto,
		e.Nombre as Etapa,
		l.Numero as NumeroLote,
		v.Tipo,
		v.Plazo,
		v.Prima,
		count(pp.PlanPagoID) as TotalCuotas,
		sum(case when pp.Estado = 'Pagado' then 1 else 0 end) as CuotasPagadas,
		sum(case when pp.Estado = 'Pendiente' then 1 else 0 end) as CuotasPendientes,
		round(sum(case when pp.Estado = 'Pagado' then pp.Monto else 0 end), 2) as MontoPagado,
		round(sum(case when pp.Estado = 'Pendiente' then pp.Monto else 0 end), 2) as SaldoPendiente,
		round(sum(pp.Monto), 2) as MontoTotal
	from Cliente c
	inner join Venta v on c.ClienteID = v.ClienteID
	inner join Lote l on v.LoteID = l.LoteID
	inner join Bloque b on l.BloqueID = b.BloqueID
	inner join Etapa e on b.EtapaID = e.EtapaID
	inner join Proyecto p on e.ProyectoID = p.ProyectoID
	inner join PlanPago pp on v.VentaID = pp.VentaID
	group by c.ClienteID, c.Nombre, c.DNI, c.Telefono, 
			 v.VentaID, p.Nombre, e.Nombre, l.Numero, 
			 v.Tipo, v.Plazo, v.Prima;
go

create or alter view vIngresosPorEtapa 
as
	select 
		p.Nombre as Proyecto,
		e.EtapaID,
		e.Nombre as Etapa,
		count(distinct pa.PagoID) as TotalPagos,
		round(sum(pa.MontoPagado), 2) as IngresoTotal,
		round(sum(pa.CapitalPagado), 2) as CapitalRecuperado,
		round(sum(pa.InteresPagado), 2) as InteresesRecaudados
	from Etapa e
	inner join Proyecto p on e.ProyectoID = p.ProyectoID
	inner join Bloque b on b.EtapaID = e.EtapaID
	inner join Lote l on l.BloqueID = b.BloqueID
	inner join Venta v on v.LoteID = l.LoteID
	inner join Pago pa on pa.VentaID = v.VentaID
	group by p.Nombre, e.EtapaID, e.Nombre;
go

create or alter view vGastosPorProyecto 
as
	select 
		p.ProyectoID,
		p.Nombre as Proyecto,
		p.Departamento,
		p.Municipio,
		count(g.GastoID) as TotalRegistros,
		round(sum(g.Monto), 2) as GastoTotal,
		min(g.Fecha) as PrimerGasto,
		max(g.Fecha) as UltimoGasto
	from Proyecto p
	left join Gastos g on p.ProyectoID = g.ProyectoID
	group by p.ProyectoID, p.Nombre, p.Departamento, p.Municipio;
go