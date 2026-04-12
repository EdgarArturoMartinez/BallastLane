SELECT Id, Username, Email, PasswordHash, Salt, Role
FROM Users
WHERE Id = @id
