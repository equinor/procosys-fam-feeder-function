namespace Core.Interfaces;

public interface IDbStatusFeederService
{
    Task<string> RunFeeder();
}