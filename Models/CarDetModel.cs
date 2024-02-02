using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotnetAPI.Models
{
    public partial class CarDetItem
    {
        public int cID { get; set; }
        public string status { get; set; }
        public CarPostItem post { get; set; }
        public CarFinanceItem widgetFinance { get; set; }
        public CarCheckPricesItem widgetCheckPrices { get; set; }
        public CarSameModelItem widgetSameModel { get; set; }
        public CarOtherItemWithTitle widgetSimilarBody { get; set; }
        public CarOtherItemWithTitle widgetSimilarPrice { get; set; }
        public CarOtherItemWithMakeImg widgetSimilarMake { get; set; }
        public CarOtherItem widgetOtherCarsDeal { get; set; }
        public CarOtherItem widgetOtherCarsFirstOwned { get; set; }
        public CarOtherItem widgetOtherCarsHot { get; set; }
        public CarOtherItemWithButton widgetOtherCarsNew { get; set; }
        public CarUsrItem user { get; set; }
    }
    public partial class CarPostItem
    {
        public string displayFullName { get; set; }
        public string displayBodyName { get; set; }
        public string displaySellingPrice { get; set; }
        public string displayNewPrice { get; set; }
        public bool isDownPrice { get; set; }
        public string displayPreviousPrice { get; set; }
        public string displayDiscountText { get; set; }
        public string displayLocation { get; set; }
        public string displayMilage { get; set; }
        public string displayYear { get; set; }
        public string displayRegistrationNumber { get; set; }
        public string displayGear { get; set; }
        public string displayColor { get; set; }
        public string displayGas { get; set; }
        public string displayLastUpdate { get; set; }
        public string displayPageView { get; set; }
        public string displayTitle { get; set; }

        public string displayDescription { get; set; }
        public string displayCarFinanceDescription { get; set; }
        public CarSellerItem seller { get; set; }
        public CarWarrantyItem warranty { get; set; }
        public List<string> imageList { get; set; }
        public int favoriteCount { get; set; }
        public int followerCount { get; set; }
    }

    public partial class CarSellerItem
    {
        public string displayName { get; set; }
        public string displayNumber { get; set; }
        public string profileImage { get; set; }
    }
    public partial class CarWarrantyItem
    {
        public bool type1 { get; set; }
        public bool type2 { get; set; }
        public string displayText { get; set; }
    }

    public partial class CarFinanceItem
    {
        public Decimal defaultInterest { get; set; }
        public int durationMaximum { get; set; }
        public Decimal interestRate48 { get; set; }
        public Decimal interestRate60 { get; set; }
        public Decimal interestRate72 { get; set; }

    }

    public partial class CarCheckPricesItem
    {
        public string displayTitle { get; set; }
        public List<CarPriceListItem> priceList { get; set; }
    }
    public partial class CarPriceListItem
    {
        public int year { get; set; }
        public int avgPrice { get; set; }
        public int minPrice { get; set; }
        public int maxPrice { get; set; }
        public int carCount { get; set; }
        public InputParametersWithYr inputParameters { get; set; }
    }
    public partial class CarSameModelItem
    {
        public string displayTitle { get; set; }
        public CarPriceListItem priceList { get; set; }
        public Dictionary<string, CarDiffItem> differentFunction { get; set; }
        public List<CarDiffItem> differentBodyList { get; set; }
        public List<CarDiffItem> differentTrimList { get; set; }
    }
    public partial class CarDiffItem
    {
        public string displayName { get; set; }
        public int carCount { get; set; }
        public InputParameters inputParameters { get; set; }
    }


    public partial class CarOtherItemWithTitle
    {
        public string displayTitle { get; set; }
        public List<CarWidgetItem> carList { get; set; }

    }
    public partial class CarOtherItemWithMakeImg
    {
        public string makeImage { get; set; }
        public string displayTitle { get; set; }
        public List<CarWidgetItem> carList { get; set; }

    }

    public partial class CarOtherItemWithButton {
        public List<CarWidgetItem> carList { get; set; }
        public List<CarButtonItem> buttonList { get; set; }
    }

    public partial class CarOtherItem
    {
        public List<CarWidgetItem> carList { get; set; }

    }

    public partial class CarWidgetItem
    {
        public int cID { get; set; }
        public string previewImage300 { get; set; }
        public string displayName { get; set; }
        public string displayYear { get; set; }
        public string displayPrice { get; set; }
    }

    public partial class CarButtonItem
    {
        public string display { get; set; }
        public InputParameters inputParameters { get; set; }
    }
    public partial class InputParametersWithYr
    {
        public int Yr1 { get; set; }
        public int Yr2 { get; set; }
        public string Fno { get; set; }
        public int MkID { get; set; }
        public int MdID { get; set; }
        public int BdID { get; set; }
        public int TaID { get; set; }
    }
    public partial class InputParameters
    {
        public string Fno { get; set; }
        public int MkID { get; set; }
        public int MdID { get; set; }
        public int BdID { get; set; }
        public int TaID { get; set; }
    }

    public partial class CarUsrItem
    {
        public bool isFavorite { get; set; }
    }
}