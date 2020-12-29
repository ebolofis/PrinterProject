using ExtECRMainLogic.Classes.ExtECR.Types;
using ExtECRMainLogic.Classes.Extenders;
using ExtECRMainLogic.Classes.Helpers;
using ExtECRMainLogic.Classes.Loggers;
using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Exceptions;
using ExtECRMainLogic.Hubs;
using ExtECRMainLogic.Models.CommunicationModels;
using ExtECRMainLogic.Models.ConfigurationModels;
using ExtECRMainLogic.Models.DriverModels;
using ExtECRMainLogic.Models.HTMLReceiptModels;
using ExtECRMainLogic.Models.KitchenModels;
using ExtECRMainLogic.Models.LCDModels;
using ExtECRMainLogic.Models.PrinterModels;
using ExtECRMainLogic.Models.ReceiptModels;
using ExtECRMainLogic.Models.ReportModels;
using ExtECRMainLogic.Models.ReservationModels;
using ExtECRMainLogic.Models.TemplateModels;
using ExtECRMainLogic.Models.ZReportModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ExtECRMainLogic.Classes.ExtECR
{
    public class ExtECR_Class
    {
        #region Print Settings
        /// <summary>
        /// The list of instances' description (InstallationData). InstallationData are elements in InstallationData.xml.
        /// </summary>
        public InstallationDataModel installationData;
        /// <summary>
        /// The list of all available printers (print settings), as they appear in "ExtECR Settings"/"Print Settings", for all available instances.
        /// </summary>
        public List<KitchenPrinterModel> availablePrinters;
        /// <summary>
        /// The list of available Printers (Esc Chars included) from PrintersXML.xml.
        /// </summary>
        public List<Printer> AvailEscCharsTemplates;
        /// <summary>
        /// The dictionary of Print Templates from template xml files. [Key: xml filename, Value: an RollerTypeReportModel object (contents of xml)]
        /// </summary>
        public Dictionary<string, RollerTypeReportModel> AvailableTemplates;
        /// <summary>
        /// Fiscal name
        /// </summary>
        public string FiscalName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private FiscalManager KitchenObj;
        /// <summary>
        /// 
        /// </summary>
        private FiscalManager RepObj;
        /// <summary>
        /// 
        /// </summary>
        public FiscalManager FmObj;
        /// <summary>
        /// 
        /// </summary>
        PrintResultModel lastReceipt;
        /// <summary>
        /// 
        /// </summary>
        byte[] imageByteArray;
        #endregion
        #region Communication
        /// <summary>
        /// Communication settings
        /// </summary>
        private CommunicationSettingsModel communicationSettings;
        /// <summary>
        /// SignalR communication
        /// </summary>
        private SignalR_CommsClass signalRCommunication;
        #endregion
        #region HTML Receipt
        /// <summary>
        /// HTML receipt settings
        /// </summary>
        private HTMLReceiptSettingsModel htmlReceiptSettings;
        #endregion
        #region EFTPOS
        /// <summary>
        /// EFTPOS settings
        /// </summary>
        private EFTPOSSettingsModel eftposSettings;
        #endregion
        #region LCD
        /// <summary>
        /// LCD settings
        /// </summary>
        private LCDSettingsModel lcdSettings;
        #endregion
        #region Scale
        /// <summary>
        /// Scale settings
        /// </summary>
        private ScaleSettingsModel scaleSettings;
        /// <summary>
        /// Used to repeat polling the scale for valid measures
        /// </summary>
        private int repeatLoopScaleMeasure;
        #endregion
        /// <summary>
        /// Application path
        /// </summary>
        private readonly string applicationPath;
        /// <summary>
        /// Application builder
        /// </summary>
        private readonly IApplicationBuilder applicationBuilder;
        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration configuration;
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<ExtECR_Class> logger;
        /// <summary>
        /// Local hub invoker
        /// </summary>
        private readonly LocalHubInvoker localHubInvoker;
        /// <summary>
        /// ExtECR displayer
        /// </summary>
        private readonly ExtECRDisplayer extecrDisplayer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="installationData"></param>
        /// <param name="availPrinters"></param>
        /// <param name="availEscChars"></param>
        /// <param name="availTemplates"></param>
        /// <param name="communicationSettings"></param>
        /// <param name="htmlReceiptSettings"></param>
        /// <param name="eftposSettings"></param>
        /// <param name="lcdSettings"></param>
        /// <param name="scaleSettings"></param>
        /// <param name="applicationPath"></param>
        /// <param name="configuration"></param>
        /// <param name="applicationBuilder"></param>
        public ExtECR_Class(InstallationDataModel installationData, List<KitchenPrinterModel> availPrinters, List<Printer> availEscChars, Dictionary<string, RollerTypeReportModel> availTemplates, CommunicationSettingsModel communicationSettings, HTMLReceiptSettingsModel htmlReceiptSettings, EFTPOSSettingsModel eftposSettings, LCDSettingsModel lcdSettings, ScaleSettingsModel scaleSettings, string applicationPath, IConfiguration configuration, IApplicationBuilder applicationBuilder)
        {
            this.installationData = installationData;
            this.availablePrinters = availPrinters;
            this.AvailEscCharsTemplates = availEscChars;
            this.AvailableTemplates = availTemplates;
            this.FiscalName = installationData.FiscalName;
            this.communicationSettings = communicationSettings;
            this.htmlReceiptSettings = htmlReceiptSettings;
            this.eftposSettings = eftposSettings;
            this.lcdSettings = lcdSettings;
            this.scaleSettings = scaleSettings;
            this.applicationPath = applicationPath;
            this.configuration = configuration;
            this.applicationBuilder = applicationBuilder;
            this.logger = (ILogger<ExtECR_Class>)applicationBuilder.ApplicationServices.GetService(typeof(ILogger<ExtECR_Class>));
            this.localHubInvoker = (LocalHubInvoker)applicationBuilder.ApplicationServices.GetService(typeof(LocalHubInvoker));
            this.extecrDisplayer = (ExtECRDisplayer)applicationBuilder.ApplicationServices.GetService(typeof(ExtECRDisplayer));
        }

        /// <summary>
        /// Initialize the driver.
        /// </summary>
        public void InitializeDriver()
        {
            string errorInstance = string.Empty;
            try
            {
                // LOAD KITCHEN PRINTER DATA TEMPLATES
                errorInstance = "load kitchen template";
                var kitchenTemplates = LoadPrintTemplate(PrinterTypeEnum.Kitchen);
                // LOAD KITCHEN PRINTERS
                errorInstance = "load kitchen printer";
                var availKitchenPrinters = GetPrintersOfType(PrinterTypeEnum.Kitchen);
                // CREATE GENERIC PRINTER
                errorInstance = "call kitchen generic constructor";
                KitchenObj = new GenericExtcer(kitchenTemplates, AvailEscCharsTemplates, availKitchenPrinters, installationData, FiscalName, applicationPath, configuration, applicationBuilder);
                ExtcerTypesEnum extcerType = installationData.ExtcerType ?? ExtcerTypesEnum.Generic;
                // LOAD PRINTER DATA ACCORDING TO extcerType-> Generic/OPOS
                switch (extcerType)
                {
                    case ExtcerTypesEnum.EpsonFiscal:
                        Initialize_EpsonFiscal();
                        break;
                    case ExtcerTypesEnum.HDM:
                        Initialize_HDMFiscal();
                        break;
                    case ExtcerTypesEnum.Opos:
                        Initialize_OPOS();
                        break;
                    case ExtcerTypesEnum.Opos3:
                        Initialize_OPOS3();
                        break;
                    case ExtcerTypesEnum.SynergyPF500:
                        Initialize_SynergyPF500();
                        break;
                    case ExtcerTypesEnum.ZipDitron:
                        Initialize_ZipDitron();
                        break;
                    case ExtcerTypesEnum.RBS:
                        Initialize_RBS();
                        break;
                    case ExtcerTypesEnum.Generic:
                    default:
                        Initialize_Generic();
                        break;
                }
                // INITIALIZE SIGNALR CONNECTION
                string connectionHub = configuration.GetValue<string>("SignalRHub");
                signalRCommunication = new SignalR_CommsClass(communicationSettings.connectionUrl, connectionHub, communicationSettings.connectionStoreId, FiscalName, applicationBuilder);
                signalRCommunication.InitializeHubAsync(OnHub_NewReceipt, OnHub_NewTableReservation, OnHub_PrintItem, OnHub_PartialPrintConnectivity, OnHub_ConnectedUsers, OnHub_IssueReportZ, OnHub_IssueReportX, OnHub_IssueReport, OnHub_CreditCardAmount, OnHub_Drawer, OnHub_Image, OnHub_Kitchen, OnHub_KitchenInstruction, OnHub_KitchenInstructionLogger, OnHub_LcdMessage, OnHub_StartWeighting, OnHub_StopWeighting, OnHub_HeartBeat);
            }
            catch (Exception exception)
            {
                logger.LogError("Initialize driver:  Error location: " + errorInstance + " Message: " + exception.ToString());
            }
        }

        #region Print Initialization

        /// <summary>
        /// Initialize epson
        /// </summary>
        private void Initialize_EpsonFiscal()
        {
            string errorInstance = string.Empty;
            try
            {
                // Add associated templates
                var availTemplates = new Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>>();
                // Add receipt templates
                errorInstance = "EpsonFiscal load receipt template";
                var receiptTemplate = LoadPrintTemplate(PrinterTypeEnum.Receipt);
                foreach (var item in receiptTemplate)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add void templates
                errorInstance = "EpsonFiscal load void template";
                var voidTemplates = LoadPrintTemplate(PrinterTypeEnum.Void);
                foreach (var item in voidTemplates)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add general template
                errorInstance = "EpsonFiscal load general template";
                var generalTemplate = LoadPrintTemplate(PrinterTypeEnum.General).FirstOrDefault();
                if (generalTemplate.Key != null && !availTemplates.ContainsKey(generalTemplate.Key))
                {
                    availTemplates.Add(generalTemplate.Key, generalTemplate.Value);
                }
                // Add reports templates
                errorInstance = "EpsonFiscal load reports template";
                var reportsTemplates = LoadPrintTemplate(PrinterTypeEnum.Report);
                foreach (var item in reportsTemplates)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add z report template
                errorInstance = "EpsonFiscal load Z report template";
                var ZTemplate = LoadPrintTemplate(PrinterTypeEnum.ZReport).FirstOrDefault();
                if (ZTemplate.Key != null && !availTemplates.ContainsKey(ZTemplate.Key))
                {
                    availTemplates.Add(ZTemplate.Key, ZTemplate.Value);
                }
                // Add receipt preview template for EpsonFiscal
                errorInstance = "EpsonFiscal load OPOSPreview template";
                var oposPreviewTemplate = LoadPrintTemplate(PrinterTypeEnum.OposPreview).FirstOrDefault();
                if (oposPreviewTemplate.Key != null && !availTemplates.ContainsKey(oposPreviewTemplate.Key))
                {
                    availTemplates.Add(oposPreviewTemplate.Key, oposPreviewTemplate.Value);
                }
                // Add associated printers
                var availPrinters = new List<KitchenPrinterModel>();
                // Add receipt printers
                errorInstance = "EpsonFiscal load receipt printer";
                availPrinters = GetPrintersOfType(PrinterTypeEnum.Receipt);
                // Add void printers
                errorInstance = "EpsonFiscal load void printer";
                availPrinters.AddRange(GetPrintersOfType(PrinterTypeEnum.Void));
                // Add general printer
                var generalPrinter = GetPrintersOfType(PrinterTypeEnum.General).FirstOrDefault();
                if (generalPrinter != null)
                {
                    availPrinters.Add(generalPrinter);
                }
                // Add report printers
                var reportPrinters = GetPrintersOfType(PrinterTypeEnum.Report);
                foreach (var item in reportPrinters)
                {
                    if (item != null && !availPrinters.Contains(item))
                    {
                        availPrinters.Add(item);
                    }
                }
                // Add z report printer
                errorInstance = "EpsonFiscal load Z printer";
                var zTempl = GetPrintersOfType(PrinterTypeEnum.ZReport).FirstOrDefault();
                if (zTempl != null)
                {
                    availPrinters.Add(zTempl);
                }
                errorInstance = "EpsonFiscal call EpsonFiscal constructor";
                // For general printer enum type, non theorimenes apodeikseis
                RepObj = new GenericExtcer(availTemplates, AvailEscCharsTemplates, availPrinters, installationData, FiscalName, applicationPath, configuration, applicationBuilder);
                logger.LogInformation("Rep Printer Created");
                // For the rest printer enum types,theorimenes apodeikseis
                FmObj = new EpsonFiscalExtcer(availTemplates, AvailEscCharsTemplates, availPrinters, installationData, FiscalName, applicationPath, configuration, applicationBuilder);
                logger.LogInformation(FiscalName + " is initialized.");
            }
            catch (Exception exception)
            {
                logger.LogError("Initialize driver:  errorLocation: " + errorInstance + ", Message: " + exception.ToString());
            }
        }

        /// <summary>
        /// Initialize hdm
        /// </summary>
        private void Initialize_HDMFiscal()
        {
            string errorInstance = string.Empty;
            try
            {
                // Add associated templates
                var availTemplates = new Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>>();
                // Add receipt templates
                errorInstance = "HDMFiscal load receipt template";
                var receiptTemplate = LoadPrintTemplate(PrinterTypeEnum.Receipt);
                foreach (var item in receiptTemplate)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add void template
                errorInstance = "HDMFiscal load void template";
                var voidTemplate = LoadPrintTemplate(PrinterTypeEnum.Void).FirstOrDefault();
                if (voidTemplate.Key != null && !availTemplates.ContainsKey(voidTemplate.Key))
                {
                    availTemplates.Add(voidTemplate.Key, voidTemplate.Value);
                }
                // Add general template
                errorInstance = "HDMFiscal load general template";
                var generalTemplate = LoadPrintTemplate(PrinterTypeEnum.General).FirstOrDefault();
                if (generalTemplate.Key != null && !availTemplates.ContainsKey(generalTemplate.Key))
                {
                    availTemplates.Add(generalTemplate.Key, generalTemplate.Value);
                }
                // Add reports templates
                errorInstance = "HDMFiscal load reports template";
                var reportsTemplates = LoadPrintTemplate(PrinterTypeEnum.Report);
                foreach (var item in reportsTemplates)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add z report template
                errorInstance = "HDMFiscal load Z report template";
                var ZTemplate = LoadPrintTemplate(PrinterTypeEnum.ZReport).FirstOrDefault();
                if (ZTemplate.Key != null && !availTemplates.ContainsKey(ZTemplate.Key))
                {
                    availTemplates.Add(ZTemplate.Key, ZTemplate.Value);
                }
                // Add receipt preview template for HDMFiscal
                errorInstance = "HDMFiscal load OPOSPreview template";
                var oposPreviewTemplate = LoadPrintTemplate(PrinterTypeEnum.OposPreview).FirstOrDefault();
                if (oposPreviewTemplate.Key != null && !availTemplates.ContainsKey(oposPreviewTemplate.Key))
                {
                    availTemplates.Add(oposPreviewTemplate.Key, oposPreviewTemplate.Value);
                }
                // Add associated printers
                var availPrinters = new List<KitchenPrinterModel>();
                // Add receipt printers
                errorInstance = "HDMFiscal load receipt printer";
                availPrinters = GetPrintersOfType(PrinterTypeEnum.Receipt);
                // Add void printers
                errorInstance = "HDMFiscal load void printer";
                availPrinters.AddRange(GetPrintersOfType(PrinterTypeEnum.Void));
                // Add general printer
                var generalPrinter = GetPrintersOfType(PrinterTypeEnum.General).FirstOrDefault();
                if (generalPrinter != null)
                {
                    availPrinters.Add(generalPrinter);
                }
                // Add report printers
                var reportPrinters = GetPrintersOfType(PrinterTypeEnum.Report);
                foreach (var item in reportPrinters)
                {
                    if (item != null && !availPrinters.Contains(item))
                    {
                        availPrinters.Add(item);
                    }
                }
                // Add z report printer
                errorInstance = "HDMFiscal load Z printer";
                var zTempl = GetPrintersOfType(PrinterTypeEnum.ZReport).FirstOrDefault();
                if (zTempl != null)
                {
                    availPrinters.Add(zTempl);
                }
                errorInstance = "HDMFiscal call HDMFiscal constructor";
                // For general printer enum type, non theorimenes apodeikseis
                RepObj = new GenericExtcer(availTemplates, AvailEscCharsTemplates, availPrinters, installationData, FiscalName, applicationPath, configuration, applicationBuilder);
                logger.LogInformation("Rep Printer Created");
                // For the rest printer enum types,theorimenes apodeikseis
                FmObj = new HDMExtcer(availTemplates, AvailEscCharsTemplates, availPrinters, installationData, FiscalName, applicationPath, configuration, applicationBuilder);
                logger.LogInformation(FiscalName + " is initialized.");
            }
            catch (Exception exception)
            {
                logger.LogError("Initialize driver:  errorLocation: " + errorInstance + ", Message: " + exception.ToString());
            }
        }

        /// <summary>
        /// Initialize opos
        /// </summary>
        private void Initialize_OPOS()
        {
            string errorInstance = string.Empty;
            try
            {
                // Add associated templates
                var availTemplates = new Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>>();
                // Add receipt templates
                errorInstance = "OPOS load receipt template";
                var receiptTemplate = LoadPrintTemplate(PrinterTypeEnum.Receipt);
                foreach (var item in receiptTemplate)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add void templates
                errorInstance = "OPOS load void template";
                var voidTemplates = LoadPrintTemplate(PrinterTypeEnum.Void);
                foreach (var item in voidTemplates)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add general template
                errorInstance = "OPOS load general template";
                var generalTemplate = LoadPrintTemplate(PrinterTypeEnum.General).FirstOrDefault();
                if (generalTemplate.Key != null && !availTemplates.ContainsKey(generalTemplate.Key))
                {
                    availTemplates.Add(generalTemplate.Key, generalTemplate.Value);
                }
                // Add reports templates
                errorInstance = "OPOS load reports template";
                var reportsTemplates = LoadPrintTemplate(PrinterTypeEnum.Report);
                foreach (var item in reportsTemplates)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add z report template
                errorInstance = "OPOS load Z report template";
                var ZTemplate = LoadPrintTemplate(PrinterTypeEnum.ZReport).FirstOrDefault();
                if (ZTemplate.Key != null && !availTemplates.ContainsKey(ZTemplate.Key))
                {
                    availTemplates.Add(ZTemplate.Key, ZTemplate.Value);
                }
                // Add receipt preview template for OPOS
                errorInstance = "OPOS load OPOSPreview template";
                var oposPreviewTemplate = LoadPrintTemplate(PrinterTypeEnum.OposPreview).FirstOrDefault();
                if (oposPreviewTemplate.Key != null && !availTemplates.ContainsKey(oposPreviewTemplate.Key))
                {
                    availTemplates.Add(oposPreviewTemplate.Key, oposPreviewTemplate.Value);
                }
                // Add associated printers
                var availPrinters = new List<KitchenPrinterModel>();
                // Add receipt printers
                errorInstance = "OPOS load receipt printer";
                availPrinters = GetPrintersOfType(PrinterTypeEnum.Receipt);
                // Add void printers
                errorInstance = "OPOS load void printer";
                availPrinters.AddRange(GetPrintersOfType(PrinterTypeEnum.Void));
                // Add general printer
                var generalPrinter = GetPrintersOfType(PrinterTypeEnum.General).FirstOrDefault();
                if (generalPrinter != null)
                {
                    availPrinters.Add(generalPrinter);
                }
                // Add report printers
                var reportPrinters = GetPrintersOfType(PrinterTypeEnum.Report);
                foreach (var item in reportPrinters)
                {
                    if (item != null && !availPrinters.Contains(item))
                    {
                        availPrinters.Add(item);
                    }
                }
                // Add z report printer
                errorInstance = "OPOS load Z printer";
                var zTempl = GetPrintersOfType(PrinterTypeEnum.ZReport).FirstOrDefault();
                if (zTempl != null)
                {
                    availPrinters.Add(zTempl);
                }
                errorInstance = "OPOS call OPOS constructor";
                // For general printer enum type, non theorimenes apodeikseis
                RepObj = new GenericExtcer(availTemplates, AvailEscCharsTemplates, availPrinters, installationData, FiscalName, applicationPath, configuration, applicationBuilder);
                logger.LogInformation("Rep Printer Created");
                // For the rest printer enum types,theorimenes apodeikseis
                FmObj = new OposExtcer(availTemplates, AvailEscCharsTemplates, availPrinters, installationData, FiscalName, applicationPath, configuration, applicationBuilder, installationData.OposDeviceName);
                logger.LogInformation(FiscalName + " is initialized.");
            }
            catch (Exception exception)
            {
                logger.LogError("Initialize driver:  errorLocation: " + errorInstance + " Message: " + exception.ToString());
            }
        }

        /// <summary>
        /// Initialize opos3
        /// </summary>
        private void Initialize_OPOS3()
        {
            string errorInstance = string.Empty;
            try
            {
                // Add associated templates
                var availTemplates = new Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>>();
                // Add receipt templates
                errorInstance = "OPOS3 load receipt template";
                var receiptTemplate = LoadPrintTemplate(PrinterTypeEnum.Receipt);
                foreach (var item in receiptTemplate)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add void templates
                errorInstance = "OPOS3 load void template";
                var voidTemplates = LoadPrintTemplate(PrinterTypeEnum.Void);
                foreach (var item in voidTemplates)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add general template
                errorInstance = "OPOS3 load general template";
                var generalTemplate = LoadPrintTemplate(PrinterTypeEnum.General).FirstOrDefault();
                if (generalTemplate.Key != null && !availTemplates.ContainsKey(generalTemplate.Key))
                {
                    availTemplates.Add(generalTemplate.Key, generalTemplate.Value);
                }
                // Add reports templates
                errorInstance = "OPOS3 load reports template";
                var reportsTemplates = LoadPrintTemplate(PrinterTypeEnum.Report);
                foreach (var item in reportsTemplates)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add z report template
                errorInstance = "OPOS3 load Z report template";
                var ZTemplate = LoadPrintTemplate(PrinterTypeEnum.ZReport).FirstOrDefault();
                if (ZTemplate.Key != null && !availTemplates.ContainsKey(ZTemplate.Key))
                {
                    availTemplates.Add(ZTemplate.Key, ZTemplate.Value);
                }
                // Add receipt preview template for OPOS3
                errorInstance = "OPOS3 load OPOSPreview template";
                var oposPreviewTemplate = LoadPrintTemplate(PrinterTypeEnum.OposPreview).FirstOrDefault();
                if (oposPreviewTemplate.Key != null && !availTemplates.ContainsKey(oposPreviewTemplate.Key))
                {
                    availTemplates.Add(oposPreviewTemplate.Key, oposPreviewTemplate.Value);
                }
                // Add associated printers
                var availPrinters = new List<KitchenPrinterModel>();
                // Add receipt printers
                errorInstance = "OPOS3 load receipt printer";
                availPrinters = GetPrintersOfType(PrinterTypeEnum.Receipt);
                // Add void printers
                errorInstance = "OPOS3 load void printer";
                availPrinters.AddRange(GetPrintersOfType(PrinterTypeEnum.Void));
                // Add general printer
                var generalPrinter = GetPrintersOfType(PrinterTypeEnum.General).FirstOrDefault();
                if (generalPrinter != null)
                {
                    availPrinters.Add(generalPrinter);
                }
                // Add report printers
                var reportPrinters = GetPrintersOfType(PrinterTypeEnum.Report);
                foreach (var item in reportPrinters)
                {
                    if (item != null && !availPrinters.Contains(item))
                    {
                        availPrinters.Add(item);
                    }
                }
                // Add z report printer
                errorInstance = "OPOS3 load Z printer";
                var zTempl = GetPrintersOfType(PrinterTypeEnum.ZReport).FirstOrDefault();
                if (zTempl != null)
                {
                    availPrinters.Add(zTempl);
                }
                errorInstance = "OPOS3 call OPOS constructor";
                // For general printer enum type, non theorimenes apodeikseis
                RepObj = new GenericExtcer(availTemplates, AvailEscCharsTemplates, availPrinters, installationData, FiscalName, applicationPath, configuration, applicationBuilder);
                logger.LogInformation("Rep Printer Created");
                // For the rest printer enum types,theorimenes apodeikseis
                ServerCommunicationModel serverCommunicationInfo = new ServerCommunicationModel();
                serverCommunicationInfo.connectionUrl = communicationSettings.connectionUrl;
                serverCommunicationInfo.authorizationUsername = communicationSettings.authorizationUsername;
                serverCommunicationInfo.authorizationPassword = communicationSettings.authorizationPassword;
                FmObj = new Opos3Extcer(availTemplates, AvailEscCharsTemplates, availPrinters, installationData, FiscalName, serverCommunicationInfo, applicationPath, configuration, applicationBuilder);
                logger.LogInformation(FiscalName + " is initialized.");
            }
            catch (Exception exception)
            {
                logger.LogError("Initialize driver:  errorLocation: " + errorInstance + " Message: " + exception.ToString());
            }
        }

        /// <summary>
        /// Initialize synergypf500
        /// </summary>
        private void Initialize_SynergyPF500()
        {
            string errorInstance = string.Empty;
            try
            {
                // Add associated templates
                var availTemplates = new Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>>();
                // Add associated printers
                var availPrinters = new List<KitchenPrinterModel>();
                // Add receipt printers
                errorInstance = "SynergyPF500 load receipt printer";
                var rectempl = GetPrintersOfType(PrinterTypeEnum.Receipt);
                if (rectempl != null)
                {
                    availPrinters = rectempl;
                }
                // Add void printers
                errorInstance = "SynergyPF500 load void printer";
                var voidtempls = GetPrintersOfType(PrinterTypeEnum.Void);
                foreach (var item in voidtempls)
                {
                    if (item != null && !availPrinters.Contains(item))
                    {
                        availPrinters.Add(item);
                    }
                }
                // Add general printer
                var generalPrinter = GetPrintersOfType(PrinterTypeEnum.General).FirstOrDefault();
                if (generalPrinter != null)
                {
                    availPrinters.Add(generalPrinter);
                }
                // Add report printers
                var reportPrinters = GetPrintersOfType(PrinterTypeEnum.Report);
                foreach (var item in reportPrinters)
                {
                    if (item != null && !availPrinters.Contains(item))
                    {
                        availPrinters.Add(item);
                    }
                }
                // Add z report printer
                errorInstance = "SynergyPF500 load Z printer";
                var zTempl = GetPrintersOfType(PrinterTypeEnum.ZReport).FirstOrDefault();
                if (zTempl != null)
                {
                    availPrinters.Add(zTempl);
                }
                // Add x report printer
                var xPrinter = GetPrintersOfType(PrinterTypeEnum.XReport).FirstOrDefault();
                if (xPrinter != null)
                {
                    availPrinters.Add(xPrinter);
                }
                errorInstance = "SynergyPF500 call generic constructor";
                // For the rest printer enum types,theorimenes apodeikseis
                FmObj = new SynergyPF500Extcer(availTemplates, AvailEscCharsTemplates, availPrinters, installationData, FiscalName, applicationPath, configuration, applicationBuilder);
                logger.LogInformation(FiscalName + " is initialized.");
            }
            catch (Exception exception)
            {
                logger.LogError("Initialize driver:  errorLocation: " + errorInstance + " Message: " + exception.ToString());
            }
        }

        /// <summary>
        /// Initialize zipditron
        /// </summary>
        private void Initialize_ZipDitron()
        {
            string errorInstance = string.Empty;
            try
            {
                // Add associated templates
                var availTemplates = new Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>>();
                // Add associated printers
                var availPrinters = new List<KitchenPrinterModel>();
                // Add receipt printers
                errorInstance = "ZipDitron load receipt printer";
                var rectempl = GetPrintersOfType(PrinterTypeEnum.Receipt);
                if (rectempl != null)
                {
                    availPrinters = rectempl;
                }
                // Add void printers
                errorInstance = "ZipDitron load void printer";
                var voidtempls = GetPrintersOfType(PrinterTypeEnum.Void);
                foreach (var item in voidtempls)
                {
                    if (item != null && !availPrinters.Contains(item))
                    {
                        availPrinters.Add(item);
                    }
                }
                // Add general printer
                var generalPrinter = GetPrintersOfType(PrinterTypeEnum.General).FirstOrDefault();
                if (generalPrinter != null)
                {
                    availPrinters.Add(generalPrinter);
                }
                // Add report printers
                var reportPrinters = GetPrintersOfType(PrinterTypeEnum.Report);
                foreach (var item in reportPrinters)
                {
                    if (item != null && !availPrinters.Contains(item))
                    {
                        availPrinters.Add(item);
                    }
                }
                // Add z report printer
                errorInstance = "ZipDitron load Z printer";
                var zTempl = GetPrintersOfType(PrinterTypeEnum.ZReport).FirstOrDefault();
                if (zTempl != null)
                {
                    availPrinters.Add(zTempl);
                }
                // Add x report printer
                var xPrinter = GetPrintersOfType(PrinterTypeEnum.XReport).FirstOrDefault();
                if (xPrinter != null)
                {
                    availPrinters.Add(xPrinter);
                }
                errorInstance = "ZipDitron call generic constructor";
                // For the rest printer enum types,theorimenes apodeikseis
                FmObj = new ZipDitronExtcer(availTemplates, AvailEscCharsTemplates, availPrinters, installationData, FiscalName, applicationPath, configuration, applicationBuilder);
                logger.LogInformation(FiscalName + " is initialized.");
            }
            catch (Exception exception)
            {
                logger.LogError("Initialize driver:  errorLocation: " + errorInstance + " Message: " + exception.ToString());
            }
        }

        /// <summary>
        /// Initialize rbs
        /// </summary>
        private void Initialize_RBS()
        {
            string errorInstance = string.Empty;
            try
            {
                // Add associated templates
                var availTemplates = new Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>>();
                // Add receipt templates
                errorInstance = "RBS load receipt template";
                var receiptTemplate = LoadPrintTemplate(PrinterTypeEnum.Receipt);
                foreach (var item in receiptTemplate)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add void templates
                errorInstance = "RBS load void template";
                var voidTemplates = LoadPrintTemplate(PrinterTypeEnum.Void);
                foreach (var item in voidTemplates)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add general template
                errorInstance = "RBS load general template";
                var generalTemplate = LoadPrintTemplate(PrinterTypeEnum.General).FirstOrDefault();
                if (generalTemplate.Key != null && !availTemplates.ContainsKey(generalTemplate.Key))
                {
                    availTemplates.Add(generalTemplate.Key, generalTemplate.Value);
                }
                // Add reports templates
                errorInstance = "RBS load reports template";
                var reportsTemplates = LoadPrintTemplate(PrinterTypeEnum.Report);
                foreach (var item in reportsTemplates)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add z report template
                errorInstance = "RBS load Z report template";
                var ZTemplate = LoadPrintTemplate(PrinterTypeEnum.ZReport).FirstOrDefault();
                if (ZTemplate.Key != null && !availTemplates.ContainsKey(ZTemplate.Key))
                {
                    availTemplates.Add(ZTemplate.Key, ZTemplate.Value);
                }
                // Add x report template
                errorInstance = "RBS load X report template";
                var XTemplate = LoadPrintTemplate(PrinterTypeEnum.XReport).FirstOrDefault();
                if (XTemplate.Key != null && !availTemplates.ContainsKey(XTemplate.Key))
                {
                    availTemplates.Add(XTemplate.Key, XTemplate.Value);
                }
                // Add receipt preview template for RBS
                errorInstance = "RBS load OPOSPreview template";
                var oposPreviewTemplate = LoadPrintTemplate(PrinterTypeEnum.OposPreview).FirstOrDefault();
                if (oposPreviewTemplate.Key != null && !availTemplates.ContainsKey(oposPreviewTemplate.Key))
                {
                    availTemplates.Add(oposPreviewTemplate.Key, oposPreviewTemplate.Value);
                }
                // Add associated printers
                var availPrinters = new List<KitchenPrinterModel>();
                // Add receipt printers
                errorInstance = "RBS load receipt printer";
                availPrinters = GetPrintersOfType(PrinterTypeEnum.Receipt);
                // Add void printers
                errorInstance = "RBS load void printer";
                availPrinters.AddRange(GetPrintersOfType(PrinterTypeEnum.Void));
                // Add RBS printers
                errorInstance = "RBS load RBS printer";
                availPrinters.AddRange(GetPrintersOfType(PrinterTypeEnum.RBS));
                // Add general printer
                var generalPrinter = GetPrintersOfType(PrinterTypeEnum.General).FirstOrDefault();
                if (generalPrinter != null)
                {
                    availPrinters.Add(generalPrinter);
                }
                // Add report printers
                var reportPrinters = GetPrintersOfType(PrinterTypeEnum.Report);
                foreach (var item in reportPrinters)
                {
                    if (item != null && !availPrinters.Contains(item))
                    {
                        availPrinters.Add(item);
                    }
                }
                // Add z report printer
                errorInstance = "RBS load Z printer";
                var zTempl = GetPrintersOfType(PrinterTypeEnum.ZReport).FirstOrDefault();
                if (zTempl != null)
                {
                    availPrinters.Add(zTempl);
                }
                // Add x report printer
                errorInstance = "RBS load X printer";
                var xTempl = GetPrintersOfType(PrinterTypeEnum.XReport).FirstOrDefault();
                if (xTempl != null)
                {
                    availPrinters.Add(xTempl);
                }
                errorInstance = "RBS call OPOS constructor";
                // For general printer enum type, non theorimenes apodeikseis
                RepObj = new GenericExtcer(availTemplates, AvailEscCharsTemplates, availPrinters, installationData, FiscalName, applicationPath, configuration, applicationBuilder);
                logger.LogInformation("Rep Printer Created");
                // For the rest printer enum types,theorimenes apodeikseis
                FmObj = new RBSExtecr(availTemplates, AvailEscCharsTemplates, availPrinters, installationData, FiscalName, applicationPath, configuration, applicationBuilder);
                logger.LogInformation(FiscalName + " is initialized.");
            }
            catch (Exception exception)
            {
                logger.LogError("Initialize driver:  errorLocation: " + errorInstance + " Message: " + exception.ToString());
            }
        }

        /// <summary>
        /// Initialize generic
        /// </summary>
        private void Initialize_Generic()
        {
            string errorInstance = string.Empty;
            try
            {
                // Add associated templates
                var availTemplates = new Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>>();
                // Add receipt templates
                errorInstance = "generic load receipt template";
                var receiptTemplate = LoadPrintTemplate(PrinterTypeEnum.Receipt);
                foreach (var item in receiptTemplate)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add void templates
                errorInstance = "generic load void template";
                var voidTemplates = LoadPrintTemplate(PrinterTypeEnum.Void);
                foreach (var item in voidTemplates)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add general template
                errorInstance = "generic load general template";
                var generalTemplate = LoadPrintTemplate(PrinterTypeEnum.General).FirstOrDefault();
                if (generalTemplate.Key != null && !availTemplates.ContainsKey(generalTemplate.Key))
                {
                    availTemplates.Add(generalTemplate.Key, generalTemplate.Value);
                }
                // Add reports templates
                errorInstance = "generic load reports template";
                var reportsTemplates = LoadPrintTemplate(PrinterTypeEnum.Report);
                foreach (var item in reportsTemplates)
                {
                    if (item.Key != null && !availTemplates.ContainsKey(item.Key))
                    {
                        availTemplates.Add(item.Key, item.Value);
                    }
                }
                // Add z report template
                errorInstance = "generic load Z template";
                var ZTemplate = LoadPrintTemplate(PrinterTypeEnum.ZReport).FirstOrDefault();
                if (ZTemplate.Key != null && !availTemplates.ContainsKey(ZTemplate.Key))
                {
                    availTemplates.Add(ZTemplate.Key, ZTemplate.Value);
                }
                // Add x report template
                errorInstance = "generic load X template";
                var XTemplate = LoadPrintTemplate(PrinterTypeEnum.XReport).FirstOrDefault();
                if (XTemplate.Key != null && !availTemplates.ContainsKey(XTemplate.Key))
                {
                    availTemplates.Add(XTemplate.Key, XTemplate.Value);
                }
                // Add associated printers
                var availPrinters = new List<KitchenPrinterModel>();
                // Add receipt printers
                errorInstance = "generic load receipt printer";
                var rectempl = GetPrintersOfType(PrinterTypeEnum.Receipt);
                if (rectempl != null)
                {
                    availPrinters = rectempl;
                }
                // Add void printers
                errorInstance = "generic load void printer";
                var voidtempls = GetPrintersOfType(PrinterTypeEnum.Void);
                foreach (var item in voidtempls)
                {
                    if (item != null && !availPrinters.Contains(item))
                    {
                        availPrinters.Add(item);
                    }
                }
                // Add reservation printer
                var reservationPrinter = GetPrintersOfType(PrinterTypeEnum.Reservation).FirstOrDefault();
                if (reservationPrinter != null)
                {
                    availPrinters.Add(reservationPrinter);
                    var reservationTemplates = LoadPrintTemplate(PrinterTypeEnum.Reservation);
                    foreach (var item in reservationTemplates)
                    {
                        if (item.Key != null && !availTemplates.ContainsKey(item.Key) && reservationPrinter.TemplateShortName.Equals(item.Key))
                        {
                            availTemplates.Add(item.Key, item.Value);
                        }
                    }
                }
                // Add general printer
                var generalPrinter = GetPrintersOfType(PrinterTypeEnum.General).FirstOrDefault();
                if (generalPrinter != null)
                {
                    availPrinters.Add(generalPrinter);
                }
                // Add report printers
                var reportPrinters = GetPrintersOfType(PrinterTypeEnum.Report);
                foreach (var item in reportPrinters)
                {
                    if (item != null && !availPrinters.Contains(item))
                    {
                        availPrinters.Add(item);
                    }
                }
                // Add z report printer
                errorInstance = "generic load Z printer";
                var zTempl = GetPrintersOfType(PrinterTypeEnum.ZReport).FirstOrDefault();
                if (zTempl != null)
                {
                    availPrinters.Add(zTempl);
                }
                // Add x report printer
                var xPrinter = GetPrintersOfType(PrinterTypeEnum.XReport).FirstOrDefault();
                if (xPrinter != null)
                {
                    availPrinters.Add(xPrinter);
                }
                errorInstance = "generic call generic constructor";
                // For general printer enum type, non theorimenes apodeikseis
                FmObj = new GenericExtcer(availTemplates, AvailEscCharsTemplates, availPrinters, installationData, FiscalName, applicationPath, configuration, applicationBuilder);
                logger.LogInformation(FiscalName + " is initialized.");
            }
            catch (Exception exception)
            {
                logger.LogError("Initialize driver:  errorLocation: " + errorInstance + " Message: " + exception.ToString());
            }
        }

        /// <summary>
        /// Load kitchen printer template.
        /// </summary>
        /// <param name="printerType"></param>
        /// <returns></returns>
        private Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>> LoadPrintTemplate(PrinterTypeEnum printerType)
        {
            try
            {
                Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>> result = new Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>>();
                // get the template name for the given printer
                var templateName = availablePrinters.Where(f => f.PrinterType == printerType).Select(f => f.Template);
                var templates = AvailableTemplates.Where(f => templateName.Contains(f.Key + ".xml") || templateName.Contains(f.Key + ".rpt"));
                foreach (var item in templates)
                {
                    if (!result.ContainsKey(item.Key))
                    {
                        result.Add(item.Key, new Dictionary<PrinterTypeEnum, RollerTypeReportModel>() { { printerType, item.Value } });
                    }
                }
                return result;
            }
            catch (Exception exception)
            {
                logger.LogError("Error in LoadPrintTemplate :" + exception.ToString());
                return null;
            }
        }

        /// <summary>
        /// Load kitchen printer.
        /// </summary>
        /// <param name="printerType"></param>
        /// <returns></returns>
        private List<KitchenPrinterModel> GetPrintersOfType(PrinterTypeEnum printerType)
        {
            return availablePrinters.Where(f => f.PrinterType == printerType).ToList();
        }

        #endregion

        #region Print Processing

        /// <summary>
        /// ENTRY POINT for a PRINT
        /// </summary>
        /// <param name="data">Data to print (for reprint see objReprintData). Deserialised from json like: {"TableTotal":null,"PaymentTypeId":0,"PaymentsList":[{"Description":"Cash",........</param>
        /// <param name="command">Printing type, eg: Receipt, Kitchen, LCD, Void etc</param>
        /// <param name="fiscalName">Instance name</param>
        /// <param name="strInvoiceNumber">Invoice num. It comes from the json eg:  Receipt:64656:{\".....</param>
        /// <param name="objReprintData">For manually reprint. Data from Log/ArtcasBackup and Log/ReceiptResults folders</param>
        /// <returns></returns>
        private PrintResultModel ProccessRequest(object data, PrintModeEnum command, string fiscalName, string strInvoiceNumber = "", object objReprintData = null)
        {
            if (FmObj == null)
            {
                if (command == PrintModeEnum.Report)
                {
                    logger.LogInformation("Redirected to RepObj");
                    RepObj.PrintReport(data.ToString(), PrinterTypeEnum.Report);
                }
            }
            switch (FmObj.InstanceID)
            {
                case ExtcerTypesEnum.EpsonFiscal:
                    return ProcessRequest_EpsonFiscal(command, data, fiscalName, objReprintData);
                case ExtcerTypesEnum.HDM:
                    return ProcessRequest_HDM(command, data, fiscalName, objReprintData);
                case ExtcerTypesEnum.Opos:
                    return ProcessRequest_OPOS(command, data, fiscalName, objReprintData);
                case ExtcerTypesEnum.Opos3:
                    return ProcessRequest_OPOS3(command, data, fiscalName, objReprintData);
                case ExtcerTypesEnum.SynergyPF500:
                    return ProcessRequest_SynergyPF500(command, data, fiscalName, strInvoiceNumber);
                case ExtcerTypesEnum.ZipDitron:
                    return ProcessRequest_ZipDitron(command, data, fiscalName, strInvoiceNumber);
                case ExtcerTypesEnum.RBS:
                    return ProcessRequest_RBS(command, data, fiscalName, strInvoiceNumber, objReprintData);
                case ExtcerTypesEnum.Generic:
                    return ProcessRequest_Generic(command, data, fiscalName, strInvoiceNumber, objReprintData);
                case ExtcerTypesEnum.UNDEFINED:
                default:
                    break;
            }
            return new PrintResultModel();
        }

        /// <summary>
        /// Print epson
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        /// <param name="fiscalName"></param>
        /// <param name="objReprintData"></param>
        /// <returns></returns>
        private PrintResultModel ProcessRequest_EpsonFiscal(PrintModeEnum command, object data, string fiscalName, object objReprintData = null)
        {
            PrintResultModel result = new PrintResultModel();
            switch (command)
            {
                case PrintModeEnum.Receipt:
                    if (data != null)
                    {
                        result = FmObj.PrintReceipt(data as ReceiptModel, fiscalName);
                    }
                    break;
                case PrintModeEnum.ZReport:
                    if (data != null)
                    {
                        result = FmObj.PrintZ(data as ZReportModel);
                    }
                    break;
                case PrintModeEnum.Photo:
                    if (data != null)
                    {
                        string dataStr = data.ToString();
                        if (!string.IsNullOrEmpty(dataStr))
                        {
                            PrintPhoto(dataStr);
                        }
                    }
                    break;
                case PrintModeEnum.XReport:
                    if (data != null)
                    {
                        result = FmObj.PrintX();
                    }
                    break;
                case PrintModeEnum.Kitchen:
                    if (data != null)
                    {
                        result = KitchenObj.PrintKitchen(data as ReceiptModel);
                    }
                    break;
                case PrintModeEnum.Drawer:
                    FmObj.OpenDrawer();
                    break;
                case PrintModeEnum.General:
                    RepObj.PrintReport(data.ToString(), PrinterTypeEnum.General);
                    break;
                case PrintModeEnum.KitchenInstruction:
                    if (data != null)
                    {
                        result = KitchenObj.PrintKitchenInstruction(data as KitchenInstructionModel);
                    }
                    break;
                case PrintModeEnum.ZTotals:
                    result = FmObj.GetZTotal();
                    break;
                case PrintModeEnum.InvoiceSum:
                    if (data != null)
                    {
                        result = FmObj.PrintReceiptSum(data as ReceiptModel);
                    }
                    break;
                case PrintModeEnum.Report:
                    if (data != null)
                    {
                        result = RepObj.PrintReports(data as ReportsModel, fiscalName);
                    }
                    break;
                case PrintModeEnum.Reservation:
                    throw new NotImplementedException();
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// Print hdm
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        /// <param name="fiscalName"></param>
        /// <param name="objReprintData"></param>
        /// <returns></returns>
        private PrintResultModel ProcessRequest_HDM(PrintModeEnum command, object data, string fiscalName, object objReprintData = null)
        {
            PrintResultModel result = new PrintResultModel();
            switch (command)
            {
                case PrintModeEnum.Receipt:
                    if (data != null)
                    {
                        result = FmObj.PrintReceipt(data as ReceiptModel, fiscalName);
                    }
                    break;
                case PrintModeEnum.ZReport:
                    if (data != null)
                    {
                        result = FmObj.PrintZ(data as ZReportModel);
                    }
                    break;
                case PrintModeEnum.Photo:
                    if (data != null)
                    {
                        string dataStr = data.ToString();
                        if (!string.IsNullOrEmpty(dataStr))
                        {
                            PrintPhoto(dataStr);
                        }
                    }
                    break;
                case PrintModeEnum.XReport:
                    if (data != null)
                    {
                        FmObj.PrintX(data as ZReportModel, out result);
                    }
                    break;
                case PrintModeEnum.Kitchen:
                    if (data != null)
                    {
                        result = KitchenObj.PrintKitchen(data as ReceiptModel);
                    }
                    break;
                case PrintModeEnum.Drawer:
                    FmObj.OpenDrawer();
                    break;
                case PrintModeEnum.General:
                    RepObj.PrintReport(data.ToString(), PrinterTypeEnum.General);
                    break;
                case PrintModeEnum.KitchenInstruction:
                    if (data != null)
                    {
                        result = KitchenObj.PrintKitchenInstruction(data as KitchenInstructionModel);
                    }
                    break;
                case PrintModeEnum.ZTotals:
                    result = FmObj.GetZTotal();
                    break;
                case PrintModeEnum.InvoiceSum:
                    if (data != null)
                    {
                        result = FmObj.PrintReceiptSum(data as ReceiptModel);
                    }
                    break;
                case PrintModeEnum.Report:
                    if (data != null)
                    {
                        result = RepObj.PrintReports(data as ReportsModel, fiscalName);
                    }
                    break;
                case PrintModeEnum.PrintConnectivity:
                    FmObj.PrinterConnectivity();
                    break;
                case PrintModeEnum.Reservation:
                    throw new NotImplementedException();
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// Print opos
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        /// <param name="fiscalName"></param>
        /// <param name="objReprintData"></param>
        /// <returns></returns>
        private PrintResultModel ProcessRequest_OPOS(PrintModeEnum command, object data, string fiscalName, object objReprintData = null)
        {
            PrintResultModel result = new PrintResultModel();
            switch (command)
            {
                case PrintModeEnum.Receipt:
                    if (data != null)
                    {
                        result = FmObj.PrintReceipt(data as ReceiptModel, fiscalName);
                    }
                    break;
                case PrintModeEnum.ZReport:
                    if (data != null)
                    {
                        result = FmObj.PrintZ(data as ZReportModel);
                    }
                    break;
                case PrintModeEnum.Photo:
                    if (data != null)
                    {
                        string dataStr = data.ToString();
                        if (!string.IsNullOrEmpty(dataStr))
                        {
                            PrintPhoto(dataStr);
                        }
                    }
                    break;
                case PrintModeEnum.XReport:
                    if (data != null)
                    {
                        result = FmObj.PrintX();
                    }
                    break;
                case PrintModeEnum.Kitchen:
                    if (data != null)
                    {
                        result = KitchenObj.PrintKitchen(data as ReceiptModel);
                    }
                    break;
                case PrintModeEnum.Drawer:
                    FmObj.OpenDrawer();
                    break;
                case PrintModeEnum.General:
                    RepObj.PrintReport(data.ToString(), PrinterTypeEnum.General);
                    break;
                case PrintModeEnum.KitchenInstruction:
                    if (data != null)
                    {
                        result = KitchenObj.PrintKitchenInstruction(data as KitchenInstructionModel);
                    }
                    break;
                case PrintModeEnum.ZTotals:
                    result = FmObj.GetZTotal();
                    break;
                case PrintModeEnum.InvoiceSum:
                    if (data != null)
                    {
                        result = FmObj.PrintReceiptSum(data as ReceiptModel);
                    }
                    break;
                case PrintModeEnum.Report:
                    if (data != null)
                    {
                        result = RepObj.PrintReports(data as ReportsModel, fiscalName);
                    }
                    break;
                case PrintModeEnum.Reservation:
                    throw new NotImplementedException();
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// Print opos3
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        /// <param name="fiscalName"></param>
        /// <param name="objReprintData"></param>
        /// <returns></returns>
        private PrintResultModel ProcessRequest_OPOS3(PrintModeEnum command, object data, string fiscalName, object objReprintData = null)
        {
            PrintResultModel result = new PrintResultModel();
            switch (command)
            {
                case PrintModeEnum.Receipt:
                    if (data != null)
                    {
                        result = FmObj.PrintReceipt(data as ReceiptModel, fiscalName);
                    }
                    break;
                case PrintModeEnum.ZReport:
                    if (data != null)
                    {
                        result = FmObj.PrintZ(data as ZReportModel);
                    }
                    break;
                case PrintModeEnum.Photo:
                    if (data != null)
                    {
                        string dataStr = data.ToString();
                        if (!string.IsNullOrEmpty(dataStr))
                        {
                            PrintPhoto(dataStr);
                        }
                    }
                    break;
                case PrintModeEnum.XReport:
                    if (data != null)
                    {
                        result = FmObj.PrintX();
                    }
                    break;
                case PrintModeEnum.Kitchen:
                    if (data != null)
                    {
                        result = KitchenObj.PrintKitchen(data as ReceiptModel);
                    }
                    break;
                case PrintModeEnum.Drawer:
                    FmObj.OpenDrawer();
                    break;
                case PrintModeEnum.General:
                    RepObj.PrintReport(data.ToString(), PrinterTypeEnum.General);
                    break;
                case PrintModeEnum.KitchenInstruction:
                    if (data != null)
                    {
                        result = KitchenObj.PrintKitchenInstruction(data as KitchenInstructionModel);
                    }
                    break;
                case PrintModeEnum.ZTotals:
                    result = FmObj.GetZTotal();
                    break;
                case PrintModeEnum.InvoiceSum:
                    if (data != null)
                    {
                        result = FmObj.PrintReceiptSum(data as ReceiptModel);
                    }
                    break;
                case PrintModeEnum.Report:
                    if (data != null)
                    {
                        result = RepObj.PrintReports(data as ReportsModel, fiscalName);
                    }
                    break;
                case PrintModeEnum.PrintConnectivity:
                    FmObj.PrinterConnectivity();
                    break;
                case PrintModeEnum.Reservation:
                    throw new NotImplementedException();
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// Print synergypf500
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        /// <param name="fiscalName"></param>
        /// <param name="strInvoiceNumber"></param>
        /// <returns></returns>
        private PrintResultModel ProcessRequest_SynergyPF500(PrintModeEnum command, object data, string fiscalName, string strInvoiceNumber)
        {
            PrintResultModel result = new PrintResultModel();
            switch (command)
            {
                case PrintModeEnum.Receipt:
                    if (data != null)
                    {
                        // print for the first time
                        result = FmObj.PrintReceipt(data as ReceiptModel, fiscalName, strInvoiceNumber);
                    }
                    break;
                case PrintModeEnum.ZReport:
                    if (data != null)
                    {
                        result = FmObj.PrintZ(data as ZReportModel);
                    }
                    break;
                case PrintModeEnum.XReport:
                    if (data != null)
                    {
                        FmObj.PrintX(data as ZReportModel, out result);
                    }
                    break;
                case PrintModeEnum.Reservation:
                    throw new NotImplementedException();
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// Print zipditron
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        /// <param name="fiscalName"></param>
        /// <param name="strInvoiceNumber"></param>
        /// <returns></returns>
        private PrintResultModel ProcessRequest_ZipDitron(PrintModeEnum command, object data, string fiscalName, string strInvoiceNumber)
        {
            PrintResultModel result = new PrintResultModel();
            switch (command)
            {
                case PrintModeEnum.Receipt:
                    if (data != null)
                    {
                        // print for the first time
                        result = FmObj.PrintReceipt(data as ReceiptModel, fiscalName, strInvoiceNumber);
                    }
                    break;
                case PrintModeEnum.ZReport:
                    if (data != null)
                    {
                        result = FmObj.PrintZ(data as ZReportModel);
                    }
                    break;
                case PrintModeEnum.XReport:
                    if (data != null)
                    {
                        FmObj.PrintX(data as ZReportModel, out result);
                    }
                    break;
                case PrintModeEnum.Reservation:
                    throw new NotImplementedException();
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// Print rbs
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        /// <param name="fiscalName"></param>
        /// <param name="strInvoiceNumber"></param>
        /// <param name="objReprintData"></param>
        /// <returns></returns>
        private PrintResultModel ProcessRequest_RBS(PrintModeEnum command, object data, string fiscalName, string strInvoiceNumber, object objReprintData = null)
        {
            PrintResultModel result = new PrintResultModel();
            switch (command)
            {
                case PrintModeEnum.Receipt:
                    if (data != null)
                    {
                        if (objReprintData != null)
                        {
                            // reprint
                            result = FmObj.PrintReceipt(data as ReceiptModel, fiscalName, strInvoiceNumber, objReprintData);
                        }
                        else
                        {
                            // print for the first time
                            result = FmObj.PrintReceipt(data as ReceiptModel, fiscalName, strInvoiceNumber);
                        }
                    }
                    break;
                case PrintModeEnum.ZReport:
                    if (data != null)
                    {
                        result = FmObj.PrintZ(data as ZReportModel);
                    }
                    break;
                case PrintModeEnum.Photo:
                    if (data != null)
                    {
                        string dataStr = data.ToString();
                        if (!string.IsNullOrEmpty(dataStr))
                        {
                            PrintPhoto(dataStr);
                        }
                    }
                    break;
                case PrintModeEnum.XReport:
                    if (data != null)
                    {
                        FmObj.PrintX(data as ZReportModel, out result);
                    }
                    break;
                case PrintModeEnum.Kitchen:
                    if (data != null)
                    {
                        bool blnIsVoid = (data as ReceiptModel).IsVoid;
                        result = KitchenObj.PrintKitchen(data as ReceiptModel, blnIsVoid);
                    }
                    break;
                case PrintModeEnum.General:
                    FmObj.PrintReport(data.ToString(), PrinterTypeEnum.General);
                    break;
                case PrintModeEnum.KitchenInstruction:
                    if (data != null)
                    {
                        result = KitchenObj.PrintKitchenInstruction(data as KitchenInstructionModel);
                    }
                    break;
                case PrintModeEnum.ZTotals:
                    result = new PrintResultModel() { ResponseValue = "" };
                    break;
                case PrintModeEnum.InvoiceSum:
                    if (data != null)
                    {
                        result = FmObj.PrintReceiptSum(data as ReceiptModel);
                    }
                    break;
                case PrintModeEnum.Report:
                    if (data != null)
                    {
                        result = FmObj.PrintReports(data as ReportsModel, fiscalName);
                    }
                    break;
                case PrintModeEnum.Reservation:
                    if (data != null)
                    {
                        result = FmObj.PrintReservations(data as ExtecrTableReservetionModel, fiscalName);
                    }
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// Print generic
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        /// <param name="fiscalName"></param>
        /// <param name="strInvoiceNumber"></param>
        /// <param name="objReprintData"></param>
        /// <returns></returns>
        private PrintResultModel ProcessRequest_Generic(PrintModeEnum command, object data, string fiscalName, string strInvoiceNumber, object objReprintData = null)
        {
            PrintResultModel result = new PrintResultModel();
            switch (command)
            {
                case PrintModeEnum.Receipt:
                    if (data != null)
                    {
                        if (objReprintData != null)
                        {
                            // reprint
                            result = FmObj.PrintReceipt(data as ReceiptModel, fiscalName, strInvoiceNumber, objReprintData);
                        }
                        else
                        {
                            // print for the first time
                            result = FmObj.PrintReceipt(data as ReceiptModel, fiscalName, strInvoiceNumber);
                        }
                    }
                    break;
                case PrintModeEnum.ZReport:
                    if (data != null)
                    {
                        result = FmObj.PrintZ(data as ZReportModel);
                    }
                    break;
                case PrintModeEnum.Photo:
                    if (data != null)
                    {
                        string dataStr = data.ToString();
                        if (!string.IsNullOrEmpty(dataStr))
                        {
                            PrintPhoto(dataStr);
                        }
                    }
                    break;
                case PrintModeEnum.XReport:
                    if (data != null)
                    {
                        FmObj.PrintX(data as ZReportModel, out result);
                    }
                    break;
                case PrintModeEnum.Kitchen:
                    if (data != null)
                    {
                        bool blnIsVoid = (data as ReceiptModel).IsVoid;
                        result = KitchenObj.PrintKitchen(data as ReceiptModel, blnIsVoid);
                    }
                    break;
                case PrintModeEnum.General:
                    FmObj.PrintReport(data.ToString(), PrinterTypeEnum.General);
                    break;
                case PrintModeEnum.KitchenInstruction:
                    if (data != null)
                    {
                        result = KitchenObj.PrintKitchenInstruction(data as KitchenInstructionModel);
                    }
                    break;
                case PrintModeEnum.ZTotals:
                    result = new PrintResultModel() { ResponseValue = "" };
                    break;
                case PrintModeEnum.InvoiceSum:
                    if (data != null)
                    {
                        result = FmObj.PrintReceiptSum(data as ReceiptModel);
                    }
                    break;
                case PrintModeEnum.Report:
                    if (data != null)
                    {
                        result = FmObj.PrintReports(data as ReportsModel, fiscalName);
                    }
                    break;
                case PrintModeEnum.Reservation:
                    if (data != null)
                    {
                        result = FmObj.PrintReservations(data as ExtecrTableReservetionModel, fiscalName);
                    }
                    break;
                default:
                    break;
            }
            return result;
        }

        #endregion

        #region Communication Messages

        /// <summary>
        /// New receipt entrypoint when PrintType=PrintWhole or PrintType=PrintEnd
        /// </summary>
        /// <param name="extecrName"></param>
        /// <param name="invoiceId"></param>
        /// <param name="blnPrintFiscalSign"></param>
        /// <param name="blnDoPrintInKitchen"></param>
        /// <param name="printType"></param>
        /// <param name="additionalInfo"></param>
        /// <param name="tempPrint"></param>
        private void OnHub_NewReceipt(string extecrName, string invoiceId, bool blnPrintFiscalSign, bool blnDoPrintInKitchen, PrintType printType = PrintType.PrintWhole, string additionalInfo = "", bool tempPrint = false)
        {
            try
            {
                string[] strItems = extecrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
                string receiptJson = GetReceiptById(invoiceId);
                PrintReceipt(invoiceId, receiptJson, blnPrintFiscalSign, blnDoPrintInKitchen, printType, additionalInfo, tempPrint);
                CreateAndPostHTMLReceipt(invoiceId, receiptJson);
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_NewReceipt: " + exception.ToString());
            }
        }

        /// <summary>
        /// Receipt for reservation
        /// </summary>
        /// <param name="reservationId"></param>
        private void OnHub_NewTableReservation(long reservationId)
        {
            try
            {
                string tableReservationJson = GetTableReservationById(reservationId);
                PrintReservation(reservationId, tableReservationJson);
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_NewTableReservation: " + exception.ToString());
            }
        }

        /// <summary>
        /// New receipt entrypoint when PrintType<>PrintWhole and PrintType<>PrintEnd
        /// </summary>
        /// <param name="extecrName"></param>
        /// <param name="model"></param>
        /// <param name="printType"></param>
        /// <param name="additionalInfo"></param>
        /// <param name="tempPrint"></param>
        private void OnHub_PrintItem(string extecrName, string model, PrintType printType = PrintType.PrintWhole, string additionalInfo = "", bool tempPrint = false)
        {
            try
            {
                string[] strItems = extecrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
                string invoiceId = "0";
                bool printFiscalSign = true;
                bool printKitchen = false;
                PrintReceipt(invoiceId, model, printFiscalSign, printKitchen, printType, additionalInfo, tempPrint);
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_PrintItem: " + exception.ToString());
            }
        }

        /// <summary>
        /// Ping for printer connectivity. If printer is connected then return similar signalR message, orherwise (exception is thrown) do nothing
        /// </summary>
        /// <param name="extecrName"></param>
        /// <param name="posName"></param>
        /// <param name="reserOrder"></param>
        private void OnHub_PartialPrintConnectivity(string extecrName, string posName, bool reserOrder)
        {
            try
            {
                string[] strItems = extecrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
                CheckPrinterConnectivity();
                // Printer is available. Pos will add the item into the order.   
                logger.LogDebug("Invoking Printer availability back to POS. POS will add the item into the order...");
                signalRCommunication.Invoke_PartialPrintConnectivity(posName, extecrName, false);
            }
            catch (PrinterConnectivityException exception)
            {
                if (exception.ResetOrder)
                {
                    // Printer is unavailable with retCode = 255 for a long time. POS must reset the current order. 
                    logger.LogError(exception.Message);
                    logger.LogWarning("Printer IS NOT available. Invoking Printer unavailability back to POS. POS MUST RESET the current order.");
                    signalRCommunication.Invoke_PartialPrintConnectivity(posName, extecrName, true);
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_PartialPrintConnectivity: " + exception.ToString());
            }
        }

        /// <summary>
        /// Connected users
        /// </summary>
        /// <param name="strConnectedUsers"></param>
        private void OnHub_ConnectedUsers(string strConnectedUsers)
        {
            try
            {
                // if my name is not within the received list -> do reconnect me
                if (!strConnectedUsers.Contains(FiscalName))
                {
                    // intentionally stop connection to trigger a reconnection
                    signalRCommunication.ResetConnection();
                    logger.LogInformation("OnConnectedUsers cannot find " + FiscalName);
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_ConnectedUsers: " + exception.ToString());
            }
        }

        /// <summary>
        /// Event handler for Z report.
        /// </summary>
        /// <param name="extecrName"></param>
        /// <param name="ZJson"></param>
        private void OnHub_IssueReportZ(string extecrName, string ZJson)
        {
            try
            {
                string[] strItems = extecrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
                PrintZReport(ZJson);
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_IssueReportZ: " + exception.ToString());
            }
        }

        /// <summary>
        /// Event handler for X report.
        /// </summary>
        /// <param name="extecrName"></param>
        /// <param name="XJson"></param>
        private void OnHub_IssueReportX(string extecrName, string XJson)
        {
            try
            {
                string[] strItems = extecrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
                PrintXReport(XJson);
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_IssueReportX: " + exception.ToString());
            }
        }

        /// <summary>
        /// Event handler for general report.
        /// </summary>
        /// <param name="extecrName"></param>
        /// <param name="RJson"></param>
        private void OnHub_IssueReport(string extecrName, string RJson)
        {
            try
            {
                string[] strItems = extecrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
                PrintReport(RJson);
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_IssueReport: " + exception.ToString());
            }
        }

        /// <summary>
        /// Event handler for credit card reader.
        /// </summary>
        /// <param name="strExtEcrName"></param>
        /// <param name="strSender"></param>
        /// <param name="strAmount"></param>
        private void OnHub_CreditCardAmount(string strExtEcrName, string strSender, string strAmount)
        {
            try
            {
                string[] strItems = strExtEcrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
                strAmount = decimal.Parse(strAmount).ToString("#0.00").Replace(',', '.');
                if (eftposSettings.enableEFTPOS && string.Compare(FiscalName, eftposSettings.EFTPOSInstance) == 0)
                {
                    SetCreditCardAmount(strAmount);
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_CreditCardAmount: " + exception.ToString());
            }
        }

        /// <summary>
        /// Event handler for drawer.
        /// </summary>
        /// <param name="strExtEcrName"></param>
        private void OnHub_Drawer(string strExtEcrName)
        {
            try
            {
                string[] strItems = strExtEcrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
                OpenPrinterDrawer();
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_Drawer: " + exception.ToString());
            }
        }

        /// <summary>
        /// Event handler for image.
        /// </summary>
        /// <param name="strExtEcrName"></param>
        /// <param name="strSender"></param>
        /// <param name="strImageData"></param>
        private void OnHub_Image(string strExtEcrName, string strSender, string strImageData)
        {
            try
            {
                string[] strItems = strExtEcrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_Image: " + exception.ToString());
            }
        }

        /// <summary>
        /// Event handler for kitchen message.
        /// </summary>
        /// <param name="strExtEcrName"></param>
        /// <param name="strSender"></param>
        /// <param name="strMessageData"></param>
        private void OnHub_Kitchen(string strExtEcrName, string strSender, string strMessageData)
        {
            try
            {
                string[] strItems = strExtEcrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_Kitchen: " + exception.ToString());
            }
        }

        /// <summary>
        /// Event handler for kitchenInstruction message.
        /// </summary>
        /// <param name="strExtEcrName"></param>
        /// <param name="strSender"></param>
        /// <param name="strMessageData"></param>
        private void OnHub_KitchenInstruction(string strExtEcrName, string strSender, string strMessageData)
        {
            try
            {
                string[] strItems = strExtEcrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
                PrintKitchenInstruction(strMessageData);
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_KitchenInstruction: " + exception.ToString());
            }
        }

        /// <summary>
        /// Event handler for kitchenInstructionLogger message.
        /// </summary>
        /// <param name="strExtEcrName"></param>
        /// <param name="strSender"></param>
        /// <param name="strKitchenInstructionLoggerId"></param>
        /// <param name="strMessageData"></param>
        private void OnHub_KitchenInstructionLogger(string strExtEcrName, string strSender, string strKitchenInstructionLoggerId, string strMessageData)
        {
            try
            {
                string[] strItems = strExtEcrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
                PrintKitchenInstructionLogger(strKitchenInstructionLoggerId, strMessageData);
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_KitchenInstructionLogger: " + exception.ToString());
            }
        }

        /// <summary>
        /// Event handler for LcdMessage message.
        /// </summary>
        /// <param name="strExtEcrName"></param>
        /// <param name="strJsonData"></param>
        private void OnHub_LcdMessage(string strExtEcrName, string strJsonData)
        {
            try
            {
                string[] strItems = strExtEcrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
                if (lcdSettings.enableLCD && string.Compare(FiscalName, lcdSettings.LCDInstance) == 0)
                {
                    DisplayLCDMessage(strJsonData);
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_LcdMessage: " + exception.ToString());
            }
        }

        /// <summary>
        /// Event handler for StartWeighting message.
        /// </summary>
        /// <param name="strExtEcrName"></param>
        /// <param name="strSender"></param>
        private void OnHub_StartWeighting(string strExtEcrName, string strSender)
        {
            try
            {
                string[] strItems = strExtEcrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
                if (scaleSettings.enableScale && string.Compare(FiscalName, scaleSettings.scaleInstance) == 0)
                {
                    StartWeighing(strSender);
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_StartWeighting: " + exception.ToString());
            }
        }

        /// <summary>
        /// Event handler for StopWeighting message.
        /// </summary>
        /// <param name="strExtEcrName"></param>
        /// <param name="strSender"></param>
        private void OnHub_StopWeighting(string strExtEcrName, string strSender)
        {
            try
            {
                string[] strItems = strExtEcrName.Split('|');
                if (strItems[1] != FiscalName)
                    return;
                if (scaleSettings.enableScale && string.Compare(FiscalName, scaleSettings.scaleInstance) == 0)
                {
                    StopWeighing(strSender);
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_StopWeighting: " + exception.ToString());
            }
        }

        /// <summary>
        /// Event handler for HeartBeat message.
        /// </summary>
        private void OnHub_HeartBeat()
        {
            try
            {
                //TODO GEO
                //signalRCommunication.Invoke_IAmAlive();
            }
            catch (Exception exception)
            {
                logger.LogError("Error at OnHub_LcdMessage: " + exception.ToString());
            }
        }

        #endregion

        #region API Communication

        /// <summary>
        /// Get receipt by id from api
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <returns></returns>
        private string GetReceiptById(string invoiceId)
        {
            string result = "";
            try
            {
                int loop = 0;
                do
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(communicationSettings.connectionUrl);
                        AuthenticationHeaderValue authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(new ASCIIEncoding().GetBytes(string.Format("{0}:{1}", communicationSettings.authorizationUsername, communicationSettings.authorizationPassword))));
                        client.DefaultRequestHeaders.Authorization = authHeader;
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        string request;
                        bool useOldURL = configuration.GetValue<bool>("UseOldURL");
                        if (useOldURL)
                            request = "/api/InvoiceForDisplay?storeid=" + communicationSettings.connectionStoreId + "&id=" + invoiceId + "&forExtecr=true";
                        else
                            request = "/api/v3/ReceiptDetailsForExtecr/GetReceiptDetailsForExtecr/invoiceId/" + invoiceId;
                        logger.LogInformation("Requesting Server: " + request);
                        HttpResponseMessage response = client.GetAsync(request).Result;
                        if (response.StatusCode != HttpStatusCode.OK)
                            logger.LogWarning("Returned Http Status: " + response.StatusCode.ToString());
                        result = response.Content.ReadAsStringAsync().Result;
                    }
                    if (!result.Contains("An error has occurred"))
                        loop = 1000;
                    else
                    {
                        loop++;
                        if (loop <= 3)
                            logger.LogInformation("Server Returned Error: " + result.Substring(0, 130) + "...");
                        else
                            logger.LogError("Server Returned Error (see bellow for the error description): " + result.Substring(0, 130) + "...");
                        if (loop <= 3)
                            Thread.Sleep(1200 * loop);
                    }
                } while (loop <= 3);
            }
            catch (Exception exception)
            {
                logger.LogError("Error at getting receipt from api: " + exception.ToString());
            }
            return result;
        }

        /// <summary>
        /// Get receipt by id for kitchen from api 
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <returns></returns>
        private string GetKitchenReceiptById(string invoiceId)
        {
            string result = "";
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(communicationSettings.connectionUrl);
                    AuthenticationHeaderValue authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(new ASCIIEncoding().GetBytes(string.Format("{0}:{1}", communicationSettings.authorizationUsername, communicationSettings.authorizationPassword))));
                    client.DefaultRequestHeaders.Authorization = authHeader;
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string request = "/api/v3/ReceiptDetailsForExtecr/GetReceiptDetailsForExtecr/invoiceId/" + invoiceId + "?isForKitchen=true";
                    logger.LogInformation("Requesting Server: " + request);
                    HttpResponseMessage response = client.GetAsync(request).Result;
                    result = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error at getting kitchen receipt from api: " + exception.ToString());
            }
            return result;
        }

        /// <summary>
        /// Get table reservation by id from api
        /// </summary>
        /// <param name="reservationId"></param>
        /// <returns></returns>
        private string GetTableReservationById(long reservationId)
        {
            string result = "";
            try
            {
                int loop = 0;
                do
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(communicationSettings.connectionUrl);
                        AuthenticationHeaderValue authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(new ASCIIEncoding().GetBytes(string.Format("{0}:{1}", communicationSettings.authorizationUsername, communicationSettings.authorizationPassword))));
                        client.DefaultRequestHeaders.Authorization = authHeader;
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        string request = "api/v3/ReservationCustomers/GetCustomersReservation/Id/" + reservationId;
                        logger.LogInformation("Requesting Server: " + request);
                        HttpResponseMessage response = client.GetAsync(request).Result;
                        if (response.StatusCode != HttpStatusCode.OK)
                            logger.LogWarning("Returned Http Status: " + response.StatusCode.ToString());
                        result = response.Content.ReadAsStringAsync().Result;
                    }
                    if (!result.Contains("An error has occurred"))
                        loop = 1000;
                    else
                    {
                        loop++;
                        logger.LogInformation("Server Returned Error: " + result.Substring(0, 130) + "...");
                        if (loop <= 3)
                            Thread.Sleep(1200 * loop);
                    }
                } while (loop <= 3);
            }
            catch (Exception exception)
            {
                logger.LogError("Error at getting table reservation from api: " + exception.ToString());
            }
            return result;
        }

        /// <summary>
        /// Update receipt number and print status in api
        /// </summary>
        private void UpdateReceiptPrintData(string receiptId, string receiptNo, bool isPrinted, string extecrCode)
        {
            if (receiptId == "0")
                return;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(communicationSettings.connectionUrl);
                    AuthenticationHeaderValue authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(new ASCIIEncoding().GetBytes(string.Format("{0}:{1}", communicationSettings.authorizationUsername, communicationSettings.authorizationPassword))));
                    client.DefaultRequestHeaders.Authorization = authHeader;
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpRequestMessage myRequest = new HttpRequestMessage();
                    myRequest.Method = new HttpMethod("PUT");
                    myRequest.RequestUri = new Uri(communicationSettings.connectionUrl + string.Format("/api/InvoiceForDisplay?storeid={0}" + "&receiptId={1}" + "&receiptNo={2}" + "&isPrinted={3}" + "&extecrCode={4}", communicationSettings.connectionStoreId, receiptId, receiptNo, isPrinted, extecrCode));
                    logger.LogInformation("Sending PUT call to Server: " + myRequest.RequestUri.ToString());
                    HttpResponseMessage response = client.SendAsync(myRequest).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        logger.LogError("Failed to Inform Server with PUT. ReceiptID = " + receiptId);
                    }
                    else
                    {
                        logger.LogInformation("Successful Server update with PUT. ReceiptID = " + receiptId);
                    }
                }
            }
            catch (Exception exception)
            {
                logger.LogError("at UpdateReceiptPrintData. Error: " + exception.ToString());
            }
        }

        /// <summary>
        /// Update kitchen instruction print status in api
        /// </summary>
        /// <param name="kitchenInstructionLoggerId"></param>
        /// <param name="isPrinted"></param>
        private void UpdateKitchenInstructionLoggerPrintData(string kitchenInstructionLoggerId, bool isPrinted)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(communicationSettings.connectionUrl);
                    AuthenticationHeaderValue authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(new ASCIIEncoding().GetBytes(string.Format("{0}:{1}", communicationSettings.authorizationUsername, communicationSettings.authorizationPassword))));
                    client.DefaultRequestHeaders.Authorization = authHeader;
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpRequestMessage myRequest = new HttpRequestMessage();
                    myRequest.Method = new HttpMethod("PUT");
                    myRequest.RequestUri = new Uri(communicationSettings.connectionUrl + string.Format("/api/KitchenInstructionLogger?storeid={0}" + "&kitchenInstructionLoggerId={1}" + "&status={2}", communicationSettings.connectionStoreId, kitchenInstructionLoggerId, isPrinted ? 1 : 2));
                    logger.LogInformation("Sending PUT call to Server: " + myRequest.RequestUri.ToString());
                    HttpResponseMessage response = client.SendAsync(myRequest).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        logger.LogError("Failed to Inform Server with PUT. KitchenInstructionLoggerId = " + kitchenInstructionLoggerId);
                    }
                    else
                    {
                        logger.LogInformation("Successful Server update with PUT. KitchenInstructionLoggerId = " + kitchenInstructionLoggerId);
                    }
                }
            }
            catch (Exception exception)
            {
                logger.LogError("at UpdateKitchenInstructionLoggerPrintData. Error: " + exception.ToString());
            }
        }

        /// <summary>
        /// Post html receipt to api
        /// </summary>
        /// <param name="html"></param>
        /// <param name="invoiceId"></param>
        private void PostHtmlReceipt(string html, long invoiceId)
        {
            try
            {
                PMSReceiptHTMLModel data = new PMSReceiptHTMLModel();
                data.InvoiceId = invoiceId;
                data.HtmlReceipt = html;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(communicationSettings.connectionUrl);
                    AuthenticationHeaderValue authorizationHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(new ASCIIEncoding().GetBytes(string.Format("{0}:{1}", communicationSettings.authorizationUsername, communicationSettings.authorizationPassword))));
                    client.DefaultRequestHeaders.Authorization = authorizationHeader;
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string url = "/api/v3/Payments/UpdatePmsHtmlReceipt";
                    string json = JsonSerializer.Serialize(data);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync(url, content).Result;
                    string result = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception exception)
            {
                logger.LogError("at PostHtmlReceipt. Error: " + exception.ToString());
            }
        }

        #endregion

        #region Print Invoices

        /// <summary>
        /// Print receipt
        /// </summary>
        /// <param name="receiptJson"></param>
        /// <param name="blnPrintFiscalSign"></param>
        /// <param name="blnDoPrintInKitchen"></param>
        /// <param name="printType"></param>
        /// <param name="additionalInfo"></param>
        /// <param name="tempPrint"></param>
        private void PrintReceipt(string invoiceId, string receiptJson, bool blnPrintFiscalSign, bool blnDoPrintInKitchen, PrintType printType = PrintType.PrintWhole, string additionalInfo = "", bool tempPrint = false)
        {
            try
            {
                logger.LogInformation(ExtcerLogger.Log("NEW MESSAGE:  Receipt: " + invoiceId + " : " + receiptJson, FiscalName));
                ReceiptModel receiptModel = null;
                string strInvoiceId = string.Empty;
                string strInvoiceNumber = string.Empty;

                //1. deserialize to receipt model
                receiptModel = JsonSerializer.Deserialize<ReceiptModel>(receiptJson);
                receiptModel.InvoiceIdStr = invoiceId;
                receiptModel.PrintFiscalSign = blnPrintFiscalSign;
                receiptModel.PrintKitchen = blnDoPrintInKitchen;
                receiptModel.PrintType = printType;
                receiptModel.ItemAdditionalInfo = additionalInfo;
                receiptModel.TempPrint = tempPrint;
                receiptModel.daorigin = receiptModel.DA_Origin.ToString();
                receiptModel.Sanitize();
                logger.LogInformation(ExtcerLogger.Log("ReceiptType: " + (receiptModel.ReceiptTypeDescription ?? "<null>"), FiscalName));
                logger.LogInformation(ExtcerLogger.Log("PrintType: " + receiptModel.PrintType.ToString(), FiscalName));

                //2. get receipt values
                strInvoiceId = receiptModel.InvoiceIdStr;
                strInvoiceNumber = receiptModel.ReceiptNo;

                //3. create print result object
                PrintResultModel artcasResultObj = new PrintResultModel();
                artcasResultObj.OrderNo = receiptModel.OrderNo;
                artcasResultObj.FiscalName = FiscalName;
                artcasResultObj.SenderName = receiptModel.Pos;
                artcasResultObj.ReceiptReceiveType = ReceiptReceiveTypeEnum.WEB;

                //4. print receipt
                PrintResultModel printResult;
                printResult = ProccessRequest(receiptModel, PrintModeEnum.Receipt, FiscalName, strInvoiceId);
                if (receiptModel.OrderId == null && printResult.ErrorDescription != null)
                    printResult.ErrorDescription = "Error, Try to print again...";
                printResult.FiscalName = FiscalName;
                printResult.ReceiptData = XMLHelper.RemoveIllegalXmlchars(printResult.ReceiptData);
                lastReceipt = printResult;
                if (printResult.PrintType == PrintType.PrintWhole || printResult.PrintType == PrintType.PrintEnd)
                {
                    extecrDisplayer.AddPrintedReceipt(printResult);
                }
                artcasResultObj.Status = printResult.Status;

                //5. send response of receipt status to api and pos
                if (signalRCommunication.IsConnected())
                {
                    var receiptStatus = printResult.Status == PrintStatusEnum.Printed ? true : false;
                    var receiptResponseMsg = receiptModel.Pos + "|ReceiptResponse~" + receiptModel.OrderId + "@@@" + printResult.ReceiptNo + "@@@" + receiptStatus + "@@@" + receiptModel.DetailsId + "@@@" + receiptJson;
                    if (printResult.Status == PrintStatusEnum.Failed)
                    {
                        receiptResponseMsg += "@@@" + printResult.ErrorDescription;
                        string strMessageToPos = "1|" + printResult.ErrorDescription + "|" + strInvoiceId + "|" + receiptModel.Total + "|" + receiptModel.TableNo + "|" + printResult.ReceiptNo;
                        signalRCommunication.Invoke_ExtECRError(strMessageToPos);
                    }
                    // respond to signalR server with receipt number followed by print status
                    bool blnIsPrinted = (PrintStatusEnum.Printed == printResult.Status);
                    bool blnSameInvoiceNumber;
                    string extecrCode = "";
                    if (installationData.ExtcerType == ExtcerTypesEnum.Opos || installationData.ExtcerType == ExtcerTypesEnum.Opos3 || installationData.ExtcerType == ExtcerTypesEnum.EpsonFiscal || installationData.ExtcerType == ExtcerTypesEnum.HDM)
                    {
                        if (installationData.ExtcerType == ExtcerTypesEnum.HDM)
                        {
                            if (printResult.crn != null)
                                extecrCode = printResult.crn + "|" + printResult.rseq;
                            logger.LogInformation(string.Format("extecrCode: {0}", extecrCode));
                        }
                        blnSameInvoiceNumber = printResult.ReceiptNo.Equals(strInvoiceNumber);
                        // do always update server with OPOS data
                        blnSameInvoiceNumber = false;
                    }
                    else
                    {
                        blnSameInvoiceNumber = true;
                    }
                    logger.LogInformation(string.Format("Receipt Printout Status (SignalR): Printed -> {0}, Receipt ID -> {1}, Receipt Number -> {2}", blnIsPrinted, strInvoiceId, printResult.ReceiptNo));
                    if (!blnIsPrinted || !blnSameInvoiceNumber)
                    {
                        logger.LogInformation(string.Format("About to call ReceiptPrintResult (SignalR): Printed -> {0}, Receipt ID -> {1}, Receipt Number -> {2}, extecrCode -> {3}", blnIsPrinted, strInvoiceId, printResult.ReceiptNo, extecrCode));
                        UpdateReceiptPrintData(strInvoiceId, printResult.ReceiptNo, blnIsPrinted, extecrCode);
                    }
                }

                //6. print to kitchen if needed
                if (printResult.Status == PrintStatusEnum.Printed && receiptModel.PrintKitchen)
                {
                    // check if we print in kitchen for specific invoice type
                    bool sendToKitchen = true;
                    string invTypesStr = configuration.GetValue<string>("AvoidKitchenPrint");
                    int[] invTypes = invTypesStr.Split(',').Select(s => Convert.ToInt32(s)).ToArray();
                    foreach (int i in invTypes)
                    {
                        if (i == receiptModel.InvoiceType)
                        {
                            sendToKitchen = false;
                            break;
                        }
                    }
                    if (sendToKitchen)
                    {
                        string kitchenReceiptJson = GetKitchenReceiptById(strInvoiceId);
                        ReceiptModel kitchenReceiptModel = null;
                        kitchenReceiptModel = JsonSerializer.Deserialize<ReceiptModel>(kitchenReceiptJson);
                        kitchenReceiptModel.InvoiceIdStr = strInvoiceId;
                        kitchenReceiptModel.PrintFiscalSign = blnPrintFiscalSign;
                        kitchenReceiptModel.PrintKitchen = blnDoPrintInKitchen;
                        kitchenReceiptModel.PrintType = printType;
                        kitchenReceiptModel.ItemAdditionalInfo = additionalInfo;
                        kitchenReceiptModel.TempPrint = tempPrint;
                        kitchenReceiptModel.daorigin = kitchenReceiptModel.DA_Origin.ToString();
                        kitchenReceiptModel.Sanitize();
                        if (kitchenReceiptModel.IsForKitchen)
                        {
                            logger.LogInformation("In main kitchenReceiptModel.PrintKitchen: " + kitchenReceiptModel.PrintKitchen);
                            if (installationData.ExtcerType == ExtcerTypesEnum.Opos && !kitchenReceiptModel.IsVoid)
                            {
                                PrintResultModel kitchenPrintResult = ProccessRequest(kitchenReceiptModel, PrintModeEnum.Kitchen, FiscalName);
                                lastReceipt.SecondaryPrintersList = kitchenPrintResult.SecondaryPrintersList;
                            }
                            else if (installationData.ExtcerType != ExtcerTypesEnum.Opos && kitchenReceiptModel.PrintKitchen)
                            {
                                PrintResultModel kitchenPrintResult = ProccessRequest(kitchenReceiptModel, PrintModeEnum.Kitchen, "");
                                lastReceipt.SecondaryPrintersList = kitchenPrintResult.SecondaryPrintersList;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error receipt print MESSAGE: " + receiptJson);
                logger.LogError("Error receipt print ERROR: " + exception.ToString());
            }
        }

        /// <summary>
        /// Print table reservation
        /// </summary>
        /// <param name="reservationId"></param>
        /// <param name="tableReservationJson"></param>
        private void PrintReservation(long reservationId, string tableReservationJson)
        {
            try
            {
                logger.LogInformation(ExtcerLogger.Log("NEW MESSAGE:  Table Reservation: " + reservationId + " : " + tableReservationJson, FiscalName));
                PrintResultModel printResult;
                ExtecrTableReservetionModel reservationsModel = JsonSerializer.Deserialize<ExtecrTableReservetionModel>(tableReservationJson);
                printResult = ProccessRequest(reservationsModel, PrintModeEnum.Reservation, FiscalName);
                extecrDisplayer.AddPrintedReceipt(printResult);
            }
            catch (Exception exception)
            {
                logger.LogError("Error table reservation print MESSAGE: " + tableReservationJson);
                logger.LogError("Error table reservation print ERROR: " + exception.ToString());
            }
        }

        /// <summary>
        /// Print kitchen instruction
        /// </summary>
        /// <param name="kitchenInstructionJson"></param>
        private void PrintKitchenInstruction(string kitchenInstructionJson)
        {
            try
            {
                logger.LogInformation(ExtcerLogger.Log("NEW MESSAGE:  Kitchen Instruction: " + kitchenInstructionJson, FiscalName));
                KitchenInstructionModel ktchInstruction = JsonSerializer.Deserialize<KitchenInstructionModel>(kitchenInstructionJson);
                PrintResultModel printResult = ProccessRequest(ktchInstruction, PrintModeEnum.KitchenInstruction, FiscalName);
            }
            catch (Exception exception)
            {
                logger.LogError("Error kitchen instruction print MESSAGE: " + kitchenInstructionJson);
                logger.LogError("Error kitchen instruction print ERROR: " + exception.ToString());
            }
        }

        /// <summary>
        /// Print kitchen instruction logger
        /// </summary>
        /// <param name="kitchenInstructionLoggerId"></param>
        /// <param name="kitchenInstructionLoggerJson"></param>
        private void PrintKitchenInstructionLogger(string kitchenInstructionLoggerId, string kitchenInstructionLoggerJson)
        {
            try
            {
                logger.LogInformation(ExtcerLogger.Log("NEW MESSAGE:  Kitchen Instruction Logger: " + kitchenInstructionLoggerId + ":" + kitchenInstructionLoggerJson, FiscalName));
                KitchenInstructionModel ktchInstruction = JsonSerializer.Deserialize<KitchenInstructionModel>(kitchenInstructionLoggerJson);
                PrintResultModel printResult = ProccessRequest(ktchInstruction, PrintModeEnum.KitchenInstruction, FiscalName);
                UpdateKitchenInstructionLoggerPrintData(kitchenInstructionLoggerId, printResult.Status == PrintStatusEnum.Printed);
            }
            catch (Exception exception)
            {
                logger.LogError("Error kitchen instruction print MESSAGE: " + kitchenInstructionLoggerJson);
                logger.LogError("Error kitchen instruction print ERROR: " + exception.ToString());
            }
        }

        /// <summary>
        /// Print general report
        /// </summary>
        /// <param name="reportJson"></param>
        private void PrintReport(string reportJson)
        {
            try
            {
                logger.LogInformation(ExtcerLogger.Log("NEW MESSAGE:  General Report: " + reportJson, FiscalName));
                ReportsModel reportstModel = JsonSerializer.Deserialize<ReportsModel>(reportJson);
                ProccessRequest(reportstModel, PrintModeEnum.Report, FiscalName);
            }
            catch (Exception exception)
            {
                logger.LogError("Error general report print MESSAGE: " + reportJson);
                logger.LogError("Error general report print ERROR: " + exception.ToString());
            }
        }

        /// <summary>
        /// Print X report
        /// </summary>
        /// <param name="xReportJson"></param>
        private void PrintXReport(string xReportJson)
        {
            // create print result object
            PrintResultModel artcasResultObj = new PrintResultModel();
            artcasResultObj.ReceiptType = PrintModeEnum.XReport;
            try
            {
                logger.LogInformation(ExtcerLogger.Log("NEW MESSAGE:  X Report: " + xReportJson, FiscalName));
                // get X report data
                ZReportModel xReportData = JsonSerializer.Deserialize<ZReportModel>(xReportJson);
                artcasResultObj = ProccessRequest(xReportData, PrintModeEnum.XReport, FiscalName);
                artcasResultObj.ReceiptReceiveType = ReceiptReceiveTypeEnum.WEB;
                artcasResultObj.FiscalName = FiscalName;
            }
            catch (Exception exception)
            {
                logger.LogError("Error x report print MESSAGE: " + xReportJson);
                logger.LogError("Error x report print ERROR: " + exception.ToString());
                artcasResultObj.Status = PrintStatusEnum.Failed;
            }
            artcasResultObj.ReceiptData = XMLHelper.RemoveIllegalXmlchars(artcasResultObj.ReceiptData);
            extecrDisplayer.AddPrintedReceipt(artcasResultObj);
        }

        /// <summary>
        /// Print Z report
        /// </summary>
        /// <param name="zReportJson"></param>
        private void PrintZReport(string zReportJson)
        {
            // create print result object
            PrintResultModel artcasResultObj = new PrintResultModel();
            artcasResultObj.ReceiptType = PrintModeEnum.ZReport;
            try
            {
                logger.LogInformation(ExtcerLogger.Log("NEW MESSAGE:  Z Report: " + zReportJson, FiscalName));
                // get zReport data
                ZReportModel zReportData = JsonSerializer.Deserialize<ZReportModel>(zReportJson);
                artcasResultObj = ProccessRequest(zReportData, PrintModeEnum.ZReport, FiscalName);
                artcasResultObj.ReceiptReceiveType = ReceiptReceiveTypeEnum.WEB;
                artcasResultObj.FiscalName = FiscalName;
                // respond to POS with receipt number followed by print status
                var zStatus = (artcasResultObj.Status == PrintStatusEnum.Printed) ? true : false;
                string zResponseMsg = string.Empty;
                zResponseMsg = zStatus ? "0:OK" : "1:";
                if (artcasResultObj.Status == PrintStatusEnum.Failed)
                {
                    zResponseMsg += artcasResultObj.ErrorDescription;
                }
                signalRCommunication.Invoke_ZReportResponse(zResponseMsg);
            }
            catch (Exception exception)
            {
                logger.LogError("Error z report print MESSAGE: " + zReportJson);
                logger.LogError("Error z report print ERROR: " + exception.ToString());
                artcasResultObj.Status = PrintStatusEnum.Failed;
            }
            artcasResultObj.ReceiptData = XMLHelper.RemoveIllegalXmlchars(artcasResultObj.ReceiptData);
            extecrDisplayer.AddPrintedReceipt(artcasResultObj);
        }

        #endregion

        #region Print Photo

        /// <summary>
        /// Print photo
        /// </summary>
        /// <param name="photoData"></param>
        private void PrintPhoto(string photoData)
        {
            var img = Base64Converters.Base64ToImage(photoData);
            MemoryStream ms = new MemoryStream();
            img.Save(ms, ImageFormat.Jpeg);
            imageByteArray = ms.ToArray();
            if (imageByteArray == null || imageByteArray.Length < 1)
            {
                return;
            }
            PrepareImage(imageByteArray);
        }

        /// <summary>
        /// Prepare image for print
        /// </summary>
        /// <param name="imageByteArray"></param>
        private void PrepareImage(byte[] imageByteArray)
        {
            using (var ms = new MemoryStream(imageByteArray))
            {
                //TODO GEO
                //BitmapImage bi = new BitmapImage();
                //bi.BeginInit();
                //bi.CreateOptions = BitmapCreateOptions.None;
                //bi.CacheOption = BitmapCacheOption.OnLoad;
                //bi.StreamSource = ms;
                //bi.EndInit();
            }
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += new PrintPageEventHandler(printDoc_PrintPage);
            printDoc.Print();
        }

        /// <summary>
        /// Print image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void printDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Point ulCorner = new Point(100, 100);
            MemoryStream ms = new MemoryStream(imageByteArray);
            Image returnImage = Image.FromStream(ms);
            g.DrawImage(returnImage, 0, 0, 500, 500);
            // Set font family, size and style
            string text = "Test text";
            Font fontText = new Font("Times New Roman", 24, FontStyle.Regular);
            g.DrawString(text, fontText, Brushes.Black, new Point(54, 250));
            // Draw a horizontal line
            Pen pen = new Pen(Color.Gray);
            g.DrawLine(pen, new Point(10, 500), new Point(200, 500));
        }

        #endregion

        #region Printer Actions

        /// <summary>
        /// Check printer connectivity status
        /// </summary>
        private void CheckPrinterConnectivity()
        {
            PrintResultModel printResult = ProccessRequest(null, PrintModeEnum.PrintConnectivity, FiscalName);
        }

        /// <summary>
        /// Open printer drawer
        /// </summary>
        private void OpenPrinterDrawer()
        {
            PrintResultModel printResult = ProccessRequest(null, PrintModeEnum.Drawer, FiscalName);
        }

        #endregion

        #region HTML Receipt

        /// <summary>
        /// Transform receipt to html and post to api
        /// </summary>
        /// <param name="receiptId"></param>
        /// <param name="receiptJson"></param>
        private void CreateAndPostHTMLReceipt(string receiptId, string receiptJson)
        {
            try
            {
                if (htmlReceiptSettings.enableHTMLReceipt)
                {
                    long invoiceId = Convert.ToInt64(receiptId);
                    ReceiptModel receipt = JsonSerializer.Deserialize<ReceiptModel>(receiptJson);
                    RollerTypeReportModel template = DetermineTemplateForHtmlReceipt(receipt.IsVoid, receipt.InvoiceIndex);
                    if (template == null)
                    {
                        template = GetDefaultTemplate();
                    }
                    if (template != null)
                    {
                        string html = HTMLReceiptHelper.CreateHtmlReceipt(receipt, template);
                        PostHtmlReceipt(html, invoiceId);
                    }
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error creating and posting html from receipt.");
                logger.LogError("Error: " + exception.ToString());
                logger.LogError("Model: " + receiptJson);
            }
        }

        /// <summary>
        /// Determine template for receipt transformation to html
        /// </summary>
        /// <param name="isVoid"></param>
        /// <param name="invoiceIndex"></param>
        /// <returns></returns>
        private RollerTypeReportModel DetermineTemplateForHtmlReceipt(bool isVoid, string invoiceIndex)
        {
            RollerTypeReportModel template = null;
            if (installationData.ExtcerType == ExtcerTypesEnum.Generic)
            {
                KitchenPrinterModel printer;
                if (isVoid)
                {
                    printer = availablePrinters.FirstOrDefault(p => p.PrinterType == PrinterTypeEnum.Void && p.SlotIndex == invoiceIndex);
                }
                else
                {
                    printer = availablePrinters.FirstOrDefault(p => p.PrinterType == PrinterTypeEnum.Receipt && p.SlotIndex == invoiceIndex);
                }
                if (printer != null)
                {
                    if (printer.IsCrystalReportsPrintout == null || printer.IsCrystalReportsPrintout == false)
                    {
                        bool templateFound = AvailableTemplates.TryGetValue(printer.TemplateShortName, out template);
                        if (!templateFound)
                        {
                            template = null;
                        }
                    }
                }
            }
            return template;
        }

        /// <summary>
        /// Get default template for receipt transformation to html
        /// </summary>
        /// <returns></returns>
        private RollerTypeReportModel GetDefaultTemplate()
        {
            RollerTypeReportModel template;
            try
            {
                string templateName = htmlReceiptSettings.htmlReceiptTemplate;
                string templateNameFormatted = templateName.Split(new char[] { '.' }, 2)[0];
                bool templateFound = AvailableTemplates.TryGetValue(templateNameFormatted, out template);
                if (!templateFound)
                {
                    template = null;
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception.ToString());
                template = null;
            }
            return template;
        }

        #endregion

        #region EFTPOS

        /// <summary>
        /// Set amount to charge in credit card
        /// </summary>
        /// <param name="strAmountToCharge"></param>
        private void SetCreditCardAmount(string strAmountToCharge)
        {
            Task task = Task.Run(() => PaymentTerminalHelper.EFTPOS_Sale(strAmountToCharge, eftposSettings.EFTPOSIPAddress, eftposSettings.EFTPOSTCPIPPort));
        }

        #endregion

        #region LCD

        /// <summary>
        /// Display LCD message
        /// </summary>
        /// <param name="lcdMessageJson"></param>
        private void DisplayLCDMessage(string lcdMessageJson)
        {
            try
            {
                LCDModel lcdObj = JsonSerializer.Deserialize<LCDModel>(lcdMessageJson);
                var result = LcdDisplayHelper.GetLcdMessage(lcdSettings.LCDType, lcdObj, lcdSettings.LCDLength, lcdSettings.LCDCOMPort);
                lcdObj = null;
                if (lcdSettings.LCDType == LcdTypeEnum.Use_VF60Commander)
                {
                    ExecuteExternalApp("VF60Commander.exe");
                }
                if (result.Length == 0)
                {
                    return;
                }
                // Send message to LCD.
                SendToSerialPort(result);
            }
            catch (Exception exception)
            {
                logger.LogError("Error LCD display MESSAGE: " + lcdMessageJson);
                logger.LogError("Error LCD display ERROR: " + exception.ToString());
            }
        }

        /// <summary>
        /// Send LCD message to serial port
        /// </summary>
        /// <param name="msg"></param>
        private void SendToSerialPort(byte[] msg)
        {
            if (lcdSettings.LCDSerialPort != null)
            {
                try
                {
                    lcdSettings.LCDSerialPort.Open();
                    lcdSettings.LCDSerialPort.Write(msg, 0, msg.Length);
                    lcdSettings.LCDSerialPort.Close();
                }
                catch (Exception exception)
                {
                    logger.LogError("at SendToSerialPort error: " + exception.ToString());
                }
                finally
                {
                    lcdSettings.LCDSerialPort.Close();
                }
            }
            else
            {
                localHubInvoker.SendError("Serial port is not created");
            }
        }

        #endregion

        #region Scale

        /// <summary>
        /// Start weighing
        /// </summary>
        /// <param name="sender"></param>
        private void StartWeighing(string sender)
        {
            repeatLoopScaleMeasure = 1;
            StartScaleMeasure(sender);
        }

        /// <summary>
        /// Stop weighing
        /// </summary>
        /// <param name="sender"></param>
        private void StopWeighing(string sender)
        {
            repeatLoopScaleMeasure = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private void StartScaleMeasure(string sender)
        {
            if (!scaleSettings.scaleSerialPort.IsOpen)
            {
                scaleSettings.scaleSerialPort.Open();
            }
            scaleSettings.scaleSerialPort.DiscardOutBuffer();
            scaleSettings.scaleSerialPort.DiscardInBuffer();
            try
            {
                bool blnValidMeasureSend = false;
                while (!blnValidMeasureSend)
                {
                    scaleSettings.scaleSerialPort.Write("98000001\r\n");
                    string strReadInBuffer = scaleSettings.scaleSerialPort.ReadLine();
                    if (strReadInBuffer.StartsWith("99") && strReadInBuffer.EndsWith("\r") && strReadInBuffer[2] == '0')
                    {
                        // good measure
                        string strScaleCurrentMeasure = strReadInBuffer.Substring(3, 5);
                        if (strScaleCurrentMeasure == "00000")
                        {
                            // zero value measured -> do not send
                        }
                        else
                        {
                            // not zero value measured -> send to POS last measure
                            blnValidMeasureSend = true;
                            signalRCommunication.Invoke_ItemWeighted(sender, strScaleCurrentMeasure.Insert(2, "."));
                        }
                    }
                }
                if (scaleSettings.scaleSerialPort.IsOpen)
                {
                    scaleSettings.scaleSerialPort.Close();
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error on StartScaleMeasure: " + exception.ToString());
            }
        }

        #endregion

        /// <summary>
        /// Used to execute an external application or tool - no wait for return or results.
        /// </summary>
        /// <param name="strPathAppName"></param>
        private void ExecuteExternalApp(string strPathAppName)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = strPathAppName;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = "";
            try
            {
                Process exeProcess = Process.Start(startInfo);
            }
            catch (Exception exception)
            {
                logger.LogError("External application or tool: " + strPathAppName + " Error: " + exception.ToString());
            }
        }
    }
}