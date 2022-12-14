using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.ViewModels
{
    public class MessageViewModal
    {
        public int Id { get; set; }
        public string NameAuthor { get; set; }
        public string Text { get; set; }
        public DateTime SendTime { get; set; }
        public DateTime ReadTime { get; set; }
        public int FromUser { get; set; }
        public int ToUser { get; set; }
        public string Role { get; set; }
    }
}
