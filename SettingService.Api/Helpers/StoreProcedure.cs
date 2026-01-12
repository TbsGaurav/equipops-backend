namespace SettingService.Api.Helpers
{
    public class StoreProcedure
    {
        public const string UserRole = "sp_user_role_create_update";

        //language stored procedures
        public const string LanguageCreateUpdate = "master.sp_language_create_update";
        public const string GetLanguageList = "master.sp_language_list_get";
        public const string GetLanguageById = "master.sp_language_get_by_id";
        public const string DeleteLanguage = "master.sp_language_delete";

        //subscription stored procedures
        public const string SubscriptionCreateUpdate = "master.sp_subscription_create_update";

        //subscription type stored procedures
        public const string SubscriptionTypeCreateUpdate = "master.sp_subscription_type_create_update";
        public const string GetSubscriptionList = "master.sp_subscription_list_get";
        public const string GetSubscriptionById = "master.sp_subscription_get_by_id";
        public const string GetSubscriptionByOrganizationId = "master.sp_subscription_login_list";
        public const string DeleteSubscription = "master.sp_subscription_delete";

        //menu type stored procedures
        public const string MenuTypeCreateUpdate = "master.sp_menu_type_create_update";
        public const string GetMenuTypeList = "master.sp_menu_type_list_get";
        public const string GetMenuTypeById = "master.sp_menu_type_get_by_id";
        public const string DeleteMenuType = "master.sp_menu_type_delete";

        //menu language stored procedures
        public const string MenuLanguageCreateUpdate = "master.sp_menu_language_create_update";
        public const string GetMenuLanguageList = "master.sp_menu_language_list_get";
        public const string GetMenuLanguageById = "master.sp_menu_language_get_by_id";
        public const string DeleteMenuLanguage = "master.sp_menu_language_delete";
        public const string GetMenuLanguageByLanguage = "master.sp_menu_language_get_by_language";
        public const string MenuLanguageByLanguageUpdate = "master.sp_menu_language_bulk_update_by_language";

        //master dropdown stored procedure
        public const string GetMasterDropdownList = "master.sp_master_dropdown_list_get";
    }
}
