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
            GetDataMainPro();
            GetDataPlantOEE();
            GetDataHC();
            return View();
        }

        //GET DATA CHO CHART Main Production
        public JsonResult GetDataMainPro()
        {
            bool status = false;
            //Ngày bắt đầu trước ngày hiện tại 1 ngày
            string v_FROMDATE = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd HH:mm:ss");
            //Ngày kết thúc là lấy thời gian hiện tại
            string v_TODATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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

            double OEE = 0;

            string[] paramName = { "v_FROMDATE", "v_TODATE", "v_IID", "v_USERID" };
            string[] paraValue = { v_FROMDATE, v_TODATE, "0", user };


            try
            {
                DataSet _dataSet = dataOperation.GetDataSet(GlobalVariable.DBOEE, procName, paramName, paraValue);
                DataTable dt1 = _dataSet.Tables[0];
                List<DataPoint> dataPointsColumns = new List<DataPoint>();
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
                        status = true;
                    }

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
                        status = true;
                    }
                }
                OEE = Q_MainProd * P_MainProd * Q_MainProd;
                if (OEE != null)
                {
                    ViewBag.OEE = Math.Round(OEE, 2);
                }
                dataPointsColumns.Add(new DataPoint("A", A_MainProd, "A"));
                dataPointsColumns.Add(new DataPoint("P", P_MainProd, "P"));
                dataPointsColumns.Add(new DataPoint("Q", Q_MainProd, "Q"));
                ViewBag.dataPointsColumns = JsonConvert.SerializeObject(dataPointsColumns);
                #endregion

            }
            catch (Exception ex)
            {
                ex.ToString();
                throw;
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }

        //GET DATA CHO CHART Plant OEE
        public JsonResult GetDataPlantOEE()
        {
            bool status = false;
            //Proc Name
            string procName = "sp_Chart";
            //Category type
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

            double A = 0;
            double P = 0;
            double Q = 0;

            double OEE = 0;

            List<int> dataPlanOEE = new List<int>();

            //Ngày bắt đầu trước ngày hiện tại 1 ngày
            string v_FROMDATE = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");
            string[] paramName = { "v_FROMDATE", "v_TODATE", "v_IID", "v_USERID" };
            try
            {
                //Lấy dữ liệu 5 giờ trước
                for (int i = 5; i > 0; i--)
                {
                    string v_TODATE = DateTime.Now.AddHours(-i).ToString("yyyy-MM-dd HH:mm:ss");
                    string[] paraValue = { v_FROMDATE, v_TODATE, "0", user };

                    //Dùng DataSet để lấy dữ liệu dưới store procedure
                    DataSet _dataSet = dataOperation.GetDataSet(GlobalVariable.DBOEE, procName, paramName, paraValue);
                    //Dùng DataTable để lấy dữ liệu table 1 từ data set
                    DataTable dataTable = _dataSet.Tables[0];
                    //Convert về từ datatable về kiểu list
                    var list = dataTable.Select().AsQueryable().ToList();

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

                            if (sumOTz4 != null)
                            {
                                A = sumOTz4 / sumPPT3 * 100;
                                if (Double.IsNaN(A) || Double.IsInfinity(A))
                                {
                                    A = 0;
                                }
                            }
                            if (sumNOT4 != null)
                            {
                                P = sumNOT4 / sumOTz4 * 100;
                                if (Double.IsNaN(P) || Double.IsInfinity(P))
                                {
                                    P = 0;
                                }
                            }
                            if (sumQCG != null)
                            {
                                Q = sumQCG / sumBOutput * 100;
                                if (Double.IsNaN(Q) || Double.IsInfinity(Q))
                                {
                                    Q = 0;
                                }
                            }

                        }
                    }
                    OEE = A * P * Q;
                    Console.WriteLine(OEE);
                    dataPlanOEE.Add((int)Math.Ceiling((double)OEE));
                }
                ViewBag.Data = dataPlanOEE;
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

            //Ngày bắt đầu trước ngày hiện tại 1 ngày
            string v_FROMDATE = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd HH:mm:ss");
            //Ngày kết thúc là lấy thời gian hiện tại
            string v_TODATE = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");
            string[] paramName = { "v_FROMDATE", "v_TODATE", "V_CATEGORYID", "v_USERID" };
            string[] paraValue = { v_FROMDATE, v_TODATE, "001", user };
            try
            {
                DataSet _dataSet = dataOperation.GetDataSet(GlobalVariable.DBOEE, procName, paramName, paraValue);
                DataTable dt1 = _dataSet.Tables[0];
                List<DataPoint> dataPointsColumns = new List<DataPoint>();
                var list = dt1.Select().AsQueryable().ToList();
                foreach (var item in list)
                {

                }
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