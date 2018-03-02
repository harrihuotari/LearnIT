-- =========================================
-- Create table template Windows Azure SQL Database 
-- =========================================

-- IF OBJECT_ID('<schema_name, sysname, dbo>.<table_name, sysname, sample_table>', 'U') IS NOT NULL
--  DROP TABLE <schema_name, sysname, dbo>.<table_name, sysname, sample_table>
-- GO

-- CREATE TABLE <schema_name, sysname, dbo>.<table_name, sysname, sample_table>
-- (
--	<columns_in_primary_key, , c1> <column1_datatype, , int> <column1_nullability,, NOT NULL>, 
--	<column2_name, sysname, c2> <column2_datatype, , char(10)> <column2_nullability,, NULL>, 
--	<column3_name, sysname, c3> <column3_datatype, , datetime> <column3_nullability,, NULL>, 
--    CONSTRAINT <contraint_name, sysname, PK_sample_table> PRIMARY KEY (<columns_in_primary_key, , c1>)
--)
-- GO
USE slaneTestdb
go
create table SensorData
(
	Id int primary key,
	SensorId int,
	Temperature float,
	Humidity float,
	AccelerationX int,
	AccelerationY int,
	AccelerationZ int,
	AmbientLight int,
	Pressure int,
	SwitchDataUser nvarchar(50),
	Hall bit,
	VibrationFrequency int,
	VibrationPower int,
	BatteryLevel int
)
go

use slaneTestdb
go
insert into SensorData
values(112345, 12346, 24.55, 30.55, 1, 0, 1, 5500, 850, 'NodeK', 0, 250, 160, 85)

select * from SensorData
go

USE slaneTestdb
go
create table SensorDataValues
(
	Id int primary key,
	SensorId int,
	Temperature float,
	Humidity float,
	AccelerationX float,
	AccelerationY float,
	AccelerationZ float,
	AmbientLight int,
	Pressure int,
	SwitchDataUser nvarchar(50),
	Hall bit,
	VibrationFrequency int,
	VibrationPower int,
	BatteryLevel int
)
go

USE slaneTestdb
go
create table SensorDataStream
(
	Id int not null identity(1,1) primary key,
	SensorId int,
	Temperature float,
	Humidity float,
	AccelerationX float,
	AccelerationY float,
	AccelerationZ float,
	AmbientLight int,
	Pressure int,
	SwitchDataUser nvarchar(50),
	Hall bit,
	VibrationFrequency int,
	VibrationPower int,
	BatteryLevel int
)
go


