namespace Common.Services.Services.Implementation
{
    public class GenerateCandidateImageUrl
    {
        public string GenerateImageUrl(string folderName, string fileName, string gatewayBaseurl)
        {
            if (string.IsNullOrWhiteSpace(folderName) || string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            // Public path owned by Organization Service
            var publicPath = $"org/uploads/candidates/{folderName}/{fileName}"
                .Replace("\\", "/");

            return $"{gatewayBaseurl.TrimEnd('/')}/{publicPath}";
        }
    }
}
