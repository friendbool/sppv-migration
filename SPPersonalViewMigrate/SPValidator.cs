namespace Microsoft.SharePoint.StsAdmin
{
    using System;

    internal class SPValidator : ISPValidator
    {
        public virtual bool Validate(string str)
        {
            return true;
        }
    }
}

