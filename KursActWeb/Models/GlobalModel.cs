using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Models
{
    public class GlobalModel
    {
        public static int Id { get; set; }
        public static DateTime sDate { get; set; }
        public static DateTime eDate { get; set; }
        public static int positionScroll { get; set; }
        
        public static int GetPositionScroll()
        {
            return positionScroll;
        }

        public static int GetIdToUser()
        {
            return Id;
        }
    }
}
