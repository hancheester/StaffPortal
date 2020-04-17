using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using StaffPortal.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaffPortal.Web.Extensions
{
    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));            
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        public static void SetCollection<T>(this ISession session, string key, T value)
        {
            List<T> collection = new List<T>(){
                value
            };

            session.SetString(key, JsonConvert.SerializeObject(collection));
        }

        public static List<T> GetCollection<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default(List<T>) : JsonConvert.DeserializeObject<List<T>>(value);
        }

        public static OperationResult<T> AddObject<T>(this ISession session, string key, T value)
        {
            OperationResult<T> result = new OperationResult<T>(value);

            var collection = GetCollection<T>(session, key);

            if (collection != null)
            {
                collection.Add(value);
                session.SetString(key, JsonConvert.SerializeObject(collection));
            }
            else
                SetCollection<T>(session, key, value);
            
            return result;
        }




    }
}
