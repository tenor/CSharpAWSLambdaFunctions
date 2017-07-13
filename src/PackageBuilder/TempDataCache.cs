using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;

namespace PackageBuilder
{
    public class TempDataCache : ITempCache
    {
        ITempDataDictionary tempdata;
        public TempDataCache(ITempDataDictionary tempdata)
        {
            this.tempdata = tempdata;
        }

        public object Peek(string key)
        {
            return tempdata.Peek(key);
        }

        public object Get(string key)
        {
            return tempdata[key];
        }

        public string Store(object value)
        {
            var key = Guid.NewGuid().ToString("N");
            tempdata[key] = value;
            return key;
        }
    }
}
