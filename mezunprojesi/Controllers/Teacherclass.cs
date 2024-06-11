using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace mezunprojesi.Controllers
{
    public class Teacherclass
    {
        [Required]
        public int TeacherNumber { get; set; }

        [Required]
        public string TeacherName { get; set; }

        [Required]
        public string TeacherSurname { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string AccessLevel { get; set; }

        [Required]
        public int Department { get; set; }
    }
}