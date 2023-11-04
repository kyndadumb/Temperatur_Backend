using API.Helpers;
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

                // Übergebene Sensorendaten in der Datenbank ablegen
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO sensors (Serverschrank, Adresse, Hersteller, Max_Temperature) VALUES (@Serverschrank, @Adresse, @Hersteller, @Max_Temperature)";
                command.Parameters.AddWithValue("@Serverschrank", Shared_Tools.NullableStringToQueryValue(newSensor.serverschrank));
                command.Parameters.AddWithValue("@Adresse", Shared_Tools.NullableStringToQueryValue(newSensor.adresse));
                command.Parameters.AddWithValue("@Hersteller", Shared_Tools.NullableStringToQueryValue(newSensor.hersteller));
                command.Parameters.AddWithValue("@Max_Temperature", newSensor.max_temperature);
                command.ExecuteNonQuery();
                connection.Close();

                // Status 200 zurückgeben
                return Ok($"Der Sensor im Serverschrank {newSensor.serverschrank} wurde angelegt!");
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Erstellung des Sensors ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }

        [HttpPost("{sensor_id}/delete_sensor")]
        public IActionResult SensorDeletion(String sensor_id)
        {
            // Variablen
            string connectionString = _configuration.GetConnectionString("mysqlConnection");
            string output = null;

            // Sensor-ID konvertieren, bei unlogischen Daten Bad Request zurückgeben
            if (!int.TryParse(sensor_id, out int parsed_sensorID)) { return BadRequest($"Die übergebene Sensor ID {sensor_id} ist ungültig!"); }

            try
            {
                // konnte Connection-String erfolgreich gelesen werden?
                Shared_Tools.Assert(!string.IsNullOrEmpty(connectionString), "Fehler beim Parsen der DB-Verbindungsinformationen");

                // Datenbankverbindung eröffnen
                using MySqlConnection connection = new(connectionString);
                connection.Open();

                // Prüfen, ob ein Sensor mit übergebener ID in der Datenbank vorhanden ist
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT SensorID FROM sensors WHERE SensorID = @SensorID";
                command.Parameters.AddWithValue("@SensorID", parsed_sensorID);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        output = Shared_Tools.SqlDataReader_ReadNullableString(reader, 0);
                    }
                }

                // if - Ergebnis ist leer --> BadRequest zurückgeben
                if (output == null) { return BadRequest($"Ein Sensor mit der ID {parsed_sensorID} ist nicht vorhanden!"); }

                // Command für das Löschen des Sensors konfigurieren und ausführen
                MySqlCommand command_deletion = connection.CreateCommand();
                command_deletion.CommandText = "DELETE FROM sensors WHERE SensorID = @SensorID";
                command_deletion.Parameters.AddWithValue("@SensorID", parsed_sensorID);
                command_deletion.ExecuteNonQuery();

                // Verbindung zur Datenbank schließen
                connection.Close();

                // Statuscode 200 zurückgeben
                return Ok($"Der Sensor (ID: {parsed_sensorID}) wurde entfernt!");
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Erstellung des Sensors ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }

        [HttpPost("modify_sensor")]
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

                // UPDATE-Statement für alle Sensorendaten konfigurieren und ausführen
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = $"UPDATE sensors SET Serverschrank = @Serverschrank, Adresse = @Adresse, Hersteller = @Hersteller, Max_Temperature = @Max_Temperature WHERE SensorID = {sensor.SensorID}";
                command.Parameters.AddWithValue("@Serverschrank", sensor.serverschrank);
                command.Parameters.AddWithValue("@Adresse", sensor.adresse);
                command.Parameters.AddWithValue("@Hersteller", sensor.hersteller);
                command.Parameters.AddWithValue("@Max_Temperature", sensor.max_temperature);
                command.ExecuteNonQuery();

                // Datenbankverbindung schließen
                connection.Close();

                // Statuscode 200 zurückgeben
                return Ok($"Sensordaten für den Sensor {sensor.SensorID} geändert!");
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Änderung des Sensors ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }

        [HttpGet("{sensor_id}/ten_last_temperatures_of_sensor")]
        public IActionResult lastTenTemperatures(string sensor_id)
        {
            // Variablen
            string connectionString = _configuration.GetConnectionString("mysqlConnection");
            List<double> temperatures = new();

            // Sensor-ID konvertieren, bei unlogischen Daten Bad Request zurückgeben
            if (!int.TryParse(sensor_id, out int parsed_sensorID)) { return BadRequest($"Die übergebene Sensor ID {sensor_id} ist ungültig!"); }

            try
            {
                // konnte Connection-String erfolgreich gelesen werden?
                Shared_Tools.Assert(!string.IsNullOrEmpty(connectionString), "Fehler beim Parsen der DB-Verbindungsinformationen");

                // Datenbankverbindung eröffnen
                using MySqlConnection connection = new(connectionString);
                connection.Open();

                // Command für die letzten 10 Temperaturdaten des übergebenen Sensors konfigurieren und ausführen
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT temperature FROM temperatures where SensorID = @SensorID ORDER BY timestamp ASC LIMIT 10";
                command.Parameters.AddWithValue("@SensorID", parsed_sensorID);

                MySqlDataReader reader = command.ExecuteReader();

                // while - Reader ließt Daten --> Temperaturen lesen und zur Liste hinzufügen
                while (reader.Read())
                {
                    double temp_temp = reader.GetDouble(0);
                    temperatures.Add(temp_temp);
                }

                // Datenbankverbindung schließen
                connection.Close();

                // Statuscode 200 und Temperaturliste zurückgeben
                return Ok(temperatures);
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Änderung des Sensors ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }

        [HttpGet("{sensor_id}/last_temperature_of_sensor")]
        public IActionResult lastTemperature(string sensor_id)
        {
            // Variablen
            string connectionString = _configuration.GetConnectionString("mysqlConnection");
            double? temperature = null;

            // Sensor-ID konvertieren, bei unlogischen Daten Bad Request zurückgeben
            if (!int.TryParse(sensor_id, out int parsed_sensorID)) { return BadRequest($"Die übergebene Sensor ID {sensor_id} ist ungültig!"); }

            try
            {
                // konnte Connection-String erfolgreich gelesen werden?
                Shared_Tools.Assert(!string.IsNullOrEmpty(connectionString), "Fehler beim Parsen der DB-Verbindungsinformationen");

                // Datenbankverbindung eröffnen
                using MySqlConnection connection = new(connectionString);
                connection.Open();

                // Command für die letzte Temperaturmessung des Sensors konfigurieren und ausführen
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT temperature FROM temperatures where SensorID = @SensorID ORDER BY timestamp DESC LIMIT 1";
                command.Parameters.AddWithValue("@SensorID", parsed_sensorID);

                MySqlDataReader reader = command.ExecuteReader();

                // while - Reader ließt Daten --> Temperatur lesen
                while (reader.Read())
                {
                    temperature = reader.GetDouble(0);
                }

                // Datenbankverbindung schließen
                connection.Close();

                // Statuscode 200 und Temperatur zurückgeben
                return Ok(temperature);
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Änderung des Sensors ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }

        [HttpGet("{sensor_id}/highest_temperature_of_sensor")]
        public IActionResult highestTemperature(string sensor_id)
        {
            // Variablen
            string connectionString = _configuration.GetConnectionString("mysqlConnection");
            double? temperature = null;

            // Sensor-ID konvertieren, bei unlogischen Daten Bad Request zurückgeben
            if (!int.TryParse(sensor_id, out int parsed_sensorID)) { return BadRequest($"Die übergebene Sensor ID {sensor_id} ist ungültig!"); }

            try
            {
                // konnte Connection-String erfolgreich gelesen werden?
                Shared_Tools.Assert(!string.IsNullOrEmpty(connectionString), "Fehler beim Parsen der DB-Verbindungsinformationen");

                // Datenbankverbindung eröffnen
                using MySqlConnection connection = new(connectionString);
                connection.Open();

                // Command für die höchste Temperatur aller Zeiten konfigurieren und ausführen
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT temperature FROM temperatures where SensorID = @SensorID ORDER BY temperature DESC LIMIT 1";
                command.Parameters.AddWithValue("@SensorID", parsed_sensorID);

                MySqlDataReader reader = command.ExecuteReader();

                // while - Reader ließt Daten --> Temperatur speichern
                while (reader.Read())
                {
                    temperature = reader.GetDouble(0);
                }

                // Verbindung zur Datenbank schließen
                connection.Close();

                // Statuscode 200 und Temperatur zurückgeben
                return Ok(temperature);
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Änderung des Sensors ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }

        [HttpGet("{sensor_id}/average_temperature_of_sensor")]
        public IActionResult averageTemperature(string sensor_id)
        {
            // Variablen
            string connectionString = _configuration.GetConnectionString("mysqlConnection");
            double? temperature = null;

            // Sensor-ID konvertieren, bei unlogischen Daten Bad Request zurückgeben
            if (!int.TryParse(sensor_id, out int parsed_sensorID)) { return BadRequest($"Die übergebene Sensor ID {sensor_id} ist ungültig!"); }

            try
            {
                // konnte Connection-String erfolgreich gelesen werden?
                Shared_Tools.Assert(!string.IsNullOrEmpty(connectionString), "Fehler beim Parsen der DB-Verbindungsinformationen");

                // Datenbankverbindung eröffnen
                using MySqlConnection connection = new(connectionString);
                connection.Open();

                // Command für die Durchschnittstemperatur des Sensors konfigurieren und ausführen
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT AVG(temperature) FROM temperatures where SensorID = @SensorID";
                command.Parameters.AddWithValue("@SensorID", parsed_sensorID);

                MySqlDataReader reader = command.ExecuteReader();

                // while - Reader ließt Daten --> Temperatur speichern
                while (reader.Read())
                {
                    temperature = reader.GetDouble(0);
                }

                // Datenbankverbindung schließen
                connection.Close();

                // Statuscode 200 und Temperatur zurückgeben
                return Ok(temperature);
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Änderung des Sensors ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }

        [HttpPost("modify_max_temperaure_of_sensor")]
        public IActionResult maxTemperatureChange([FromBody] Sensors_MaxTempChange data)
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

                // Command für das Update der Maximaltemperatur eines Sensors konfigurieren und ausführen
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "UPDATE sensors SET Max_Temperature = @Max_Temperature WHERE SensorID = @SensorID";
                command.Parameters.AddWithValue("@Max_Temperature", data.MaxTemperature);
                command.Parameters.AddWithValue("@SensorID", data.SensorID);
                command.ExecuteNonQuery();

                // Command für den Log-Eintrag konfigurieren
                MySqlCommand command_log = connection.CreateCommand();
                command_log.CommandText = "INSERT INTO logs (max_temperature, UserID, SensorID) VALUES (@Max_Temperature, @UserID, @SensorID)";
                command_log.Parameters.AddWithValue("@Max_Temperature", data.MaxTemperature);
                command_log.Parameters.AddWithValue("@UserID", data.UserID);
                command_log.Parameters.AddWithValue("@SensorID", data.SensorID);
                command_log.ExecuteNonQuery();

                // Datenbankverbindung schlie0en
                connection.Close();

                // Statuscode 200 zurückgeben
                return Ok("Die Maximaltemperatur wurde geändert und ein Log erstellt!");
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Änderung des Sensors ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }
    }
}
