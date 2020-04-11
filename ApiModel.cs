using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DB_Generator
{
   public class ApiModel
   {
        public class Id
        {
            public long ID;
        }

       
        public class Name
        {
            public string first;
            public string last;
        }
        public class Login
        {
            public string username;
            public string password;
        }
        public class Location
        {
            public string city;

        }
        public class Coordinates
        {
            public string latitude { get; set; }
            public string longitude { get; set; }
        }
        public class Registrered
        {
            public DateTime date;
            public int age;

        }
        public class APIUser
        {
            public ProjectUser[] results;
        }
        public class ProjectUser
        {
            public Id id;
            public Name name;
            public Login login;
            public Location location;
            public string phone;
            public string cell;
            public Registrered registrered;
        }
       
   }
 
}
