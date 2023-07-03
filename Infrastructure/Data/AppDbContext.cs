using Dapper;
using Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        SqlMapper.AddTypeHandler(typeof(Guid), GuidTypeHandler.Default);
        SqlMapper.AddTypeHandler(typeof(DateTime), DateTimeUtcHandler.Default);
        SqlMapper.AddTypeHandler(typeof(bool), OracleBooleanHandler.Default);
        SqlMapper.AddTypeHandler(typeof(DateOnly), DateOnlyHandler.Default);
    }

}