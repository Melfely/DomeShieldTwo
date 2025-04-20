using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using BrilliantSkies.Core.Collections;

namespace AdvShields
{
    public class NewDictionaryOfTypesWithConstructor<TForConstructor>
    {
        // Token: 0x060022F5 RID: 8949 RVA: 0x0008E910 File Offset: 0x0008CB10
        public void Add<T>(T t)
        {
            this._newdictionary.Add(typeof(T), t);
        }

        // Token: 0x060022F6 RID: 8950 RVA: 0x0008E930 File Offset: 0x0008CB30
        public T Get<T>()
        {
            object obj;
            bool flag = this._newdictionary.TryGetValue(typeof(T), out obj);
            T result;
            if (flag)
            {
                result = (T)((object)obj);
            }
            else
            {
                result = default(T);
            }
            return result;
        }
        public T GetOrConstruct<T>(TForConstructor C, Func<TForConstructor, T> fnConstruct)
        {
            object obj;
            bool flag = this._newdictionary.TryGetValue(typeof(T), out obj);
            T result;
            if (flag)
            {
                result = (T)((object)obj);
            }
            else
            {
                T t = fnConstruct(C);
                this.Add<T>(t);
                result = t;
            }
            return result;
        }

        // Token: 0x060022F8 RID: 8952 RVA: 0x0008E9BD File Offset: 0x0008CBBD
        public void CleanUp()
        {
            this._newdictionary.Clear();
        }

        // Token: 0x060022F9 RID: 8953 RVA: 0x0008E9CC File Offset: 0x0008CBCC
        public List<T> GetAll<T>()
        {
            return this._newdictionary.Values.Cast<T>().ToList<T>();
        }
        private Dictionary<Type, object> _newdictionary = new Dictionary<Type, object>();
    }
}
