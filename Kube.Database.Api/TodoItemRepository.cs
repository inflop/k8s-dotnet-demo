using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Kube.Contracts;

namespace Kube.Database.Api;

public class TodoItemRepository
{
    private readonly string _connectionString = "";
    private readonly string _listQuery = "SELECT * FROM todo";
    private readonly string _getQuery = "SELECT * FROM todo WHERE id = @id";
    private readonly string _insertQuery = "INSERT todo (title) VALUES (@title)";
    private readonly string _updateQuery = "UPDATE todo SET title = @title, isCompleted = @isCompleted, updatedAt = GETDATE() WHERE id = @id";
    private readonly string _deleteQuery = "DELETE FROM todo WHERE id = @id";

    public TodoItemRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<TodoItem>> Get()
    {
        using var connection = new SqlConnection(_connectionString);
        var list = await connection.QueryAsync<TodoItem>(_listQuery);
        return list;
    }

    public async Task<TodoItem> Get(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        var item = await connection.QuerySingleOrDefaultAsync<TodoItem>(_getQuery, new { id });
        return item;
    }

    public async Task Add(string title)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.QueryAsync(_insertQuery, new { title });
    }

    public async Task Update(int id, string title, bool isCompleted)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.QueryAsync(_updateQuery, new { id, title, isCompleted });
    }

    public async Task Delete(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.QueryAsync(_deleteQuery, new { id });
    }
}
