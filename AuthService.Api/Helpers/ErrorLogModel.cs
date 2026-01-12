namespace AuthService.Api.Helpers
{
    public class ErrorLogModel
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public string Detail { get; set; }
        public Guid? User_Id { get; set; }
        public string Request_Data { get; set; }
        public Guid? Organization_Id { get; set; }
        public Guid? Created_By { get; set; }
    }
}
