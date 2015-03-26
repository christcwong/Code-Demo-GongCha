using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GongChaWebApplication.Models
{
    public class MessageReplyViewModel
    {
        public Message ParentMessage { get; set; }
        public Message NewMessage { get; set; }
    }
}
