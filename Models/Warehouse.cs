using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string Suburb { get; set; }

        public int LocationId { get; set; }
        public virtual Location Location { get; set; }

    }
}