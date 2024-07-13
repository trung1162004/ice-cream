using Newtonsoft.Json;

namespace Project.Helpers
{
    public class SessionHelper
    {
        public static void SetObjectJson(ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T? GetObjectJson<T>(ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
