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

                // Daten des Benutzers in der Datenbank ablegen
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO users (UserName, Hashed_Password, Phone_Number, Is_Admin, Name) VALUES (@Nutzername, @Password_Hash, @Phone_Nr, @isAdmin, @Name)";
                command.Parameters.AddWithValue("@Nutzername", newUser.username);
                command.Parameters.AddWithValue("@Password_Hash", PasswordHasher.HashPassword(newUser.password));
                command.Parameters.AddWithValue("@Phone_Nr", newUser.phone);
                command.Parameters.AddWithValue("@isAdmin", newUser.isAdmin);
                command.Parameters.AddWithValue("@Name", newUser.name);
                command.ExecuteNonQuery();

                // Verbindung schließen
                connection.Close();

                // Status 200 zurückgeben
                return Ok($"Der Benutzer {newUser.username} wurde angelegt!");
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Registrierung ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
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

                // Passwort des übergebenen Benutzers auslesen
                MySqlCommand command = new("SELECT Hashed_Password FROM users WHERE UserName = @Nutzername", connection);
                command.Parameters.AddWithValue("@Nutzername", login.username);

                using MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read()) { stored_password = reader.GetString(0); }

                // konnte ein Benutzer gefunden werden?
                Shared_Tools.Assert(stored_password != null, $"Es konnte kein Benutzer {login.username} gefunden werden!");

                // if - Passwort kann nicht verifiziert werden --> BadRequest zurückgeben
                if (!PasswordHasher.VerifyPassword(login.password, stored_password, PasswordHasher.salt)) { return BadRequest("Ungültige Anmeldeinformationen."); }

                // Verbindung schließen
                connection.Close();

                // Benutzer konnte nicht eingeloggt werden
                return Ok($"Der Benutzer {login.username} wurde erfolgreich eingeloggt!");
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei dem Login ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }

        [HttpGet("show_users")]
        public IActionResult ShowUserList()
        {
            // Variablen
            string connectionString = _configuration.GetConnectionString("mysqlConnection");
            List<Users_List> registered_users = new();

            try
            {
                // konnte Connection-String erfolgreich gelesen werden?
                Shared_Tools.Assert(!string.IsNullOrEmpty(connectionString), "Fehler beim Parsen der DB-Verbindungsinformationen");

                // Datenbankverbindung eröffnen
                using MySqlConnection connection = new(connectionString);
                connection.Open();

                // alle Benutzerdaten auslesen
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT UserID, UserName, Phone_Number, Name, Is_Admin FROM users";
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    // Daten des aktuellen Nutzers lesen
                    Users_List temp_user = new();
                    {
                        temp_user.UserID = reader.GetInt32(0);
                        temp_user.username = Shared_Tools.SqlDataReader_ReadNullableString(reader, 1);
                        temp_user.phone = Shared_Tools.SqlDataReader_ReadNullableString(reader, 2);
                        temp_user.name = Shared_Tools.SqlDataReader_ReadNullableString(reader, 3);
                        temp_user.isAdmin = reader.GetString(4);
                    }
                    
                    // Nutzer in der Liste ablegen
                    registered_users.Add(temp_user);
                }

                // Verbindung schließen
                connection.Close();

                // Status 200 & Benutzerliste zurückgeben
                return Ok(registered_users);
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Registrierung ist ein Fehler aufgetreten:\n--> {ex.Message}"); };
        }
    }
}