using PulseBanking.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PulseBanking.WebApp.Models;

public class TenantRegistrationModel
{
    private string bankName = string.Empty;
    private string adminFirstName = string.Empty;
    private string adminLastName = string.Empty;
    private string adminEmail = string.Empty;
    private string adminPassword = string.Empty;
    private string adminPhoneNumber = string.Empty;

    [Required(ErrorMessage = "Bank name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Bank name must be between 2 and 100 characters")]
    public string BankName
    {
        get => bankName;
        set => bankName = value ?? string.Empty;
    }

    [Required(ErrorMessage = "Time zone is required")]
    public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;

    [Required(ErrorMessage = "Currency code is required")]
    public string CurrencyCode { get; set; } = "USD";

    [Range(0, 1000000, ErrorMessage = "Transaction limit must be between 0 and 1,000,000")]
    public decimal DefaultTransactionLimit { get; set; } = 10000m;

    [Required(ErrorMessage = "Deployment type is required")]
    public DeploymentType DeploymentType { get; set; } = DeploymentType.Shared;

    [Required(ErrorMessage = "Region is required")]
    public RegionCode Region { get; set; } = RegionCode.AUS;

    [Required(ErrorMessage = "Instance type is required")]
    public InstanceType InstanceType { get; set; } = InstanceType.Production;

    [Required(ErrorMessage = "Data sovereignty compliance is required")]
    public bool DataSovereigntyCompliant { get; set; } = true;

    [Required(ErrorMessage = "First name is required")]
    public string AdminFirstName
    {
        get => adminFirstName;
        set => adminFirstName = value ?? string.Empty;
    }

    [Required(ErrorMessage = "Last name is required")]
    public string AdminLastName
    {
        get => adminLastName;
        set => adminLastName = value ?? string.Empty;
    }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string AdminEmail
    {
        get => adminEmail;
        set => adminEmail = value ?? string.Empty;
    }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
    public string AdminPassword
    {
        get => adminPassword;
        set => adminPassword = value ?? string.Empty;
    }

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string AdminPhoneNumber
    {
        get => adminPhoneNumber;
        set => adminPhoneNumber = value ?? string.Empty;
    }
}
