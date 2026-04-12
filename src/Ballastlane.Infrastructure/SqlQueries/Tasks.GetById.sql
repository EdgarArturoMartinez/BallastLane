SELECT Id, Title, Description, Status, DueDate, OwnerUserId
FROM Tasks
WHERE Id = @id
