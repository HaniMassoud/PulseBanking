// Create new file: src/PulseBanking.Infrastructure/Attributes/SkipTenantValidationAttribute.cs
namespace PulseBanking.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SkipTenantValidationAttribute : Attribute
{
}