using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class WageType
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Color { get; set; } //Hex code for color
        [DisplayName("Target Multiplier")]
        public double targetMultiplier { get; set; }
    }
}