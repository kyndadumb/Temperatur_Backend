using System.Security.Cryptography;
using System.Text;

namespace API.Helpers
{
    public class PasswordHasher
    {
        // Variablen
        public const string salt = "qjrwngvjhefbvjhb32424";

        // HashPassowrd - Passwort hashen
        public static string HashPassword(string password, string salt = salt)
        {
            string saltedPassword = password + salt;
            byte[] passwordBytes = Encoding.UTF8.GetBytes(saltedPassword);
            byte[] hashBytes = SHA256.HashData(passwordBytes);
            return Convert.ToBase64String(hashBytes);
        }

        // VerifyPassword - Angegebenes Passwort hashen, gegen das gespeicherte gehashte PW prüfen
        public bool VerifyPassword(string providedPassword, string storedHashedPassword, string salt)
        {
            string hashedProvidedPassword = HashPassword(providedPassword, salt);

            return hashedProvidedPassword == storedHashedPassword;
        }

    }
}
