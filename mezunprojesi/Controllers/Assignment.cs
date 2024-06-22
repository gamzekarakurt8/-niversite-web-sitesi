using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;

namespace mezunprojesi.Controllers
{
    public class Assignment
    {
        [Required]
        public int dersid { get; set; }

        [Required]
        public string lessonname { get; set; }

        [Required]
        public string explanation { get; set; }

        [Required]
        public DateTime startanddate {  get; set; }

    }
}