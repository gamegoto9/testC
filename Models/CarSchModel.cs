using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotnetAPI.Models
{
    public partial class CarSchItem
    {
        public string title { get; set; }
        public int carCount { get; set; }
        public string sortType { get; set; }

        public List<CarListItem> carList;
        public List<LidNameItem> makeList { get; set; }
        public List<LidNameItem> modelList { get; set; }
        public List<LidNameItem> bodyList { get; set; }
        public List<LidNameItem> trimList { get; set; }
        public List<LidNameItem> yearList { get; set; }
    }
    public partial class CarListItem
    {
        public int cID { get; set; }
        public string displayFullName { get; set; }
        public string displayModelName { get; set; }
        public string displayPrice { get; set; }
        public bool isPricePerMonth { get; set; }
        public string displayYear { get; set; }
        public string displayTitle { get; set; }
        public string displayLastUpdate { get; set; }
        public string displayPageView { get; set; }
        public string previewImage300 { get; set; }
        public string previewImage600 { get; set; }
        public string status { get; set; }
    }
}