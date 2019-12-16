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
    public class HomeController : Controller
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

        //GET DATA CHO CHART PLANT OEE & A, P, Q
        public JsonResult GetDataPlantOEE()
        {
            bool status = false;
            try
            {
                //Proc Name
                string procName = "sp_Chart";
                //Category type
                string category = string.Empty;
                string user = User.Identity.Name;

                List<double> dataPlanOEE = new List<double>();
                List<string> dataTime = new List<string>();

                //Ngày bắt đầu trước ngày hiện tại 1 ngày
                string fromDate = DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");
                string[] paramName = { "v_FROMDATE", "v_TODATE", "v_IID", "v_USERID" };
                //Lấy dữ liệu 5 giờ trước của ngày hiện tại
                for (int i = 5; i >= 0; i--)
                {
                    string toDate = DateTime.Now.AddHours(-i).ToString("yyyy-MM-dd HH:mm:ss");
                    string[] paraValue = { fromDate, toDate, "0", "nnson" };

                    //Dùng DataSet để lấy dữ liệu dưới store procedure
                    DataSet _dataSet = dataOperation.GetDataSet(GlobalVariable.DBOEE, procName, paramName, paraValue);
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
                        if (category != "Sample" && category != "Logo")
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
                    double result = OEE * 100;
                    dataPlanOEE.Add(Math.Round(result, 2));

                    string time = DateTime.Now.AddHours(-i).ToString("h:mm tt");
                    dataTime.Add(time);
                }

                ViewBag.Data = dataPlanOEE;
                ViewBag.Time = dataTime;
            }
            catch (Exception ex)
            {
                ex.ToString();
                throw;
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        //GET DATA CHO CHART MAIN PRODUCTION
        public JsonResult GetDataMainPro()
        {
            bool status = false;
            //Ngày bắt đầu trước ngày hiện tại 1 ngày
            string fromDate = DateTime.Now.Date.AddDays(-2).ToString("yyyy-MM-dd HH:mm:ss");
            //Ngày kết thúc là lấy thời gian hiện tại
            string toDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //Store Proceudure Name
            string procName = "sp_Chart";
            string category = string.Empty;
            string user = User.Identity.Name;

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

            //double OEE = 0;

            string[] paramName = { "v_FROMDATE", "v_TODATE", "v_IID", "v_USERID" };
            string[] paraValue = { fromDate, toDate, "0", user };

            try
            {
                DataSet _dataSet = dataOperation.GetDataSet(GlobalVariable.DBOEE, procName, paramName, paraValue);
                DataTable dt1 = _dataSet.Tables[0];
                List<DataPoint> dataPointsMainProd = new List<DataPoint>();
                var list = dt1.Select().AsQueryable().ToList();
                //ViewBag.dataPointsLine = DataTableToJsonObj(dt1);

                #region Chart Availability- Performance-Quantity
                foreach (var item in list)
                {
                    category = item.ItemArray[5].ToString();

                    //Vì theo y/c của Ms.Quyên không lấy những Category có tên là Sample và Logo
                    if (category != "Sample" && category != "Logo")
                    {
                        //Get data from list array
                        OTz4 = mFunction.ToDouble(item.ItemArray[0].ToString());
                        PPT3 = mFunction.ToDouble(item.ItemArray[1].ToString());
                        NOT4 = mFunction.ToDouble(item.ItemArray[2].ToString());
                        QCG = mFunction.ToDouble(item.ItemArray[3].ToString());
                        B_Output = mFunction.ToDouble(item.ItemArray[4].ToString());

                        //Count sum data
                        sumOTz4 += OTz4;
                        sumPPT3 += PPT3;
                        sumNOT4 += NOT4;
                        sumQCG += QCG;
                        sumBOutput += B_Output;

                        status = true;
                    }

                    //Vì theo y/c của Ms.Quyên không lấy những Category có tên là Sample, Logo và Heel Counters
                    if (category != "Sample" && category != "Logo" && category != "Heel Counters")
                    {
                        //Get data from list array
                        OTz4 = mFunction.ToDouble(item.ItemArray[0].ToString());
                        PPT3 = mFunction.ToDouble(item.ItemArray[1].ToString());
                        NOT4 = mFunction.ToDouble(item.ItemArray[2].ToString());
                        QCG = mFunction.ToDouble(item.ItemArray[3].ToString());
                        B_Output = mFunction.ToDouble(item.ItemArray[4].ToString());

                        //Count sum data
                        sumOTz4_MainProd += OTz4;
                        sumPPT3_MainProd += PPT3;
                        sumNOT4_MainProd += NOT4;
                        sumQCG_MainProd += QCG;
                        sumBOutput_MainProd += B_Output;

                        status = true;
                    }
                }

                #region Calculate SUM 
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

                #region Calculate SUM Main Production
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
                #endregion

                //OEE = Q_MainProd * P_MainProd * Q_MainProd;
                //if (OEE != null)
                //{
                //    ViewBag.OEE = Math.Round(OEE, 2);
                //}
                dataPointsMainProd.Add(new DataPoint("A", A_MainProd, "A"));
                dataPointsMainProd.Add(new DataPoint("P", P_MainProd, "P"));
                dataPointsMainProd.Add(new DataPoint("Q", Q_MainProd, "Q"));
                ViewBag.DataPointsColumns = JsonConvert.SerializeObject(dataPointsMainProd);
                #endregion

            }
            catch (Exception ex)
            {
                ex.ToString();
                throw;
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        //GET DATA CHO CHART HEEL COUNTER
        public JsonResult GetDataHC()
        {
            bool status = false;
            //Proc Name
            string procName = "OEE16_Chart";
            //Category type
            string category = string.Empty;
            string user = User.Identity.Name;

            //Data chart 
            string nameArburg = "arburg";
            string nameEngel‎ = "engel‎‎";

            double arburg    = 0;
            double engel     = 0;
            double twn       = 0;
            double sumArburg = 0;
            double sumEngel‎ = 0;
            double sumTWN‎   = 0;

            double avgArburg = 0;
            double avgEngel‎ = 0;
            double avgTWN‎ = 0;

            int i = 0;
            int j = 0;
            int z = 0;

            //Ngày bắt đầu là ngày trước ngày hiện tại 12 giờ
            string fromDate = DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");
            //Ngày kết thúc là lấy thời gian hiện tại
            string toDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string[] paramName = { "v_FROMDATE", "v_TODATE", "V_CATEGORYID", "v_USERID" };
            string[] paraValue = { fromDate, toDate, "001", "nnson"};
            try
            {
                DataSet _dataSet = dataOperation.GetDataSet(GlobalVariable.DBOEE, procName, paramName, paraValue);
                DataTable dt1 = _dataSet.Tables[0];
                List<DataPoint> dataPointsHC = new List<DataPoint>();
              
                var list = dt1.Select().AsQueryable().ToList();
                foreach (var item in list)
                {
                    category = item.ItemArray[0].ToString().Trim().ToLower();
                    if (category.Contains(nameArburg))
                    {
                        arburg = mFunction.ToDouble(item.ItemArray[1].ToString());
                        sumArburg += arburg;
                        i++;
                    }
                    else if (category.Contains(nameEngel‎) || category.Contains("(engel)"))
                    {
                        engel = mFunction.ToDouble(item.ItemArray[1].ToString());
                        sumEngel += engel;
                        j++;
                    }
                    else if (!category.Contains(nameArburg) && !category.Contains(nameEngel‎))
                    {
                        twn = mFunction.ToDouble(item.ItemArray[1].ToString());
                        sumTWN += twn;
                        z++;
                    }

                }
                avgArburg = sumArburg / i;
                avgEngel  = sumEngel / j;
                avgTWN    = sumTWN / z;
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
            catch (Exception ex)
            {
                ex.ToString();
                throw;
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }
    }
}