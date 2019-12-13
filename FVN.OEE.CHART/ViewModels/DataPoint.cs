using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FVN.OEE.CHART.ViewModels
{
    //DataContract for Serializing Data - required to serve in JSON format
    [DataContract]
    public class DataPoint
    {
        public DataPoint(string name, double y, string drilldown)
        {
            this.name = name;
            this.Y = y;
            this.drilldown = drilldown;
        }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "name")]
        public string name = string.Empty;

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "y")]
        public Nullable<double> Y = null;

        [DataMember(Name = "drilldown")]
        public string drilldown = null;
    }
}