using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using temple.Models.FriendsController;
using temple.Models;

namespace ExtensionMethods
{
    public static class QueryStringExtension
    {
        public static string GenerateQueryString<T>(this T t) {
            var result = new ResultModel();
            var queryString = from p in t.GetType().GetProperties()
                              where p.GetValue(t, null) != null
                              select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(t, null).ToString());

            

            return ("?"+String.Join("&",queryString.ToArray()));
        }
    }
}
