using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace CsDO.Lib
{
    [Serializable]
    public class DataPool : Singleton
    {
        private HybridDictionary pool = new HybridDictionary(250, false);

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
                    return (result == null ? null : (DataObject) result);
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

                pool[key] = new WeakReference(value); ;
            }
        }

        public void add(DataObject data)
        {
            data.AfterDelete += this.AfterDelete;
            data.BeforeInsert += this.BeforeInsert;
            data.AfterInsert += this.AfterInsert;
            //data.BeforeFetch += this.BeforeInsert;
            //data.AfterFetch += this.AfterInsert;

            this[data] = data;
        }

        public void Clear()
        {
            foreach (WeakReference obj in pool.Values)
            {
                if (obj != null)
                {
                    DataObject data = (DataObject)obj.Target;

                    if (data != null && 
                        data.getState() == DisposableState.Alive)
                    {
                        data.AfterDelete -= this.AfterDelete;
                        data.BeforeInsert -= this.BeforeInsert;
                        data.AfterInsert -= this.AfterInsert;
                        //data.BeforeFetch -= this.BeforeInsert;
                        //data.AfterFetch -= this.AfterInsert;
                    }
                }
            }

            pool.Clear();
        }

        public void remove(DataObject data)
        {
            if (data.getState() == DisposableState.Alive)
            {
                data.AfterDelete -= this.AfterDelete;
                data.BeforeInsert -= this.BeforeInsert;
                data.AfterInsert -= this.AfterInsert;
                //data.BeforeFetch -= this.BeforeInsert;
                //data.AfterFetch -= this.AfterInsert;
            }

            pool.Remove(data.ToString());
        }

        internal void AfterDelete(DataObject sender, bool success)
        {
            if (success)
                remove(sender);
        }

        internal void BeforeInsert(DataObject sender)
        {
            if ((sender.getPrimaryKey() != null) && ((int)sender.getPrimaryKey() != 0))
            {
                remove(sender);
            }
        }

        internal void AfterInsert(DataObject sender, bool success)
        {
            if (success)
                add(sender);
        }

        new public static DataPool New() { return (DataPool)Instance(typeof(DataPool)); }
    }
}
