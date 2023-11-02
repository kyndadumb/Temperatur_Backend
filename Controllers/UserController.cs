using API.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using API.Helpers;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        

        [HttpPost("register_user")]
        public IActionResult UserCreation([FromBody] Users_Registration newUser)
        {
            // Variablen
            string connectionString = _configuration.GetConnectionString("mysqlConnection");
            
            try
            {
                // konnte Connection-String erfolgreich gelesen werden?
                Shared_Tools.Assert(!string.IsNullOrEmpty(connectionString), "Fehler beim Parsen der DB-Verbindungsinformationen");

                // if - Benutzername oder Passwort sind leer
                if (string.IsNullOrEmpty(newUser.username) || string.IsNullOrEmpty(newUser.password))
                {
                    return BadRequest("Benutzername und Passwort sind erforderlich.");
                }

                // Datenbankverbindung eröffnen
                using MySqlConnection connection = new(connectionString);
                connection.Open();

                // konnte die Datenbankverbindung aufgebaut werden?
                Shared_Tools.Assert(connection.State == System.Data.ConnectionState.Open, "Die Datenbank konnte nicht erreicht werden!");

                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO users (UserName, Hashed_Password, Phone_Number, Is_Admin, Name) VALUES (@Nutzername, @Password_Hash, @Phone_Nr, @isAdmin, @Name)";
                command.Parameters.AddWithValue("@Nutzername", newUser.username);
                command.Parameters.AddWithValue("@Password_Hash", PasswordHasher.HashPassword(newUser.password));
                command.Parameters.AddWithValue("@Phone_Nr", newUser.phone);
                command.Parameters.AddWithValue("@isAdmin", newUser.isAdmin);
                command.Parameters.AddWithValue("@Name", Shared_Tools.NullableStringToQueryValue(newUser.name));

                command.ExecuteNonQuery();

                connection.Close();

                return Ok($"Der Benutzer {newUser.username} wurde angelegt!");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Registrierung ist ein Fehler aufgetreten:\n--> {ex.Message}");
            }
        }

        [HttpPost("login_user")]
        public IActionResult Login([FromBody] Users_Login login)
        {
            // Variablen
            string connectionString = _configuration.GetConnectionString("mysqlConnection");
            string stored_password = null;

            try
            {
                // konnte Connection-String erfolgreich gelesen werden?
                Shared_Tools.Assert(!string.IsNullOrEmpty(connectionString), "Fehler beim Parsen der DB-Verbindungsinformationen");

                // if - Benutzername oder Passwort sind leer
                if (string.IsNullOrEmpty(login.username) || string.IsNullOrEmpty(login.password))
                {
                    return BadRequest("Benutzername und Passwort sind erforderlich.");
                }

                // Datenbankverbindung eröffnen
                using MySqlConnection connection = new(connectionString);
                connection.Open();

                // konnte die Datenbankverbindung aufgebaut werden?
                Shared_Tools.Assert(connection.State == System.Data.ConnectionState.Open, "Die Datenbank konnte nicht erreicht werden!");

                MySqlCommand command = new("SELECT Hashed_Password FROM users WHERE UserName = @Nutzername", connection);
                command.Parameters.AddWithValue("@Nutzername", login.username);

                using MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read()) { stored_password = reader.GetString(0); }

                // konnte ein Benutzer gefunden werden?
                Shared_Tools.Assert(stored_password != null, $"Es konnte kein Benutzer {login.username} gefunden werden!");

                // if - Passwort kann nicht verifiziert werden
                if (!PasswordHasher.VerifyPassword(login.password, stored_password, PasswordHasher.salt)) { return BadRequest("Ungültige Anmeldeinformationen."); }

                // Benutzer konnte nicht eingeloggt werden
                return Ok($"Der Benutzer {login.username} wurde erfolgreich eingeloggt!");
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei dem Login ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }
    }
}