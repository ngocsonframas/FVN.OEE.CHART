using FVN.OEE.CHART.Commons;
using FVN.OEE.CHART.Models.OEE;
using FVN.OEE.CHART.Models.VNT86;
using FVN.OEE.CHART.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FVN.OEE.CHART.Controllers
{
    public class HomeController : Controller
    {
        #region Variabal connect string
        private VNT86Entities db_VNT86 = new VNT86Entities();
        private OEEEntities db_OEE = new OEEEntities();
        #endregion

        public ActionResult Index(string productCode)
        {
            //Passing data to view
            productCode = "6111011803";
            var data = db_VNT86.sp_FinanceDAByProduct(productCode).ToList();
           // var data1 = db_OEE.OEE1v2(DateTime.Now, DateTime.Now, "003", "01", "nnson", 1).ToList();

            List<DataPoint> dataPointsLine = new List<DataPoint>();
            List<DataPoint> dataPointsColumns = new List<DataPoint>();

            foreach (var item in data)
            {
                dataPointsLine.Add(new DataPoint(item.C032, mFunction.ToDouble(item.AmoQty)));
                dataPointsColumns.Add(new DataPoint(item.C032, mFunction.ToDouble(item.Qty)));
            }
            ViewBag.dataPointsLine = JsonConvert.SerializeObject(dataPointsLine);
            ViewBag.dataPointsColumns = JsonConvert.SerializeObject(dataPointsColumns);
            return View();
        }
    }
}