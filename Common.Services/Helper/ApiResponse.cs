namespace Common.Services.Helper
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public int StatusCode { get; set; }

        public ApiResponse() { }

        public ApiResponse(T data, string? message = null, int statusCode = 200)
        {
            Success = true;
            Data = data;
            Message = message;
            StatusCode = statusCode;
        }

        public ApiResponse(string message, int statusCode = 400)
        {
            Success = false;
            Message = message;
            StatusCode = statusCode;
        }
    }

    public enum ApiStatusCode
    {
        Success = 200,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        Conflict = 409,
        ServerError = 500
    }


}
