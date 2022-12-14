using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Models
{
    public class ShortNameModel
    {
        public static string ShortName(string name)
        {
            try
            {
                if (name != null)
                {
                    string[] arr = name.Split(" ");
                    string shortName = "";
                    if (arr.Length == 3)
                    {
                        shortName = arr[0] + " ";

                        for (int i = 1; i < arr.Length; i++)
                        {
                            shortName += (arr[i])[0] + ". ";
                        }
                    }
                    else
                    {
                        shortName = name;
                    }
                    return shortName;
                }
                else
                {
                    return "";
                }
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
