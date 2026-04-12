-- Migration 0003: Audit table for tracking changes
-- Stores entity, entity id, action, user, and JSON payloads for old/new values.

CREATE TABLE Audits (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Entity NVARCHAR(100) NOT NULL,
    EntityId NVARCHAR(100) NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    UserId UNIQUEIDENTIFIER NULL,
    Username NVARCHAR(200) NULL,
    OldValues NVARCHAR(MAX) NULL, -- JSON payload (nullable)
    NewValues NVARCHAR(MAX) NULL, -- JSON payload (nullable)
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE INDEX IX_Audits_Entity_CreatedAt ON Audits(Entity, CreatedAt);
