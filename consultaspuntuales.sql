-- =================================================================================
-- SCRIPT DE CONSULTAS PARA CLIENTE LIGERO
-- Este script centraliza todas las lecturas de la aplicación en procedimientos
-- almacenados para evitar que el cliente C# tenga lógica SQL embebida.
-- =================================================================================

USE InversionesLotes; -- Asegúrate de que este es el nombre correcto de tu DB
GO

-- 1. Reporte de Morosidad
CREATE OR ALTER PROCEDURE sp_GetMorosidadReport
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        c.ClienteID, 
        c.Nombre,
        dbo.fnSaldoPendiente(c.ClienteID) AS Saldo,
        CASE WHEN dbo.fnEsClienteMoroso(c.ClienteID) = 1 THEN 'MOROSO' ELSE 'AL DÍA' END AS Estado
    FROM Cliente c 
    ORDER BY Saldo DESC;
END
GO

-- 2. Resumen Financiero Global
CREATE OR ALTER PROCEDURE sp_GetResumenFinancieroGlobal
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProyectoID, Proyecto, IngresoTotal, GastoTotal, BeneficioTotal 
    FROM vResumenFinancieroProyecto 
    ORDER BY Proyecto;
END
GO

-- 3. Lista de Empleados
CREATE OR ALTER PROCEDURE sp_GetListaEmpleados
AS
BEGIN
    SET NOCOUNT ON;
    SELECT EmpleadoID, Nombre, Cargo 
    FROM Empleado 
    ORDER BY Nombre;
END
GO

-- 4. Lista de Clientes
CREATE OR ALTER PROCEDURE sp_GetListaClientes
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ClienteID, Nombre, DNI, Telefono 
    FROM Cliente 
    ORDER BY Nombre;
END
GO

-- 5. Detalle de Plan de Pagos por Venta
CREATE OR ALTER PROCEDURE sp_GetPlanPagoDetalle
    @VentaID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CuotaNumero, FechaVencimiento, Monto, Capital, Interes, Estado 
    FROM PlanPago 
    WHERE VentaID = @VentaID 
    ORDER BY CuotaNumero;
END
GO

-- 6. Detalle de Venta Completo (Usa la vista existente)
CREATE OR ALTER PROCEDURE sp_GetDetalleVentaCompleto
    @VentaID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * 
    FROM vDetalleVentaCompleto 
    WHERE VentaID = @VentaID;
END
GO

-- 7. Resumen Financiero por Proyecto (Usa la vista existente)
CREATE OR ALTER PROCEDURE sp_GetResumenFinancieroProyecto
    @ProyectoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IngresoTotal, GastoTotal, BeneficioTotal 
    FROM vResumenFinancieroProyecto 
    WHERE ProyectoID = @ProyectoID;
END
GO

-- 8. Cuentas Bancarias Formateadas (Para Combos)
CREATE OR ALTER PROCEDURE sp_GetCuentasCombo
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CuentaID, Banco + ' - ' + NumeroCuenta AS Detalle 
    FROM Cuenta;
END
GO

-- 9. Etapas por Proyecto
CREATE OR ALTER PROCEDURE sp_GetEtapasPorProyecto
    @ProyectoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT EtapaID, Nombre 
    FROM Etapa 
    WHERE ProyectoID = @ProyectoID 
    ORDER BY Nombre;
END
GO

-- 10. Sugerir Siguiente Número de Bloque
CREATE OR ALTER PROCEDURE sp_GetSiguienteNumeroBloque
    @EtapaID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(MAX(NumeroBloque), 0) + 1 AS SiguienteNumero
    FROM Bloque 
    WHERE EtapaID = @EtapaID;
END
GO

-- 11. Rendimiento de Empleado (Usa la vista existente)
CREATE OR ALTER PROCEDURE sp_GetVentasPorEmpleado
    @EmpleadoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TotalVentas, VentasContado, VentasCredito, PrimeraVenta, UltimaVenta 
    FROM vVentasPorEmpleado 
    WHERE EmpleadoID = @EmpleadoID;
END
GO

-- 12. Ingresos por Etapa
CREATE OR ALTER PROCEDURE sp_GetIngresosPorEtapa
    @ProyectoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Etapa, TotalPagos, IngresoTotal, CapitalRecuperado, InteresesRecaudados 
    FROM vIngresosPorEtapa 
    WHERE ProyectoID = @ProyectoID;
END
GO

-- 13. Gastos por Proyecto
CREATE OR ALTER PROCEDURE sp_GetGastosPorProyecto
    @ProyectoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Fecha, Concepto, Monto 
    FROM Gastos 
    WHERE ProyectoID = @ProyectoID 
    ORDER BY Fecha DESC;
END
GO

-- 14. Historial de Ventas por Proyecto
CREATE OR ALTER PROCEDURE sp_GetHistorialVentasProyecto
    @ProyectoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT VentaID, Lote, Cliente, Fecha, Tipo, ValorLote, Prima 
    FROM vHistorialVentasProyecto 
    WHERE ProyectoID = @ProyectoID 
    ORDER BY Fecha DESC;
END
GO

-- 15. Plan de Pagos por Cliente
CREATE OR ALTER PROCEDURE sp_GetPlanPagoClienteHome
    @ClienteID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Proyecto, Lote, VentaID, CuotaNumero, FechaVencimiento, Monto, Capital, Interes, Estado 
    FROM vPlanPagoCliente 
    WHERE ClienteID = @ClienteID 
    ORDER BY FechaVencimiento;
END
GO

-- 16. Historial de Pagos por Cliente
CREATE OR ALTER PROCEDURE sp_GetHistorialPagosClienteHome
    @ClienteID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PagoID AS Recibo, FechaPago, Proyecto, Lote, MontoPagado, CapitalPagado, InteresPagado, MoraPagada 
    FROM vHistorialPagosCliente 
    WHERE ClienteID = @ClienteID 
    ORDER BY FechaPago DESC;
END
GO

-- 17. Facturas por Cliente
CREATE OR ALTER PROCEDURE sp_GetFacturasCliente
    @ClienteID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.FacturaID as [# Factura], f.FechaEmision as [Fecha Emisión], p.Nombre as Proyecto, l.Numero as Lote, pa.MontoPagado as Monto, e.Nombre as Cajero 
    FROM Factura f 
    INNER JOIN Pago pa ON f.PagoID = pa.PagoID 
    INNER JOIN Venta v ON pa.VentaID = v.VentaID 
    INNER JOIN Lote l ON v.LoteID = l.LoteID 
    INNER JOIN Bloque b ON l.BloqueID = b.BloqueID 
    INNER JOIN Etapa et ON b.EtapaID = et.EtapaID 
    INNER JOIN Proyecto p ON et.ProyectoID = p.ProyectoID 
    INNER JOIN Empleado e ON pa.EmpleadoID = e.EmpleadoID 
    WHERE v.ClienteID = @ClienteID 
    ORDER BY f.FechaEmision DESC;
END
GO

-- 18. Cuotas Pendientes por Cliente
CREATE OR ALTER PROCEDURE sp_GetCuotasPendientesCliente
    @ClienteID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT pp.PlanPagoID, v.VentaID, p.Nombre as Proyecto, l.Numero as Lote, pp.CuotaNumero, pp.FechaVencimiento, pp.Monto
    FROM PlanPago pp
    INNER JOIN Venta v ON pp.VentaID = v.VentaID
    INNER JOIN Lote l ON v.LoteID = l.LoteID
    INNER JOIN Bloque b ON l.BloqueID = b.BloqueID
    INNER JOIN Etapa e ON b.EtapaID = e.EtapaID
    INNER JOIN Proyecto p ON e.ProyectoID = p.ProyectoID
    WHERE v.ClienteID = @ClienteID AND pp.Estado = 'Pendiente'
    ORDER BY pp.FechaVencimiento ASC;
END
GO

-- 19. Resumen de Ventas por Empleado (Usa la función existente)
CREATE OR ALTER PROCEDURE sp_GetResumenVentasPorEmpleado
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.fnResumenVentasPorEmpleado();
END
GO

-- 20. Lista de Proyectos con detalles de Etapa y Plazo
CREATE OR ALTER PROCEDURE sp_GetListaProyectosDetalle
AS
BEGIN
    SET NOCOUNT ON;
    SELECT P.ProyectoID, P.Nombre, P.Departamento, P.Municipio, P.PlazoMaximo,
           COUNT(E.EtapaID) AS TotalEtapas
    FROM Proyecto P
    LEFT JOIN Etapa E ON P.ProyectoID = E.ProyectoID
    GROUP BY P.ProyectoID, P.Nombre, P.Departamento, P.Municipio, P.PlazoMaximo
    ORDER BY P.Nombre;
END
GO

-- 21. Detalle de Lote para Proceso de Venta
CREATE OR ALTER PROCEDURE sp_GetLoteDetalleParaVenta
    @LoteID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT L.Numero, B.NumeroBloque, E.Nombre AS Etapa, P.Nombre AS Proyecto,
           dbo.fnValorLote(L.LoteID) AS Valor, E.Interes
    FROM Lote L
    JOIN Bloque B ON L.BloqueID = B.BloqueID
    JOIN Etapa E  ON B.EtapaID  = E.EtapaID
    JOIN Proyecto P ON E.ProyectoID = P.ProyectoID
    WHERE L.LoteID = @LoteID;
END
GO

-- 22. Lista de Avales
CREATE OR ALTER PROCEDURE sp_GetListaAvales
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AvalID, Nombre FROM Aval ORDER BY Nombre;
END
GO

-- 23. Lista de Beneficiarios
CREATE OR ALTER PROCEDURE sp_GetListaBeneficiarios
AS
BEGIN
    SET NOCOUNT ON;
    SELECT BeneficiarioID, Nombre FROM Beneficiario ORDER BY Nombre;
END
GO

-- 24. Detalle Completo de un Cliente
CREATE OR ALTER PROCEDURE sp_GetDetalleCliente
    @ClienteID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ClienteID, Nombre, DNI, Telefono, Trabajo, Sueldo
    FROM Cliente
    WHERE ClienteID = @ClienteID;
END
GO
