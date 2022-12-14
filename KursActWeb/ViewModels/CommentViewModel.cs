using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.ViewModels
{
    public class CommentViewModel
    {
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public bool CanDelete { get; set; }
      
    }
}
