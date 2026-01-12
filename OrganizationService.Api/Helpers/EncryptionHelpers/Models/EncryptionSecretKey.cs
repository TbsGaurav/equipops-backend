namespace OrganizationService.Api.Helpers.EncryptionHelpers.Models
{
    public class EncryptionSecretKey
    {
        public string Secret { get; set; } = string.Empty;
        public string DevelopmentSecret { get; set; } = string.Empty;
        public string ProductionSecret { get; set; } = string.Empty;
    }
}
