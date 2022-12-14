using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIPManager;

namespace ActGIPelectroWeb.ViewModels
{
    public class RegPointRowViewModel
    {
        public int Id { get; set; }
        public string Address { get; set; }

        public string InstallPlace { get; set; }
        public string TypePU { get; set; }
        public string SerialNum { get; set; }
        public string PhoneNum { get; set; }

        public string DataFullestPercent { get => "%"; } //TODO

        //Flags
        public bool IsLinkOk { get; set; }
        public bool IsAscueChecked { get; set; }
        public bool IsAscueOk { get; set; }
        public bool IsReplace { get; set; }
        //Flags colors
        public string IsLinkOkColor { get => (IsLinkOk) ? "orange" : "gray"; }
        public string IsAscueCheckedColor { get => (IsAscueChecked) ? "dodgerblue" : "gray"; }
        public string IsAscueOkColor { get => (IsAscueOk) ? "green" : "gray"; }
        public string IsReplaceColor { get => (IsReplace) ? "red" : "gray"; }

        public RegPointRowViewModel() { }
        public RegPointRowViewModel(RegPoint regPoint)
        {
            if(regPoint != null)
            {
                Id = regPoint.Id;
                Address = FormatAddress(
                    regPoint.Consumer.O_Local,
                    regPoint.Consumer.O_Local_Secondary,
                    regPoint.Consumer.O_Street,
                    regPoint.Consumer.O_House,
                    regPoint.Consumer.O_Build,
                    regPoint.Consumer.O_Flat
                    );
                
                InstallPlace = regPoint.InstallAct.InstallPlaceType.Name + regPoint.InstallAct.InstallPlaceNumber;
                TypePU = (regPoint.Device == null)? "" : regPoint.Device.DeviceType.Type;
                SerialNum = (regPoint.Device == null) ? "" : regPoint.Device.SerialNumber;

                var link = (regPoint.Device != null && regPoint.Device.DeviceType.Description.ToLower().Contains("plc")) ?
                    (ILinkData)regPoint.Substation.SubstationLinks.LastOrDefault() :
                    regPoint.Device.Links.LastOrDefault();
                PhoneNum = (link == null)? "" : link.PhoneNumber;
                //Flags
                IsLinkOk = regPoint.RegPointFlags.IsLinkOk;
                IsAscueChecked = regPoint.RegPointFlags.IsAscueChecked;
                IsAscueOk = regPoint.RegPointFlags.IsAscueOk;
                IsReplace = regPoint.RegPointFlags.IsReplace;
            }
            else
                throw new Exception("Нулевая ссылка на объект RegPoint при создании модели RegPointViewModel");
        }
        public RegPointRowViewModel(string adrress, string installPlace, string typePU, string serialNum, string phoneNum)
        {
            Address = adrress;
            InstallPlace = installPlace;
            TypePU = typePU;
            SerialNum = serialNum;
            PhoneNum = phoneNum;
        }

        public static string GetNetAddress(string serial, string typePU)
        {
            string result = "";
            if (typePU.ToLower().Contains("тт"))
            {
                result = (serial.Length > 3) ? serial.Substring(serial.Length - 3, 3) : "";
            }
            if (typePU.ToLower().Contains("plc"))
            {
                result = (serial.Length > 5) ? serial.Substring(serial.Length - 5, 5) : "";
            }
            if (typePU.ToLower().Contains("gsm"))
            {
                result = (serial.Length > 9) ? serial.Substring(serial.Length - 9, 9) : "";
            }
            return result;
        }

        public static string FormatAddress(string Local, string Local_Secondary, string Street, string House, string Build, string Flat)
        {
            string result = "";
            if (Local != "" && Local != null) result += Local;
            if (Local_Secondary != "" && Local_Secondary != null) result += ", " + Local_Secondary;
            if (Street != "" && Street != null) result += ", ул. " + Street;
            if (House != "" && House != "-" && House != "0" && House != null) result += ", д. " + House;
            if (Build != "" && Build != "-" && Build != "0" && Build != null) result += "/" + Build;
            if (Flat != "" && Flat != "-" && Flat != "0" && Flat != null) result += ", кв. " + Flat;

            return result;
        }

        public static string FormatOAddress(Consumer consumer)
        {
            return FormatAddress(consumer.O_Local, consumer.O_Local_Secondary, consumer.O_Street, consumer.O_House, consumer.O_Build, consumer.O_Flat);
        }

        public static string FormatUAddress(Consumer consumer)
        {
            return FormatAddress(consumer.U_Local, consumer.U_Local_Secondary, consumer.U_Street, consumer.U_House, consumer.U_Build, consumer.U_Flat);
        }
    }
}
