using System.Data;
using AutoMapper;
using Dapper;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace DotnetAPI.Data
{
    public class CarRepository : ICarRepository
    {

        IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private readonly string Cache_CID = "JSON_CID_v60.2_";
        private readonly IHttpContextAccessor _httpContextAccessor;
        DataTable tbC, tbL, tbY;
        IMapper _mapper;


        public CarRepository(IConfiguration config, IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;

            _mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SearchDto, SearchDto>();
            }));
        }


        public void AddToCache(string key, object value)
        {
            _memoryCache.Set(key, value, TimeSpan.FromMinutes(60)); // Cache for 30 minutes
        }

        public object GetFromCache(string key)
        {
            return _memoryCache.TryGetValue(key, out var value) ? value : null;
        }

        public object test()
        {
            throw new NotImplementedException();
        }

        private static void AccessDataSetValues(DataSet dataSet)
        {
            foreach (DataTable table in dataSet.Tables)
            {
                Console.WriteLine($"Table: {table.TableName}");

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        Console.WriteLine($"{column.ColumnName}: {row[column]}");
                    }
                    Console.WriteLine();
                }
            }
        }

        public object search(SearchDto iParam)
        {

            int tableIndex = 0;

            if (iParam.Bt2 == "-1") iParam.Bt2 = "0";
            DataSet DS = new DataSet();
            if (string.IsNullOrEmpty(iParam.Gr) || (iParam.Gr.ToLower() != "a" && iParam.Gr.ToLower() != "m")) iParam.Gr = "b";
            if (string.IsNullOrEmpty(iParam.Gs) || (iParam.Gs.ToLower() != "y" && iParam.Gs.ToLower() != "x")) iParam.Gs = "n";
            string MMTs = "";
            if (iParam.Fno == "new") iParam.Sort = "u";

            Console.WriteLine("search - - -- - - - - - -- - - - -- - - - - - --", iParam.Fno + "->" + iParam.MkID + "," + iParam.MdID);

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                var parameters = new DynamicParameters();


                parameters.Add("@Fno", iParam.Fno);
                parameters.Add("@Prc1", iParam.Bt1);
                parameters.Add("@Prc2", iParam.Bt2);
                parameters.Add("@Yr1", iParam.Yr1);
                parameters.Add("@Yr2", iParam.Yr2);
                parameters.Add("@cType", iParam.Tys);
                parameters.Add("@isDPmt", iParam.IsDpmt);
                parameters.Add("@Dpmt", iParam.Dpmt);
                parameters.Add("@Gear", iParam.Gr);
                parameters.Add("@isGasOnly", iParam.Gs);
                parameters.Add("@clIDb", iParam.Cl);
                parameters.Add("@JvID", iParam.Jv);
                parameters.Add("@cSort", iParam.Sort);
                MMTs = iParam.MkID + "." + iParam.MdID + "." + iParam.BdID + "." + iParam.TaID;
                parameters.Add("@MkID", iParam.MkID);
                parameters.Add("@MdID", iParam.MdID);
                parameters.Add("@BdID", iParam.BdID);
                parameters.Add("@TaID", iParam.TaID);
                // parameters.Add("@CID", Cid, DbType.Int32, ParameterDirection.Input);

                using (var result = connection.QueryMultiple("w60.SchCar", parameters, commandType: CommandType.StoredProcedure))
                {

                    // Iterate through each result set in the GridReader

                    while (!result.IsConsumed)
                    {
                        // Read each result set into a List of objects
                        List<object> resultList = result.Read<object>().ToList();

                        // Convert the list to a DataTable

                        // DataTable dataTable = DataTableHelper.ConvertListToDataTable2(resultList);
                        // DataTableDto dataTableDto = DataTableHelper.ConvertListToDataTableDto2(resultList);
                        //  DataSet dataSet = DataTableHelper.ConvertListToDataSet(resultList);
                        // Add the DataTable to the DataSet
                        //  DS.Add(dataSet);
                        DataTableHelper.AddListToDataSet(DS, resultList);




                        tableIndex++;
                    }
                    connection.Close();
                }

                AccessDataSetValues(DS);

                var tbC = DS.Tables[0];
                var tbL = DS.Tables[1];
                var tbY = DS.Tables[2];

                var schcars = new CarSchItem();

                schcars.title = "";
                schcars.carCount = tbC.Rows.Count;
                if (schcars.carCount == 600)
                    schcars.carCount = 601;
                schcars.sortType = iParam.Sort;

                schcars.carList = new List<CarListItem>();

                for (int i = 0; i < tbC.Rows.Count; i++)
                {
                    var dr = tbC.Rows[i];
                    var car = new CarListItem();
                    car.cID = Convert.ToInt32(dr["Cid"]);
                    car.displayModelName = string.Format("{0}", dr["Model"]);
                    car.displayFullName = string.Format("{0}", dr["NameMMT"]);
                    car.displayYear = dr["Yr"].ToString();
                    car.displayPrice = string.Format("{0:#,###}", dr["Baht"]);
                    if (dr["IsDPrc"].ToString() == "Y")
                        car.displayPrice = "*" + car.displayPrice;

                    // car.previewImage300 = cImg.GetImgUrl("imgc1", dr["CIDX"], dr["CID"], "1T3");
                    // car.previewImage600 = cImg.GetImgUrl("imgc1", dr["CIDX"], dr["CID"], "1");
                    car.displayPageView = Convert.ToInt32(dr["iPgVw"]) > 0 ? string.Format("{0:#,##0}", dr["iPgVw"]) : "";
                    car.status = CondCarStatus.CarStatus(dr["Icon"].ToString().Trim());

                    //if (Fno.ToLower() != "new")
                    //  car.Icon = car.Icon.Replace("NW.", "NWC.");

                    TimeSpan Diff = DateTime.Now - (DateTime)dr["dtUpd"];
                    if ((int)Math.Floor(Diff.TotalDays) > 1)
                        car.displayLastUpdate = "";
                    else
                        // car.displayLastUpdate = cUtils.DtDiff(dr["dtUpd"]);

                    car.isPricePerMonth = dr["cBt"].ToString() == "BMP";
                    car.displayTitle = dr["Title"].ToString();

                    schcars.carList.Add(car);


                }

                schcars.makeList = new List<LidNameItem>();
                schcars.modelList = new List<LidNameItem>();
                schcars.bodyList = new List<LidNameItem>();
                schcars.trimList = new List<LidNameItem>();

                for (int i = 0; i < tbL.Rows.Count; i++)
                {
                    var dr = tbL.Rows[i];
                    int mkID = Convert.ToInt32(dr["mkID"]);
                    int mdID = Convert.ToInt32(dr["mdID"]);
                    int bdID = Convert.ToInt32(dr["bdID"]);
                    int taID = Convert.ToInt32(dr["taID"]);
                    if (mkID > 0 && mdID == 0)
                        schcars.makeList.Add(new LidNameItem() { id = mkID, displayName = dr["Name"].ToString() });
                    else if (bdID == 0 && taID == 0)
                        schcars.modelList.Add(new LidNameItem() { id = mdID, displayName = dr["Name"].ToString() });
                    else if (bdID > 0 && taID == 0)
                        schcars.bodyList.Add(new LidNameItem() { id = bdID, displayName = dr["Name"].ToString() });
                    else if (bdID == 0 && taID > 0)
                        schcars.trimList.Add(new LidNameItem() { id = taID, displayName = dr["Name"].ToString() });
                }
                schcars.yearList = new List<LidNameItem>();
                for (int i = 0; i < tbY.Rows.Count; i++)
                {
                    var dr = tbY.Rows[i];
                    // schcars.yearList.Add(new LidNameItem() { id =Convert.ToInt32(dr["Yr"]), displayName = dr["Yr"].ToString() });
                }
                return schcars;
            }

            // return tbC;
            // return tbC.Rows[0]["CID"];
            /*
                            var schcars = new CarSearchModel();

                            for (int i = 0; i < tbC.Rows.Count; i++)
                            {

                                var car = new Car();
                                car.CId = Convert.ToInt32(tbC.Rows[i]["CID"]);
                                car.Status = "";
                                car.DispTitle = tbC.Rows[i]["title"].ToString();
                                car.Img300Url = "";
                                car.Img600Url = "";
                                car.Year = int.Parse(tbC.Rows[i]["Yr"].ToString());
                                car.Name = tbC.Rows[i]["NameMMT"].ToString();
                                car.ModelName = tbC.Rows[i]["Model"].ToString();
                                car.Price = string.Format("{0:#,#.##} ", tbC.Rows[i]["Baht"].ToString());
                                car.IsPricePerMonth = false;
                                car.LastUpdate = tbC.Rows[i]["dtUpd"].ToString();
                                car.PageView = int.Parse(tbC.Rows[i]["iPgVw"].ToString());

                                if (schcars.Cars == null)
                                {
                                    schcars.Cars = new List<Car>();
                                }
                                schcars.Cars.Add(car);
                            }

                            schcars.Title = "";
                            schcars.CarCnt = tbC.Rows.Count;
                            schcars.Sort = iParam.Sort;
                            schcars.Makes = new List<Mark>;

                            return schcars;
                        }
            /*
                            var schcars = new CarSchItem();

                            schcars.title = (cCond.GetCMM(Fno, Bt1, Bt2, Yr1, Yr2, MMTs + "", Tys + "", Dpmt + "", Gr + "", Gs + "", Cl + "", Jv + "", Sort + "") + " " + cCond.GetFno("fno:" + Fno)).Trim();
                            schcars.carCount = tbC.Rows.Count;
                            if (schcars.carCount == 600)
                                schcars.carCount = 601;
                            schcars.sortType = Sort;

                            schcars.carList = new List<CarListItem>();

                            for (int i = 0; i < tbC.Rows.Count; i++)
                            {
                                var dr = tbC.Rows[i];
                                var car = new CarListItem();
                                car.cID = checked((int)dr["Cid"]);
                                car.displayModelName = string.Format("{0}", dr["Model"]);
                                car.displayFullName = string.Format("{0}", dr["NameMMT"]);
                                car.displayYear = dr["Yr"].ToString();
                                car.displayPrice = string.Format("{0:#,###}", dr["Baht"]);
                                if (dr["IsDPrc"].ToString() == "Y")
                                    car.displayPrice = "*" + car.displayPrice;

                                car.previewImage300 = cImg.GetImgUrl("imgc1", dr["CIDX"], dr["CID"], "1T3");
                                car.previewImage600 = cImg.GetImgUrl("imgc1", dr["CIDX"], dr["CID"], "1");
                                car.displayPageView = checked((int)dr["iPgVw"]) > 0 ? string.Format("{0:#,##0}", dr["iPgVw"]) : "";
                                car.status = CondCarStatus.CarStatus(dr["Icon"].ToString().Trim());

                                //if (Fno.ToLower() != "new")
                                //  car.Icon = car.Icon.Replace("NW.", "NWC.");

                                TimeSpan Diff = DateTime.Now - (DateTime)dr["dtUpd"];
                                if ((int)Math.Floor(Diff.TotalDays) > 1)
                                    car.displayLastUpdate = "";
                                else
                                    car.displayLastUpdate = cUtils.DtDiff(dr["dtUpd"]);

                                car.isPricePerMonth = dr["cBt"].ToString() == "BMP";
                                car.displayTitle = dr["Title"].ToString();

                                schcars.carList.Add(car);


                            }

                            schcars.makeList = new List<LidNameItem>();
                            schcars.modelList = new List<LidNameItem>();
                            schcars.bodyList = new List<LidNameItem>();
                            schcars.trimList = new List<LidNameItem>();

                            for (int i = 0; i < tbL.Rows.Count; i++)
                            {
                                var dr = tbL.Rows[i];
                                int mkID = checked((int)dr["mkID"]);
                                int mdID = checked((int)dr["mdID"]);
                                int bdID = checked((int)dr["bdID"]);
                                int taID = checked((int)dr["taID"]);
                                if (mkID > 0 && mdID == 0)
                                    schcars.makeList.Add(new LidNameItem() { id = mkID, displayName = dr["Name"].ToString() });
                                else if (bdID == 0 && taID == 0)
                                    schcars.modelList.Add(new LidNameItem() { id = mdID, displayName = dr["Name"].ToString() });
                                else if (bdID > 0 && taID == 0)
                                    schcars.bodyList.Add(new LidNameItem() { id = bdID, displayName = dr["Name"].ToString() });
                                else if (bdID == 0 && taID > 0)
                                    schcars.trimList.Add(new LidNameItem() { id = taID, displayName = dr["Name"].ToString() });
                            }
                            schcars.yearList = new List<LidNameItem>();
                            for (int i = 0; i < tbY.Rows.Count; i++)
                            {
                                var dr = tbY.Rows[i];
                                schcars.yearList.Add(new LidNameItem() { id = checked((int)dr["Yr"]), displayName = dr["Yr"].ToString() });
                            }
                            return schcars;
                            */

        }

        /*

                public void CarDet(string Cid)
                {
                    DataSet DS = new DataSet();
                    var cardet = new CarDetItem();
                    if (GetFromCache(Cache_CID + Cid) != null)
                    {
                        cardet = (CarDetItem)GetFromCache(Cache_CID + Cid);
                        UpdLastVisit(Convert.ToInt32(Cid), 0);
                    }
                    else
                    {
                        using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                        {
                            connection.Open();

                            var parameters = new DynamicParameters();
                            parameters.Add("@CID", Cid, DbType.Int32, ParameterDirection.Input);

                            using (var result = connection.QueryMultiple("w60.GetCarDet", parameters, commandType: CommandType.StoredProcedure))
                            {

                                // Iterate through each result set in the GridReader
                                int tableIndex = 0;
                                while (!result.IsConsumed)
                                {
                                    // Read each result set into a List of objects
                                    List<object> resultList = result.Read<object>().ToList();

                                    // Convert the list to a DataTable
                                    DataTable dataTable = DataTableHelper.ConvertListToDataTable(resultList);

                                    // Add the DataTable to the DataSet
                                    DS.Tables.Add(dataTable);
                                    tableIndex++;
                                }
                                connection.Close();

                                if (DS.Tables.Count > 0 && DS.Tables[0].Rows.Count > 0)
                                {
                                    DataTable tbD = DS.Tables[0];
                                    if (DS.Tables.Count == 1)
                                    {
                                        // return MCommand.ToJSONSingleRow(tbD);
                                    }
                                    DataTable tb1CMd = DS.Tables[1];
                                    DataTable tb1CBd = DS.Tables[2];
                                    DataTable tb1CTa = DS.Tables[3];
                                    DataTable tbCGrp = DS.Tables[4];
                                    DataTable tbChkP = DS.Tables[5];

                                    var dr = tbD.Rows[0];
                                    cardet.cID = checked((int)dr["Cid"]);
                                    cardet.status = CondCarStatus.CarStatus(dr["Icon"].ToString());

                                    cardet.post = new CarPostItem();
                                    cardet.post.displayYear = dr["Yr"].ToString();
                                    cardet.post.displayFullName = dr["NmMMT"].ToString().Trim();
                                    cardet.post.displayMilage = string.Format("{0:#,##0}", dr["Km"]);
                                    cardet.post.displayBodyName = dr["BdName"].ToString().Trim();
                                    cardet.post.displayPageView = dr["iPgVw"].ToString();

                                    if (cardet.post.displayFullName.EndsWith(cardet.post.displayBodyName))
                                        cardet.post.displayFullName = cardet.post.displayFullName.Replace(cardet.post.displayBodyName, "");
                                    string bdNm = MapingBodyNm(cardet.post.displayBodyName);
                                    if (cardet.post.displayFullName.EndsWith(bdNm))
                                        cardet.post.displayFullName = cardet.post.displayFullName.Replace(bdNm, "");

                                    cardet.post.seller = new CarSellerItem();
                                    cardet.post.seller.displayName = dr["Name"].ToString();
                                    if (dr["isDeposit"].ToString() != "Y")
                                        cardet.post.seller.displayNumber = dr["TelNo"].ToString();
                                    else
                                        cardet.post.seller.displayNumber = "";

                                    // cardet.post.seller.profileImage = cImg.GetPicMem2(dr["TelNo"].ToString().PadRight(12).Substring(0, 12).Replace("-", ""), dr["IsTelImg"]).Replace("imgc1.", "www.");

                                    cardet.post.displayLocation = dr["JvName"] + ", " + dr["JvDet"];
                                    cardet.post.displayRegistrationNumber = dr["RegNo"].ToString();
                                    cardet.post.displayGear = dr["Gear"].ToString() == "A" ? "เกียร์ออโต้" : "เกียร์ธรรมดา";
                                    cardet.post.displayColor = dr["Color"].ToString();

                                    if (dr["cGas"].ToString().ToUpper() == "N")
                                        cardet.post.displayGas = "ติดแก๊ส  NGV";
                                    else if (dr["cGas"].ToString().ToUpper() == "L")
                                        cardet.post.displayGas = "ติดแก๊ส  LPG";
                                    else
                                        cardet.post.displayGas = "ไม่ติดแก๊ส";
                                    TimeSpan Diff = DateTime.Now - (DateTime)dr["dtUpd"];
                                    if ((int)Math.Floor(Diff.TotalDays) > 1)
                                        cardet.post.displayLastUpdate = "";
                                    else
                                        // cardet.post.displayLastUpdate = cUtils.DtDiff(dr["dtUpd"]);

                                        cardet.post.displaySellingPrice = string.Format("{0:#,##0}", dr["Price"]);
                                    cardet.post.displayNewPrice = string.Format("{0:#,##0}", dr["BtNew"]);
                                    cardet.post.displayPreviousPrice = string.Format("{0:#,##0}", dr["PriceBFDsc"]);
                                    cardet.post.isDownPrice = dr["IsDPrc"].ToString() == "Y";
                                    cardet.post.displayDiscountText = "ผู้ขายลดเหลือ " + string.Format("{0:#,##0}", dr["PrcTBx"]) + @"\r\nถ้าตกลงซื้อภายใน " + cUtils.StripTags(cUtils.DtTBx(dr["dtTbx"]));

                                    cardet.widgetFinance = new CarFinanceItem();
                                    cardet.widgetFinance.defaultInterest = checked((int)dr["eFin_Int"]);
                                    cardet.widgetFinance.durationMaximum = checked((int)dr["eFin_Mth"]);
                                    cardet.widgetFinance.interestRate48 = checked((int)dr["eFin_Int48"]);
                                    cardet.widgetFinance.interestRate60 = checked((int)dr["eFin_Int60"]);
                                    cardet.widgetFinance.interestRate72 = checked((int)dr["eFin_Int72"]);


                                    // cardet.post.displayTitle = cUtils.RpText(dr["Title"].ToString());
                                    // cardet.post.displayDescription = cUtils.RpText(dr["Descr"].ToString());
                                    // cardet.post.displayCarFinanceDescription = cUtils.RpText(dr["DetCF"].ToString());

                                    cardet.post.imageList = new List<string>();

                                    if ((short)dr["imgCnt"] > 0)
                                    {
                                        ushort[] bits = new ushort[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };

                                        for (int i = 1; i <= bits.Length; i++)
                                        {
                                            if ((bits[i - 1] & (short)dr["imgCnt"]) > 0)
                                            {
                                                // cardet.post.imageList.Add(cImg.GetImgUrl("img1", dr["CIDX"], dr["CID"], i.ToString()));
                                            }
                                        }
                                    }

                                    cardet.post.favoriteCount = checked((int)dr["n_Fav"]);
                                    cardet.post.followerCount = checked((int)dr["n_Alarm"]);

                                    cardet.post.warranty = new CarWarrantyItem();
                                    int wrtItem = checked((int)dr["wrtItem"]);
                                    cardet.post.warranty.type1 = false;
                                    cardet.post.warranty.type2 = false;
                                    cardet.post.warranty.displayText = "";

                                    if (wrtItem > 0)
                                    {
                                        if ((wrtItem & 0x10) > 0)
                                        {
                                            cardet.post.warranty.type1 = true;
                                        }
                                        if ((wrtItem & 0x20) > 0)
                                            cardet.post.warranty.type2 = true;

                                        if ((wrtItem & 0x1) > 0)
                                            cardet.post.warranty.displayText += @"รับคืนในราคาซื้อขาย";
                                        if ((wrtItem & 0x2) > 0)
                                            cardet.post.warranty.displayText += @"ชดเชยเป็นเงินสดทันที จำนวน " + string.Format("{0:#,###}", dr["wrtBt"]) + " บาท";
                                    }


                                    cardet.widgetCheckPrices = new CarCheckPricesItem();
                                    cardet.widgetCheckPrices.displayTitle = "เช็คราคา " + dr["MdName"] + " " + dr["BdName"];
                                    cardet.widgetCheckPrices.priceList = new List<CarPriceListItem>();
                                    for (int i = 0; i < tbChkP.Rows.Count; i++)
                                    {
                                        cardet.widgetCheckPrices.priceList.Add(new CarPriceListItem
                                        {
                                            year = checked((int)tbChkP.Rows[i]["Yr"]),
                                            avgPrice = checked((int)tbChkP.Rows[i]["avgPrc"]),
                                            minPrice = checked((int)tbChkP.Rows[i]["minPrc"]),
                                            maxPrice = checked((int)tbChkP.Rows[i]["maxPrc"]),
                                            carCount = checked((int)tbChkP.Rows[i]["nCar"]),
                                            inputParameters = new InputParametersWithYr()
                                            {
                                                Yr1 = checked((int)tbChkP.Rows[i]["Yr"]),
                                                Yr2 = checked((int)tbChkP.Rows[i]["Yr"]),
                                                Fno = "all",
                                                MkID = checked((int)dr["MkID"]),
                                                MdID = checked((int)dr["MdID"]),
                                                BdID = checked((int)dr["BdID"])
                                            }
                                        });
                                    }

                                    cardet.widgetSameModel = new CarSameModelItem();
                                    cardet.widgetSameModel.displayTitle = dr["MdName"] + " ทั้งหมดบนตลาดรถ";
                                    cardet.widgetSameModel.differentFunction = new Dictionary<string, CarDiffItem>();
                                    var mdkeys = new string[] { "week", "all", "deal", "new", "highlight" };
                                    var mdfnos = new string[] { "week", "all", "deal", "new", "hot" };
                                    for (int i = 0; i < mdkeys.Length; i++)
                                    {
                                        var mdr = tb1CMd.Select("fno='" + mdfnos[i] + "'");
                                        if (mdr.Length == 0 || checked((int)mdr[0]["nCar"]) == 0)
                                        {
                                            cardet.widgetSameModel.differentFunction.Add(mdkeys[i], null);
                                            continue;
                                        }

                                        for (int j = 0; j < mdr.Length; j++)
                                        {
                                            if (checked((int)mdr[j]["BdID"]) > 0) continue;
                                            if (checked((int)mdr[j]["TaID"]) > 0) continue;
                                            cardet.widgetSameModel.differentFunction.Add(mdkeys[i], new CarDiffItem()
                                            {
                                                displayName = mdr[j]["Text"].ToString(),
                                                carCount = checked((int)mdr[j]["nCar"]),
                                                inputParameters = new InputParameters()
                                                {
                                                    Fno = mdfnos[i],
                                                    MkID = checked((int)dr["MkID"]),
                                                    MdID = checked((int)dr["MdID"])
                                                }
                                            });
                                        }
                                    }

                                    cardet.widgetSameModel.differentBodyList = new List<CarDiffItem>();
                                    var bdr = tb1CMd.Select("fno='all' and BdID>0");
                                    for (int j = 0; j < bdr.Length; j++)
                                    {
                                        cardet.widgetSameModel.differentBodyList.Add(new CarDiffItem()
                                        {
                                            displayName = bdr[j]["Text"].ToString(),
                                            carCount = checked((int)bdr[j]["nCar"]),
                                            inputParameters = new InputParameters()
                                            {
                                                Fno = "all",
                                                MkID = checked((int)dr["MkID"]),
                                                MdID = checked((int)dr["MdID"]),
                                                BdID = checked((int)bdr[j]["BdID"])
                                            }
                                        });
                                    }
                                    cardet.widgetSameModel.differentTrimList = new List<CarDiffItem>();
                                    var tdr = tb1CMd.Select("fno='all' and TaID>0");
                                    for (int j = 0; j < tdr.Length; j++)
                                    {
                                        cardet.widgetSameModel.differentTrimList.Add(new CarDiffItem()
                                        {
                                            displayName = tdr[j]["Text"].ToString(),
                                            carCount = checked((int)tdr[j]["nCar"]),
                                            inputParameters = new InputParameters()
                                            {
                                                Fno = "all",
                                                MkID = checked((int)dr["MkID"]),
                                                MdID = checked((int)dr["MdID"]),
                                                BdID = checked((int)tdr[j]["TaID"])
                                            }
                                        });
                                    }

                                    cardet.widgetSimilarBody = new CarOtherItemWithTitle()
                                    {
                                        displayTitle = dr["MdName"] + " " + dr["BdName"] + " ทั้งหมด",
                                        carList = new List<CarWidgetItem>()
                                    };
                                    cardet.widgetSimilarPrice = new CarOtherItemWithTitle()
                                    {
                                        displayTitle = "รถอื่นๆ ในงบ " + string.Format("{0:#,##0}", dr["Price"]),
                                        carList = new List<CarWidgetItem>()
                                    };
                                    cardet.widgetSimilarMake = new CarOtherItemWithMakeImg
                                    {
                                        displayTitle = "รถ " + dr["mkName"] + " รุ่นอื่นๆ",
                                        makeImage = "https://www.taladrod.com/c/Mk/svg/" + string.Format("{0:000}", dr["MkID"]) + ".svg",
                                        carList = new List<CarWidgetItem>()
                                    };
                                    cardet.widgetOtherCarsDeal = new CarOtherItem()
                                    {
                                        carList = new List<CarWidgetItem>()
                                    };
                                    cardet.widgetOtherCarsFirstOwned = new CarOtherItem()
                                    {
                                        carList = new List<CarWidgetItem>()
                                    };
                                    cardet.widgetOtherCarsHot = new CarOtherItem()
                                    {
                                        carList = new List<CarWidgetItem>()
                                    };
                                    cardet.widgetOtherCarsNew = new CarOtherItemWithButton
                                    {
                                        carList = new List<CarWidgetItem>(),
                                        buttonList = new List<CarButtonItem>()
                                    };
                                    int nMkID = 0, nMdID = 0;
                                    string nDisplay = "";
                                    for (int i = 0; i < tbCGrp.Rows.Count; i++)
                                    {
                                        var car = new CarWidgetItem()
                                        {
                                            cID = checked((int)tbCGrp.Rows[i]["Cid"]),
                                            // previewImage300 = cImg.GetImgUrl("imgc1", tbCGrp.Rows[i]["CIDX"], tbCGrp.Rows[i]["CID"], "1T3"),
                                            displayName = tbCGrp.Rows[i]["Model"].ToString(),
                                            displayYear = tbCGrp.Rows[i]["Yr"].ToString(),
                                            displayPrice = string.Format("{0:#,##0}", tbCGrp.Rows[i]["Baht"])
                                        };


                                        string Grp = tbCGrp.Rows[i]["Grp"].ToString().Trim().ToUpper();
                                        if (Grp == "SBD") cardet.widgetSimilarBody.carList.Add(car);
                                        if (Grp == "SBT") cardet.widgetSimilarPrice.carList.Add(car);
                                        if (Grp == "SMK") cardet.widgetSimilarMake.carList.Add(car);
                                        if (Grp == "PM") cardet.widgetOtherCarsDeal.carList.Add(car);
                                        if (Grp == "M1") cardet.widgetOtherCarsFirstOwned.carList.Add(car);
                                        if (Grp == "HT") cardet.widgetOtherCarsHot.carList.Add(car);
                                        if (Grp == "NW") cardet.widgetOtherCarsNew.carList.Add(car);

                                        if (Grp == "NW" && nMkID == 0)
                                        {
                                            nMkID = checked((int)tbCGrp.Rows[i]["MkID"]);
                                            nMdID = checked((int)tbCGrp.Rows[i]["MdID"]);
                                            nDisplay = tbCGrp.Rows[i]["Model"].ToString();
                                        }
                                    }
                                    cardet.widgetOtherCarsNew.buttonList.Add(new CarButtonItem()
                                    {
                                        display = "https://m.taladrod.com/assets/images/24icon.svg",
                                        inputParameters = new InputParameters()
                                        {
                                            Fno = "new"
                                        }
                                    });
                                    if (cardet.widgetOtherCarsNew.carList.Count > 0)
                                    {
                                        cardet.widgetOtherCarsNew.buttonList.Add(new CarButtonItem()
                                        {
                                            display = "https://www.taladrod.com/c/Mk/svg/" + string.Format("{0:000}", nMkID) + ".svg",
                                            inputParameters = new InputParameters()
                                            {
                                                Fno = "new",
                                                MkID = nMkID
                                            }
                                        });
                                        cardet.widgetOtherCarsNew.buttonList.Add(new CarButtonItem()
                                        {
                                            display = nDisplay.ToUpper().PadRight(3).Substring(0, 3).Trim(),
                                            inputParameters = new InputParameters()
                                            {
                                                Fno = "new",
                                                MkID = nMkID,
                                                MdID = nMdID
                                            }
                                        });
                                    }


                                    UpdLastVisit(Convert.ToInt32(Cid), tbD.Rows[0]["CIdx"]);

                                }

                                Cache.Insert(Cache_CID + Cid, cardet, null, DateTime.Now.AddMinutes(10), TimeSpan.Zero);
                            }
                        }


                    }
                    cardet.user = new CarUsrItem();
                    if (IsFavCar( checked((int)Cid)))
                    {
                        cardet.user.isFavorite = true;
                    }
                    return cardet;
                }

                private bool IsFavCar(int Cid)
                {
                    if (cAuth.MID == 0)
                        return false;

                    DataSet DS = new DataSet();

                    string Cache_CFav = "DS_CFAV_v60_";
                    if (GetFromCache(Cache_CFav + cAuth.MID) != null)
                    {
                        DS = (DataSet)GetFromCache(Cache_CFav + cAuth.MID);
                    }
                    else
                    {
                         using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                        {
                            connection.Open();

                            var parameters = new DynamicParameters();
                            // parameters.Add("@MID", cAuth.MID, DbType.Int32, ParameterDirection.Input);

                            using (var result = connection.QueryMultiple("w60.getFavC", parameters, commandType: CommandType.StoredProcedure))
                            {


                                int tableIndex = 0;
                                while (!result.IsConsumed)
                                {

                                    List<object> resultList = result.Read<object>().ToList();


                                    DataTable dataTable = DataTableHelper.ConvertListToDataTable(resultList);


                                    DS.Tables.Add(dataTable);
                                    tableIndex++;
                                }
                                connection.Close();
                            }
                    }

                    var tbC = DS.Tables[1];
                    for (int i = 0; i < tbC.Rows.Count; i++)
                    {
                        if (Convert.ToInt32(tbC.Rows[i]["Cid"]) == Cid)
                            return true;
                    }
                    return false;
                }

                private void UpdLastVisit(int cid, object cidx)
                {
                    //   string cookieValue = _httpContextAccessor.HttpContext.Request.Cookies["MyCookieName"];
                    var Request = _httpContextAccessor.HttpContext.Request;
                    var Response = _httpContextAccessor.HttpContext.Response;
                    var Cookie_VisitCID = "VISITCID_V5_";
                    bool isUpd = true;
                    string SCID = cid.ToString() + "|" + cidx;
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(1),
                    };
                    if (!string.IsNullOrEmpty(Request.Cookies[Cookie_VisitCID]))
                    {
                        string vsCid = _httpContextAccessor.HttpContext.Request.Cookies[Cookie_VisitCID];
                        String[] vsCids = vsCid.Split('$');
                        vsCid = "";
                        for (int i = 0; i < vsCids.Length && i < 40; i++)
                        {
                            if (vsCids[i] == SCID)
                            {
                                isUpd = false;
                                continue;
                            }
                            vsCid += "$" + vsCids[i];
                        }
                        vsCid = SCID + vsCid;

                        // Response.Cookies.Append(Cookie_VisitCID, vsCid);
                        Response.Cookies.Append(Cookie_VisitCID, vsCid, cookieOptions);
                    }
                    else
                    {
                        // Response.Cookies[Cookie_VisitCID].Value = SCID;
                        Response.Cookies.Append(Cookie_VisitCID, SCID, cookieOptions);

                    }

                    if (isUpd)
                    {
                        // cPgCnt.AddCIBView(cid);
                    }
                }



                static public string MapingBodyNm(string Name)
                {
                    Dictionary<string, string> mappingBd = new Dictionary<string, string>();
                    mappingBd.Add("DOUBLE CAB", "DBL CAB");

                    if (mappingBd.ContainsKey(Name))
                        return mappingBd[Name];

                    return Name;
                }
         */

    }

}