using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Repositories;

public class PlantRepository : IPlantRepository
{
    private readonly AppDbContext _context;

    public PlantRepository(AppDbContext context) => _context = context;

    public async Task<List<string>> GetAllPlants()
    {
        var dbConnection = _context.Database.GetDbConnection();
        var connectionWasClosed = dbConnection.State != ConnectionState.Open;
        if (connectionWasClosed)
        {
            await _context.Database.OpenConnectionAsync();
        }
        try
        {
            await using var command = dbConnection.CreateCommand();
            command.CommandText = "Select projectschema from projectschema";
            await using var result = await command.ExecuteReaderAsync();
            var plants = new List<string>();

            while (await result.ReadAsync())
            {
                plants.Add((string)result[0]);
            }
            return plants;
        }
        finally
        {
            //If we open it, we have to close it.
            if (connectionWasClosed)
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
    }
}