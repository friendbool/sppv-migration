using System;
using System.Data.SqlClient;
using System.Text;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.StsAdmin;

namespace SPPersonalViewMigrate
{
    internal class SPExportPersonalView : SPOperation
    {
        public SPExportPersonalView()
            : base()
        {
            SPParamCollection @params = new SPParamCollection();
            @params.Add(new SPParam("url", "url", true, null, new SPNonEmptyValidator()));
            @params.Add(new SPParam("filePath", "file", true, null, new SPNonEmptyValidator()));
            base.Init(@params, Usage.Export);
        }

        public override void Run(System.Collections.Specialized.StringDictionary keyValues)
        {
            string url = RemoveTrailingForwardSlash(base.Params["url"].Value);
            string filePath = base.Params["file"].Value;
            Guid siteID = Guid.Empty;
            Guid webID = Guid.Empty;
            string connectionString = string.Empty;

            using (var site = new SPSite(url))
            {
                siteID = site.ID;
                if (!site.Url.Equals(url, StringComparison.OrdinalIgnoreCase))
                {
                    using (var web = site.OpenWeb())
                    {
                        webID = web.ID;
                    }
                }
                connectionString = site.ContentDatabase.DatabaseConnectionString;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(GetSqlText(siteID, webID), connection))
                {
                    command.CommandTimeout = 0;
                    connection.Open();
                    using (var reader = command.ExecuteXmlReader())
                    {
                        using (var writer = XmlWriter.Create(filePath))
                        {
                            writer.WriteNode(reader, true);
                        }
                    }
                }
            }
        }

        static string GetSqlText(Guid siteId, Guid webId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("WITH XMLNAMESPACES ('http://www.w3.org/2001/XMLSchema' AS xsd, 'http://www.w3.org/2001/XMLSchema-instance' AS xsi)");
            sb.AppendLine();
            sb.Append("SELECT tp_DisplayName AS ViewName,Webs.FullUrl AS WebUrl,UI.tp_Login AS UserLogin,SUBSTRING(Docs.DirName, LEN(Webs.FullUrl) + 2, LEN(Docs.DirName)) AS ListUrl,ISNULL(master.dbo.fn_varbintohexstr(WP.tp_ContentTypeId), '0x') AS ContentTypeId,tp_View AS ViewSchema,WP.tp_Flags AS Flags");
            sb.AppendLine();
            sb.Append("FROM AllWebParts WP WITH (NOLOCK, INDEX=PageUrlID_FK)");
            sb.AppendLine();
            sb.Append("JOIN Docs WITH (NOLOCK) ON Docs.Id = WP.tp_PageUrlID AND Docs.LeafName = N'PersonalViews.aspx'");
            sb.AppendLine();
            sb.Append("JOIN Webs WITH (NOLOCK) ON Webs.Id = Docs.WebId");
            sb.AppendLine();
            sb.Append("JOIN UserInfo UI WITH (NOLOCK) ON UI.tp_SiteId = WP.tp_SiteId AND UI.tp_ID = WP.tp_UserID");
            sb.AppendLine();
            sb.AppendFormat("WHERE tp_IsCurrentVersion = CONVERT(bit, 1) AND tp_PageVersion = 0 AND tp_View IS NOT NULL AND WP.tp_SiteId = '{0}'", siteId);
            if (webId != Guid.Empty)
            {
                sb.AppendFormat(" AND Webs.Id = '{0}'", webId);
            }
            sb.AppendLine();
            sb.Append("FOR XML PATH('View'), ROOT('ArrayOfView')");
            return sb.ToString();
        }

        static string RemoveTrailingForwardSlash(string url)
        {
            int index = url.LastIndexOf("/");

            if (url.Length == (index + 1))
            {
                return url.Substring(0, index);
            }
            else
            {
                return url;
            }
        }
    }
}
