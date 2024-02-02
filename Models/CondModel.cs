using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotnetAPI.Models
{
    public partial class CondCarStatus
    {
        private static Dictionary<string, string> carStatus
        {
            get
            {
                var carstatus = new Dictionary<string, string>
                {
                    { "","default" },
                    { "NW" ,"new" },
                    { "HT","highlight" },
                    { "DC" ,"discount" },
                    { "D1","discount24Hours" },
                    { "D3","discount3Days" },
                    { "DP","reserved" },
                    { "SD","sold" },
                    { "VA","firstOwnedVIP" },
                    { "VC","firstOwnedGold" },
                    { "VB" ,"firstOwnedSilver"}
                };

                return carstatus;
            }
        }

        public static string CarStatus(string icon)
        {
            if(carStatus.ContainsKey(icon))
                return carStatus[icon];
            
            return "default";
        }
        public static string CondStatus(string status)
        {
            foreach (var key in carStatus.Keys)
            {
                if (carStatus[key]== status)
                {
                    return key;
                }

            }
            return "";
        }
    }

    public partial class CondItem
    {
        public List<LMakeItem> makes { get; set; }
        public List<LidNameItem> inputProvinceList { get; set; }
        public List<LidNameItem> inputCarTypeList { get; set; }
        public List<LidNameItem> inputYearList { get; set; }
        public List<LidNameItem> inputPriceList { get; set; }
        public List<LidNameItem> inputInstallmentPriceList { get; set; }
        public List<LidNameItem> inputInstallmentPeriodList { get; set; }

        public List<LIDNameItem> inputGears { get; set; }
        public List<LIDNameItem> inputGasList { get; set; }
        public List<LidNameItem> inputColorList { get; set; }
        public List<LIDNameItem> inputSortList { get; set; }
        public Dictionary<string, CarStatItem> carStatuses { get; set; }
        public string DataSrc { get; set; }
    }

    public partial class CarStatItem
    {
        public string overlayIcon { get; set; }
    }

    public partial class LidNameItem
    {
        public int id { get; set; }
        public string displayName { get; set; }
    }
    public partial class LIDNameItem
    {
        public string id { get; set; }
        public string displayName { get; set; }
    }

    public partial class LMakeItem
    {
        public int id { get; set; }
        public string disp { get; set; }
        public bool isHot { get; set; }
        public List<LModelItem> models { get; set; }
    }

    public partial class LModelItem
    {
        public int id { get; set; }
        public string disp { get; set; }
        public bool isHot { get; set; }
        public List<LBodyItem> bodys { get; set; }

        public List<LTrimItem> trims { get; set; }
    }

    public partial class LBodyItem
    {
        public int id { get; set; }
        public string disp { get; set; }
        public List<int> trimIds { get; set; }
    }

    public partial class LTrimItem
    {
        public int id { get; set; }
        public string disp { get; set; }
        public List<int> bodyIds { get; set; }
    }
}