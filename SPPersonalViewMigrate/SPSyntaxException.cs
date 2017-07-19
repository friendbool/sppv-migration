namespace Microsoft.SharePoint.StsAdmin
{
    using System;

    internal class SPSyntaxException : ApplicationException
    {
        public SPSyntaxException(string strMessage) : base(strMessage)
        {
        }
    }
}

