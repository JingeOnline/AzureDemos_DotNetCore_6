using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBus_SharedLibrary
{
    public class MessageModel
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public DateTime SendTime { get; set; }
    }
}
