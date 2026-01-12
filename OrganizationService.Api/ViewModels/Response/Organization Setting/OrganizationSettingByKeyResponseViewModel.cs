namespace OrganizationService.Api.ViewModels.Response.OrganizationSetting
{
    public class OrganizationSettingByKeyResponseViewModel
    {
        public Guid Id { get; set; }
        public Guid Organization_Id { get; set; }
        public string Third_Party { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool Is_Active { get; set; }
        public Guid? Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public Guid? Updated_By { get; set; }
        public DateTime Updated_Date { get; set; }
    }
}
