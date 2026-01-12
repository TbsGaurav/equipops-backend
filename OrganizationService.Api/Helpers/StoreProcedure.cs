namespace OrganizationService.Api.Helpers
{
    public class StoreProcedure
    {
        //Organization stored procedures
        public const string GetOrganizationList = "master.sp_organization_list_get";
        public const string GetOrganizationById = "master.sp_organization_get_by_id";
        public const string OrganizationCreateUpdate = "master.sp_organization_create_update";
        public const string OrganizationDelete = "master.sp_organization_delete";
        public const string GetAllOrganizationList = "master.sp_getall_organizations";
        public const string GetAllOrganizationUsersList = "master.sp_getall_users_by_orgId";
        public const string UpdateOrganizationStatus = "master.sp_update_organization_status";
        public const string GetOrganizationByStatus = "master.sp_get_all_orgs_by_status";

        //User Token stored procedures
        public const string UserTokenCreate = "master.sp_user_token_create";
        public const string UserTokenVerify = "master.sp_user_token_verify";
        public const string AddUserToken = "master.sp_user_token_create";

        //User stored procedures
        public const string UserCreateUpdate = "master.sp_user_create_update";
        public const string GetUserList = "master.sp_user_list_get";
        public const string GetUserById = "master.sp_user_get_by_id";
        public const string DeleteUser = "master.sp_user_delete";

        //Candidate stored procedures
        public const string GetCandidateList = "master.sp_candidate_detail_list_get";
        public const string GetCandidateById = "master.sp_candidate_detail_get_by_id";
        public const string CandidateCreateUpdate = "master.sp_candidate_detail_create_update";
        public const string CandidateDelete = "master.sp_candidate_detail_delete";
        public const string CandidateInterviewInvitationUpdate = "interviews.sp_candidate_interview_invitation_create_update";

        //Organization Setting stored procedures
        public const string GetOrganizationSettingList = "master.sp_organization_setting_list_get";
        public const string GetOrganizationSettingByKey = "master.sp_organization_setting_get_by_key";
        public const string OrganizationSettingCreateUpdate = "master.sp_organization_setting_create_update";
        public const string OrganizationSettingDelete = "master.sp_organization_setting_delete";

        //job template
        public const string GetJobTemplateList = "master.sp_jobtemplate_list_get";
        public const string GetJobTemplateById = "master.sp_jobtemplate_get_by_id";
        public const string JobTemplateCreateUpdate = "master.sp_jobtemplate_create_update";
        public const string JobTemplateDelete = "master.sp_jobtemplate_delete";

        //job post
        public const string GetJobPostList = "master.sp_jobpost_list_get";
        public const string GetJobPostById = "master.sp_jobpost_get_by_id";
        public const string JobPostPublish = "master.sp_jobpost_publish";
        public const string JobPostDelete = "master.sp_jobpost_delete";

        //Location stored procedures
        public const string GetCountryList = "master.sp_country_list_get";
        public const string GetStatesByCountryList = "master.sp_state_list_get_by_country";
        public const string GetCitiesByStateList = "master.sp_city_list_get_by_state";
    }
}
