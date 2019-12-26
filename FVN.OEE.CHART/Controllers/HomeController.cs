using FramasVietNam.Common;
using FVN.OEE.CHART.Commons;
using FVN.OEE.CHART.Models;
using FVN.OEE.CHART.Models.OEE;
using FVN.OEE.CHART.Models.VNT86;
using FVN.OEE.CHART.ViewModels;
using Lime.Protocol.Network;
using MSharp.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Mvc;
namespace FVN.OEE.CHART.Controllers
{
    public  class HomeController : Controller
    {
        #region Variabal connect string
        private OEEEntities db_OEE = new OEEEntities();
        #endregion Variabal connect string\

        public ActionResult Index()
        {
            GetDataPlantOEE();
            GetDataMainPro();
            GetDataHC();
            return View();
        }

        //GET DATA CHART PLANT OEE & A, P, Q
        public JsonResult GetDataPlantOEE()
       {
            bool status = false;
            try
            {
                //Proc Name
                string procName = "sp_EXP_RAWDATA_CHART";
                //Category type
                string category = string.Empty;
                string user = User.Identity.Name;

                List<double> dataPlanOEE = new List<double>();
                List<string> dataTime = new List<string>();

                string[] paramName = { "v_FROMDATE", "v_TODATE", "v_IID", "v_USERID" };
                //Lấy dữ liệu 5 giờ trước của ngày hiện tại
                for (int i = 5; i >= 0; i--)
                {
                    //Thời gian bắt đầu tính từ thời gian hiện tại trừ đi 6h
                    string fromDate = DateTime.Now.AddHours(-(i + 6)).ToString("yyyy-MM-dd HH:mm:ss");
                    string toDate = DateTime.Now.AddHours(-i).ToString("yyyy-MM-dd HH:mm:ss");
                    string[] paraValue = { fromDate, toDate, "0", "nnson" };

                    //Dùng DataSet để lấy dữ liệu dưới store procedure
                    DataSet _dataSet = dataOperation.GetDataSet(GlobalVariable.DBOEE, procName, paramName, paraValue);
                    if (_dataSet.Tables.Count > 0)
                    {
                        //Dùng DataTable để lấy dữ liệu table 1 từ data set
                        DataTable dataTable = _dataSet.Tables[0];
                        //Convert về từ datatable về kiểu list
                        var list = dataTable.Select().AsQueryable().ToList();
                        double OTz4 = 0;
                        double PPT3 = 0;
                        double NOT4 = 0;
                        double QCG = 0;
                        double B_Output = 0;

                        double sumOTz4 = 0;
                        double sumPPT3 = 0;
                        double sumNOT4 = 0;
                        double sumQCG = 0;
                        double sumBOutput = 0;

                        double A = 0;
                        double P = 0;
                        double Q = 0;

                        foreach (var item in list)
                        {
                            category = item.ItemArray[5].ToString();

                            //Vì theo y/c của Ms.Quyên không lấy những Category có tên là Sample và Logo
                            if (!category.Contains("Sample")  && !category.Contains("Logo"))
                            {
                                //Get data from list array
                                OTz4 = Math.Round(mFunction.ToDouble(item.ItemArray[0].ToString()), 2);
                                PPT3 = Math.Round(mFunction.ToDouble(item.ItemArray[1].ToString()), 2);
                                NOT4 = Math.Round(mFunction.ToDouble(item.ItemArray[2].ToString()), 2);
                                QCG = Math.Round(mFunction.ToDouble(item.ItemArray[3].ToString()), 2);
                                B_Output = Math.Round(mFunction.ToDouble(item.ItemArray[4].ToString()), 2);

                                //Count sum data
                                sumOTz4 += Math.Round(OTz4, 2);
                                sumPPT3 += Math.Round(PPT3, 2);
                                sumNOT4 += Math.Round(NOT4, 2);
                                sumQCG += Math.Round(QCG, 2);
                                sumBOutput += Math.Round(B_Output, 2);
                            }
                        }

                        A = Math.Round((sumOTz4 / sumPPT3 * 100), 2);
                        if (Double.IsNaN(A) || Double.IsInfinity(A))
                        {
                            A = 0;
                        }
                        P = Math.Round((sumNOT4 / sumOTz4 * 100), 2);
                        if (Double.IsNaN(P) || Double.IsInfinity(P))
                        {
                            P = 0;
                        }
                        Q = Math.Round((sumQCG / sumBOutput * 100), 2);
                        if (Double.IsNaN(Q) || Double.IsInfinity(Q))
                        {
                            Q = 0;
                        }
                        double OEE = (ConvertUtility.ConvertPercen(A) * ConvertUtility.ConvertPercen(P) * ConvertUtility.ConvertPercen(Q));
                        OEE = OEE * 100;
                        dataPlanOEE.Add(Math.Round(OEE, 2));

                        string time = DateTime.Now.AddHours(-i).ToString("h:mm tt");
                        dataTime.Add(time);
                    }
                }

                ViewBag.Data = dataPlanOEE;
                ViewBag.Time = dataTime;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        //GET DATA CHART MAIN PRODUCTION
        public JsonResult GetDataMainPro()
        {
            bool  status     = false;
            //Ngày bắt đầu trước ngày hiện tại 1 ngày
            string fromDate = DateTime.Now.AddHours(-6).ToString("yyyy-MM-dd HH:mm:ss");
            //Ngày kết thúc là lấy thời gian hiện tại
            string toDate   = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //Store Proceudure Name
            string procName = "sp_EXP_RAWDATA_CHART";
            string category = string.Empty;
            string user     = User.Identity.Name;

            double  sumOTz4 = 0;
            double sumPPT3 = 0;
            double sumNOT4 = 0;
            double sumQCG = 0;
            double sumBOutput = 0;

            double sumOTz4_MainProd = 0;
            double sumPPT3_MainProd = 0;
            double sumNOT4_MainProd = 0;
            double sumQCG_MainProd = 0;
            double sumBOutput_MainProd = 0;

            double A = 0;
            double P = 0;
            double Q = 0;

            double A_MainProd = 0;
            double P_MainProd = 0;
            double Q_MainProd = 0;

            double OEE = 0;

            string[] paramName = { "v_FROMDATE", "v_TODATE", "v_IID", "v_USERID" };
            string[] paraValue = { fromDate, toDate, "0", user };

            #region HEEL COUNTER
            double A_HC = 0;
            double P_HC = 0;
            double Q_HC = 0;

            double sumOTz4_HC = 0;
            double sumPPT3_HC = 0;
            double sumNOT4_HC = 0;
            double sumQCG_HC = 0;
            double sumBOutput_HC = 0;
            double OEE_HC = 0;
            #endregion

            try
            {
                DataSet _dataSet = dataOperation.GetDataSet(GlobalVariable.DBOEE, procName, paramName, paraValue);
                if (_dataSet.Tables.Count > 0)
                {
                    DataTable dt1 = _dataSet.Tables[0];
                    List<DataPoint> dataPointsMainProd = new List<DataPoint>();
                    var list = dt1.Select().AsQueryable().ToList();
                    //ViewBag.dataPointsLine = DataTableToJsonObj(dt1);
                    foreach (var item in list)
                    {
                        category = item.ItemArray[5].ToString();

                        ///Vì theo y/c của Ms.Quyên không lấy những Category có tên là Sample và Logo
                        ///Get data
                        if (!category.Contains("Sample") && !category.Contains("Logo"))
                        {
                            //Count sum data
                            sumOTz4 += mFunction.ToDouble(item.ItemArray[0].ToString()); ;
                            sumPPT3 += mFunction.ToDouble(item.ItemArray[1].ToString()); ;
                            sumNOT4 += mFunction.ToDouble(item.ItemArray[2].ToString()); ;
                            sumQCG += mFunction.ToDouble(item.ItemArray[3].ToString()); ;
                            sumBOutput += mFunction.ToDouble(item.ItemArray[4].ToString()); ;

                            status = true;
                        }

                        ///Vì theo y/c của Ms.Quyên không lấy những Category có tên là Sample, Logo và Heel Counters
                        if (!category.Contains("Sample") && !category.Contains("Logo") && !category.Contains("Heel Counters"))
                        {
                            //Count sum data
                            sumOTz4_MainProd += mFunction.ToDouble(item.ItemArray[0].ToString());
                            sumPPT3_MainProd += mFunction.ToDouble(item.ItemArray[1].ToString());
                            sumNOT4_MainProd += mFunction.ToDouble(item.ItemArray[2].ToString());
                            sumQCG_MainProd += mFunction.ToDouble(item.ItemArray[3].ToString());
                            sumBOutput_MainProd += mFunction.ToDouble(item.ItemArray[4].ToString());
                            status = true;
                        }

                        //Get data OEE cho chart heel counter
                        if (category.Contains("Heel Counters") || category.Contains("Heel Counter"))
                        {
                            //Count sum data
                            sumOTz4_HC += mFunction.ToDouble(item.ItemArray[0].ToString()); ;
                            sumPPT3_HC += mFunction.ToDouble(item.ItemArray[1].ToString());
                            sumNOT4_HC += mFunction.ToDouble(item.ItemArray[2].ToString());
                            sumQCG_HC += mFunction.ToDouble(item.ItemArray[3].ToString());
                            sumBOutput_HC += mFunction.ToDouble(item.ItemArray[4].ToString());
                        }
                    }

                    #region Calculate SUM A,P, Q
                    if (sumOTz4 != null)
                    {
                        A = sumOTz4 / sumPPT3 * 100;
                        if (Double.IsNaN(A) || Double.IsInfinity(A))
                        {
                            A = 0;
                        }
                        ViewBag.Availability = Math.Round(A, 2);
                    }
                    if (sumNOT4 != null)
                    {
                        P = sumNOT4 / sumOTz4 * 100;
                        if (Double.IsNaN(P) || Double.IsInfinity(P))
                        {
                            P = 0;
                        }
                        ViewBag.Performance = Math.Round(P, 2);
                    }
                    if (sumQCG != null)
                    {
                        Q = sumQCG / sumBOutput * 100;
                        if (Double.IsNaN(Q) || Double.IsInfinity(Q))
                        {
                            Q = 0;
                        }
                        ViewBag.Quality = Math.Round(Q, 2);
                    }
                    #endregion

                    #region Calculate SUM OEE HEEL COUNTER
                    if (sumOTz4 != null)
                    {
                        A_HC = sumOTz4_HC / sumPPT3_HC * 100;
                        if (Double.IsNaN(A_HC) || Double.IsInfinity(A_HC))
                        {
                            A_HC = 0;
                        }
                    }
                    if (sumNOT4_HC != null)
                    {
                        P_HC = sumNOT4_HC / sumOTz4_HC * 100;
                        if (Double.IsNaN(P_HC) || Double.IsInfinity(P_HC))
                        {
                            P_HC = 0;
                        }
                    }
                    if (sumQCG_HC != null)
                    {
                        Q_HC = sumQCG_HC / sumBOutput_HC * 100;
                        if (Double.IsNaN(Q_HC) || Double.IsInfinity(Q_HC))
                        {
                            Q_HC = 0;
                        }
                    }

                    OEE_HC = (ConvertUtility.ConvertPercen(A_HC) * ConvertUtility.ConvertPercen(P_HC) * ConvertUtility.ConvertPercen(Q_HC));
                    OEE_HC = OEE_HC * 100;
                    if (OEE_HC != null)
                    {
                        ViewBag.OEE_HC = Math.Round(OEE_HC, 2);
                    }
                    #endregion

                    #region Calculate SUM OEE Main Production
                    if (sumOTz4_MainProd != null)
                    {
                        A_MainProd = sumOTz4_MainProd / sumPPT3_MainProd * 100;
                        if (Double.IsNaN(A_MainProd) || Double.IsInfinity(A_MainProd))
                        {
                            A_MainProd = 0;
                        }
                    }
                    if (sumNOT4_MainProd != null)
                    {
                        P_MainProd = sumNOT4_MainProd / sumOTz4_MainProd * 100;
                        if (Double.IsNaN(P_MainProd) || Double.IsInfinity(P_MainProd))
                        {
                            P_MainProd = 0;
                        }
                    }
                    if (sumQCG_MainProd != null)
                    {
                        Q_MainProd = sumQCG_MainProd / sumBOutput_MainProd * 100;
                        if (Double.IsNaN(Q_MainProd) || Double.IsInfinity(Q_MainProd))
                        {
                            Q_MainProd = 0;
                        }
                    }

                    dataPointsMainProd.Add(new DataPoint("A", A_MainProd, "A"));
                    dataPointsMainProd.Add(new DataPoint("P", P_MainProd, "P"));
                    dataPointsMainProd.Add(new DataPoint("Q", Q_MainProd, "Q"));
                    ViewBag.DataPointsColumns = JsonConvert.SerializeObject(dataPointsMainProd);

                    OEE = (ConvertUtility.ConvertPercen(A_MainProd) * ConvertUtility.ConvertPercen(P_MainProd) * ConvertUtility.ConvertPercen(Q_MainProd));
                    OEE = OEE * 100;
                    if (OEE != null)
                    {
                        ViewBag.OEE = Math.Round(OEE, 2);
                    }
                    #endregion

                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        //GET DATA CHART HEEL COUNTER
        public JsonResult GetDataHC()
        {
            bool status = false;
            //Proc Name
            string procName = "sp_OEE16_CHART";
            //Category type
            string category = string.Empty;
            string user = User.Identity.Name;

            //Data chart 
            string nameArburg = "arburg";
            string nameEngel‎ = "engel‎‎";

            double sumArburg = 0;
            double sumEngel‎ = 0;
            double sumTWN‎   = 0;

            double avgArburg = 0;
            double avgEngel‎ = 0;
            double avgTWN‎ = 0;

            int i = 0;
            int j = 0;
            int z = 0;

            //Thời gian bắt đầu tính từ thời gian hiện tại trừ đi 6h
            string fromDate = DateTime.Now.AddHours(-6).ToString("yyyy-MM-dd HH:mm:ss");
            //Ngày kết thúc là lấy thời gian hiện tại
            string toDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string[] paramName = { "v_FROMDATE", "v_TODATE", "V_CATEGORYID", "v_USERID" };
            string[] paraValue = { fromDate, toDate, "001", "nnson"};
            try
            {
                DataSet _dataSet = dataOperation.GetDataSet(GlobalVariable.DBOEE, procName, paramName, paraValue);
                if (_dataSet.Tables.Count > 0)
                {
                    DataTable dt1 = _dataSet.Tables[0];
                    List<DataPoint> dataPointsHC = new List<DataPoint>();
                    var list = dt1.Select().AsQueryable().ToList();
                    foreach (var item in list)
                    {
                        category = item.ItemArray[0].ToString().Trim().ToLower();
                        if (category.Contains(nameArburg))
                        {
                            sumArburg += mFunction.ToDouble(item.ItemArray[1].ToString()); ;
                            i++;
                        }
                        else if (category.Contains(nameEngel‎) || category.Contains("(engel)"))
                        {
                            sumEngel += mFunction.ToDouble(item.ItemArray[1].ToString()); ;
                            j++;
                        }
                        else if (!category.Contains(nameArburg) && !category.Contains(nameEngel‎))
                        {
                            sumTWN += mFunction.ToDouble(item.ItemArray[1].ToString()); ;
                            z++;
                        }

                    }
                    avgArburg = sumArburg / i;
                    avgEngel = sumEngel / j;
                    avgTWN = sumTWN / z;
                    if (Double.IsNaN(avgArburg) || Double.IsInfinity(avgArburg))
                    {
                        avgArburg = 0;
                    }
                    if (Double.IsNaN(avgEngel) || Double.IsInfinity(avgEngel))
                    {
                        avgEngel = 0;
                    }
                    if (Double.IsNaN(avgTWN) || Double.IsInfinity(avgTWN))
                    {
                        avgTWN = 0;
                    }
                    dataPointsHC.Add(new DataPoint("EGL", avgEngel, "EGL"));
                    dataPointsHC.Add(new DataPoint("ABG", avgArburg, "ABG"));
                    dataPointsHC.Add(new DataPoint("TWN", avgTWN, "TWN"));
                    ViewBag.DataPointsHC = JsonConvert.SerializeObject(dataPointsHC);
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }
    }
}