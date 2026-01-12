namespace InterviewService.Api.ViewModels.Response.Interview
{
	public class InterviewInitResponseViewModel
	{
		public List<IntervieweInitData> InterviewerList { get; set; } = new List<IntervieweInitData>();
		public List<InterviewTypeInitData> InterviewTypeList { get; set; } = new List<InterviewTypeInitData>();
		public List<WorkModeInitData> WorkModeList { get; set; } = new List<WorkModeInitData>();
		public List<DepartmentInitData> DepartmentList { get; set; } = new List<DepartmentInitData>();
		public string Generate_question_key { get; set; } = string.Empty;
	}
	public class IntervieweInitData
	{
		public Guid? Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Avatar_url { get; set; } = string.Empty;
	}
	public class InterviewTypeInitData
	{
		public Guid? Id { get; set; }
		public string Interview_type { get; set; } = string.Empty;

	}
	public class DepartmentInitData
	{
		public Guid? Id { get; set; }
		public string Name { get; set; } = string.Empty;
	}
	public class WorkModeInitData
	{
		public Guid? Id { get; set; }
		public string Work_Mode { get; set; } = string.Empty;
	}
}
