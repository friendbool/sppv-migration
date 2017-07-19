namespace Microsoft.SharePoint.StsAdmin
{
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Utilities;
    using System;
    using System.Collections.Specialized;

    internal abstract class SPOperation : ISPOperation
    {
        private bool m_bTopLevelOperation;
        private SPParamCollection m_Params;
        private string m_strHelpMessage;

        public SPOperation()
            : this(true)
        {
        }

        public SPOperation(bool bTopLevelOperation)
        {
            this.m_bTopLevelOperation = bTopLevelOperation;
        }

        protected void Init(SPParamCollection Params, string strHelpMessage)
        {
            this.m_Params = Params;
            this.m_strHelpMessage = strHelpMessage;
        }

        public virtual void InitParameters(StringDictionary keyValues)
        {
            foreach (SPParam param in this.Params)
            {
                param.InitValueFrom(keyValues);
            }
        }

        internal void OutputSucceedMessage()
        {
            if (this.m_bTopLevelOperation)
            {
                Console.WriteLine(SPResource.GetString("OperationSuccess", new object[0]));
                Console.WriteLine();
            }
        }

        public abstract void Run(StringDictionary keyValues);

        public virtual void Validate(StringDictionary keyValues)
        {
            string strMessage = null;
            //if (this.m_bTopLevelOperation)
            //{
            //    foreach (string str2 in keyValues.Keys)
            //    {
            //        if ((str2 != "o") && (this.Params[str2] == null))
            //        {
            //            strMessage = strMessage + SPResource.GetString("CommandLineErrorInvalidParameter", new object[0]) + "\n";
            //            break;
            //        }
            //    }
            //}
            //if (strMessage != null)
            //{
            //    throw new SPSyntaxException(strMessage);
            //}
            //strMessage = null;
            for (int i = 0; i < this.Params.Count; i++)
            {
                SPParam param = this.Params[i];
                if ((param.Enabled && param.IsRequired) && !param.UserTypedIn)
                {
                    strMessage = strMessage + SPResource.GetString("MissRequiredArg", new object[] { param.Name }) + "\n";
                }
            }
            if (strMessage != null)
            {
                throw new SPSyntaxException(strMessage);
            }
            strMessage = null;
            foreach (SPParam param2 in this.m_Params)
            {
                if ((param2.Enabled && param2.UserTypedIn) && !param2.Validate())
                {
                    strMessage = strMessage + SPResource.GetString("InvalidArg", new object[] { param2.Name });
                    if ((param2.HelpMessage != null) && (param2.HelpMessage != string.Empty))
                    {
                        strMessage = strMessage + "\n\t" + param2.HelpMessage;
                    }
                    strMessage = strMessage + "\n";
                }
            }
            if (strMessage != null)
            {
                throw new SPSyntaxException(strMessage);
            }
        }

        public string HelpMessage
        {
            get
            {
                return this.m_strHelpMessage;
            }
        }

        protected internal SPParamCollection Params
        {
            get
            {
                return this.m_Params;
            }
        }
    }
}

