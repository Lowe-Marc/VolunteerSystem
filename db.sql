CREATE DATABASE Volunteer;
CREATE TABLE UserTable(id INT NOT NULL, UserName VARCHAR(25) UNIQUE, LastName VARCHAR(50), FirstName VARCHAR(50), Email VARCHAR(50), Phone CHAR(10), IsAdmin BIT, IsManager BIT, Password VARCHAR(25), PRIMARY KEY(id));
CREATE TABLE Event(StartTime DATETIME NOT NULL, EndTime DATETIME, Description VARCHAR(255), NumVolunteers INT, Location VARCHAR(100) PRIMARY KEY(StartTime));
INSERT INTO UserTable (id, UserName, IsAdmin, IsManager, Password) VALUES (1,'sysadmin', 1, 1, 'password');