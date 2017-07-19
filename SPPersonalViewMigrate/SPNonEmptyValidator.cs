namespace Microsoft.SharePoint.StsAdmin
{
    using System;

    internal class SPNonEmptyValidator : SPValidator
    {
        public override bool Validate(string str)
        {
            return ((str != null) && (str.Trim().Length != 0));
        }
    }
}

