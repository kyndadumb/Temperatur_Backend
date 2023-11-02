using MySql.Data.MySqlClient;

namespace API.Helpers
{
    public class Shared_Tools
    {
        // if - Condition == false --> Exception werfen
        public static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(message);
            }
        }

        // Nullable String vom SqlDataReader lesen
        internal static string SqlDataReader_ReadNullableString(MySqlDataReader reader, int i)
        {

            // Variablen
            string output = reader.IsDBNull(i) ? null : reader.GetString(i);

            // Erfolg liefern
            return output;

        } // SqlDataReader_ReadNullableString

        // Nullable String in eine Query-Value umwandeln
        internal static String NullableStringToQueryValue(string value, int max_length = Int32.MaxValue)
        {

            // Variable
            string output;

            // Text ist NICHT NULL
            if (value != null)
            {

                // Variablen
                string _value = value;

                // ggf. Text einkürzen
                if (_value.Length > max_length) _value = _value.Substring(0, max_length);

                // Hochkommas umwandeln
                _value = _value.Replace("'", "''");

                // Ausgabetext setzen
                output = $"'{_value}'";

            }
            else
            {

                // Ausgabetext setzen
                output = "NULL";

            } // if/else - Text ist NICHT NULL

            // Ergebnis liefern
            return output;

        } // NullableStringToQueryValue
    }
}
