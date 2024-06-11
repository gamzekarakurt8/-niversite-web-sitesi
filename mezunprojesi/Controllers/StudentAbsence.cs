using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace mezunprojesi.Controllers
{
    public class StudentAbsence
    {
        [Required]
        public int dersid { get; set; }

        [Required]
        public int userid { get; set; }

        [Required]
        public int devamsizlik { get; set; }
    }


}
