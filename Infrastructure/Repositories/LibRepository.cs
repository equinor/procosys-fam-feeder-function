
using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repositories;

public class LibRepository : ILibRepository
{
    
    private readonly int _messageChunkSize;
    private readonly AppDbContext _context;

    public LibRepository(AppDbContext context, IConfiguration configuration)
    {
        _messageChunkSize = int.Parse(configuration["MessageChunkSize"]);
        _context = context;
    }



    private async Task<List<LibraryData>> ExecuteQuery(string query)
    {
       
        await using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;

        await _context.Database.OpenConnectionAsync();

        await using var result = await command.ExecuteReaderAsync();
        var entities = new List<LibraryData>();

        while (await result.ReadAsync()) entities.Add(new LibraryData
        {
            Id = (long)result[0],
            Code = result[1] != DBNull.Value ? (string)result[1] : null,
            Description = result[2] != DBNull.Value ? (string)result[2] : null
        });

        //await context.Database.CloseConnectionAsync();

        return entities;
    }

    public async Task<List<LibraryData>> GetLibraryValues()
    {
        const string query = @"select library_id as Id, Code, Description 
                    from Library where projectschema = 'PCS$JOHAN_CASTBERG'";

        return await ExecuteQuery(query);
    }

    public async Task<List<LibraryData>> GetResponsibleValues()
    {
        const string query = @"select responsible_id as Id, Code, Description 
                    from Responsible where projectschema = 'PCS$JOHAN_CASTBERG'";

        return await ExecuteQuery(query);
    }
}