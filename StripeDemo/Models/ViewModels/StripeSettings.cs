namespace StripeDemo.Models.ViewModels;

public class StripeSettings
{
    public const string SettingsOptions = "StripeSettings";
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public string WebHookSecret { get; set; }
}