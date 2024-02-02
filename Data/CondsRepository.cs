using System.Data;
using Dapper;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DotnetAPI.Data
{
    public class CondsRepository : ICondsRepository
    {

        IConfiguration _config;
        private readonly IMemoryCache _memoryCache;
        private readonly string Cache_Cond = "JSON_Cond_v60_2";

        public CondsRepository(IConfiguration config, IMemoryCache memoryCache)
        {
            _config = config;
            _memoryCache = memoryCache;
        }

        public DataSet ExecuteYourStoredProcedure()
        {
            DataSet dataSet = new DataSet();

            // Get connection string from appsettings.json
            string connectionString = _config.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                using (SqlCommand command = new SqlCommand("EXEC w60.getMMT", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {

                    }
                }
            }

            return dataSet;
        }

        public IEnumerable<MMT> CallYourStoredProcedure()
        {
            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                using (var result = connection.QueryMultiple(@"w60.getMMT", commandType: CommandType.StoredProcedure))
                {
                    var users = result.Read<MMT>().ToList();
                    // var products = result.Read<Product>().ToList();

                    return (users);
                }
            }
        }


        public CondItem getCond()
        {
            var schCondItem = new CondItem();
            if (GetFromCache(Cache_Cond) != null)
            {
                schCondItem = (CondItem)GetFromCache(Cache_Cond);
                schCondItem.DataSrc = "Cache";
                return schCondItem;
            }
            else
            {
                DataSet DS = GetMMTs();

                DataTable tbMk = DS.Tables[0];
                DataTable tbCl = DS.Tables[3];
                DataTable tbJv = DS.Tables[6];




                for (int i = 0; i < tbMk.Rows.Count; i++)
                {

                    var make = new LMakeItem();
                    make.id = Convert.ToInt32(tbMk.Rows[i]["MkID"]);
                    make.disp = tbMk.Rows[i]["Name"].ToString();
                    make.isHot = Convert.ToInt32(tbMk.Rows[i]["isHot"]) == 1;
                    make.models = GetModel(DS, make.id);
                    //if(!make.IsHot)
                    if (schCondItem.makes == null)
                    {
                        schCondItem.makes = new List<LMakeItem>();
                    }
                    schCondItem.makes.Add(make);
                }

                schCondItem.inputProvinceList = new List<LidNameItem>();
                schCondItem.inputProvinceList.Add(new LidNameItem()
                {
                    id = 0,
                    displayName = "ทั่วไทย"
                });

                for (int i = 0; i < tbJv.Rows.Count; i++)
                {
                    var dr = tbJv.Rows[i];
                    schCondItem.inputProvinceList.Add(new LidNameItem()
                    {
                        id = checked((int)dr["JvID"]),
                        displayName = dr["Name"].ToString()
                    });
                }

                schCondItem.inputColorList = new List<LidNameItem>();
                schCondItem.inputColorList.Add(new LidNameItem()
                {
                    id = 0,
                    displayName = "ทุกสี"
                });
                for (int i = 0; i < tbCl.Rows.Count; i++)
                {
                    var dr = tbCl.Rows[i];
                    schCondItem.inputColorList.Add(new LidNameItem()
                    {
                        id = checked((int)dr["ClIDb"]),
                        displayName = dr["TName"].ToString()
                    });
                }

                schCondItem.inputCarTypeList = new List<LidNameItem>();
                schCondItem.inputCarTypeList.Add(new LidNameItem()
                {
                    id = 0,
                    displayName = "ทุกประเภท"
                });
                schCondItem.inputCarTypeList.Add(new LidNameItem()
                {
                    id = 1,
                    displayName = "รถเก๋ง"
                });
                schCondItem.inputCarTypeList.Add(new LidNameItem()
                {
                    id = 2,
                    displayName = "รถอเนกประสงค์"
                });
                schCondItem.inputCarTypeList.Add(new LidNameItem()
                {
                    id = 4,
                    displayName = "รถออฟโรด"
                });
                schCondItem.inputCarTypeList.Add(new LidNameItem()
                {
                    id = 8,
                    displayName = "รถกระบะ"
                });
                schCondItem.inputCarTypeList.Add(new LidNameItem()
                {
                    id = 16,
                    displayName = "รถตู้"
                });
                schCondItem.inputCarTypeList.Add(new LidNameItem()
                {
                    id = 32,
                    displayName = "รถสปอร์ต"
                });
                schCondItem.inputCarTypeList.Add(new LidNameItem()
                {
                    id = 64,
                    displayName = "รถหรู"
                });


                schCondItem.inputGears = new List<LIDNameItem>();
                schCondItem.inputGears.Add(new LIDNameItem()
                {
                    id = "b",
                    displayName = "ทุกเกียร์"
                });
                schCondItem.inputGears.Add(new LIDNameItem()
                {
                    id = "a",
                    displayName = "เกียร์ออโต้"
                });
                schCondItem.inputGears.Add(new LIDNameItem()
                {
                    id = "m",
                    displayName = "เกียร์ธรรมดา"
                });

                schCondItem.inputGasList = new List<LIDNameItem>();
                schCondItem.inputGasList.Add(new LIDNameItem()
                {
                    id = "n",
                    displayName = "ทั้งหมด"
                });
                schCondItem.inputGasList.Add(new LIDNameItem()
                {
                    id = "y",
                    displayName = "ติดแก๊ส"
                });
                schCondItem.inputGasList.Add(new LIDNameItem()
                {
                    id = "x",
                    displayName = "ไม่ติดแก๊ส"
                });

                schCondItem.inputSortList = new List<LIDNameItem>();
                schCondItem.inputSortList.Add(new LIDNameItem()
                {
                    id = "y",
                    displayName = "ปีล่าสุด"
                });
                schCondItem.inputSortList.Add(new LIDNameItem()
                {
                    id = "b",
                    displayName = "ราคาต่ำสุด"
                });
                schCondItem.inputSortList.Add(new LIDNameItem()
                {
                    id = "u",
                    displayName = "อัพเดทล่าสุด"
                });


                string[] aY = new string[] { "ต่ำสุด", "0", "2009", "2009", "2014", "2014", "2017", "2017", "2019", "2019" };
                schCondItem.inputYearList = new List<LidNameItem>();
                for (int i = 0; i < aY.Length; i += 2)
                {
                    schCondItem.inputYearList.Add(new LidNameItem()
                    {
                        id = int.Parse(aY[i + 1]),
                        displayName = aY[i]
                    });
                }
                for (int i = 2020; i <= DateTime.Now.Year; i++)
                {
                    schCondItem.inputYearList.Add(new LidNameItem()
                    {
                        id = i,
                        displayName = i.ToString()
                    });
                }

                string[] aP = new string[] { "ต่ำสุด", "0", "100,000", "100000", "200,000", "200000", "300,000", "300000", "500,000", "500000", "700,000", "700000", "1,000,000", "1000000", "3,000,000", "3000000", "สูงสุด", "-1" };
                string[] aPm = new string[] { "2,000", "2000", "4,000", "4000", "6,000", "6000", "8,000", "8000", "10,000", "10000", "15,000", "15000", "20,000", "20000", "40,000", "40000", "สูงสุด", "-1" };


                schCondItem.inputPriceList = new List<LidNameItem>();

                for (int i = 0; i < aP.Length; i += 2)
                {
                    schCondItem.inputPriceList.Add(new LidNameItem()
                    {
                        id = int.Parse(aP[i + 1]),
                        displayName = aP[i]
                    });
                }
                schCondItem.inputInstallmentPriceList = new List<LidNameItem>();

                for (int i = 0; i < aPm.Length; i += 2)
                {
                    schCondItem.inputInstallmentPriceList.Add(new LidNameItem()
                    {
                        id = int.Parse(aPm[i + 1]),
                        displayName = aPm[i]
                    });
                }

                schCondItem.inputInstallmentPeriodList = new List<LidNameItem>();
                schCondItem.inputInstallmentPeriodList.Add(new LidNameItem()
                {
                    id = 48,
                    displayName = "48"
                });
                schCondItem.inputInstallmentPeriodList.Add(new LidNameItem()
                {
                    id = 60,
                    displayName = "60"
                });
                schCondItem.inputInstallmentPeriodList.Add(new LidNameItem()
                {
                    id = 72,
                    displayName = "72"
                });
                schCondItem.inputInstallmentPeriodList.Add(new LidNameItem()
                {
                    id = 84,
                    displayName = "84"
                });

                schCondItem.carStatuses = new Dictionary<string, CarStatItem>();
                schCondItem.carStatuses.Add("default", new CarStatItem() { overlayIcon = "" });
                schCondItem.carStatuses.Add("new", new CarStatItem() { overlayIcon = "" });
                schCondItem.carStatuses.Add("highlight", new CarStatItem() { overlayIcon = "" });
                schCondItem.carStatuses.Add("discount", new CarStatItem() { overlayIcon = "" });
                schCondItem.carStatuses.Add("discount24Hours", new CarStatItem() { overlayIcon = "" });
                schCondItem.carStatuses.Add("discount3Days", new CarStatItem() { overlayIcon = "" });
                schCondItem.carStatuses.Add("reserved", new CarStatItem() { overlayIcon = "" });
                schCondItem.carStatuses.Add("sold", new CarStatItem() { overlayIcon = "" });
                schCondItem.carStatuses.Add("firstOwnedVIP", new CarStatItem() { overlayIcon = "" });
                schCondItem.carStatuses.Add("firstOwnedGold", new CarStatItem() { overlayIcon = "" });
                schCondItem.carStatuses.Add("firstOwnedSilver", new CarStatItem() { overlayIcon = "" });

                foreach (string key in schCondItem.carStatuses.Keys)
                {
                    if (key == "default")
                        continue;
                    schCondItem.carStatuses[key].overlayIcon = string.Format("https://m.taladrod.com/assets/icons/{0}.png", CondCarStatus.CondStatus(key));
                }

                AddToCache(Cache_Cond, schCondItem);
                schCondItem.DataSrc = "newQuery";

                return schCondItem;

            }
        }

        public HomeItem getHomeProcude()
        {
            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                using (var result = connection.QueryMultiple(@"w60.home", commandType: CommandType.StoredProcedure))
                {
                    var tbH = result.Read<tbH>().ToList();
                    var tbC = result.Read<tbC>().ToList();
                    var tbM = result.Read<tbM>().ToList();

                    var ret = new HomeItem();
                    ret.newCarCount = tbH[0].nNew;
                    ret.carCount = tbH[0].nTTL;
                    ret.carListDisplayName = tbH[0].CarList;

                    ret.widgetPreviewNew = new List<CarWidgetHome>();
                    for (int i = 0; i < tbC.Count; i++)
                    {
                        var car = new CarWidgetHome();
                        car.cID = tbC[i].CID;
                        car.displayName = tbC[i].model.ToString();
                        // car.previewImage300 = cImg.GetImgUrl("imgc1", tbC.Rows[i]["CIDX"], tbC.Rows[i]["CID"], "1T3");
                        car.displayPrice = string.Format("{0:#,##0}", tbC[i].Baht);

                        ret.widgetPreviewNew.Add(car);
                    }

                    ret.widgetMarketOverviewNew = new List<MakeListItem>();
                    for (int i = 0; i < tbM.Count; i++)
                    {
                        var make = new MakeListItem();
                        make.mkID = tbM[i].mkID;
                        make.displayName = tbM[i].Name;
                        make.makeImage = "https://www.taladrod.com/c/Mk/svg/" + string.Format("{0:000}", make.mkID) + ".svg";
                        make.carCount = tbM[i].nCar;
                        make.carChangeCount = tbM[i].nChg;

                        ret.widgetMarketOverviewNew.Add(make);
                    }

                    return ret;


                }
            }
        }


        private DataSet GetMMTs()
        {
            DataSet dataSet = new DataSet();
            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                using (var result = connection.QueryMultiple(@"w60.getMMT", commandType: CommandType.StoredProcedure))
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
                        dataSet.Tables.Add(dataTable);

                        tableIndex++;

                    }

                    
                    return dataSet;
                }
            }
        }
      
        private List<LModelItem> GetModel(DataSet DS, Int32 MkID)
        {
            var models = new List<LModelItem>();
            DataTable tbMd = DS.Tables[1];
            for (int i = 0; i < tbMd.Rows.Count; i++)
            {
                if (checked((int)tbMd.Rows[i]["MkID"]) != MkID)
                    continue;

                var model = new LModelItem();
                model.id = checked((int)tbMd.Rows[i]["MdID"]);
                model.disp = tbMd.Rows[i]["Name"].ToString();
                model.isHot = Convert.ToInt32(tbMd.Rows[i]["isHot"]) == 1;
                model.bodys = GetBody(DS, model.id);
                model.trims = GetTrim(DS, model.id);

                models.Add(model);
            }

            return models;

        }

        private List<LBodyItem> GetBody(DataSet DS, int MdID)
        {
            var bodys = new List<LBodyItem>();
            DataTable tbBd = DS.Tables[4];
            for (int i = 0; i < tbBd.Rows.Count; i++)
            {
                if (checked((int)tbBd.Rows[i]["MdID"]) != MdID)
                    continue;

                var body = new LBodyItem();
                body.id = checked((int)tbBd.Rows[i]["BdID"]);
                body.disp = tbBd.Rows[i]["Name"].ToString();

                DataRow[] dr = DS.Tables[5].Select("BdID=" + body.id);
                body.trimIds = new List<int>();

                for (int j = 0; j < dr.Length; j++)
                {
                    body.trimIds.Add(checked((int)dr[j]["TaID"]));
                }


                bodys.Add(body);
            }

            return bodys;

        }

        private List<LTrimItem> GetTrim(DataSet DS, int MdID)
        {
            var trims = new List<LTrimItem>();
            DataTable tbTa = DS.Tables[2];
            for (int i = 0; i < tbTa.Rows.Count; i++)
            {
                if (checked((int)tbTa.Rows[i]["MdID"]) != MdID)
                    continue;

                var trim = new LTrimItem();
                trim.id = checked((int)tbTa.Rows[i]["TrID"]);
                trim.disp = tbTa.Rows[i]["Name"].ToString();
                DataRow[] dr = DS.Tables[5].Select("TaID=" + trim.id);
                trim.bodyIds = new List<int>();

                for (int j = 0; j < dr.Length; j++)
                {
                    trim.bodyIds.Add(checked((int)dr[j]["BdID"]));
                }

                trims.Add(trim);
            }

            return trims;

        }

        public void AddToCache(string key, object value)
        {
            _memoryCache.Set(key, value, TimeSpan.FromMinutes(60)); // Cache for 30 minutes
        }

        public object GetFromCache(string key)
        {
            return _memoryCache.TryGetValue(key, out var value) ? value : null;
        }

        public Int32 convertInt(Int16 value)
        {

            Int32 intValue = Int32.Parse(value.ToString());
            return value;
        }



    }
}