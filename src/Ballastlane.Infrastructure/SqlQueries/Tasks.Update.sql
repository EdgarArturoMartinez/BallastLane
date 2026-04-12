UPDATE Tasks
SET Title       = @title,
    Description = @description,
    Status      = @status,
    DueDate     = @dueDate
WHERE Id = @id
