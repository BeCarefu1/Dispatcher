using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ActGIPelectroWeb.Models
{
    public class GetRuRoleModel
    {
        public static string GetRuRole(int roleId)
        {
            switch(roleId)
            {
                case 1:
                    return "Администратор";
                    break;
                case 2:
                    return "Кладовщик";
                    break;
                case 3:
                    return "Настройщик";
                    break;
                case 4:
                    return "Инженер";
                    break;
                case 5:
                    return "Монтажник";
                    break;
                case 6:
                    return "Куратор";
                    break;
                case 7:
                    return "Оператор";
                    break;
                case 8:
                    return "Начальник службы безопасности";
                    break;
                case 9:
                    return "ПНР";
                    break;
                case 10:
                    return "Поставщик потребительских данных";
                    break;
                case 11:
                    return "Бухгалтер";
                    break;
                case 12:
                    return "Смета";
                    break;
                case 13:
                    return "Гость";
                    break;
                case 14:
                    return "Диспетчер";
                    break;
                case 15:
                    return "Мастер";
                    break;
                case 16:
                    return "Подписант";
                    break;
                case 17:
                    return "Рабочий";
                    break;
                case 18:
                    return "Главный диспетчер";
                    break;
                case 19:
                    return "Главный Инженер";
                    break;
                case 20:
                    return "Главный ПТО";
                    break;
                default:
                    return "";
                    break;
            }
        }
    }
}
