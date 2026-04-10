CREATE OR ALTER PROCEDURE sp_GenerarReporteMorosidadMasiva
AS
BEGIN

    -- Declaración de variables para almacenar los datos procesados por el cursor
    DECLARE @ClienteID INT
    DECLARE @Nombre VARCHAR(100)
    DECLARE @SaldoPendiente FLOAT
    DECLARE @EsMoroso BIT

    -- Definición del cursor con el prefijo cr_ solicitado
    DECLARE cr_Clientes CURSOR FOR 
    SELECT ClienteID, Nombre FROM Cliente

    -- Apertura del cursor para iniciar el recorrido
    OPEN cr_Clientes

    -- Captura de la primera fila de la tabla clientes
    FETCH NEXT FROM cr_Clientes INTO @ClienteID, @Nombre

    -- Ciclo de procesamiento mientras existan registros (@@FETCH_STATUS = 0)
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Verificamos si el cliente tiene cuotas vencidas usando la función de mora [2]
        SET @EsMoroso = dbo.fnEsClienteMoroso(@ClienteID)

        IF @EsMoroso = 1
        BEGIN
            -- Obtenemos el monto total adeudado mediante la función de saldo [3]
            SET @SaldoPendiente = dbo.fnSaldoPendiente(@ClienteID)

            -- Generamos el reporte individual en la consola de mensajes
            PRINT 'AVISO DE COBRO - Cliente: ' + @Nombre + 
                  ' (ID: ' + CAST(@ClienteID AS VARCHAR) + ') ' +
                  'Deuda Pendiente: L. ' + CAST(@SaldoPendiente AS VARCHAR)
        END

        -- Avanzamos a la siguiente fila del cursor
        FETCH NEXT FROM cr_Clientes INTO @ClienteID, @Nombre
    END

    -- Cierre y liberación de recursos del cursor
    CLOSE cr_Clientes
    DEALLOCATE cr_Clientes
END
GO


CREATE OR ALTER PROCEDURE sp_AjustePreciosLotesDisponibles
    @EtapaID INT,
    @IncrementoVara FLOAT
AS
BEGIN
    SET NOCOUNT ON

    -- DECLARAMOS LAS VARIABLES PARA EL MANEJO DE DATOS
    DECLARE @LoteID INT
    DECLARE @Area FLOAT
    DECLARE @PrecioVaraActual FLOAT
    DECLARE @NuevoValorProyectado FLOAT

    -- DEFINIMOS EL CURSOR CON EL PREFIJO CR_
    -- SELECCIONA LOTES DISPONIBLES QUE SON DE ESQUINA EN UNA ETAPA ESPECÍFICA
    DECLARE cr_LotesReval CURSOR FOR 
    SELECT L.LoteID, L.Area, E.PrecioVara
    FROM Lote L
    INNER JOIN Bloque B ON L.BloqueID = B.BloqueID
    INNER JOIN Etapa E ON B.EtapaID = E.EtapaID
    WHERE E.EtapaID = @EtapaID 
    AND L.Estado = 'Disponible'
    AND L.Esquina = 1 -- CONDICIÓN ADICIONAL SOLICITADA

    -- APERTURA DEL DISPOSITIVO DE CONTROL
    OPEN cr_LotesReval

    -- CAPTURA DE LA PRIMERA FILA
    FETCH NEXT FROM cr_LotesReval INTO @LoteID, @Area, @PrecioVaraActual

    -- CICLO DE PROCESAMIENTO MIENTRAS EXISTAN REGISTROS
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- CALCULAMOS EL NUEVO VALOR APLICANDO EL INCREMENTO A LA VARA CUADRADA
        -- SE INCLUYE EL 10% ADICIONAL POR SER DE ESQUINA SEGÚN LA LÓGICA FINANCIERA
        SET @NuevoValorProyectado = (@Area * (@PrecioVaraActual + @IncrementoVara)) * 1.10

        -- GENERAMOS EL REPORTE DE REVALUACIÓN PROYECTADA
        PRINT 'ANÁLISIS DE PLUSVALÍA - Lote ID: ' + CAST(@LoteID AS VARCHAR) + 
              ' | Área: ' + CAST(@Area AS VARCHAR) + ' v2' +
              ' | Nuevo Valor Estimado: L. ' + CAST(@NuevoValorProyectado AS VARCHAR)

        -- AVANZAMOS A LA SIGUIENTE FILA DEL CURSOR
        FETCH NEXT FROM cr_LotesReval INTO @LoteID, @Area, @PrecioVaraActual
    END

    -- CIERRE Y LIBERACIÓN DE RECURSOS
    CLOSE cr_LotesReval
    DEALLOCATE cr_LotesReval
END
GO


CREATE OR ALTER PROCEDURE sp_ConsolidarPagosDiariosCaja
AS
BEGIN
    SET NOCOUNT ON

    -- 1. DECLARAMOS UNA VARIABLE TIPO TABLA PARA ALMACENAR LOS RESULTADOS
    -- ESTO PERMITE QUE EL RESULTADO FINAL SE VEA COMO UNA TABLA FÍSICA [3, 6]
    DECLARE @ReporteFinal TABLE (
        Banco VARCHAR(50),
        NumeroCuenta VARCHAR(50),
        MontoTotal FLOAT
    )

    -- DECLARAMOS LAS VARIABLES PARA EL TRABAJO DEL CURSOR
    DECLARE @CuentaID INT
    DECLARE @Banco VARCHAR(50)
    DECLARE @NumeroCuenta VARCHAR(50)
    DECLARE @MontoConsolidado FLOAT

    -- 2. DEFINIMOS EL CURSOR CON EL PREFIJO CR_
    DECLARE cr_PagosDiarios CURSOR FOR 
    SELECT 
        C.CuentaID, 
        C.Banco, 
        C.NumeroCuenta, 
        SUM(P.MontoPagado)
    FROM Pago P
    INNER JOIN Cuenta C ON P.CuentaID = C.CuentaID
    WHERE CAST(P.FechaPago AS DATE) = CAST(GETDATE() AS DATE)
    AND P.TipoPago = 'Efectivo'
    GROUP BY C.CuentaID, C.Banco, C.NumeroCuenta

    OPEN cr_PagosDiarios

    FETCH NEXT FROM cr_PagosDiarios INTO @CuentaID, @Banco, @NumeroCuenta, @MontoConsolidado

    -- 3. CICLO DE PROCESAMIENTO
    WHILE @@FETCH_STATUS = 0
    BEGIN
        INSERT INTO @ReporteFinal (Banco, NumeroCuenta, MontoTotal)
        VALUES (@Banco, @NumeroCuenta, @MontoConsolidado)

        FETCH NEXT FROM cr_PagosDiarios INTO @CuentaID, @Banco, @NumeroCuenta, @MontoConsolidado
    END

    CLOSE cr_PagosDiarios
    DEALLOCATE cr_PagosDiarios

    SELECT * FROM @ReporteFinal
END
GO
