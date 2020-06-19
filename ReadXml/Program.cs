using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ReadXml
{
	class Program
	{
		static void Main(string[] args)
		{
			var filePath = "personalView.txt";
			List<View> views = LoadViews(filePath);

			var viewNames = new[]
			{
				"RMA Da Gestire CON DATA DDT",
				"In Arrivo - Forlì",
				"In Carico - Forlì",
				"Presso Assistenza - Forlì",
				"In carico - Piacenza",
				"Presso Assistenza - Piacenza",
				"In Arrivo - Piacenza",
				"In carico - Carini",
				"In Arrivo - Carini",
				"Presso Assistenza - Carini"
			};

			var userViews = from v in views
								//where v.WebUrl.Equals(sourceUrl, StringComparison.InvariantCultureIgnoreCase) && (!base.Params["login"].UserTypedIn || v.UserLogin.Equals(base.Params["login"].Value, StringComparison.InvariantCultureIgnoreCase)) && (!base.Params["view"].UserTypedIn || v.ViewName.Equals(base.Params["view"].Value, StringComparison.InvariantCultureIgnoreCase))
							where v.UserLogin == "i:0#.w|mp\\abertozzi" && viewNames.Contains(v.ViewName)
							select v;
			//group v by v.UserLogin into g
			//select new { LoginName = g.Key, Views = g };

			using (SPSite site = new SPSite("http://collaboration.mp.sgmd.local/cross/PortaleContestazioni/rma"))
			using (SPWeb web = site.OpenWeb())
			{
				var list = web.Lists["RMA"];
				foreach (var view in userViews.Take(10))
				{
					string method = string.Format(CreateCommand(view, false), list.ID);
					string BatchFormat = @"<?xml version=""1.0"" encoding=""UTF-8""?><ows:Batch OnError=""Return"">{0}</ows:Batch>";
					string result = web.ProcessBatchData(string.Format(BatchFormat, method));
				}
			}

			

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
			writer.Append(@"<SetVar Name=""Personal"">FALSE</SetVar>");

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
							throw new Exception(string.Format("Invalid export file: {0}", filePath));
						}
					}
				}
			}
			else
			{
				throw new Exception(string.Format("Export file not found: {0}", filePath));
			}

			return views;
		}
	}
}
