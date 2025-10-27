using System.Security.Cryptography;

namespace StudentReportInitial.Data
{
    public static class PasswordHasher
    {
        public static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using var hmac = new HMACSHA512();
            var saltBytes = hmac.Key;
            var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            passwordSalt = Convert.ToBase64String(saltBytes);
            passwordHash = Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPasswordHash(string password, string passwordHash, string passwordSalt)
        {
            using var hmac = new HMACSHA512(Convert.FromBase64String(passwordSalt));
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            var decodedHash = Convert.FromBase64String(passwordHash);

            return computedHash.SequenceEqual(decodedHash);
        }
    }
}
