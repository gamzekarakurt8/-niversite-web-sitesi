using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;

namespace mezunprojesi.Controllers
{
    public class Announcement
    {
        [Required]
        public string lessonname { get; set; }


        [Required]
        public string announce { get; set; }
    }
}