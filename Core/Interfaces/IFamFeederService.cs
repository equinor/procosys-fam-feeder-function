using Core.Models;

namespace Core.Interfaces;

public interface IFamFeederService
{
    Task RunFeeder(QueryParameters queryParams);
    Task<List<string>> GetAllPlants();
}