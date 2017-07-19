namespace Microsoft.SharePoint.StsAdmin
{
    using System;

    internal class SPUrlValidator : SPNonEmptyValidator
    {
        public override bool Validate(string str)
        {
            if (!base.Validate(str))
            {
                return false;
            }
            if (str.IndexOf('\\') >= 0)
            {
                return false;
            }
            try
            {
                Uri uri = new Uri(str);
                if ((uri.Fragment != "") || (uri.Query != ""))
                {
                    return false;
                }
                if ((uri.Scheme != Uri.UriSchemeHttp) && (uri.Scheme != Uri.UriSchemeHttps))
                {
                    return false;
                }
            }
            catch (UriFormatException)
            {
                return false;
            }
            return true;
        }
    }
}

