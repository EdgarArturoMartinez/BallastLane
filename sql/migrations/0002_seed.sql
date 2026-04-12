-- Migration 0002: Seed demo user and sample tasks
-- Demo credentials: username=demo  password=Demo@12345
-- PasswordHash and Salt were pre-generated with PBKDF2 (100000 iterations, SHA256).
-- To regenerate: run Ballastlane.Api with /api/auth/register and copy the values.
--
-- NOTE: These are development-only seed credentials. Never use in production.

DECLARE @demoUserId UNIQUEIDENTIFIER = 'A1B2C3D4-0000-0000-0000-000000000001';

INSERT INTO Users (Id, Username, Email, PasswordHash, Salt, Role)
VALUES (
    @demoUserId,
    'demo',
    'demo@ballastlane.dev',
    'SEED_HASH_REPLACE_ON_FIRST_RUN',   -- replaced by DbSeeder on startup if still placeholder
    'SEED_SALT_REPLACE_ON_FIRST_RUN',
    'Admin'
);

INSERT INTO Tasks (Id, Title, Description, Status, DueDate, OwnerUserId)
VALUES
    (NEWID(), 'Set up project', 'Scaffold solution and configure CI', 'Done',    NULL,                    @demoUserId),
    (NEWID(), 'Implement API',  'Build REST endpoints for tasks',     'InProgress', '2026-04-30 00:00:00', @demoUserId),
    (NEWID(), 'Write tests',    'Unit and integration tests',         'Todo',      '2026-05-15 00:00:00', @demoUserId);
