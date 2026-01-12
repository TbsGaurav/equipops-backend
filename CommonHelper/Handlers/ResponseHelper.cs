
using CommonHelper.Enums;
using CommonHelper.Models;

namespace CommonHelper.ResponseHelpers.Handlers
{
    public static class ResponseHelper<T>
    {
        public static ResponseResult<T> Success(string message, T data)
        {
            return new ResponseResult<T>(ResponseStatus.Success, message, data);
        }
        public static ResponseResult<T> Success(string message)
        {
            return new ResponseResult<T>(ResponseStatus.Success, message);
        }

        public static ResponseResult<T> Error(
                    string message,
                    List<string>? errors = null,
                    Exception? exception = null,
                    bool isWarning = false,
                    StatusCodeEnum statusCode = StatusCodeEnum.BAD_REQUEST
                )
        {
            var exceptionResponse = exception != null
                ? new ExceptionResponse(exception, isWarning, message, statusCode)
                : null;

            return new ResponseResult<T>(ResponseStatus.Fail, message, default, exceptionResponse, errors);
        }
    }

    public class ResponseResult<T>
    {
        public ResponseStatus Status { get; }
        public string Message { get; }
        public T? Data { get; }
        public ExceptionResponse? ExceptionDetails { get; }
        public List<string>? Errors { get; }

        public ResponseResult(ResponseStatus status, string message, T? data, ExceptionResponse? exceptionDetails = null, List<string>? errors = null)
        {
            Status = status;
            Message = message;
            Data = data;
            ExceptionDetails = exceptionDetails;
            Errors = errors;
        }
        public ResponseResult(ResponseStatus status, string message, ExceptionResponse? exceptionDetails = null, List<string>? errors = null)
        {
            Status = status;
            Message = message;
            ExceptionDetails = exceptionDetails;
            Errors = errors;
        }

        public Dictionary<string, object> ToDictionary()
        {
            var result = new Dictionary<string, object>
            {
                { "status", Status.ToString() },
                { "message", Message }
            };

            if (!EqualityComparer<object>.Default.Equals(Data, default))
                result["data"] = Data;

            if (ExceptionDetails != null)
                result["exception"] = new
                {
                    StatusCode = ExceptionDetails.StatusCode,
                    Message = ExceptionDetails.ExceptionMessage,
                    StackTrace = ExceptionDetails.ExceptionStackTrace,
                    IsWarning = ExceptionDetails.IsWarning
                };

            if (Errors != null && Errors.Any())
                result["errors"] = Errors;

            return result;
        }
    }
}
