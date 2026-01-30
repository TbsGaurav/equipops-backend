namespace Common.Services.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
            => BCrypt.Net.BCrypt.HashPassword(password);

        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("Hash cannot be null or empty.", nameof(hash));

            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public static string GenerateRandomPassword(int length = 12)
        {
            if (length < 1) throw new ArgumentException("Password length must be at least 1.");

            const string upper = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*?_-";

            var random = new Random();
            char[] password = new char[length];

            int specialPos = random.Next(length);
            password[specialPos] = special[random.Next(special.Length)];

            string allChars = upper + lower + digits + special;
            for (int i = 0; i < length; i++)
            {
                if (i == specialPos) continue;
                password[i] = allChars[random.Next(allChars.Length)];
            }

            return new string(password.OrderBy(x => random.Next()).ToArray());
        }
    }
}
