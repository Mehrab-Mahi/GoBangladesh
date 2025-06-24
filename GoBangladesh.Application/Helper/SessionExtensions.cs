using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;

namespace GoBangladesh.Application.Helper
{
    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            var jsonValue = JsonConvert.SerializeObject(value);
            session.Set(key, Encoding.UTF8.GetBytes(jsonValue));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            byte[] strValue;
            session.TryGetValue(key, out strValue);
            if(strValue is null)
            {
                return default(T);
            }
            var value = Encoding.UTF8.GetString(strValue);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        public static void SetStr(this ISession session, string key, string value)
        {
            session.Set(key, Encoding.UTF8.GetBytes(value));
        }

        public static string GetStr(this ISession session, string key)
        {
            byte[] strValue;
            session.TryGetValue(key, out strValue);
            return strValue == null ? "" : Encoding.UTF8.GetString(strValue);
        }
    }
}