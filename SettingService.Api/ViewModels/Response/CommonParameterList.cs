namespace SettingService.Api.ViewModels.Response
{
    public class CommonParameterList
    {
        public int TotalNumbers { get; set; }
    }

    public class CommonParameterAllList
    {
        public bool is_delete { get; set; }
        public bool is_active { get; set; }
        public Guid? created_by { get; set; }
        public DateTime? created_date { get; set; }
        public Guid? updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}
