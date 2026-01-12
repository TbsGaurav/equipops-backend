namespace InterviewService.Api.Helpers
{
    public class StoreProcedure
    {
        //Interview stored procedures
        public const string GetInterviewList = "interviews.sp_interview_list_get";
        public const string GetInterviewById = "interviews.sp_interview_get_by_id";
        public const string InterviewCreate = "interviews.sp_interview_create";
        public const string InterviewUpdate = "interviews.sp_interview_update";
        public const string InterviewDelete = "interviews.sp_interview_delete";

        public const string GetInterviewInit = "interviews.sp_interview_init_get";
        public const string GetInterviewOverview = "interviews.sp_interview_overview_get";
        public const string AddUserToken = "master.sp_user_token_create";
        public const string VerifyInterviewToken = "master.sp_interview_token_verify";

        //Interview Type stored procedures
        public const string GetInterviewTypeList = "interviews.sp_interview_type_list_get";
        public const string GetInterviewTypeById = "interviews.sp_interview_type_get_by_id";
        public const string InterviewTypeCreateUpdate = "interviews.sp_interview_type_create_update";
        public const string InterviewTypeDelete = "interviews.sp_interview_type_delete";

        //Interview Transcript stored procedures
        public const string InterviewTranscriptCreate = "interviews.sp_interview_transcript_create";

        //Interviewer stored procedures
        public const string GetInterviewerList = "interviews.sp_interviewer_list_get";
        public const string GetInterviewerById = "interviews.sp_interviewer_get_by_id";
        public const string InterviewerCreate = "interviews.sp_interviewer_create";
        public const string InterviewerUpdate = "interviews.sp_interviewer_update";
        public const string InterviewerDelete = "interviews.sp_interviewer_delete";

        //InterviewerSetting stored procedures
        public const string GetInterviewerSettingList = "interviews.sp_interviewer_setting_list_get";
        public const string GetInterviewerSettingById = "interviews.sp_interviewer_setting_get_by_id";
        public const string InterviewerSettingCreateUpdate = "interviews.sp_interviewer_setting_create_update";
        public const string InterviewerSettingDelete = "interviews.sp_interviewer_setting_delete";

        //Interview Update stored procedures
        public const string InterviewUpdateTranscript = "interviews.sp_interview_update_transcript";

        //Job Analysis stored procedures
        public const string JobAnalysis = "interviews.get_job_analysis_by_interview_id";
        public const string CandidateJobAnalysis = "interviews.fn_get_candidate_interview_details";

        //Candidate Interview Invitation stored procedures
        public const string GetCandidateInterviewInvitation = "interviews.sp_candidate_interview_invitation_get_by_Id";
    }
}
