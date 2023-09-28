using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using Microsoft.Extensions.Configuration;
using MySql.Data;
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
        

        [HttpPost("user_registration")]
        public IActionResult UserCreation([FromBody] Users newUser)
        {
            // Variablen
            string connectionString = _configuration.GetConnectionString("mysqlConnection");
            
            try
            {
                if (string.IsNullOrEmpty(newUser.username) || string.IsNullOrEmpty(newUser.password))
                {
                    // Wenn Benutzername oder Passwort fehlen, senden Sie einen BadRequest-Statuscode (400) zurück.
                    return BadRequest("Benutzername und Passwort sind erforderlich.");
                }

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO nutzer (Nutzername, Password_Hash, Phone_Nr, is_Admin) VALUES (@Nutzername, @Password_Hash, @Phone_Nr, @isAdmin)";
                    command.Parameters.AddWithValue("@Nutzername", newUser.username);
                    command.Parameters.AddWithValue("@Password_Hash", PasswordHasher.HashPassword(newUser.password));
                    command.Parameters.AddWithValue("@Phone_Nr", newUser.phone);
                    command.Parameters.AddWithValue("@isAdmin", newUser.isAdmin);
                    
                    command.ExecuteNonQuery();

                    connection.Close();

                    return Ok($"Der Nutzer {newUser.username} wurde angelegt!");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Registrierung ist ein Fehler aufgetreten:\n--> {ex.Message}");
            }
        }
    }
}
