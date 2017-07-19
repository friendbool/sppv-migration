namespace Microsoft.SharePoint.StsAdmin
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class SPParamCollection : IEnumerable
    {
        private ArrayList m_Collection = new ArrayList();
        private Hashtable m_NameMap = new Hashtable();
        private Hashtable m_ShortNameMap = new Hashtable();

        public void Add(SPParam param)
        {
            this.m_Collection.Add(param);
            this.m_NameMap.Add(param.Name, param);
            this.m_ShortNameMap.Add(param.ShortName, param);
        }

        public IEnumerator GetEnumerator()
        {
            return this.m_Collection.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.m_Collection.Count;
            }
        }

        public SPParam this[string strName]
        {
            get
            {
                SPParam param = (SPParam) this.m_NameMap[strName];
                if (param == null)
                {
                    param = (SPParam) this.m_ShortNameMap[strName];
                }
                return param;
            }
        }

        public SPParam this[int index]
        {
            get
            {
                return (SPParam) this.m_Collection[index];
            }
        }
    }
}

