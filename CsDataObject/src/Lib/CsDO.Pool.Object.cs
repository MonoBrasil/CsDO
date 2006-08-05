using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace CsDO.Lib
{
    public class DataPool : Singleton
    {
        private HybridDictionary pool =
            new HybridDictionary(250, false);

        public DataObject this[DataObject key]
        {
            get
            {
                if (key != null)
                    return this[key.ToString()];
                else
                    return null;
            }
            set
            {
                this[key.ToString()] = value;
            }
        }

        public DataObject this[string key]
        {
            get
            {
                if (pool.Contains(key.ToString()))
                {
                    DataObject result = (DataObject)((WeakReference)pool[key]).Target;
                    if (result != null)
                    {
                        if (result.getState() != DisposableState.Alive)
                        {
                            result = null;
                        }
                    }
                    if (result == null)
                    {
                        pool.Remove(key);
                    }
                    return (result == null ? null : (DataObject) result.Clone());
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (pool.Contains(key))
                    pool.Remove(key);

                pool[key] = new WeakReference(value.Clone()); ;
            }
        }

        public void add(DataObject obj)
        {
            this[obj] = obj;
        }

        public void Clear()
        {
            pool.Clear();
        }

        public void remove(DataObject obj)
        {
            pool.Remove(obj.ToString());
        }

        new public static DataPool New() { return (DataPool)Instance(typeof(DataPool)); }
    }
}
