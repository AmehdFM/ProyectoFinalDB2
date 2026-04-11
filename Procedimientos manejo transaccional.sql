--1)Registrar Venta con Plan de Pago y Actualización de Lote
--Cuando un cliente compra un lote, se deben insertar la venta, 
--generar el plan de pagos y marcar el lote como vendido. 
--Todo esto debe hacerse en una transacción para asegurar que
--no queden registros inconsistentes si ocurre un error.

CREATE OR ALTER PROCEDURE spRegistrarVenta
    @LoteID INT, @ClienteID INT, @EmpleadoID INT, @AvalID INT = NULL,
    @BeneficiarioID INT = NULL, @TipoPago VARCHAR(10), @Prima FLOAT = 0,
    @Plazo INT, @Interes FLOAT
AS
BEGIN
    -- 1. VALIDACIÓN DEL PLAZO MÁXIMO SEGÚN EL PROYECTO
    DECLARE @PlazoMaximoPermitido INT

    SELECT @PlazoMaximoPermitido = P.PlazoMaximo
    FROM Proyecto P
    INNER JOIN Etapa E ON P.ProyectoID = E.ProyectoID
    INNER JOIN Bloque B ON E.EtapaID = B.EtapaID
    INNER JOIN Lote L ON B.BloqueID = L.BloqueID
    WHERE L.LoteID = @LoteID

    -- SI EL PLAZO EXCEDE EL LÍMITE, DETENEMOS LA OPERACIÓN ANTES DE INICIAR LA TRANSACCIÓN
    IF @Plazo > @PlazoMaximoPermitido
    BEGIN
        RETURN
    END

    -- 2. INICIO DEL MANEJO TRANSACCIONAL
    BEGIN TRANSACTION

    -- EL INSERT DISPARARÁ AUTOMÁTICAMENTE LOS TRIGGERS tr_ActualizarEstadoLote Y tr_GenerarPlanPago
    INSERT INTO Venta (LoteID, ClienteID, EmpleadoID, AvalID, BeneficiarioID, Tipo, Prima, Plazo, Interes)
    VALUES (@LoteID, @ClienteID, @EmpleadoID, @AvalID, @BeneficiarioID, @TipoPago, @Prima, @Plazo, @Interes)

    -- 3. VALIDACIÓN DE INTEGRIDAD Y ERRORES (TODO O NADA)
    IF @@ERROR <> 0
    BEGIN
        ROLLBACK TRANSACTION

    END
    ELSE
    BEGIN
        COMMIT TRANSACTION
    END
END
GO

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



--------------------------------------------
CREATE PROCEDURE spCrearProyecto
    @Nombre VARCHAR(100),
    @Departamento VARCHAR(50),
    @Municipio VARCHAR(50),
    @PlazoMaximo INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validaciones
    IF LTRIM(RTRIM(@Nombre)) = ''
    BEGIN
        RAISERROR('El nombre del proyecto no puede estar vacío.', 16, 1);
        RETURN;
    END

    IF @PlazoMaximo <= 0
    BEGIN
        RAISERROR('El plazo máximo debe ser mayor a 0.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM Proyecto WHERE Nombre = @Nombre)
    BEGIN
        RAISERROR('Ya existe un proyecto con ese nombre.', 16, 1);
        RETURN;
    END

    -- Inserción
    INSERT INTO Proyecto (Nombre, Departamento, Municipio, PlazoMaximo)
    VALUES (@Nombre, @Departamento, @Municipio, @PlazoMaximo);

    -- Retornar el ID generado
    SELECT 
        SCOPE_IDENTITY() AS ProyectoID,
        @Nombre AS Nombre,
        @Departamento AS Departamento,
        @Municipio AS Municipio,
        @PlazoMaximo AS PlazoMaximo,
        'Proyecto creado exitosamente.' AS Mensaje;
END
GO

create procedure sp_CrearEtapa
    @ProyectoID int,
    @Nombre varchar(100),
    @PrecioVara float,
    @Interes float,
    @AreaVerde float
as
begin
    set nocount on;

    if not exists (select 1 from Proyecto where ProyectoID = @ProyectoID)
    begin
        raiserror('El proyecto especificado no existe.', 16, 1);
        return;
    end

    if @PrecioVara <= 0
    begin
        raiserror('El precio por vara debe ser mayor a 0.', 16, 1);
        return;
    end

    if @Interes < 0
    begin
        raiserror('El interés no puede ser negativo.', 16, 1);
        return;
    end

    if @AreaVerde < 0 or @AreaVerde > 1
    begin
        raiserror('El área verde debe estar entre 0 y 1.', 16, 1);
        return;
    end

    insert into Etapa (ProyectoID, Nombre, PrecioVara, Interes, AreaVerde)
    values (@ProyectoID, @Nombre, @PrecioVara, @Interes, @AreaVerde);

    select scope_identity() as EtapaID, 'Etapa creada exitosamente.' as Mensaje;
end
go

create procedure sp_CrearBloque
    @EtapaID int,
    @NumeroBloque int
as
begin
    set nocount on;

    if not exists (select 1 from Etapa where EtapaID = @EtapaID)
    begin
        raiserror('La etapa especificada no existe.', 16, 1);
        return;
    end

    if exists (select 1 from Bloque where EtapaID = @EtapaID and NumeroBloque = @NumeroBloque)
    begin
        raiserror('Ya existe un bloque con ese número en la etapa especificada.', 16, 1);
        return;
    end

    insert into Bloque (EtapaID, NumeroBloque)
    values (@EtapaID, @NumeroBloque);

    select scope_identity() as BloqueID, 'Bloque creado exitosamente.' as Mensaje;
end
go

create procedure sp_CrearLote
    @BloqueID int,
    @Numero int,
    @Area float,
    @Catastro varchar(50),
    @Matricula varchar(50),
    @Colindancias varchar(max),
    @Esquina bit,
    @Parque bit,
    @CalleCerrada bit
as
begin
    set nocount on;

    if not exists (select 1 from Bloque where BloqueID = @BloqueID)
    begin
        raiserror('El bloque especificado no existe.', 16, 1);
        return;
    end

    if @Area <= 0
    begin
        raiserror('El área debe ser mayor a 0.', 16, 1);
        return;
    end

    if exists (select 1 from Lote where BloqueID = @BloqueID and Numero = @Numero)
    begin
        raiserror('Ya existe un lote con ese número en el bloque especificado.', 16, 1);
        return;
    end

    insert into Lote (BloqueID, Numero, Area, Catastro, Matricula, Colindancias, Esquina, Parque, CalleCerrada, Estado)
    values (@BloqueID, @Numero, @Area, @Catastro, @Matricula, @Colindancias, @Esquina, @Parque, @CalleCerrada, 'Disponible');

    select scope_identity() as LoteID, 'Lote creado exitosamente.' as Mensaje;
end
go

create or alter procedure sp_CrearVenta
    @LoteID int,
    @ClienteID int,
    @EmpleadoID int,
    @AvalID int = null,
    @BeneficiarioID int = null,
    @Tipo varchar(10),
    @Prima float,
    @Plazo int,
    @Interes float
as
begin
    set nocount on;

    if not exists (select 1 from Lote where LoteID = @LoteID and Estado = 'Disponible')
    begin
        raiserror('El lote no existe o no está disponible.', 16, 1);
        return;
    end

    if not exists (select 1 from Cliente where ClienteID = @ClienteID)
    begin
        raiserror('El cliente especificado no existe.', 16, 1);
        return;
    end

    if not exists (select 1 from Empleado where EmpleadoID = @EmpleadoID)
    begin
        raiserror('El empleado especificado no existe.', 16, 1);
        return;
    end

    if @Tipo not in ('Contado', 'Credito')
    begin
        raiserror('El tipo de venta debe ser Contado o Credito.', 16, 1);
        return;
    end

    if @Prima < 0
    begin
        raiserror('La prima no puede ser negativa.', 16, 1);
        return;
    end

    if @Plazo <= 0
    begin
        raiserror('El plazo debe ser mayor a 0.', 16, 1);
        return;
    end

    if @Interes < 0
    begin
        raiserror('El interés no puede ser negativo.', 16, 1);
        return;
    end

    insert into Venta (LoteID, ClienteID, EmpleadoID, AvalID, BeneficiarioID, Tipo, Prima, Plazo, Interes)
    values (@LoteID, @ClienteID, @EmpleadoID, @AvalID, @BeneficiarioID, @Tipo, @Prima, @Plazo, @Interes);

    select scope_identity() as VentaID, 'Venta registrada exitosamente.' as Mensaje;
end
go

-- =========================================================
-- Procedimientos auxiliares: insertar personas desde el
-- formulario de venta (Cliente, Aval, Beneficiario)
-- =========================================================

CREATE OR ALTER PROCEDURE sp_InsertarCliente
    @Nombre   VARCHAR(100),
    @DNI      VARCHAR(15),
    @Telefono VARCHAR(15),
    @Trabajo  VARCHAR(100),
    @Sueldo   FLOAT
AS
BEGIN
    SET NOCOUNT ON;
    IF LTRIM(RTRIM(@Nombre)) = ''
    BEGIN RAISERROR('El nombre del cliente no puede estar vacio.', 16, 1); RETURN; END
    IF LTRIM(RTRIM(@DNI)) = ''
    BEGIN RAISERROR('El DNI del cliente es obligatorio.', 16, 1); RETURN; END
    IF EXISTS (SELECT 1 FROM Cliente WHERE DNI = @DNI)
    BEGIN RAISERROR('Ya existe un cliente con ese DNI.', 16, 1); RETURN; END
    IF @Sueldo <= 0
    BEGIN RAISERROR('El sueldo debe ser mayor a 0.', 16, 1); RETURN; END
    INSERT INTO Cliente (Nombre, DNI, Telefono, Trabajo, Sueldo)
    VALUES (@Nombre, @DNI, @Telefono, @Trabajo, @Sueldo);
    SELECT SCOPE_IDENTITY() AS ClienteID;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarAval
    @Nombre   VARCHAR(100),
    @DNI      VARCHAR(15),
    @Telefono VARCHAR(15),
    @Trabajo  VARCHAR(100),
    @Sueldo   FLOAT
AS
BEGIN
    SET NOCOUNT ON;
    IF LTRIM(RTRIM(@Nombre)) = ''
    BEGIN RAISERROR('El nombre del aval no puede estar vacio.', 16, 1); RETURN; END
    IF LTRIM(RTRIM(@DNI)) = ''
    BEGIN RAISERROR('El DNI del aval es obligatorio.', 16, 1); RETURN; END
    IF EXISTS (SELECT 1 FROM Aval WHERE DNI = @DNI)
    BEGIN RAISERROR('Ya existe un aval con ese DNI.', 16, 1); RETURN; END
    IF @Sueldo <= 0
    BEGIN RAISERROR('El sueldo debe ser mayor a 0.', 16, 1); RETURN; END
    INSERT INTO Aval (Nombre, DNI, Telefono, Trabajo, Sueldo)
    VALUES (@Nombre, @DNI, @Telefono, @Trabajo, @Sueldo);
    SELECT SCOPE_IDENTITY() AS AvalID;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarBeneficiario
    @Nombre     VARCHAR(100),
    @DNI        VARCHAR(15),
    @Telefono   VARCHAR(15),
    @Parentesco VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF LTRIM(RTRIM(@Nombre)) = ''
    BEGIN RAISERROR('El nombre del beneficiario no puede estar vacio.', 16, 1); RETURN; END
    IF LTRIM(RTRIM(@DNI)) = ''
    BEGIN RAISERROR('El DNI del beneficiario es obligatorio.', 16, 1); RETURN; END
    INSERT INTO Beneficiario (Nombre, DNI, Telefono, Parentesco)
    VALUES (@Nombre, @DNI, @Telefono, @Parentesco);
    SELECT SCOPE_IDENTITY() AS BeneficiarioID;
END
GO

-- =========================================================
-- CRUD PARA EMPLEADOS (Requerido por Rúbrica)
-- =========================================================

CREATE OR ALTER PROCEDURE sp_InsertarEmpleado
    @Nombre VARCHAR(100),
    @Cargo  VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Empleado (Nombre, Cargo)
    VALUES (@Nombre, @Cargo);
    SELECT SCOPE_IDENTITY() AS EmpleadoID;
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarEmpleado
    @EmpleadoID INT,
    @Nombre     VARCHAR(100),
    @Cargo      VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Empleado 
    SET Nombre = @Nombre, Cargo = @Cargo 
    WHERE EmpleadoID = @EmpleadoID;
END
GO

CREATE OR ALTER PROCEDURE sp_EliminarEmpleado
    @EmpleadoID INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Nota: Podría fallar si tiene ventas asociadas (integridad referencial)
    DELETE FROM Empleado WHERE EmpleadoID = @EmpleadoID;
END
GO
