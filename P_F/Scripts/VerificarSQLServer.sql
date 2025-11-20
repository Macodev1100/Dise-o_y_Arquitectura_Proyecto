-- Script para verificar la configuración de SQL Server
-- Ejecutar este script en SQL Server Management Studio para verificar la conexión

-- 1. Verificar versión de SQL Server
SELECT @@VERSION AS 'Versión SQL Server';

-- 2. Verificar si la base de datos existe
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'TallerMecanicoDb')
BEGIN
    PRINT 'La base de datos TallerMecanicoDb ya existe'
    USE TallerMecanicoDb
    
    -- Mostrar tablas existentes
    SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'
END
ELSE
BEGIN
    PRINT 'La base de datos TallerMecanicoDb no existe - será creada por Entity Framework'
END

-- 3. Verificar configuración de SQL Server Express
SELECT 
    name AS 'Nombre Base de Datos',
    state_desc AS 'Estado',
    collation_name AS 'Collation'
FROM sys.databases;

-- 4. Verificar usuarios y permisos
SELECT 
    name AS 'Login Name',
    type_desc AS 'Login Type',
    is_disabled AS 'Is Disabled'
FROM sys.server_principals
WHERE type IN ('S', 'G', 'U')
AND name NOT LIKE '##%';

-- 5. Verificar configuración de autenticación
EXEC xp_instance_regread N'HKEY_LOCAL_MACHINE', 
    N'Software\Microsoft\MSSQLServer\MSSQLServer', 
    N'LoginMode';

PRINT 'Script de verificación completado.'
PRINT 'Si no hay errores, SQL Server está configurado correctamente.'