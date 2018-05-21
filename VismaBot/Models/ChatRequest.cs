using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VismaBot.Models
{
    public class ChatRequest
    {
        public string type { get; set; }
        public string token { get; set; }
        public DateTime event_time { get; set; }
        public Space space { get; set; }
        public Message message { get; set; }
    }

    public class Space
    {
        public string name { get; set; }
        public string displayName { get; set; }
        public string type { get; set; }
    }

    public class Message
    {
        public string name { get; set; }
        public Sender sender { get; set; }
        public DateTime createTime { get; set; }
        public string text { get; set; }
        public Thread thread { get; set; }
    }

    public class Sender
    {
        public string name { get; set; }
        public string displayName { get; set; }
        public string avatarUrl { get; set; }
        public string email { get; set; }
    }

    public class Thread
    {
        public string name { get; set; }
    }

}
