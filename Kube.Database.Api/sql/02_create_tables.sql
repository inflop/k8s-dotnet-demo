if not exists (select * from sys.tables where name = 'todo')
begin
    create table dbo.todo
    (
        id int identity(1,1) not null primary key,
        title nvarchar(512) not null,
        isCompleted bit not null default 0,
        createdAt datetime not null default getdate(),
        updatedAt datetime null
    )
end
