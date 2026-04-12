SELECT Id, Username, Email, PasswordHash, Salt, Role
FROM Users
WHERE Username = @username
