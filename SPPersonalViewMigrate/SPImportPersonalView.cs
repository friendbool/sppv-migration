using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.StsAdmin;

namespace SPPersonalViewMigrate
{
    internal class SPImportPersonalView : SPOperation
    {
        public SPImportPersonalView()
            : base()
        {
            SPParamCollection @params = new SPParamCollection();
            @params.Add(new SPParam("filePath", "file", true, null, new SPNonEmptyValidator()));
            @params.Add(new SPParam("sourceUrl", "source", true, null, new SPNonEmptyValidator()));
            @params.Add(new SPParam("targetUrl", "target", true, null, new SPUrlValidator()));
            @params.Add(new SPParam("viewName", "view", false, null, new SPNonEmptyValidator()));
            @params.Add(new SPParam("userLogin", "login", false, null, new SPNonEmptyValidator()));
            @params.Add(new SPParam("excludeUserLogin", "excludeLogin", false, null, new SPNonEmptyValidator()));
            @params.Add(new SPParam("schemaPlainText", "schemaPlainText"));
            base.Init(@params, Usage.Import);
        }

        public override void Run(StringDictionary keyValues)
        {
            string filePath = null;
            string sourceUrl = null;
            string targetUrl = null;
            bool schemaPlainText = false;
            string[] loginToExclude;

            filePath = base.Params["file"].Value;
            sourceUrl = base.Params["source"].Value;
            targetUrl = base.Params["target"].Value;
            schemaPlainText = base.Params["schemaPlainText"].UserTypedIn;
            if (base.Params["excludeLogin"].UserTypedIn)
            {
                loginToExclude = base.Params["excludeLogin"].Value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                loginToExclude = new string[0];
            }

            List<View> views = LoadViews(filePath);
            
            var userViews = from v in views
                            where v.WebUrl.Equals(sourceUrl, StringComparison.InvariantCultureIgnoreCase) && (!base.Params["login"].UserTypedIn || v.UserLogin.Equals(base.Params["login"].Value, StringComparison.InvariantCultureIgnoreCase)) && (!base.Params["view"].UserTypedIn || v.ViewName.Equals(base.Params["view"].Value, StringComparison.InvariantCultureIgnoreCase))
                            group v by v.UserLogin into g
                            select new { LoginName = g.Key, Views = g };

            if (userViews.Count() == 0)
            {
                WriteTrace("Personal views not found in: " + sourceUrl);
                Console.WriteLine();
                Console.WriteLine("Personal views not found in: {0}", sourceUrl);
                Console.WriteLine();
                return;
            }

            using (SPSite site = new SPSite(targetUrl))
            {
                using (SPWeb web = site.OpenWeb(ResolveWebUrl(targetUrl.Substring(site.Url.Length)), true))
                {
                    foreach (var g in userViews)
                    {
                        if (loginToExclude.Contains(g.LoginName))
                        {
                            WriteTrace("Skip importing personal views for login: " + g.LoginName);
                            Console.WriteLine();
                            Console.WriteLine("Skip importing personal views of login: {0}", g.LoginName);
                            Console.WriteLine();
                            continue;
                        }
                        SPUser user = null;
                        try
                        {
                            user = web.EnsureUser(g.LoginName);
                        }
                        catch (SPException ex)
                        {
                            WriteTrace(ex.ToString());
                            Console.WriteLine();
                            Console.WriteLine("User not found: {0}", g.LoginName);
                            Console.WriteLine();
                            continue;
                        }
                        using (SPSite userSite = new SPSite(web.Url, user.UserToken))
                        {
                            using (SPWeb userWeb = userSite.OpenWeb())
                            {
                                foreach (var view in g.Views)
                                {
                                    WriteTrace(view.ToString());
                                    Console.WriteLine();
                                    Console.WriteLine("Importing view...");
                                    Console.WriteLine();
                                    ObjectDumper.Write(view);
                                    string listUrl = ResolveListUrl(view.ListUrl);
                                    SPList list = userWeb.GetList(userWeb.Url + "/" + listUrl); ;
                                    if (list != null)
                                    {
                                        string method = string.Format(CreateCommand(view, schemaPlainText), list.ID);
                                        WriteTrace(method);
                                        string BatchFormat = @"<?xml version=""1.0"" encoding=""UTF-8""?><ows:Batch OnError=""Return"">{0}</ows:Batch>";
                                        string result = userWeb.ProcessBatchData(string.Format(BatchFormat, method));
                                        PrintResult(Console.Out, result);
                                        WriteTrace(result);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static string ResolveListUrl(string relativeUrl)
        {
            string listUrl = relativeUrl.StartsWith("/") ? relativeUrl.Substring(1) : relativeUrl;
            if (listUrl.EndsWith("/Forms") && !listUrl.StartsWith("Lists/"))
            {
                listUrl = listUrl.Substring(0, listUrl.Length - 6);
            }
            return listUrl;
        }

        static string ResolveWebUrl(string relativeUrl)
        {
            if (relativeUrl.StartsWith("/"))
            {
                return relativeUrl.Substring(1);
            }
            return relativeUrl;
        }

        static List<View> LoadViews(string filePath)
        {
            List<View> views = null;

            if (File.Exists(filePath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(List<View>));

                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    using (XmlReader reader = new XmlTextReader(fs))
                    {
                        if (xs.CanDeserialize(reader))
                        {
                            views = (List<View>)xs.Deserialize(reader);
                        }
                        else
                        {
                            throw new SPException(string.Format("Invalid export file: {0}", filePath));
                        }
                    }
                }
            }
            else
            {
                throw new SPSyntaxException(string.Format("Export file not found: {0}", filePath));
            }

            return views;
        }

        static string CreateCommand(View view, bool schemaPlainText)
        {
            StringBuilder writer = new StringBuilder();
            writer.Append(@"<Method ID=""0,NewView"">");
            writer.Append(@"<SetList Scope=""Request"">{0}</SetList>");
            writer.Append(@"<SetVar Name=""Cmd"">NewView</SetVar>");
            writer.AppendFormat(@"<SetVar Name=""ViewType"">{0}</SetVar>", Parser.ParseViewType(view.Flags));
            writer.Append(@"<SetVar Name=""LocalizedTodayString"">&lt;Today /&gt;</SetVar>");
            writer.Append(@"<SetVar Name=""LocalizedMeString"">&lt;UserID Type=""Integer"" /&gt;</SetVar>");
            writer.AppendFormat(@"<SetVar Name=""NewViewName"">{0}</SetVar>", view.ViewName);
            writer.Append(@"<SetVar Name=""Personal"">TRUE</SetVar>");

            XmlDocument viewXml = new XmlDocument();
            string schemaXml = (schemaPlainText == true) ? view.ViewSchema : view.ViewSchema.Decompress();
            viewXml.LoadXml("<View>" + schemaXml + "</View>");

            viewXml.DocumentElement.RenderColumns(writer);
            viewXml.DocumentElement.RenderSort(writer);
            viewXml.DocumentElement.RenderFilter(writer);
            viewXml.DocumentElement.RenderGroupBy(writer);
            viewXml.DocumentElement.RenderTotals(writer);
            viewXml.DocumentElement.RenderStyle(writer);
            viewXml.DocumentElement.RenderItemLimit(writer);
            viewXml.DocumentElement.RenderCalendarViewStyles(writer);
            viewXml.DocumentElement.RenderCalendarViewData(writer);

            string scope = Parser.ParseScope(view.Flags);
            if (!string.IsNullOrEmpty(scope))
            {
                writer.AppendFormat(@"<SetVar Name=""{0}"">TRUE</SetVar>", scope);
            }
            if (!string.IsNullOrEmpty(view.ContentTypeId))
            {
                writer.AppendFormat(@"<SetVar Name=""ContentTypeId"">{0}</SetVar>", view.ContentTypeId);
            }
            writer.Append("</Method>");

            return writer.ToString();
        }

        static void PrintResult(TextWriter output, string batchResult)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(batchResult);
            XmlNode result = doc.SelectSingleNode("Results/Result");
            if (result != null)
            {
                output.WriteLine();
                int code = 0;
                int.TryParse(result.Attributes["Code"].Value, out code);
                if (code == 0)
                {
                    output.WriteLine("Operation completed successfully.");
                }
                else
                {
                    XmlNode error = result.SelectSingleNode("ErrorText");
                    if (error != null)
                    {
                        output.WriteLine("ERROR: {0}", error.InnerText);
                    }
                }
                output.WriteLine();
            }
        }

        static void WriteTrace(string message)
        {
            Trace.WriteLine(DateTime.UtcNow.ToString());
            Trace.Indent();
            Trace.WriteLine(message);
            Trace.Unindent();
        }
    }
}
