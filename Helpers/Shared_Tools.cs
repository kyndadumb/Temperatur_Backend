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
    }
}
