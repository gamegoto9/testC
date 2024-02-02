using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotnetAPI.Models
{
    public partial class HomeItem
    {
        public int carCount { get; set; } 
        public int newCarCount { get; set; }
        public string carListDisplayName { get; set; }
        public List<CarWidgetHome> widgetPreviewNew { get; set; }
        public List<MakeListItem> widgetMarketOverviewNew { get; set; }
    }

    public partial class CarWidgetHome
    {
        public int cID { get; set; }
        public string previewImage300 { get; set; }
        public string displayName { get; set; }
        public string displayPrice { get; set; }
    }
    public partial class MakeListItem
    {
        public int mkID { get; set; }
        public string displayName { get; set; }
        public string makeImage { get; set; }
        public int carCount { get; set;}
        public int carChangeCount { get; set;}
    }
}