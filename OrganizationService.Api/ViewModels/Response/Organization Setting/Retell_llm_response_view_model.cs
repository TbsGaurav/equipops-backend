namespace OrganizationService.Api.ViewModels.Response.Organization_Setting
{
    public class Retell_llm_response_view_model
    {
        public string llm_id { get; set; }
        public int version { get; set; }
        public string model { get; set; }
        public string start_speaker { get; set; }
        public KbConfig kb_config { get; set; }
        public long last_modification_timestamp { get; set; }
        public bool is_published { get; set; }
    }
    public class KbConfig
    {
        public int top_k { get; set; }
        public double filter_score { get; set; }
    }
}
