﻿//namespace MSharp.Framework.Data.Ado.Net
//{
//    using System.Collections.Generic;
//    using System.Linq;

//    public class PropertySubqueryMapping
//    {
//        public string Properties, Subquery;
//        public Dictionary<string, string> Details;

//        public PropertySubqueryMapping(string properties, string prefix, Dictionary<string, string> destinationPropertyMappings)
//        {
//            Properties = properties;
//            Details = destinationPropertyMappings.ToDictionary(x => x.Key, x =>
//                {
//                    if (x.Value.StartsWith("["))
//                        return x.Value.Insert(1, prefix);
//                    else return prefix + x.Value;
//                });
//        }
//    }
//}