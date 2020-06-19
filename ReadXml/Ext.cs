using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ReadXml
{
	public static class Ext
	{
		public static string XmlEncode(this string value)
		{
			return value.Replace("<", "&lt;").Replace(">", "&gt;");
		}

		static void ParseFilter(XmlNode where, List<string> filterExp)
		{
			if (where.Name == "Or" || where.Name == "And")
			{
				if (where.FirstChild.Name == "DateRangesOverlap")
				{
					ParseFilter(where.LastChild, filterExp);
				}
				else if (where.LastChild.Name == "DateRangesOverlap")
				{
					ParseFilter(where.FirstChild, filterExp);
				}
				else
				{
					filterExp.Add(where.LastChild.OuterXml);
					filterExp.Add(where.Name);
					if (where.FirstChild.Name == "Or" || where.FirstChild.Name == "And")
					{
						ParseFilter(where.FirstChild, filterExp);
					}
					else
					{
						filterExp.Add(where.FirstChild.OuterXml);
					}
				}
			}
			else if (where.Name != "DateRangesOverlap")
			{
				filterExp.Add(where.OuterXml);
			}
		}

		public static void RenderFilter(this XmlNode root, StringBuilder writer)
		{
			XmlNode where = root.SelectSingleNode("Query/Where");
			if (where != null && where.HasChildNodes)
			{
				List<string> we = new List<string>();

				ParseFilter(where.FirstChild, we);

				if (we.Count > 0)
				{
					we.Reverse();

					writer.Append(@"<SetVar Name=""IsThereAQuery"">TRUE</SetVar>");

					int index = 1;
					foreach (string e in we)
					{
						if (e != "Or" && e != "And")
						{
							XmlDocument doc = new XmlDocument();
							doc.LoadXml(e);
							writer.AppendFormat(@"<SetVar Name=""FieldPicker{0}"">tp_{1}</SetVar>", index, doc.DocumentElement.FirstChild.Attributes["Name"].Value);
							writer.AppendFormat(@"<SetVar Name=""OperatorPicker{0}"">{1}</SetVar>", index, doc.DocumentElement.Name);
							writer.AppendFormat(@"<SetVar Name=""CompareWithValue{0}"">{1}</SetVar>", index, doc.DocumentElement.LastChild.InnerXml.XmlEncode());
						}
						else
						{
							writer.AppendFormat(@"<SetVar Name=""NextIsAnd{0}"">{1}</SetVar>", index, e == "And" ? "TRUE" : "FALSE");
							index++;
						}
					}

					writer.AppendFormat(@"<SetVar Name=""NextIsAnd{0}"">FALSE</SetVar>", index);
				}
				else
				{
					writer.Append(@"<SetVar Name=""IsThereAQuery"">FALSE</SetVar>");
				}
			}
			else
			{
				writer.Append(@"<SetVar Name=""IsThereAQuery"">FALSE</SetVar>");
			}
		}

		public static void RenderColumns(this XmlNode root, StringBuilder writer)
		{
			int index = 0;
			XmlNodeList fields = root.SelectNodes("ViewFields/FieldRef");
			foreach (XmlNode fn in fields)
			{
				writer.AppendFormat(@"<SetVar Name=""ViewOrder{0}"">{1}_{2}</SetVar>", index, index + 1, fn.Attributes["Name"].Value);
				writer.AppendFormat(@"<SetVar Name=""ShouldDisplay{0}"">TRUE</SetVar>", fn.Attributes["Name"].Value);
				index++;
			}
		}

		public static void RenderGroupBy(this XmlNode root, StringBuilder writer)
		{
			XmlNode groupBy = root.SelectSingleNode("Query/GroupBy");
			if (groupBy != null && groupBy.HasChildNodes)
			{
				int index = 1;
				foreach (XmlNode group in groupBy.SelectNodes("FieldRef"))
				{
					writer.AppendFormat(@"<SetVar Name=""GroupField{0}"">tp_{1}</SetVar>", index, group.Attributes["Name"].Value);
					XmlAttribute asc = group.Attributes["Ascending"];
					if (asc != null)
					{
						writer.AppendFormat(@"<SetVar Name=""GroupAscending{0}"">{1}</SetVar>", index, asc.Value);
					}
					else
					{
						writer.AppendFormat(@"<SetVar Name=""GroupAscending{0}"">TRUE</SetVar>", index);
					}
					index++;
				}

				XmlAttribute collapse = groupBy.Attributes["Collapse"];
				if (collapse != null)
				{
					writer.AppendFormat(@"<SetVar Name=""CollapseGroups"">{0}</SetVar>", collapse.Value);
				}

				XmlAttribute groupLimit = groupBy.Attributes["GroupLimit"];
				if (groupLimit != null)
				{
					writer.AppendFormat(@"<SetVar Name=""GroupLimit"">{0}</SetVar>", groupLimit.Value);
				}
			}
		}

		public static void RenderTotals(this XmlNode root, StringBuilder writer)
		{
			XmlNode totals = root.SelectSingleNode("Aggregations");
			if (totals != null)
			{
				XmlAttribute totalValue = totals.Attributes["Value"];
				if (totalValue != null && totalValue.Value.Equals("On", StringComparison.InvariantCultureIgnoreCase))
				{
					foreach (XmlNode fieldRef in totals.SelectNodes("FieldRef"))
					{
						writer.AppendFormat(@"<SetVar Name=""Total{0}"">{1}</SetVar>", fieldRef.Attributes["Name"].Value, fieldRef.Attributes["Type"].Value);
					}
				}
			}
		}

		public static void RenderStyle(this XmlNode root, StringBuilder writer)
		{
			XmlNode style = root.SelectSingleNode("ViewStyle");
			if (style != null)
			{
				writer.AppendFormat(@"<SetVar Name=""ViewStyle"">{0}</SetVar>", style.Attributes["ID"].Value);
			}
			else
			{
				writer.Append(@"<SetVar Name=""ViewStyle"">Default</SetVar>");
			}
		}

		public static void RenderSort(this XmlNode root, StringBuilder writer)
		{
			XmlNode orderBy = root.SelectSingleNode("Query/OrderBy");
			if (orderBy != null && orderBy.HasChildNodes)
			{
				int index = 1;
				foreach (XmlNode order in orderBy.SelectNodes("FieldRef"))
				{
					writer.AppendFormat(@"<SetVar Name=""SortField{0}"">tp_{1}</SetVar>", index, order.Attributes["Name"].Value);
					XmlAttribute asc = order.Attributes["Ascending"];
					if (asc != null)
					{
						writer.AppendFormat(@"<SetVar Name=""SortAscending{0}"">{1}</SetVar>", index, asc.Value);
					}
					else
					{
						writer.AppendFormat(@"<SetVar Name=""SortAscending{0}"">TRUE</SetVar>", index);
					}
					index++;
				}
			}
		}

		public static void RenderItemLimit(this XmlNode root, StringBuilder writer)
		{
			XmlNode rowLimit = root.SelectSingleNode("RowLimit");
			if (rowLimit != null)
			{
				writer.AppendFormat(@"<SetVar Name=""RowLimit"">{0}</SetVar>", rowLimit.InnerText);
				XmlAttribute paged = rowLimit.Attributes["Paged"];
				if (paged != null)
				{
					writer.AppendFormat(@"<SetVar Name=""Paged"">{0}</SetVar>", paged.Value);
				}
			}
		}

		public static void RenderInlineEditing(this XmlNode root, StringBuilder writer)
		{
			XmlNode rowLimit = root.SelectSingleNode("InlineEdit");
			if (rowLimit != null)
			{
				writer.AppendFormat(@"<SetVar Name=""InlineEdit"">{0}</SetVar>", rowLimit.InnerText);
			}
		}

		public static void RenderCalendarViewStyles(this XmlNode root, StringBuilder writer)
		{
			XmlNode rowLimit = root.SelectSingleNode("CalendarViewStyles");
			if (rowLimit != null)
			{
				writer.AppendFormat(@"<SetVar Name=""CalViewStyles"">{0}</SetVar>", rowLimit.InnerText.XmlEncode());
			}
		}

		public static void RenderCalendarViewData(this XmlNode root, StringBuilder writer)
		{
			XmlNodeList fields = root.SelectNodes("ViewData/FieldRef");
			foreach (XmlNode fn in fields)
			{
				writer.AppendFormat(@"<SetVar Name=""{0}"">{1}</SetVar>", fn.Attributes["Type"].Value, fn.Attributes["Name"].Value);
			}
		}

		public static string Decompress(this SqlBinary compressedString)
		{
			string uncompressedString = String.Empty;
			if (!compressedString.IsNull)
			{
				using (MemoryStream compressedMemoryStream = new MemoryStream(compressedString.Value))
				{
					compressedMemoryStream.Position += 12; // Compress Structure Header according to [MS -WSSFO2].
					compressedMemoryStream.Position += 2;  // Zlib header.

					using (DeflateStream deflateStream = new DeflateStream(compressedMemoryStream, CompressionMode.Decompress))
					{
						using (StreamReader streamReader = new StreamReader(deflateStream))
						{
							uncompressedString = streamReader.ReadToEnd();
						}
					}
				}
			}
			return uncompressedString;
		}

		public static string Decompress(this string compressedString)
		{
			string uncompressedString = String.Empty;
			if (!string.IsNullOrEmpty(compressedString))
			{
				using (MemoryStream compressedMemoryStream = new MemoryStream(Convert.FromBase64String(compressedString)))
				{
					compressedMemoryStream.Position += 12; // Compress Structure Header according to [MS -WSSFO2].
					compressedMemoryStream.Position += 2;  // Zlib header.

					using (DeflateStream deflateStream = new DeflateStream(compressedMemoryStream, CompressionMode.Decompress))
					{
						using (StreamReader streamReader = new StreamReader(deflateStream))
						{
							uncompressedString = streamReader.ReadToEnd();
						}
					}
				}
			}
			return uncompressedString;
		}
	}
}
