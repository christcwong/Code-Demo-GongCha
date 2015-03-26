using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class ItemWeeklyUsageViewModel
    {
        public List<Location> Location { get; set; }
        public List<Item> Items { get; set; }
        public List<int> WeekNbs { get; set; }
        public Dictionary<Location, Dictionary<Item, Dictionary<int, int>>> ItemUsageOfLocationOfWeek { get; set; }
        public Dictionary<Location, Dictionary<int, Dictionary<Item, int>>> WeekUsageOfItemOfLocation { get; set; }
        public Dictionary<int, DateTime> WeekStartDate { get; set; }
        public Dictionary<int, DateTime> WeekEndDate { get; set; }
    }
}