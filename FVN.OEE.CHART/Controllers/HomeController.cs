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

        #endregion Variabal connect string

        public ActionResult Index()
        {
            string v_FROMDATE = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");
            string v_TODATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string procName = "sp_Chart";
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

            string[] paramName = { "v_FROMDATE", "v_TODATE", "v_IID", "v_USERID" };
            string[] paraValue = { v_FROMDATE, v_TODATE, "0", "nnson" };
            DataSet _dataSet = dataOperation.GetDataSet(GlobalVariable.DBOEE, procName, paramName, paraValue);
            DataTable dt1 = _dataSet.Tables[0];
            List<DataPoint> dataPointsColumns = new List<DataPoint>();
            var list = dt1.Select().AsQueryable().ToList();
            //ViewBag.dataPointsLine = DataTableToJsonObj(dt1);
            foreach (var item in list)
            {
                //Get data from list array
                OTz4     =mFunction.ToInt(item.ItemArray[0].ToString());
                PPT3     = mFunction.ToInt(item.ItemArray[1].ToString());
                NOT4     = mFunction.ToInt(item.ItemArray[2].ToString());
                QCG      = mFunction.ToInt(item.ItemArray[3].ToString());
                B_Output = mFunction.ToInt(item.ItemArray[4].ToString());

                //Count sum data
                sumOTz4 += OTz4;
                sumPPT3 += PPT3;
                sumNOT4 += NOT4;
                sumQCG += QCG;
                sumBOutput += B_Output;
            }

            A = (100 * sumOTz4) / sumPPT3;
            P = (100 * sumNOT4) / sumOTz4;
            Q = (100 * sumQCG) / sumBOutput;
            dataPointsColumns.Add(new DataPoint("A", A, "A"));
            dataPointsColumns.Add(new DataPoint("P", P, "P"));
            dataPointsColumns.Add(new DataPoint("Q", Q, "Q"));

            ViewBag.dataPointsColumns = JsonConvert.SerializeObject(dataPointsColumns);

            ViewBag.A = A;
            ViewBag.P = P;
            ViewBag.Q = Q;

            return View();
        }

        //Func Convert DataTable to Json
        public string DataTableToJsonObj(DataTable dt)
        {
            DataSet ds = new DataSet();
            ds.Merge(dt);
            StringBuilder JsonString = new StringBuilder();
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                JsonString.Append("[");
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    JsonString.Append("{");
                    for (int j = 0; j < ds.Tables[0].Columns.Count; j++)
                    {
                        if (j < ds.Tables[0].Columns.Count - 1)
                        {
                            JsonString.Append("\"" + ds.Tables[0].Columns[j].ColumnName.ToString() + "\":" + "\"" + ds.Tables[0].Rows[i][j].ToString() + "\",");
                        }
                        else if (j == ds.Tables[0].Columns.Count - 1)
                        {
                            JsonString.Append("\"" + ds.Tables[0].Columns[j].ColumnName.ToString() + "\":" + "\"" + ds.Tables[0].Rows[i][j].ToString() + "\"");
                        }
                    }
                    if (i == ds.Tables[0].Rows.Count - 1)
                    {
                        JsonString.Append("}");
                    }
                    else
                    {
                        JsonString.Append("},");
                    }
                }
                JsonString.Append("]");
                return JsonString.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}