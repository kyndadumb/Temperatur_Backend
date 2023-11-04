namespace API.Models
{
    public class Users_List
    {
        public int UserID { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string isAdmin { get; set; }

        public DateTime? lastLogin { get; set; }
    }
}
