using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.ViewModels
{
    public class DutyFileViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IFormFile ExFile { get; set; }
    }
}
