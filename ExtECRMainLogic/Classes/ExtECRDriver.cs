using ExtECRMainLogic.Classes.ExtECR;
using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Hubs;
using ExtECRMainLogic.Models.ConfigurationModels;
using ExtECRMainLogic.Models.DriverModels;
using ExtECRMainLogic.Models.PrinterModels;
using ExtECRMainLogic.Models.TemplateModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace ExtECRMainLogic.Classes
{
    public class ExtECRDriver
    {
        #region Communication Settings
        /// <summary>
        /// Communication settings
        /// </summary>
        private CommunicationSettingsModel communicationSettings;
        #endregion
        #region HTML Receipt Settings
        /// <summary>
        /// HTML receipt settings
        /// </summary>
        private HTMLReceiptSettingsModel htmlReceiptSettings;
        #endregion
        #region EFTPOS Settings
        /// <summary>
        /// EFTPOS settings
        /// </summary>
        private EFTPOSSettingsModel eftposSettings;
        #endregion
        #region LCD Settings
        /// <summary>
        /// LCD settings
        /// </summary>
        private LCDSettingsModel lcdSettings;
        #endregion
        #region Scale Settings
        /// <summary>
        /// Scale settings
        /// </summary>
        private ScaleSettingsModel scaleSettings;
        #endregion
        #region Crystal Reports
        /// <summary>
        /// The server name or ODBC connection name for Crystal Reports
        /// </summary>
        public static string strCrystalReportDataSource;
        /// <summary>
        /// The user name for the database connection for Crystal Reports
        /// </summary>
        public static string strCrystalReportUserName;
        /// <summary>
        /// The password for the database connection for Crystal Reports
        /// </summary>
        public static string strCrystalReportPassword;
        #endregion
        #region Printer Instances
        /// <summary>
        /// List of instances
        /// </summary>
        List<ExtECR_Class> ExtcerInstancesList;
        #endregion
        /// <summary>
        /// Application version
        /// </summary>
        private string version;
        /// <summary>
        /// Application path
        /// </summary>
        private string applicationPath;
        /// <summary>
        /// Application builder
        /// </summary>
        private IApplicationBuilder applicationBuilder;
        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration configuration;
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<ExtECRDriver> logger;
        /// <summary>
        /// ExtECR initializer
        /// </summary>
        private readonly ExtECRInitializer extecrInitializer;
        /// <summary>
        /// Local hub invoker
        /// </summary>
        private readonly LocalHubInvoker localHubInvoker;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        /// <param name="extecrInitializer"></param>
        /// <param name="localHubInvoker"></param>
        public ExtECRDriver(IConfiguration configuration, ILogger<ExtECRDriver> logger, ExtECRInitializer extecrInitializer, LocalHubInvoker localHubInvoker)
        {
            this.ExtcerInstancesList = new List<ExtECR_Class>();
            this.configuration = configuration;
            this.logger = logger;
            this.extecrInitializer = extecrInitializer;
            this.localHubInvoker = localHubInvoker;
        }

        /// <summary>
        /// UI and Hardware installation based on data loaded from xml files.
        /// </summary>
        /// <param name="applicationPath"></param>
        /// <param name="applicationBuilder"></param>
        public void InitializeExtECR(string version, string applicationPath, IApplicationBuilder applicationBuilder)
        {
            logger.LogInformation("Initializing ExtECRDriver...");
            try
            {
                this.version = version;
                this.applicationPath = applicationPath;
                this.applicationBuilder = applicationBuilder;
                SetupCommunicationSettings();
                string productVersion = GetProductVersion();
                bool applicationVersionChecked = CheckApplicationVersion();
                if (applicationVersionChecked)
                {
                    SetupHTMLReceiptSettings();
                    SetupEFTPOSSettings();
                    SetupLCD();
                    SetupScale();
                    SetupCrystalReports();
                    SetupPrinterInstances();
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error. Fix the problem and restart the Service." + Environment.NewLine + exception.ToString());
            }
        }

        #region Private Methods

        /// <summary>
        /// Communication settings setup
        /// </summary>
        private void SetupCommunicationSettings()
        {
            communicationSettings = new CommunicationSettingsModel();
            communicationSettings.connectionUrl = extecrInitializer.InstallationMasterData.CommunicationsUrl;
            communicationSettings.connectionStoreId = extecrInitializer.InstallationMasterData.StoreId;
            communicationSettings.authorizationUsername = extecrInitializer.InstallationMasterData.SignalR_UserName;
            communicationSettings.authorizationPassword = extecrInitializer.InstallationMasterData.SignalR_Password;
        }

        /// <summary>
        /// Get product version
        /// </summary>
        /// <returns></returns>
        private string GetProductVersion()
        {
            string productVersion = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(communicationSettings.connectionUrl);
                    logger.LogInformation("Getting product version...");
                    HttpResponseMessage response = client.GetAsync("/api/productinfo/version").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        productVersion = response.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error at getting product version: " + exception.ToString());
            }
            return productVersion;
        }

        /// <summary>
        /// Check application version in server
        /// </summary>
        /// <returns></returns>
        private bool CheckApplicationVersion()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(communicationSettings.connectionUrl);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpRequestMessage myRequest = new HttpRequestMessage();
                    myRequest.Method = new HttpMethod("GET");
                    myRequest.RequestUri = new Uri(communicationSettings.connectionUrl + string.Format("api/Security/CheckVersion?client={0}&version={1}", 3, version));
                    logger.LogInformation("Checking version compatibility...");
                    HttpResponseMessage response = client.SendAsync(myRequest).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        string errorMessage = "Failed to check version compatibility from server. Current ExtECR version is: " + version + " . Install a newer version.";
                        logger.LogError(errorMessage);
                        localHubInvoker.SendError(errorMessage);
                        return false;
                    }
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error at checking application version: " + exception.ToString());
            }
            return true;
        }

        /// <summary>
        /// HTML receipt settings setup
        /// </summary>
        private void SetupHTMLReceiptSettings()
        {
            htmlReceiptSettings = new HTMLReceiptSettingsModel();
            htmlReceiptSettings.enableHTMLReceipt = extecrInitializer.InstallationMasterData.EnableHtmlReceipt;
            htmlReceiptSettings.htmlReceiptTemplate = extecrInitializer.InstallationMasterData.HtmlReceiptTemplate;
        }

        /// <summary>
        /// EFTPOS settings setup
        /// </summary>
        private void SetupEFTPOSSettings()
        {
            eftposSettings = new EFTPOSSettingsModel();
            eftposSettings.enableEFTPOS = extecrInitializer.InstallationMasterData.EnableEFTPOS;
            eftposSettings.EFTPOSInstance = extecrInitializer.InstallationMasterData.EFTPOSInstance;
            eftposSettings.EFTPOSIPAddress = extecrInitializer.InstallationMasterData.EFTPOS_IP_Address;
            eftposSettings.EFTPOSTCPIPPort = extecrInitializer.InstallationMasterData.EFTPOS_TCPIP_Port;
        }

        /// <summary>
        /// Customer display serial port setup
        /// </summary>
        private void SetupLCD()
        {
            lcdSettings = new LCDSettingsModel();
            lcdSettings.enableLCD = extecrInitializer.InstallationMasterData.EnableDisplay;
            lcdSettings.LCDInstance = extecrInitializer.InstallationMasterData.LcdDisplayInstance;
            lcdSettings.LCDType = extecrInitializer.InstallationMasterData.DisplayType;
            lcdSettings.LCDLength = extecrInitializer.InstallationMasterData.DisplayCharLength;
            lcdSettings.LCDCOMPort = extecrInitializer.InstallationMasterData.DisplayComPortName;
            lcdSettings.LCDSerialPort = null;
            if (lcdSettings.enableLCD)
            {
                try
                {
                    if (!string.IsNullOrEmpty(lcdSettings.LCDCOMPort))
                    {
                        if (lcdSettings.LCDSerialPort != null && lcdSettings.LCDSerialPort.IsOpen)
                        {
                            lcdSettings.LCDSerialPort.Close();
                            lcdSettings.LCDSerialPort = null;
                        }
                        switch (lcdSettings.LCDType)
                        {
                            // If necessary, add here settings to configure serial port event handler
                            //_serialPort.Handshake = Handshake.None;
                            case LcdTypeEnum.Casio:
                                lcdSettings.LCDSerialPort = new SerialPort(lcdSettings.LCDCOMPort, 19200, Parity.None, 8, StopBits.One);
                                break;
                            case LcdTypeEnum.TOSHIBA_ST_A20:
                                lcdSettings.LCDSerialPort = new SerialPort(lcdSettings.LCDCOMPort, 9600, Parity.Odd, 8, StopBits.One);
                                break;
                            case LcdTypeEnum.Use_VF60Commander:
                                break;
                            case LcdTypeEnum.WINCOR_NIXDORF_BA63:
                                lcdSettings.LCDSerialPort = new SerialPort(lcdSettings.LCDCOMPort, 9600, Parity.Odd, 8, StopBits.One);
                                break;
                            case LcdTypeEnum.VFD_PD220:
                                lcdSettings.LCDSerialPort = new SerialPort(lcdSettings.LCDCOMPort, 9600, Parity.Odd, 8, StopBits.One);
                                lcdSettings.LCDSerialPort.Handshake = Handshake.None;
                                break;
                            case LcdTypeEnum.NCR:
                            case LcdTypeEnum.IBM:
                            case LcdTypeEnum.NCR_ENG:
                                lcdSettings.LCDSerialPort = new SerialPort(lcdSettings.LCDCOMPort);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        localHubInvoker.SendError("COM port is not set (Customer Display)");
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError("InitializeSerialPort (Customer Display) error :" + exception.ToString());
                }
            }
        }

        /// <summary>
        /// Electronic scale serial port setup
        /// </summary>
        private void SetupScale()
        {
            scaleSettings = new ScaleSettingsModel();
            scaleSettings.enableScale = extecrInitializer.InstallationMasterData.EnableScale;
            scaleSettings.scaleInstance = extecrInitializer.InstallationMasterData.ScaleInstance;
            scaleSettings.scaleType = extecrInitializer.InstallationMasterData.ScaleType;
            scaleSettings.scaleCOMPort = extecrInitializer.InstallationMasterData.ScaleComPortName;
            scaleSettings.scaleSerialPort = null;
            if (scaleSettings.enableScale)
            {
                try
                {
                    if (!string.IsNullOrEmpty(scaleSettings.scaleCOMPort))
                    {
                        if (scaleSettings.scaleSerialPort != null && scaleSettings.scaleSerialPort.IsOpen)
                        {
                            scaleSettings.scaleSerialPort.Close();
                            scaleSettings.scaleSerialPort = null;
                        }
                        switch (scaleSettings.scaleType)
                        {
                            // If necessary, add here settings to configure serial port event handler
                            //_serialPort_ElectronicScale.ReadTimeout = 500;
                            //_serialPort_ElectronicScale.ReceivedBytesThreshold = 9;
                            //_serialPort_ElectronicScale.DataReceived += _serialPort_ElectronicScale_DataReceived;
                            case ScaleTypeEnum.ICS_G310_TISA:
                                scaleSettings.scaleSerialPort = new SerialPort(scaleSettings.scaleCOMPort, 9600, Parity.None, 8, StopBits.One);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        localHubInvoker.SendError("COM port is not set (Electronic Scale)");
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError("InitializeSerialPort (Electronic Scale) error: " + exception.ToString());
                }
            }
        }

        /// <summary>
        /// Crystal reports connection setup
        /// </summary>
        private void SetupCrystalReports()
        {
            strCrystalReportDataSource = extecrInitializer.InstallationMasterData.CrystalDataSource;
            strCrystalReportUserName = extecrInitializer.InstallationMasterData.CrystalUserName;
            strCrystalReportPassword = extecrInitializer.InstallationMasterData.CrystalPassword;
        }

        /// <summary>
        /// Available printer instances setup
        /// </summary>
        private void SetupPrinterInstances()
        {
            if (ExtcerInstancesList.Count > 0)
            {
                ExtcerInstancesList.Clear();
            }
            if (extecrInitializer.InstallationData.Count > 0)
            {
                foreach (InstallationDataModel extecrInstance in extecrInitializer.InstallationData)
                {
                    logger.LogInformation("Starting " + extecrInstance.FiscalName + "...");
                    InstallationDataMaster installationMasterData = extecrInitializer.InstallationMasterData;
                    List<KitchenPrinterModel> printers = extecrInitializer.GetAvailablePrintersByFiscaId(extecrInstance.FiscalId);
                    List<Printer> printerDevices = extecrInitializer.GetAvailEscCharsTemplates();
                    Dictionary<string, RollerTypeReportModel> templates = extecrInitializer.GetAvailableTemplates();
                    ExtECR_Class extcerInstanceCreated = new ExtECR_Class(extecrInstance, printers, printerDevices, templates, communicationSettings, htmlReceiptSettings, eftposSettings, lcdSettings, scaleSettings, applicationPath, configuration, applicationBuilder);
                    Thread extcerInstanceThread = new Thread(new ThreadStart(() =>
                    {
                        extcerInstanceCreated.InitializeDriver();
                    }));
                    extcerInstanceThread.Name = extcerInstanceCreated.FiscalName + "_Inst_Tread";
                    extcerInstanceThread.Start();
                    Thread.Sleep(100);
                    ExtcerInstancesList.Add(extcerInstanceCreated);
                }
            }
        }

        #endregion
    }
}