namespace SettingService.Api.Helpers.ResponseHelpers.Handlers
{
    public class CustomException : Exception
    {
        public int StatusCode { get; set; }
        public string ExceptionMessage { get; set; }
        public string? ExceptionStackTrace { get; set; }
        public bool IsWarning { get; set; }

        public CustomException(string exceptionMessage)
        {
            ExceptionMessage = exceptionMessage;
        }

        public CustomException(Exception exception, bool isWarning, string exceptionMessage)
        {
            StatusCode = exception.HResult;
            ExceptionMessage = exceptionMessage + " | Original Message: " + exception.Message;
            ExceptionStackTrace = exception.StackTrace;
            IsWarning = isWarning;
        }
    }
}
