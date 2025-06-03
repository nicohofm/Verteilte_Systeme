using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.ChatModel
{
    public class ChatMessage
    {
        public string sender { get; set; }
        public string text { get; set; }
        public string clientId { get; set; }
        public string topic { get; set; }
    }
}
