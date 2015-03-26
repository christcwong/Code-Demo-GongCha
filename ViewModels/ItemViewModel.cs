using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class ItemViewModel
    {
        Item ParentItem;
        public int Id { get{ return ParentItem.Id; } }
        public String Label { get { return ParentItem.Serial +" "+ ParentItem.Name+" "+ParentItem.ChinName; } }
        
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="pItem"></param>
        public ItemViewModel(Item pItem)
        {
            this.ParentItem = pItem;
        }
    }
}