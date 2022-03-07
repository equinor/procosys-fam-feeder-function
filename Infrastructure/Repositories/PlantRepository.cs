using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PlantRepository : IPlantRepository
{
    private readonly AppDbContext _context;

    public PlantRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetAllPlants()
    {
        await using var context = _context;
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "Select projectschema from projectschema";
        await context.Database.OpenConnectionAsync();
        await using var result = await command.ExecuteReaderAsync();
        var plants = new List<string>();

        while (await result.ReadAsync()) plants.Add((string)result[0]);

        return plants;
    }
}