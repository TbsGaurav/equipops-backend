namespace SettingService.Api.ViewModels.Request.MenuLanguage
{
    public class MenuLanguageByLanguageUpdateRequestViewModel
    {
        public Guid LanguageId { get; set; }
        public List<MenuLanguageUpdateItemViewModel> Data { get; set; } = new();

        public class MenuLanguageUpdateItemViewModel
        {
            public string Key_Name { get; set; } = default!;
            public string Translate_Text { get; set; } = default!;
        }
    }
}
