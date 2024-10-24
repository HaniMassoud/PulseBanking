// In PulseBanking.Infrastructure/Services/DateTimeService.cs
using PulseBanking.Application.Interfaces;

namespace PulseBanking.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}