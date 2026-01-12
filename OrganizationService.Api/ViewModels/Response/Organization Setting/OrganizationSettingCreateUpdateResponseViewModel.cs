namespace OrganizationService.Api.ViewModels.Response.OrganizationSetting
{
    public class OrganizationSettingCreateUpdateResponseViewModel
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class OrganizationSettingInternalResult
    {
        public OrganizationSettingCreateUpdateResponseViewModel Data { get; set; } = new OrganizationSettingCreateUpdateResponseViewModel();
        public bool Exists { get; set; }
    }
}
