using Core.Models.DbStatus;

namespace Core.Interfaces;

public interface IDbStatusRepository
{
    Task<List<MetricDto>> GetMetrics();
}