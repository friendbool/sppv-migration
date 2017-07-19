using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.SharePoint;
using Microsoft.SharePoint.StsAdmin;

namespace SPPersonalViewMigrate
{
    class Program
    {
        static void PrintUsage(string usage)
        {
            Console.WriteLine();
            Console.WriteLine(usage);
            Console.WriteLine();
        }

        static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine(Usage.Export);
            Console.WriteLine();
            Console.WriteLine(Usage.Import);
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            StringDictionary keyValues = null;
            
            if (ParseInput(args, out keyValues) != 0)
            {
                Console.WriteLine(SPResource.GetString("CommandLineError", new object[0]));
                PrintUsage();
                return;
            }

            if (keyValues.ContainsKey("help"))
            {
                PrintUsage();
                return;
            }

            if (keyValues.ContainsKey("o"))
            {
                RunOperation(keyValues);
                return;
            }

            PrintUsage();

        }

        static void RunOperation(StringDictionary keyValues)
        {
            using (var fs = new FileStream("trace.log", FileMode.Append))
            {
                TextWriterTraceListener listener = new TextWriterTraceListener(fs);
                Trace.Listeners.Clear();
                Trace.Listeners.Add(listener);
                Trace.IndentSize = 3;
                Trace.AutoFlush = true;

                ISPOperation operation = null;
                try
                {
                    operation = GetOpertion(keyValues["o"]);
                    operation.InitParameters(keyValues);
                    operation.Validate(keyValues);
                    operation.Run(keyValues);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Length > 0)
                    {
                        Console.Error.WriteLine(ex.Message);
                    }
                    if (operation != null)
                    {
                        PrintUsage(operation.HelpMessage);
                    }
                    WriteTrace(ex.ToString());
                }
            }
        }

        static ISPOperation GetOpertion(string name)
        {
            ISPOperation operation = null;

            switch (name.ToLowerInvariant())
            {
                case "export":
                    operation = new SPExportPersonalView();
                    break;

                case "import":
                    operation = new SPImportPersonalView();
                    break;

                default:
                    throw new SPSyntaxException("Invalid operation.");
            }

            return operation;
        }

        static int ParseInput(string[] args, out StringDictionary keyValues)
        {
            keyValues = new StringDictionary();
            int num = 0;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-") && (args[i].Length > 1))
                {
                    string key = args[i].Substring(1).ToLower(CultureInfo.InvariantCulture);
                    string value = "";
                    if (((i + 1) < args.Length) && IsLegalParameterValue(args[i + 1]))
                    {
                        value = args[++i];
                    }
                    if (keyValues.ContainsKey(key) && (num == 0))
                    {
                        num = 1;
                    }
                    key = key.Trim();
                    value = value.Trim();
                    keyValues[key] = value;
                }
                else if (num == 0)
                {
                    num = 2;
                }
            }
            return num;
        }

        static bool IsLegalParameterValue(string value)
        {
            int result = 0;
            return (!value.StartsWith("-", StringComparison.Ordinal) || ((value.Length > 1) && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result)));
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