namespace Microsoft.SharePoint.StsAdmin
{
    using System;
    using System.Collections.Specialized;

    internal interface ISPOperation
    {
        void InitParameters(StringDictionary keyValues);
        //void Log(int iSeverity, string strMessage);
        void Run(StringDictionary keyValues);
        void Validate(StringDictionary keyValues);

        //string DisplayNameId { get; }

        string HelpMessage { get; }
    }
}

