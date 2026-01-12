namespace OrganizationService.Api.ViewModels.Response.Dashboard
{
    public class DashboardReportResponse
    {
        public DashboardSummaryResponse Summary { get; set; } = new DashboardSummaryResponse();
        public List<DepartmentCountResponse> ApplicationsByDepartment { get; set; } = [];
        public List<ApplicationsTrendResponse> ApplicationTrends { get; set; } = [];
        public CurrentVacanciesDashboardResponse CurrentVacancies { get; set; } = new CurrentVacanciesDashboardResponse();
        //public List<TaskProgressResponse> TaskProgress { get; set; } = [];
        public List<TodayScheduleResponse> TodaySchedule { get; set; } = [];
        public List<ApplicantListResponse> Applicants { get; set; } = [];
        public List<JobTypeReportResponse> jobTypeReports { get; set; } = [];
        public List<WorkModeReportResponse> workModeReports { get; set; } = [];
    }


    public class DashboardSummaryResponse
    {
        public int total_applications { get; set; }
        public int shortlisted { get; set; }
        public int hired { get; set; }
        public int rejected { get; set; }
    }

    public class DepartmentCountResponse
    {
        public string department { get; set; } = string.Empty;
        public int total_count { get; set; }
    }

    public class ApplicationsTrendRaw
    {
        public DateTime report_date { get; set; }
        public int applied { get; set; }
        public int shortlisted { get; set; }
    }

    public class ApplicationsTrendResponse
    {
        public string ReportDate { get; set; }   // "23 Dec"
        public int Applied { get; set; }
        public int Shortlisted { get; set; }
    }

    public class CurrentVacancyRaw
    {
        public Guid interview_id { get; set; }
        public string job_title { get; set; } = string.Empty;
        public string job_type { get; set; } = string.Empty;
        public int applicant_count { get; set; }
    }

    public class CurrentVacanciesDashboardResponse
    {
        public int TotalVacancies { get; set; }
        public List<CurrentVacancyRaw> Vacancies { get; set; } = [];
    }

    public class TaskProgressResponse
    {
        public Guid TaskId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public int Progress { get; set; }
        public DateTime TaskDate { get; set; }
    }
    public class TodayScheduleResponse
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Applicants { get; set; } = string.Empty;   // "120 Applicants"
        public string Time { get; set; } = string.Empty;
    }

    public class TodayScheduleRaw
    {
        public Guid Interview_Id { get; set; }
        public string Job_Title { get; set; } = string.Empty;
        public int Applicant_Count { get; set; }
        public DateTime Schedule_Time { get; set; }
    }
    public class ApplicantListResponse
    {
        public string Name { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string InterviewDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
    public class ApplicantListRaw
    {
        public Guid Candidate_Id { get; set; }
        public string Candidate_Name { get; set; } = string.Empty;
        public string Employment_Type { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime Interview_Date { get; set; }
        public string Status { get; set; } = string.Empty;
    }


    public class DashboardOrgReportResponse
    {
        public DashboardOrgSummaryResponse dashboardOrgSummary { get; set; } = new DashboardOrgSummaryResponse();
        public List<DashboardOrgMonthlySummaryResponse> dashboardOrgMonthlySummary { get; set; } = new List<DashboardOrgMonthlySummaryResponse>();
    }
    public class DashboardOrgSummaryResponse
    {
        public int total_org { get; set; }
        public int active_org { get; set; }
        public int deleted_org { get; set; }
        public int inactive_org { get; set; }
        public int new_org { get; set; }
    }
    public class DashboardOrgMonthlySummaryResponse
    {
        public string month { get; set; } = string.Empty;
        public int total_org { get; set; }
        public int active_org { get; set; }
        public int deleted_org { get; set; }
        public int inactive_org { get; set; }
        public int new_org { get; set; }
    }

    public class JobTypeReportResponse
    {
        public string job_type { get; set; } = string.Empty;
        public int total_count { get; set; }
    }

    public class WorkModeReportResponse
    {
        public string work_mode { get; set; } = string.Empty;
        public int total_count { get; set; }
    }
}
