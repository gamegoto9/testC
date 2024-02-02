using System.Data;
using Dapper;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Data
{
    public class CarDetailRepository
    {
        IConfiguration _config;
        public CarDetailRepository(IConfiguration config)
        {
            _config = config;

        }
        public object carDet(int Cid)
        {
            DataSet DS = new DataSet();
            var tableIndex = 0;
            var cardet = new CarDetItem();

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("@CID", Cid, DbType.Int32, ParameterDirection.Input);

                using (var result = connection.QueryMultiple("w60.GetCarDet", parameters, commandType: CommandType.StoredProcedure))
                {


                    while (!result.IsConsumed)
                    {
                        List<object> resultList = result.Read<object>().ToList();
                        DataTableHelper.AddListToDataSet(DS, resultList);
                        tableIndex++;
                    }
                    connection.Close();
                }

               AccessDataSetValues(DS);

                if (DS.Tables.Count > 0 && DS.Tables[0].Rows.Count > 0)
                {
                    DataTable tbD = DS.Tables[0];
                    if (DS.Tables.Count == 1)
                    {
                        return Models.MCommand.ToJSONSingleRow(tbD);
                    }
                    DataTable tb1CMd = DS.Tables[1];
                    DataTable tb1CBd = DS.Tables[2];
                    DataTable tb1CTa = DS.Tables[3];
                    DataTable tbCGrp = DS.Tables[4];
                    DataTable tbChkP = DS.Tables[5];

                    var dr = tbD.Rows[0];
                    cardet.cID = Convert.ToInt32(dr["Cid".ToString()]);
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
                    // cardet.post.displayDiscountText = "ผู้ขายลดเหลือ " + string.Format("{0:#,##0}", dr["PrcTBx"]) + @"\r\nถ้าตกลงซื้อภายใน " + cUtils.StripTags(cUtils.DtTBx(dr["dtTbx"]));
                    cardet.post.displayDiscountText = "ผู้ขายลดเหลือ " + string.Format("{0:#,##0}", dr["PrcTBx"]) + @"\r\nถ้าตกลงซื้อภายใน ";

                    cardet.widgetFinance = new CarFinanceItem();
                    cardet.widgetFinance.defaultInterest = Convert.ToInt32(dr["eFin_Int"]);
                    
                    cardet.widgetFinance.durationMaximum = Convert.ToInt32(dr["eFin_Mth"]);
                    cardet.widgetFinance.interestRate48 = Convert.ToInt32(dr["eFin_Int48"]);
                    cardet.widgetFinance.interestRate60 = Convert.ToInt32(dr["eFin_Int60"]);
                    cardet.widgetFinance.interestRate72 = Convert.ToInt32(dr["eFin_Int72"]);

                    // cardet.post.displayTitle = cUtils.RpText(dr["Title"].ToString());
                    // cardet.post.displayDescription = cUtils.RpText(dr["Descr"].ToString());
                    // cardet.post.displayCarFinanceDescription = cUtils.RpText(dr["DetCF"].ToString());

                    cardet.post.imageList = new List<string>();

                    if ((int)dr["imgCnt"] > 0)
                    {
                        ushort[] bits = new ushort[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };

                        for (int i = 1; i <= bits.Length; i++)
                        {
                            if ((bits[i - 1] & (int)dr["imgCnt"]) > 0)
                            {
                                // cardet.post.imageList.Add(cImg.GetImgUrl("img1", dr["CIDX"], dr["CID"], i.ToString()));
                            }
                        }
                    }

                    cardet.post.favoriteCount = Convert.ToInt32(dr["n_Fav"]);
                    cardet.post.followerCount = Convert.ToInt32(dr["n_Alarm"]);

                    cardet.post.warranty = new CarWarrantyItem();
                    int wrtItem = Convert.ToInt32(dr["wrtItem"]);
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
                            year = Convert.ToInt32(tbChkP.Rows[i]["Yr"]),
                            avgPrice = Convert.ToInt32(tbChkP.Rows[i]["avgPrc"]),
                            minPrice = Convert.ToInt32(tbChkP.Rows[i]["minPrc"]),
                            maxPrice = Convert.ToInt32(tbChkP.Rows[i]["maxPrc"]),
                            carCount = Convert.ToInt32(tbChkP.Rows[i]["nCar"]),
                            inputParameters = new InputParametersWithYr()
                            {
                                Yr1 = Convert.ToInt32(tbChkP.Rows[i]["Yr"]),
                                Yr2 = Convert.ToInt32(tbChkP.Rows[i]["Yr"]),
                                Fno = "all",
                                MkID = Convert.ToInt32(dr["MkID"]),
                                MdID = Convert.ToInt32(dr["MdID"]),
                                BdID = Convert.ToInt32(dr["BdID"])
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
                        if (mdr.Length == 0 || Convert.ToInt32(mdr[0]["nCar"]) == 0)
                        {
                            cardet.widgetSameModel.differentFunction.Add(mdkeys[i], null);
                            continue;
                        }

                        for (int j = 0; j < mdr.Length; j++)
                        {
                            if (Convert.ToInt32(mdr[j]["BdID"]) > 0) continue;
                            if (Convert.ToInt32(mdr[j]["TaID"]) > 0) continue;
                            cardet.widgetSameModel.differentFunction.Add(mdkeys[i], new CarDiffItem()
                            {
                                displayName = mdr[j]["Text"].ToString(),
                                carCount = Convert.ToInt32(mdr[j]["nCar"]),
                                inputParameters = new InputParameters()
                                {
                                    Fno = mdfnos[i],
                                    MkID = Convert.ToInt32(dr["MkID"]),
                                    MdID = Convert.ToInt32(dr["MdID"])
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
                            carCount = Convert.ToInt32(bdr[j]["nCar"]),
                            inputParameters = new InputParameters()
                            {
                                Fno = "all",
                                MkID = Convert.ToInt32(dr["MkID"]),
                                MdID = Convert.ToInt32(dr["MdID"]),
                                BdID = Convert.ToInt32(bdr[j]["BdID"])
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
                            carCount = Convert.ToInt32(tdr[j]["nCar"]),
                            inputParameters = new InputParameters()
                            {
                                Fno = "all",
                                MkID = Convert.ToInt32(dr["MkID"]),
                                MdID = Convert.ToInt32(dr["MdID"]),
                                BdID = Convert.ToInt32(tdr[j]["TaID"])
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
                            cID = Convert.ToInt32(tbCGrp.Rows[i]["Cid"]),
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
                            nMkID = Convert.ToInt32(tbCGrp.Rows[i]["MkID"]);
                            nMdID = Convert.ToInt32(tbCGrp.Rows[i]["MdID"]);
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
                    
                }

                // cardet.user = new CarUsrItem();
                // if (IsFavCar(cConvert.ToInt(Cid)))
                // {
                //     cardet.user.isFavorite = true;
                // }

                return cardet;
            }
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

        static public string MapingBodyNm(string Name)
        {
            Dictionary<string, string> mappingBd = new Dictionary<string, string>();
            mappingBd.Add("DOUBLE CAB", "DBL CAB");

            if (mappingBd.ContainsKey(Name))
                return mappingBd[Name];

            return Name;
        }
    }
}