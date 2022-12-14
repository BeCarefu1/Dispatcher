using ActGIPelectroWeb.EmailServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Models
{
    public static class Helper
    {
        public static string EnumToJson(this Type type)
        {
            if (!type.IsEnum)
                throw new InvalidOperationException("enum expected");

            var results =
                Enum.GetValues(type).Cast<object>()
                    .ToDictionary(enumValue => (int)enumValue, enumValue => ((Enum)enumValue).GetAttributeOfType<EnumMemberAttribute>().Value);

            return Newtonsoft.Json.JsonConvert.SerializeObject(results);

        }

        /// <summary>
        /// Gets the type of the attribute of.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumVal">The enum value.</param>
        /// <returns></returns>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }
    }
}
