using ExtECRMainLogic.Classes.Loggers;
using ExtECRMainLogic.Hubs;
using ExtECRMainLogic.Models.ConfigurationModels;
using ExtECRMainLogic.Models.PrinterModels;
using ExtECRMainLogic.Models.TemplateModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ExtECRMainLogic.Classes
{
    public class ExtECRInitializer
    {
        /// <summary>
        /// Contains the contents of InstallationData.xml: Extecr communication settings and the list of instances (InstallationData).
        /// </summary>
        public InstallationDataMaster InstallationMasterData;
        /// <summary>
        /// The list of instances' description (InstallationData). InstallationData are elements in InstallationData.xml.
        /// </summary>
        public List<InstallationDataModel> InstallationData;
        /// <summary>
        /// The list of available Printers (Esc Chars included) from PrintersXML.xml.
        /// </summary>
        public List<Printer> AvailEscCharsTemplates;
        /// <summary>
        /// The dictionary of Print Templates from template xml files. [Key: xml filename, Value: an RollerTypeReportModel object (contents of xml)]
        /// </summary>
        public Dictionary<string, RollerTypeReportModel> AvailableTemplates;
        /// <summary>
        /// The list of all available printers (print settings), as they appear in "ExtECR Settings"/"Print Settings", for all available instances.
        /// </summary>
        public List<KitchenPrinterModel> AvailablePrinters;
        /// <summary>
        /// The list of global initialization errors.
        /// </summary>
        private List<string> GlobalErrors;
        /// <summary>
        /// Application path
        /// </summary>
        private string applicationPath;
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<ExtECRInitializer> logger;
        /// <summary>
        /// Local hub invoker
        /// </summary>
        private readonly LocalHubInvoker localHubInvoker;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="localHubInvoker"></param>
        public ExtECRInitializer(ILogger<ExtECRInitializer> logger, LocalHubInvoker localHubInvoker)
        {
            this.InstallationData = new List<InstallationDataModel>();
            this.InstallationMasterData = new InstallationDataMaster();
            this.AvailEscCharsTemplates = new List<Printer>();
            this.AvailableTemplates = new Dictionary<string, RollerTypeReportModel>();
            this.AvailablePrinters = new List<KitchenPrinterModel>();
            this.GlobalErrors = new List<string>();
            this.logger = logger;
            this.localHubInvoker = localHubInvoker;
        }

        /// <summary>
        /// Load data from config xml files:
        /// 1. Deserialize from InstallationData.xml. Data are stored into object and list: InstallationMasterData and InstallationData
        /// 2. Load printers and escape characters from PrintersXML.xml. Data are stored into list: AvailEscCharsTemplates
        /// 3. Load print templates from Templates directory. Data are stored into dictionary: AvailableTemplates [ Key: xml filename, Value: a RollerTypeReportModel object (contents of xml)]
        /// 4. Read previously printed receipts for the current date. Older receipts are stored into list: ReceiptResultsList
        /// 5. Initialize HitSpool data from HitSpoolXML.xml file. HitSpoolXML.xml contains user's configuration from 'Print Settings'. Data are stored into list: AvailablePrinters
        /// </summary>
        public void InitializeExtECR(string applicationPath)
        {
            logger.LogInformation("Initializing ExtECRInitializer...");
            try
            {
                this.applicationPath = applicationPath;
                GetInstallationData();
                LoadAvailablePrinters();
                LoadAvailableTemplates();
                // must be after installation data
                InitializeHitSpool();
                ShowTipsIfError();
            }
            catch (Exception exception)
            {
                logger.LogError("Error. Fix the problem and restart the Service." + Environment.NewLine + exception.ToString());
            }
        }

        /// <summary>
        /// Get installation master data
        /// </summary>
        /// <returns></returns>
        public InstallationDataMaster GetInstallationMasterData()
        {
            return InstallationMasterData;
        }

        /// <summary>
        /// Get available printers for fiscal id
        /// </summary>
        /// <param name="fiscalId"></param>
        /// <returns></returns>
        public List<KitchenPrinterModel> GetAvailablePrintersByFiscaId(Guid fiscalId)
        {
            return AvailablePrinters.Where(f => f.FiscalId == fiscalId).ToList();
        }

        /// <summary>
        /// Get available printers (Esc Chars included)
        /// </summary>
        /// <returns></returns>
        public List<Printer> GetAvailEscCharsTemplates()
        {
            return AvailEscCharsTemplates;
        }

        /// <summary>
        /// Get available templates
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, RollerTypeReportModel> GetAvailableTemplates()
        {
            return AvailableTemplates;
        }

        /// <summary>
        /// Deserialize from InstallationData.xml. Data are stored into lists: InstallationMasterData and InstallationData
        /// </summary>
        private void GetInstallationData()
        {
            if (InstallationData != null && InstallationData.Count > 0)
                logger.LogInformation("InstallationData (list of instanses) before reading xml file: " + string.Join(", ", InstallationData));
            try
            {
                lock (this)
                {
                    string[] pathComponents = new string[3] { "Config", "Settings", "InstallationData.xml" };
                    using (TextReader textReader = new StreamReader(GetExecutingPath(pathComponents)))
                    {
                        // deserialize from installation data file
                        XmlSerializer deserializer = new XmlSerializer(typeof(InstallationDataMaster));
                        InstallationMasterData = (InstallationDataMaster)deserializer.Deserialize(textReader);
                        InstallationData = null;
                        InstallationData = InstallationMasterData.ExtcerDetailsList;
                        deserializer = null;
                    }
                }
                logger.LogInformation(">>>> InstallationData.xml has been read. InstallationData (list of instances): " + string.Join(", ", InstallationData));
            }
            catch (IOException exception)
            {
                GlobalErrors.Add("Could not access InstallationData.xml");
                logger.LogError(ExtcerLogger.Log("Error accessing InstallationData.xml: ", exception.Message));
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.Log("Error deserializing InstallationData.xml: ", exception.Message));
                ShowTipsIfError();
            }
        }

        /// <summary>
        /// Load printers & escape characters from PrintersXML.xml. Data are stored into list: AvailEscCharsTemplates
        /// </summary>
        private void LoadAvailablePrinters()
        {
            AvailEscCharsTemplates = new List<Printer>();
            try
            {
                // deserialize from item type group file
                XmlSerializer deserializer = new XmlSerializer(typeof(Printers));
                string[] pathComponents = new string[3] { "Config", "Settings", "PrintersXML.xml" };
                TextReader textReader = new StreamReader(GetExecutingPath(pathComponents));
                var printers = (Printers)deserializer.Deserialize(textReader);
                foreach (var item in printers.PrinterList)
                {
                    AvailEscCharsTemplates.Add(item);
                }
                textReader.Close();
            }
            catch (IOException exception)
            {
                GlobalErrors.Add("Could not access PrintersXML.xml");
                logger.LogError(ExtcerLogger.Log("Error accessing PrintersXML.xml: ", exception.Message));
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.Log("Error deserializing PrintersXML.xml: ", exception.Message));
            }
        }

        /// <summary>
        /// Load print templates (xml files) from Templates directory. Data are stored into dictionary: AvailableTemplates [ Key: xml filename, Value: an RollerTypeReportModel object (contents of xml)]
        /// </summary>
        private void LoadAvailableTemplates()
        {
            AvailableTemplates = new Dictionary<string, RollerTypeReportModel>();
            // get the applications directory
            string[] pathComponents = new string[2] { "Config", "Templates" };
            string templatesDir = GetExecutingPath(pathComponents);
            // if the templates directory exists
            if (Directory.Exists(templatesDir))
            {
                // get the files in templates directory
                var templateFilesPaths = Directory.EnumerateFiles(templatesDir, "*.xml");
                // if templates directory has files
                if (templateFilesPaths.Count() > 0)
                {
                    // loop through template files and add the names of the files in printer template names list
                    foreach (var item in templateFilesPaths)
                    {
                        string filenameNoExtention = string.Empty;
                        string filename = string.Empty;
                        try
                        {
                            // get current file name with no extension
                            filenameNoExtention = Path.GetFileNameWithoutExtension(item);
                            filename = Path.GetFileName(item);
                            // deserialize from item type group file
                            XmlSerializer deserializer = new XmlSerializer(typeof(RollerTypeReportModel));
                            TextReader textReader = new StreamReader(item);
                            var curTemplate = (RollerTypeReportModel)deserializer.Deserialize(textReader);
                            textReader.Close();
                            // add current filename to the PrinterTemplateNamesList
                            AvailableTemplates.Add(filenameNoExtention, curTemplate);
                        }
                        catch (Exception exception)
                        {
                            logger.LogError(ExtcerLogger.Log("Error adding " + filenameNoExtention + " template. ", exception.Message));
                        }
                    }
                }
                templateFilesPaths = Directory.EnumerateFiles(templatesDir, "*.rpt");
                // if templates directory has files
                if (templateFilesPaths.Count() > 0)
                {
                    // loop through Crystal Report files and add the names of the files in printer template names list
                    RollerTypeReportModel emptyRoller = new RollerTypeReportModel();
                    emptyRoller.PrintType = 2;
                    foreach (var item in templateFilesPaths)
                    {
                        string filenameNoExtention = string.Empty;
                        string filename = string.Empty;
                        try
                        {
                            // get current file name with no extension
                            filenameNoExtention = Path.GetFileNameWithoutExtension(item);
                            filename = Path.GetFileName(item);
                            // add current filename to the PrinterTemplateNamesList
                            AvailableTemplates.Add(filenameNoExtention, emptyRoller);
                        }
                        catch (Exception exception)
                        {
                            logger.LogError(ExtcerLogger.Log("Error adding " + filenameNoExtention + " CR_template. ", exception.Message));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialize HitSpool data from HitSpoolXML.xml file. HitSpoolXML.xml contains user's configuration from 'Print Settings'. Data are stored into list: AvailablePrinters
        /// </summary>
        private void InitializeHitSpool()
        {
            string[] pathComponents = new string[3] { "Config", "Settings", "HitSpoolXML.xml" };
            string appPath = GetExecutingPath(pathComponents);
            // if HitSpool path is set in installation data
            if (File.Exists(appPath))
            {
                // clear kitchen printers list
                AvailablePrinters.Clear();
                // get HitSpool path from installation data
                string HitSpoolPath = appPath;
                try
                {
                    using (TextReader textReader = new StreamReader(HitSpoolPath))
                    {
                        // read HitSpool data from given file list
                        XmlSerializer deserializer = new XmlSerializer(typeof(List<KitchenPrinterModel>));
                        var hitspoolList = (List<KitchenPrinterModel>)deserializer.Deserialize(textReader);
                        AvailablePrinters = hitspoolList;
                    }
                }
                catch (IOException exception)
                {
                    GlobalErrors.Add("Could not access HitSpoolXML.xml");
                    logger.LogError(ExtcerLogger.Log("Error accessing HitSpoolXML.xml: ", exception.Message));
                }
                catch (Exception exception)
                {
                    logger.LogError(ExtcerLogger.Log("Error loading HitSpoolXML.xml: ", exception.Message));
                }
            }
            else
            {
                logger.LogError("Error loading HitSpoolXML.xml: file not found at installation data specified location");
            }
        }

        /// <summary>
        /// Shows errors if there are.
        /// </summary>
        private void ShowTipsIfError()
        {
            if (GlobalErrors.Count > 0)
            {
                string str = "The following errors occurred:\n";
                foreach (var err in GlobalErrors)
                {
                    str += err + "\n";
                }
                localHubInvoker.SendError(str);
            }
        }

        /// <summary>
        /// Get path from components
        /// </summary>
        /// <param name="pathDestinations"></param>
        /// <returns></returns>
        private string GetExecutingPath(string[] pathDestinations)
        {
            List<string> pathComponents = new List<string>();
            pathComponents.Add(applicationPath);
            foreach (string destination in pathDestinations)
            {
                pathComponents.Add(destination);
            }
            string path = Path.GetFullPath(Path.Combine(pathComponents.ToArray()));
            return path;
        }
    }
}