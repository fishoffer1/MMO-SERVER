using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summer.Core
{
    public class TypeAttributeStore
    {
        /// <summary>
        /// 通用属性存储
        /// </summary>
        private Dictionary<string , object> _dict = new Dictionary<string , object>();

        public void Set<T>(T value)
        {
            string key = typeof(T).FullName;
            _dict[key] = value;
        }
        
        public T Get<T>() 
        {
            string key = typeof(T).FullName;
            if (_dict.ContainsKey(key))
            {
                return (T)_dict[key];
            }
            return default(T);
        }

    }
}
