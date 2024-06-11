using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;

namespace mezunprojesi.Controllers
{
    public class ExamSchedule
    {
        [Required]
        public int dersid { get; set; }

        [Required]
        public DateTime date { get; set; }

        [Required]
        public string classs { get; set; }

        [Required]
        public string lessonname { get; set; }
    }
}