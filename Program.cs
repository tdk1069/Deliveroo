using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.DirectoryServices.AccountManagement;
using NLog;

namespace Deliveroo
{
    internal class Program
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
        var arguments = new Dictionary<string, string>();
            foreach (string argument in args)
            {
                string[] splitted = argument.Split('=');

                if (splitted.Length == 2)
                {
                    arguments[splitted[0].ToLower()] = splitted[1];
                }
            }
            string domain;
            string gpoGUID;
            if (arguments.ContainsKey("domain")) 
            {
                domain = arguments["domain"];
            } else { 
                domain = "bkhs.internal"; 
            };
            if (arguments.ContainsKey("gpo"))
            {
                gpoGUID = arguments["gpo"];
            }
            else
            {
                gpoGUID = "{F927136B-03C3-4B63-A262-8F4E1AB240DA}";
            };
            //parseJson("\\\\bkhs.internal\\packageshare$\\Printers.json");

            getGPO(domain, gpoGUID);
        }

        static void getGPO(string domain, string policyGUID)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            _log.Info("Reading GPP from " + domain + " " + policyGUID);
            // $domain = "bkhs.internal"
            //$policyGUID = "{F927136B-03C3-4B63-A262-8F4E1AB240DA}"
            if (!File.Exists("\\\\" + domain + "\\sysvol\\" + domain + "\\Policies\\" + policyGUID + "\\User\\Preferences\\Printers\\Printers.xml"))
            {
                _log.Info("GPO Not found");
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("\\\\" + domain + "\\sysvol\\" + domain + "\\Policies\\" + policyGUID + "\\User\\Preferences\\Printers\\Printers.xml");
            XmlNodeList SharedPrinter = xmlDoc.GetElementsByTagName("SharedPrinter");
            foreach (XmlNode Printer in SharedPrinter)
            {
                string prnName = Printer.Attributes["name"].Value;
                string prnPath = Printer["Properties"].Attributes["path"].Value;
                string prnAction = Printer["Properties"].Attributes["action"].Value;
//                string prnGroup = Console.WriteLine(Printer["Filters"]["FilterGroup"].Attributes["name"].Value);
                GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, Printer["Filters"]["FilterGroup"].Attributes["name"].Value);
                if (group != null)
                {
                    foreach (Principal p in group.GetMembers())
                    {
                        if (System.Environment.MachineName.ToLower() == p.Name.ToLower())
                        {
                            _log.Info("Adding {0}: {1} -> {2}", p.StructuralObjectClass, p.Name, prnPath);
                            addNetworkPrinterPath(prnPath);
                        }
                    }
                }
            }
        }

        static void parseJson(string file)
        {
            using (StreamReader r = new StreamReader(file))
            {
                string json = r.ReadToEnd();
                dynamic items = JsonConvert.DeserializeObject(json);
                foreach(dynamic item in items)
                {
                    Regex pattern = new Regex(item["Pattern"].ToString());
                    if (pattern.IsMatch(System.Environment.MachineName.ToLower())) 
                    { 
                        addNetworkPrinter(item.Server.ToString(), item.ShareName.ToString());
                    } 
                }
            }
        }
        private static void addNetworkPrinterPath(string prnPath)
        {
            using (ManagementClass win32Printer = new ManagementClass("Win32_Printer"))
            {
                using (ManagementBaseObject inputParam =
                   win32Printer.GetMethodParameters("AddPrinterConnection"))
                {
                    // Replace <server_name> and <printer_name> with the actual server and
                    // printer names.
                    inputParam.SetPropertyValue("Name", prnPath);
                    using (ManagementBaseObject result =
                        (ManagementBaseObject)win32Printer.InvokeMethod("AddPrinterConnection", inputParam, null))
                    {
                        uint errorCode = (uint)result.Properties["returnValue"].Value;

                        switch (errorCode)
                        {
                            case 0:
                                _log.Info("Successfully connected printer.");
                                break;
                            case 5:
                                _log.Error("Access Denied.");
                                break;
                            case 123:
                                _log.Error("The filename, directory name, or volume label syntax is incorrect.");
                                break;
                            case 1801:
                                _log.Error("Invalid Printer Name.");
                                break;
                            case 1930:
                                _log.Error("Incompatible Printer Driver.");
                                break;
                            case 3019:
                                _log.Error("The specified printer driver was not found on the system and needs to be downloaded.");
                                break;
                        }
                    }
                }
            }

        }

        private static void addNetworkPrinter(string server, string printer)
        {
            using (ManagementClass win32Printer = new ManagementClass("Win32_Printer"))
            {
                using (ManagementBaseObject inputParam =
                   win32Printer.GetMethodParameters("AddPrinterConnection"))
                {
                    // Replace <server_name> and <printer_name> with the actual server and
                    // printer names.
                    inputParam.SetPropertyValue("Name", "\\\\" + server + "\\" + printer);
                    using (ManagementBaseObject result =
                        (ManagementBaseObject)win32Printer.InvokeMethod("AddPrinterConnection", inputParam, null))
                    {
                        uint errorCode = (uint)result.Properties["returnValue"].Value;

                        switch (errorCode)
                        {
                            case 0:
                                Console.Out.WriteLine("Successfully connected printer.");
                                break;
                            case 5:
                                Console.Out.WriteLine("Access Denied.");
                                break;
                            case 123:
                                Console.Out.WriteLine("The filename, directory name, or volume label syntax is incorrect.");
                                break;
                            case 1801:
                                Console.Out.WriteLine("Invalid Printer Name.");
                                break;
                            case 1930:
                                Console.Out.WriteLine("Incompatible Printer Driver.");
                                break;
                            case 3019:
                                Console.Out.WriteLine("The specified printer driver was not found on the system and needs to be downloaded.");
                                break;
                        }
                    }
                }
            }

        }

    }
}
