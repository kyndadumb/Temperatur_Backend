﻿using API.Helpers;
using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SensorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("create_sensor")]
        public IActionResult SensorCreation([FromBody] Sensors_Creation newSensor)
        {
            // Variablen
            string connectionString = _configuration.GetConnectionString("mysqlConnection");

            try
            {
                // konnte Connection-String erfolgreich gelesen werden?
                Shared_Tools.Assert(!string.IsNullOrEmpty(connectionString), "Fehler beim Parsen der DB-Verbindungsinformationen");

                // Datenbankverbindung eröffnen
                using MySqlConnection connection = new(connectionString);
                connection.Open();

                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO sensors (Serverschrank, Adresse, Hersteller, Max_Temperature) VALUES (@Serverschrank, @Adresse, @Hersteller, @Max_Temperature)";
                command.Parameters.AddWithValue("@Serverschrank", Shared_Tools.NullableStringToQueryValue(newSensor.serverschrank));
                command.Parameters.AddWithValue("@Adresse", Shared_Tools.NullableStringToQueryValue(newSensor.adresse));
                command.Parameters.AddWithValue("@Hersteller", Shared_Tools.NullableStringToQueryValue(newSensor.hersteller));
                command.Parameters.AddWithValue("@Max_Temperature", newSensor.max_temperature);
                command.ExecuteNonQuery();
                connection.Close();

                return Ok($"Der Sensor im Serverschrank {newSensor.serverschrank} wurde angelegt!");
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Erstellung des Sensors ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }

        [HttpPost("delete_sensor")]
        public IActionResult SensorDeletion([FromBody] Sensors_Deletion sensor)
        {
            // Variablen
            string connectionString = _configuration.GetConnectionString("mysqlConnection");
            string output = null;

            try
            {
                // konnte Connection-String erfolgreich gelesen werden?
                Shared_Tools.Assert(!string.IsNullOrEmpty(connectionString), "Fehler beim Parsen der DB-Verbindungsinformationen");

                // Datenbankverbindung eröffnen
                using MySqlConnection connection = new(connectionString);
                connection.Open();

                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT SensorID FROM sensors WHERE SensorID = @SensorID";
                command.Parameters.AddWithValue("@SensorID", sensor.ID);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        output = Shared_Tools.SqlDataReader_ReadNullableString(reader, 0);
                    }
                }

                if (output == null) { return BadRequest($"Ein Sensor mit der ID {sensor.ID} ist nicht vorhanden!"); }

                MySqlCommand command_deletion = connection.CreateCommand();
                command_deletion.CommandText = "DELETE FROM sensors WHERE SensorID = @SensorID";
                command_deletion.Parameters.AddWithValue("@SensorID", sensor.ID);
                command_deletion.ExecuteNonQuery();

                return Ok($"Der Sensor (ID: {sensor.ID}) wurde entfernt!");
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Erstellung des Sensors ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }

        [HttpPost("change_sensor")]
        public IActionResult SensorChange([FromBody] Sensors_Change sensor)
        {
            // Variablen
            string connectionString = _configuration.GetConnectionString("mysqlConnection");

            try
            {
                // konnte Connection-String erfolgreich gelesen werden?
                Shared_Tools.Assert(!string.IsNullOrEmpty(connectionString), "Fehler beim Parsen der DB-Verbindungsinformationen");

                // Datenbankverbindung eröffnen
                using MySqlConnection connection = new(connectionString);
                connection.Open();

                MySqlCommand command = connection.CreateCommand();
                command.CommandText = $"UPDATE sensors SET Serverschrank = @Serverschrank, Adresse = @Adresse, Hersteller = @Herrsteller, Max_Temperature = @Max_Temperature WHERE SensorID = {sensor.SensorID}";
                command.Parameters.AddWithValue("@Serverschrank", sensor.serverschrank);
                command.Parameters.AddWithValue("@Adresse", sensor.adresse);
                command.Parameters.AddWithValue("@Hersteller", sensor.hersteller);
                command.Parameters.AddWithValue("@Max_Temperature", sensor.max_temperature);

                command.ExecuteNonQuery();

                return Ok($"Sensordaten für den Sensor {sensor.SensorID} geändert!");
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Änderung des Sensors ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }
    }
}
