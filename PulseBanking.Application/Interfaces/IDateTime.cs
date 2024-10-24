// In PulseBanking.Application/Interfaces/IDateTime.cs
namespace PulseBanking.Application.Interfaces;

public interface IDateTime
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}