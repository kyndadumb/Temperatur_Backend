﻿namespace API.Models
{
    public class Users_Registration
    {
        public string username { get; set; }
        public string password { get; set; } 
        public string name { get; set; }
        public string phone { get; set; }
        public bool isAdmin { get; set; }
    }
}
