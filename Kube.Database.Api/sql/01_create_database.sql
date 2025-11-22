if not exists (select name from sys.databases where name = 'todo_db')
begin
    create database todo_db
end
