﻿IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE [name] = 'HangFire') EXEC ('CREATE SCHEMA [HangFire]')
GO

ALTER AUTHORIZATION ON SCHEMA::[HangFire] TO [HangFireUser]
GO

GRANT CREATE TABLE TO [HangFireUser]
GO