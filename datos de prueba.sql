INSERT INTO Proyecto (Nombre, Departamento, Municipio, PlazoMaximo)
VALUES ('Residencial Prado Alto', 'Cortés', 'San Pedro Sula', 20)

INSERT INTO Etapa (ProyectoID, Nombre, PrecioVara, Interes, AreaVerde)
VALUES 
(1, 'Etapa 1 - Sector Norte', 1200.00, 12.5, 0.15),
(1, 'Etapa 2 - Sector Centro', 1350.00, 11.0, 0.10),
(1, 'Etapa 3 - Sector Sur', 1500.00, 10.5, 0.20)


INSERT INTO Bloque (EtapaID, NumeroBloque) 
VALUES 
(1, 1),
(1, 2),
(1, 3),
(1, 4),
(1, 5)

INSERT INTO Bloque (EtapaID, NumeroBloque) 
VALUES 
(2, 6),
(2, 7),
(2, 8),
(2, 9),
(2, 10),
(2, 11)

INSERT INTO Bloque (EtapaID, NumeroBloque) 
VALUES 
(3, 12),
(3, 13),
(3, 14),
(3, 15)

INSERT INTO Lote (BloqueID, Numero, Area, Catastro, Matricula, Colindancias, Esquina, Parque, CalleCerrada)
VALUES
(1, 101, 200.0, 'CAT-101', 'MAT-101', 'N: LIMITE NORTE PROYECTO, S: LOTE 103, E: CALLE CERRADA, O: CALLE PRINCIPAL', 1, 0, 1),
(1, 102, 210.0, 'CAT-102', 'MAT-102', 'N: LIMITE NORTE PROYECTO, S: LOTE 104, E: CALLE SECUNDARIA, O: CALLE CERRADA', 1, 0, 1),
(1, 103, 220.0, 'CAT-103', 'MAT-103', 'N: LOTE 101, S: BLOQUE B, E: LOTE 104, O: CALLE PRINCIPAL', 1, 0, 0),
(1, 104, 230.0, 'CAT-104', 'MAT-104', 'N: LOTE 102, S: BLOQUE B, E: CALLE SECUNDARIA, O: LOTE 103', 1, 0, 0)

INSERT INTO Lote (BloqueID, Numero, Area, Catastro, Matricula, Colindancias, Esquina, Parque, CalleCerrada)
VALUES
(2, 201, 215.0, 'CAT-201', 'MAT-201', 'N: CALLE NORTE, S: LOTE 203, E: LOTE 205, O: BLOQUE 1', 1, 0, 0),
(2, 202, 210.0, 'CAT-202', 'MAT-202', 'N: CALLE NORTE, S: LOTE 204, E: LIMITE ESTE, O: LOTE 205', 1, 0, 0),
(2, 203, 220.0, 'CAT-203', 'MAT-203', 'N: LOTE 201, S: CALLE SUR, E: LOTE 206, O: BLOQUE 1', 1, 0, 0),
(2, 204, 225.0, 'CAT-204', 'MAT-204', 'N: LOTE 202, S: CALLE SUR, E: LIMITE ESTE, O: LOTE 206', 1, 0, 0),
(2, 205, 205.0, 'CAT-205', 'MAT-205', 'N: CALLE NORTE, S: LOTE 206, E: LOTE 202, O: LOTE 201', 0, 0, 0),
(2, 206, 212.0, 'CAT-206', 'MAT-206', 'N: LOTE 205, S: CALLE SUR, E: LOTE 204, O: LOTE 203', 0, 0, 0)


INSERT INTO Cuenta (EtapaID, Banco, NumeroCuenta)
VALUES
(1, 'BANCO ATLANTIDA', '110-225-336'),
(2, 'BAC CREDOMATIC', '987-654-321'),
(3, 'BANCO DE OCCIDENTE', '555-666-777')

INSERT INTO Empleado (Nombre, Cargo)
VALUES 
('MARCO ANTONIO SOLIS', 'CAJERO'),
('ELENA ABIGAIL RIVERA', 'CAJERO'),
('ROBERTO CARLOS MEJIA', 'COORDINADOR DE PROYECTO')


INSERT INTO Aval (Nombre, DNI, Telefono, Trabajo, Sueldo)
VALUES 
('ROBERTO SUAZO CORDOBA', '0801-1970-00112', '8877-6655', 'CONTADOR PÚBLICO', 35000.0),
('CARMEN DE LA RUIZ', '0501-1982-99887', '3344-5566', 'ADMINISTRADORA', 28500.0)

INSERT INTO Beneficiario (Nombre, DNI, Telefono, Parentesco)
VALUES 
('EILEN SUAZO', '0801-2015-00445', '9900-1122', 'HIJA'),
('MARIANO LÓPEZ', '0501-2010-00778', '3311-2244', 'HIJO')








