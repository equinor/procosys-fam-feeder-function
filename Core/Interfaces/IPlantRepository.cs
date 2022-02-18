namespace Core.Interfaces;

public interface IPlantRepository
{
    Task<List<string>> GetAllPlants();
}