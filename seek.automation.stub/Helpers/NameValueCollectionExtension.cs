using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace seek.automation.stub
{
    public static class NameValueCollectionExtension
    {
        public static bool CollectionEquals(this NameValueCollection nameValueCollection1, NameValueCollection nameValueCollection2)
        {
            return nameValueCollection1.ToKeyValue().SequenceEqual(nameValueCollection2.ToKeyValue());
        }

        public static NameValueCollection FilterCollection(this NameValueCollection nameValueCollection)
        {
            var filteredCollection = new NameValueCollection();

            var unwantedKeys = new List<string> { "oauth_consumer_key", "oauth_timestamp", "oauth_signature" };
            foreach (var key in nameValueCollection.AllKeys)
            {
                if (unwantedKeys.Contains(key)) continue;

                filteredCollection.Add(key, nameValueCollection[key]);
            }

            return filteredCollection;
        }

        private static IEnumerable<object> ToKeyValue(this NameValueCollection nameValueCollection)
        {
            return nameValueCollection.AllKeys.OrderBy(x => x).Select(x => new { Key = x, Value = nameValueCollection[x] });
        }
    }
}