-- Migration 0001: Initial schema
-- Creates Users and Tasks tables.
-- NOTE: The __Migrations tracking table is created by DbMigrator itself; it is NOT part of this script.

CREATE TABLE Users (
    Id           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Username     NVARCHAR(100)    NOT NULL,
    Email        NVARCHAR(320)    NOT NULL,
    PasswordHash NVARCHAR(512)    NOT NULL,
    Salt         NVARCHAR(128)    NOT NULL,
    Role         NVARCHAR(50)     NOT NULL DEFAULT 'User',
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email    UNIQUE (Email)
);

CREATE TABLE Tasks (
    Id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Title       NVARCHAR(200)    NOT NULL,
    Description NVARCHAR(2000)   NOT NULL DEFAULT '',
    Status      NVARCHAR(50)     NOT NULL DEFAULT 'Todo',
    DueDate     DATETIME2        NULL,
    OwnerUserId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT FK_Tasks_Users FOREIGN KEY (OwnerUserId) REFERENCES Users(Id)
);

CREATE INDEX IX_Tasks_OwnerUserId ON Tasks(OwnerUserId);
