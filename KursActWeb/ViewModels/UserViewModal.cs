using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace ActGIPelectroWeb.ViewModels
{
    public class UserViewModal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Id_Role { get; set; }
        public string Role { get; set; }
        public Image Image { get; set; }
    }
}
