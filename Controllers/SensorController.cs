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

                // Übergebene Sensorendaten in der Datenbank ablegen
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO sensors (Serverschrank, Adresse, Hersteller, Max_Temperature) VALUES (@Serverschrank, @Adresse, @Hersteller, @Max_Temperature)";
                command.Parameters.AddWithValue("@Serverschrank", newSensor.serverschrank);
                command.Parameters.AddWithValue("@Adresse", newSensor.adresse);
                command.Parameters.AddWithValue("@Hersteller", newSensor.hersteller);
                command.Parameters.AddWithValue("@Max_Temperature", Shared_Tools.NullableDoubleToQueryValue(newSensor.max_temperature));
                command.ExecuteNonQuery();
                connection.Close();

                // Status 200 zurückgeben
                return Ok($"Der Sensor im Serverschrank {newSensor.serverschrank} wurde angelegt!");
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Bei der Erstellung des Sensors ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }

        [HttpGet("show_all_sensors")]
        public IActionResult ShowAllSensorInformation()
        {
            // Variablen
            string connectionString = _configuration.GetConnectionString("mysqlConnection");
            List<Sensors_List> all_sensors = new();

            try 
            {
                // konnte Connection-String erfolgreich gelesen werden?
                Shared_Tools.Assert(!string.IsNullOrEmpty(connectionString), "Fehler beim Parsen der DB-Verbindungsinformationen");

                // Datenbankverbindung eröffnen
                using MySqlConnection connection = new(connectionString);
                connection.Open();

                // Prüfen, ob ein Sensor mit übergebener ID in der Datenbank vorhanden ist
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT SensorID, Serverschrank, Adresse, Hersteller, Max_Temperature FROM sensors";
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read()) 
                {
                    // Daten des aktuellen Sensors lesen
                    Sensors_List temp_sensor = new();
                    {
                        temp_sensor.SensorID = reader.GetInt32(0);
                        temp_sensor.serverschrank = Shared_Tools.SqlDataReader_ReadNullableString(reader, 1);
                        temp_sensor.adresse = Shared_Tools.SqlDataReader_ReadNullableString(reader, 2);
                        temp_sensor.hersteller = Shared_Tools.SqlDataReader_ReadNullableString(reader, 3);
                        temp_sensor.max_temperature = Shared_Tools.SqlDataReader_ReadNullableDouble(reader, 4);
                    }

                    // Sensor in der Liste ablegen
                    all_sensors.Add(temp_sensor);
                }

                // Datenbankverbindung schließen
                connection.Close();

                // Status 2ßß und die Liste zurückgeben
                return Ok(all_sensors);
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Beim Abrufen der Sensorinformationen ist ein Fehler aufgetreten:\n--> {ex.Message}"); }
        }

        [HttpGet("{sensor_id}/show_sensor")]
        public IActionResult ShowSensorInformation(String sensor_id)
        {
            // Variablen
            string connectionString = _configuration.GetConnectionString("mysqlConnection");
            Sensors_List sensor = new();

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
                command.CommandText = "SELECT SensorID, Serverschrank, Adresse, Hersteller, Max_Temperature FROM sensors WHERE SensorID = @SensorID";
                command.Parameters.AddWithValue("@SensorID", parsed_sensorID);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    sensor.SensorID = reader.GetInt32(0);
                    sensor.serverschrank = Shared_Tools.SqlDataReader_ReadNullableString(reader, 1);
                    sensor.adresse = Shared_Tools.SqlDataReader_ReadNullableString(reader, 2);
                    sensor.hersteller = Shared_Tools.SqlDataReader_ReadNullableString(reader, 3);
                    sensor.max_temperature = Shared_Tools.SqlDataReader_ReadNullableDouble(reader, 4);
                }

                // DB-Verbindung schließen
                connection.Close();

                // if - Nutzer ist leer --> keine vorhandene ID zurückgegeben
                if (sensor.SensorID == null) { return NotFound($"Der Sensor mit der ID {parsed_sensorID} wurde nicht gefunden!"); }

                // Status 200 & Sensor zurückgeben
                return Ok(sensor);
            }
            catch (Exception ex) { return StatusCode(StatusCodes.Status500InternalServerError, $"Beim Auslesen der Sensor-Information ist ein Fehler aufgetreten:\n--> {ex.Message}"); };
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
                if (output == null) { return NotFound($"Ein Sensor mit der ID {parsed_sensorID} ist nicht vorhanden!"); }

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
            List<double>? temperatures = new();

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
                    double temp_temperature = reader.GetDouble(0);
                    temperatures.Add(temp_temperature);
                }

                // Datenbankverbindung schließen
                connection.Close();

                // if - Nutzer ist leer --> keine vorhandene ID zurückgegeben
                if (temperatures == null) { return NotFound($"Der Sensor mit der ID {parsed_sensorID} wurde nicht gefunden!"); }

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
                    temperature = Shared_Tools.SqlDataReader_ReadNullableDouble(reader, 0);
                }

                // Datenbankverbindung schließen
                connection.Close();

                // if - Nutzer ist leer --> keine vorhandene ID zurückgegeben
                if (temperature == null) { return NotFound($"Der Sensor mit der ID {parsed_sensorID} wurde nicht gefunden!"); }

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
                    temperature = Shared_Tools.SqlDataReader_ReadNullableDouble(reader, 0);
                }

                // Verbindung zur Datenbank schließen
                connection.Close();

                // if - Nutzer ist leer --> keine vorhandene ID zurückgegeben
                if (temperature == null) { return NotFound($"Der Sensor mit der ID {parsed_sensorID} wurde nicht gefunden!"); }

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
                    temperature = Shared_Tools.SqlDataReader_ReadNullableDouble(reader, 0);
                }

                // Datenbankverbindung schließen
                connection.Close();

                // if - Nutzer ist leer --> keine vorhandene ID zurückgegeben
                if (temperature == null) { return NotFound($"Der Sensor mit der ID {parsed_sensorID} wurde nicht gefunden!"); }

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
