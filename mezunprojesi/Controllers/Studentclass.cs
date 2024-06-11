using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;

namespace mezunprojesi.Controllers
{
    public class Studentclass
    {
        [Required]
        public int StudentNumber { get; set; }

        [Required]
        public string StudentName { get; set; }

        [Required]
        public string StudentSurname { get; set; }

        [Required]
        public string AccessLevel { get; set; }


        [Required]
        public int Department { get; set; }

        [Required]
        public string Password { get; set; }


    }
}