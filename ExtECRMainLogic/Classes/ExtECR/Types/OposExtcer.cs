using ExtECRMainLogic.Classes.Extenders;
using ExtECRMainLogic.Classes.Helpers;
using ExtECRMainLogic.Classes.Loggers;
using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Exceptions;
using ExtECRMainLogic.Hubs;
using ExtECRMainLogic.Models.ConfigurationModels;
using ExtECRMainLogic.Models.PrinterModels;
using ExtECRMainLogic.Models.ReceiptModels;
using ExtECRMainLogic.Models.TemplateModels;
using ExtECRMainLogic.Models.ZReportModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OposFiscalPrinter_1_7_Lib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExtECRMainLogic.Classes.ExtECR.Types
{
    /// <summary>
    /// OPOS Instance
    /// </summary>
    public class OposExtcer : FiscalManager
    {
        #region Properties
        /// <summary>
        /// 
        /// </summary>
        private object thisLock;
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>> PrintersTemplates;
        /// <summary>
        /// 
        /// </summary>
        private List<Printer> PrintersEscList;
        /// <summary>
        /// 
        /// </summary>
        public List<KitchenPrinterModel> availablePrinters;
        /// <summary>
        /// 
        /// </summary>
        InstallationDataModel InstallationData;
        /// <summary>
        /// 
        /// </summary>
        private string FiscalName;
        /// <summary>
        /// 
        /// </summary>
        List<int> CashAccountIds;
        /// <summary>
        /// 
        /// </summary>
        List<int> CCAccountIds;
        /// <summary>
        /// 
        /// </summary>
        List<int> CreditAccountIds;
        /// <summary>
        /// 
        /// </summary>
        string DeviceName;
        /// <summary>
        /// 
        /// </summary>
        OPOSFiscalPrinterClass OPOS;
        /// <summary>
        /// 
        /// </summary>
        private PrintResultModel printResult;
        /// <summary>
        /// 
        /// </summary>
        int ErrorCode;
        /// <summary>
        /// 
        /// </summary>
        int ClaimTimeout;
        /// <summary>
        /// 
        /// </summary>
        StreamReader streamToPrint;
        /// <summary>
        /// 
        /// </summary>
        private Font printFont;
        #endregion
        /// <summary>
        /// Application path
        /// </summary>
        private readonly string applicationPath;
        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration configuration;
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<OposExtcer> logger;
        /// <summary>
        /// Local hub invoker
        /// </summary>
        private readonly LocalHubInvoker localHubInvoker;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="printerTemplatesList"></param>
        /// <param name="printerEscList"></param>
        /// <param name="availablePrinters"></param>
        /// <param name="instData"></param>
        /// <param name="fiscName"></param>
        /// <param name="applicationPath"></param>
        /// <param name="configuration"></param>
        /// <param name="applicationBuilder"></param>
        /// <param name="deviceType"></param>
        public OposExtcer(Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>> printerTemplatesList, List<Printer> printerEscList, List<KitchenPrinterModel> availablePrinters, InstallationDataModel instData, string fiscName, string applicationPath, IConfiguration configuration, IApplicationBuilder applicationBuilder, OposDeviceNamesEnum? deviceType = OposDeviceNamesEnum.MicADHME)
        {
            // instance identification
            this.InstanceID = ExtcerTypesEnum.Opos;
            // initialization of variable to be used with lock on printouts
            this.thisLock = new object();
            this.PrintersTemplates = printerTemplatesList;
            this.PrintersEscList = printerEscList;
            this.availablePrinters = availablePrinters;
            this.InstallationData = instData;
            this.FiscalName = fiscName;
            this.applicationPath = applicationPath;
            this.configuration = configuration;
            this.DeviceName = deviceType.ToString();
            this.CashAccountIds = new List<int>();
            this.CCAccountIds = new List<int>();
            this.CreditAccountIds = new List<int>();
            this.ErrorCode = 0;
            this.ClaimTimeout = 2000;
            this.logger = (ILogger<OposExtcer>)applicationBuilder.ApplicationServices.GetService(typeof(ILogger<OposExtcer>));
            this.localHubInvoker = (LocalHubInvoker)applicationBuilder.ApplicationServices.GetService(typeof(LocalHubInvoker));
            setupAccountIds();
            InitializeFiscal();
            OpenFiscal();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~OposExtcer()
        {
            CloseFiscal();
        }

        #region Override Actions

        /// <summary>
        /// Print receipt interface
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="fiscalName"></param>
        /// <returns></returns>
        public override PrintResultModel PrintReceipt(ReceiptModel receiptModel, string fiscalName)
        {
            if (string.IsNullOrEmpty(receiptModel.InvoiceIndex) || receiptModel.InvoiceIndex == "1")
            {
                // print to OPOS
                return PrintOposReceipt(receiptModel, fiscalName);
            }
            else
            {
                // print to generic
                logger.LogInformation("Using Generic Printer instead of OPOS...");
                return GenericCommon.PrintGenericReceipt(receiptModel, fiscalName, PrintersTemplates, availablePrinters, PrintersEscList, this, thisLock);
            }
        }

        /// <summary>
        /// Print receipt summary interface
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <returns></returns>
        public override PrintResultModel PrintReceiptSum(ReceiptModel receiptModel)
        {
            PrintResultModel printResult = new PrintResultModel();
            ReceiptModel currentReceiptData = receiptModel;
            string error = "";
            try
            {
                printResult.OrderNo = receiptModel.OrderId.ToString();
                printResult.ReceiptNo = receiptModel.ReceiptNo;
                printResult.ExtcerType = ExtcerTypesEnum.Generic;
                receiptModel.FiscalType = FiscalTypeEnum.Opos;
                // get printer for receipt summary
                KitchenPrinterModel printerToPrint;
                printerToPrint = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Report);
                // get receipt printer template
                RollerTypeReportModel template;
                template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Value.ReportName == printerToPrint.TemplateShortName).Value.FirstOrDefault().Value;
                if (template == null)
                {
                    logger.LogInformation(ExtcerLogger.Log("Generic template not found", FiscalName));
                }
                printResult.ReceiptType = PrintModeEnum.InvoiceSum;
                printResult.ReceiptData = ProcessReceiptSumTemplate(template, receiptModel, printerToPrint);
                printResult.Status = PrintStatusEnum.Printed;
            }
            catch (CustomException exception)
            {
                printResult.ErrorDescription = exception.Message;
                logger.LogError(ExtcerLogger.logErr("Error printing generic ReceiptSum. ", exception, FiscalName));
                printResult.Status = PrintStatusEnum.Failed;
                error = "Error printing generic ReceiptSum: " + exception.Message + " StackTRace: " + exception.StackTrace;
            }
            catch (Exception exception)
            {
                printResult.ErrorDescription = ExtcerErrorHelper.GetErrorDescription(ExtcerErrorHelper.GENERAL_ERROR);
                logger.LogError(ExtcerLogger.logErr("Error printing generic ReceiptSum: ", exception, FiscalName));
                printResult.Status = PrintStatusEnum.Failed;
                error = "Error printing generic ReceiptSum: " + exception.Message + " StackTRace: " + exception.StackTrace;
            }
            ArtcasLogger.LogArtCasXML(receiptModel, DateTime.Now, error, printResult.ReceiptData, ReceiptReceiveTypeEnum.WEB, printResult.Status, FiscalName, printResult.Id);
            return printResult;
        }

        /// <summary>
        /// Print X report interface
        /// </summary>
        /// <returns></returns>
        public override PrintResultModel PrintX()
        {
            int intExtendedResultCode = 0;
            logger.LogInformation(ExtcerLogger.Log("Checking device availability...", FiscalName));
            CheckDeviceOk();
            printResult = new PrintResultModel();
            printResult.ExtcerType = ExtcerTypesEnum.Opos;
            printResult.ReceiptType = PrintModeEnum.XReport;
            printResult.Status = PrintStatusEnum.Failed;
            logger.LogInformation(ExtcerLogger.Log("Printing X Report...", FiscalName));
            ErrorCode = OPOS.PrintXReport();
            logger.LogInformation(ExtcerLogger.Log("X Report function returned result.", FiscalName));
            intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
            printResult.ErrorDescription = LogOposError("at PrintXReport", ErrorCode, intExtendedResultCode);
            if (ErrorCode > 0)
            {
                logger.LogError(ExtcerLogger.Log("Errocode: " + ErrorCode.ToString() + ", Description: " + (printResult.ErrorDescription ?? "<NULL>"), FiscalName));
                printResult.Status = PrintStatusEnum.Failed;
                return printResult;
            }
            else if (ErrorCode == 0)
            {
                printResult.Status = PrintStatusEnum.Printed;
            }
            return printResult;
        }

        /// <summary>
        /// Print Z report interface
        /// </summary>
        /// <param name="zData"></param>
        /// <returns></returns>
        public override PrintResultModel PrintZ(ZReportModel zData)
        {
            // get Z report totals before Z report issue
            printResult = GetZTotal();
            string strZReportTotal = printResult.ResponseValue;
            // back to normal Z report issue
            int intExtendedResultCode = 0;
            logger.LogInformation(ExtcerLogger.Log("Checking device availability...", FiscalName));
            CheckDeviceOk();
            Thread.Sleep(200);
            OPOS.PrintRecVoid("");
            Thread.Sleep(300);
            CheckDeviceOk();
            printResult = new PrintResultModel();
            printResult.ExtcerType = ExtcerTypesEnum.Opos;
            printResult.ReceiptType = PrintModeEnum.ZReport;
            printResult.Status = PrintStatusEnum.Failed;
            printResult.ReceiptNo = zData.ReportNo.ToString();
            printResult.ReceiptData = GetZText(zData);
            // retry to issue Z report, when we get device is busy error (113)
            int retries = 0;
            do
            {
                retries++;
                logger.LogInformation(ExtcerLogger.Log("Printing Z Report...", FiscalName));
                ErrorCode = OPOS.PrintZReport();
                if (ErrorCode == 113)
                {
                    logger.LogWarning(ExtcerLogger.Log("ErrorCode=113 (The device is busy), retring up to 12 times...", FiscalName));
                    Thread.Sleep(250);
                }
            } while (ErrorCode == 113 && retries < 12);
            intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
            printResult.ErrorDescription = LogOposError("at PrintzReport", ErrorCode, intExtendedResultCode);
            if (ErrorCode > 0)
            {
                printResult.Status = PrintStatusEnum.Failed;
                return printResult;
            }
            else if (ErrorCode == 0)
            {
                printResult.Status = PrintStatusEnum.Printed;
                // get receipt no
                string receiptNo;
                int args;
                logger.LogInformation(ExtcerLogger.Log("Getting Data (receiptNo) from device...", FiscalName));
                OPOS.GetData(OposErrorHelper.FPTR_GD_Z_REPORT, out args, out receiptNo);
                printResult.ReceiptNo = receiptNo;
                CleanUpJournalFiles();
                // create export file -> Z report totals (OPOS)
                logger.LogInformation(ExtcerLogger.Log("Creating export file Z_Report_Totals.txt...", FiscalName));
                using (StreamWriter streamWriter = File.AppendText("Z_Report_Totals.txt"))
                {
                    streamWriter.WriteLine(DateTime.Now.ToString("dd-MM-yyyy, HH:mm:ss, ") + zData.PosDescription + ", " + zData.PosCode + ", " + zData.ReportNo + ", " + strZReportTotal);
                }
            }
            return printResult;
        }

        /// <summary>
        /// Get Z total interface
        /// </summary>
        /// <returns></returns>
        public override PrintResultModel GetZTotal()
        {
            logger.LogInformation(ExtcerLogger.Log("Getting Ztotals", FiscalName));
            printResult = new PrintResultModel();
            printResult.ExtcerType = ExtcerTypesEnum.Opos;
            printResult.ReceiptType = PrintModeEnum.ZTotals;
            printResult.Status = PrintStatusEnum.Unknown;
            OPOS.TotalizerType = OposErrorHelper.FPTR_TT_RECEIPT;
            string data;
            int s;
            OPOS.GetData(OposErrorHelper.FPTR_GD_DAILY_TOTAL, out s, out data);
            logger.LogInformation(ExtcerLogger.Log("ZTotal data: " + (data ?? "<null>"), FiscalName));
            printResult.ResponseValue = data;
            return printResult;
        }

        /// <summary>
        /// Print graphic interface
        /// </summary>
        /// <param name="printername"></param>
        /// <param name="strToPrint"></param>
        /// <param name="blnUseDefaultMargins"></param>
        public override void PrintGraphic(string printername, string strToPrint, bool blnUseDefaultMargins = true)
        {
            logger.LogInformation("Starting 'PrintGraphic' within OposExtcer.");
            try
            {
                Stream strm = GenericCommon.GenerateStreamFromString(strToPrint);
                streamToPrint = new StreamReader(strm);
                try
                {
                    printFont = new Font("Courier New", 9, FontStyle.Bold);
                    PrintDocument pd = new PrintDocument();
                    if (blnUseDefaultMargins)
                    {
                        pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
                    }
                    else
                    {
                        pd.PrintPage += new PrintPageEventHandler(pd_PrintPage_Custom);
                    }
                    pd.PrinterSettings.PrinterName = printername;
                    pd.Print();
                }
                finally
                {
                    streamToPrint.Close();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Print image interface
        /// </summary>
        public override void PrintImage()
        {
            CheckDeviceOk();
            return;
        }

        /// <summary>
        /// Open drawer interface
        /// </summary>
        public override void OpenDrawer()
        {
            int intExtendedResultCode = 0;
            CheckDeviceOk();
            ErrorCode = OPOS.OpenDrawer();
            intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
            LogOposError("at OpenDrawer", ErrorCode, intExtendedResultCode);
            return;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Payment types initialization.
        /// </summary>
        private void setupAccountIds()
        {
            if (InstallationData != null)
            {
                // cash accounts
                var CashAccs = InstallationData.OposMethodOfPaymentCASH.Split(',');
                foreach (var item in CashAccs)
                {
                    int res = 0;
                    var isint = int.TryParse(item, out res);
                    if (isint)
                    {
                        CashAccountIds.Add(res);
                    }
                }
                // credit card accounts
                var CCAccs = InstallationData.OposMethodOfPaymentCC.Split(',');
                foreach (var item in CCAccs)
                {
                    int res = 0;
                    var isint = int.TryParse(item, out res);
                    if (isint)
                    {
                        CCAccountIds.Add(res);
                    }
                }
                // credit accounts
                var CreditAccs = InstallationData.OposMethodOfPaymentCREDIT.Split(',');
                foreach (var item in CreditAccs)
                {
                    int res = 0;
                    var isint = int.TryParse(item, out res);
                    if (isint)
                    {
                        CreditAccountIds.Add(res);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeFiscal()
        {
            OPOS = new OPOSFiscalPrinterClass();
            // register OPOS events
            OPOS.ErrorEvent += new _IOPOSFiscalPrinterEvents_ErrorEventEventHandler(Opos_ErrorEvent);
            OPOS.StatusUpdateEvent += new _IOPOSFiscalPrinterEvents_StatusUpdateEventEventHandler(Opos_StatusUpdateEvent);
            OPOS.DirectIOEvent += new _IOPOSFiscalPrinterEvents_DirectIOEventEventHandler(Opos_DirectIOEvent);
            OPOS.OpenDrawerEvent += new _IOPOSFiscalPrinterEvents_OpenDrawerEventEventHandler(Opos_OpenDrawerEvent);
            OPOS.OutputCompleteEvent += new _IOPOSFiscalPrinterEvents_OutputCompleteEventEventHandler(Opos_OutputCompleteEvent);
        }

        /// <summary>
        /// OPOS error event handler
        /// </summary>
        /// <param name="ResultCode"></param>
        /// <param name="ResultCodeExtended"></param>
        /// <param name="ErrorLocus"></param>
        /// <param name="pErrorResponse"></param>
        private void Opos_ErrorEvent(int ResultCode, int ResultCodeExtended, int ErrorLocus, ref int pErrorResponse)
        {
            logger.LogError(ExtcerLogger.logErr("OPOS error event: errorCode- " + ResultCode + " messages- " + OPOS.ErrorString, FiscalName));
            LogOposError("Event error", ResultCode);
        }

        /// <summary>
        /// OPOS status update event handler
        /// </summary>
        /// <param name="errorCode"></param>
        private void Opos_StatusUpdateEvent(int errorCode)
        {
            string error = "OPOS error StatusUpdateEvent: errorCode- " + errorCode + " message- " + OposErrorHelper.GetStandardErrorMessage(errorCode) + (114 == errorCode ? Environment.NewLine + "extended message- " + OposErrorHelper.GetExtendedErrorMessage(OPOS.ResultCodeExtended) : "");
            localHubInvoker.SendError(error);
            logger.LogError(ExtcerLogger.logErr(error, FiscalName));
        }

        /// <summary>
        /// OPOS direct IO event handler
        /// </summary>
        /// <param name="EventNumber"></param>
        /// <param name="pData"></param>
        /// <param name="pString"></param>
        private void Opos_DirectIOEvent(int EventNumber, ref int pData, ref string pString)
        {
            localHubInvoker.SendError("Opos_OutputCompleteEvent=> EventNumber:" + EventNumber + "pData: " + pData + " pString: " + pString);
        }

        /// <summary>
        /// OPOS open drawer event handler
        /// </summary>
        /// <param name="printStatus"></param>
        private void Opos_OpenDrawerEvent(int printStatus)
        {
            localHubInvoker.SendError("Opos_OpenDrawerEvent =>  printStatus: " + printStatus);
        }

        /// <summary>
        /// OPOS output complete event handler
        /// </summary>
        /// <param name="outputId"></param>
        private void Opos_OutputCompleteEvent(int outputId)
        {
            string error = "OPOS error Opos_OutputCompleteEvent: errorCode- " + outputId + " message- " + OposErrorHelper.GetStandardErrorMessage(outputId) + ((114 == outputId) ? Environment.NewLine + "extended message- " + OposErrorHelper.GetExtendedErrorMessage(OPOS.ResultCodeExtended) : "");
            localHubInvoker.SendError(error);
            logger.LogError(ExtcerLogger.logErr(error, FiscalName));
        }

        /// <summary>
        /// 
        /// </summary>
        private void OpenFiscal()
        {
            int intExtendedResultCode = 0;
            OPOS.ClearError();
            var ss = OPOS.ErrorString;
            logger.LogInformation(ExtcerLogger.Log(" ", FiscalName));
            logger.LogInformation(ExtcerLogger.Log("---------- " + this.FiscalName + " ---------", FiscalName));
            logger.LogInformation(ExtcerLogger.Log("Opos: Initializing device!", FiscalName));
            ErrorCode = OPOS.Open(DeviceName);
            LogOposError("at OposOpenDevice", ErrorCode, intExtendedResultCode);
            ErrorCode = OPOS.ClaimDevice(ClaimTimeout);
            LogOposError("at OposClaimDevice", ErrorCode, intExtendedResultCode);
            logger.LogInformation("Supports totalizers: " + OPOS.CapTotalizerType);
            OPOS.TotalizerType = OposErrorHelper.FPTR_TT_RECEIPT;
            OPOS.DeviceEnabled = true;
            var s = OPOS.ErrorString;
            if (!string.IsNullOrEmpty(s))
                logger.LogError(ExtcerLogger.logErr(s, FiscalName));
            logger.LogInformation(ExtcerLogger.Log("--------------------------------", FiscalName));
        }

        /// <summary>
        /// 
        /// </summary>
        private void CloseFiscal()
        {
            int intExtendedResultCode = 0;
            try
            {
                logger.LogInformation(ExtcerLogger.Log("OPOS: Closing Serial", FiscalName));
                OPOS.DeviceEnabled = false;
                ErrorCode = OPOS.ReleaseDevice();
                LogOposError("at OposReleaseDevice", ErrorCode, intExtendedResultCode);
                ErrorCode = OPOS.Close();
                LogOposError("at OposCloseDevice", ErrorCode, intExtendedResultCode);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Checks the device status and reinitializes it.
        /// </summary>
        private void CheckDeviceOk()
        {
            if (!OPOS.Claimed)
            {
                logger.LogError(ExtcerLogger.logErr("Opos: Device not claimed!", FiscalName));
                ErrorCode = OPOS.ClaimDevice(ClaimTimeout);
                int intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                if (ErrorCode == 0)
                {
                    logger.LogInformation(ExtcerLogger.Log("Opos: Device claimed OK!", FiscalName));
                }
                else
                {
                    LogOposError("Opos error", ErrorCode, intExtendedResultCode);
                }
            }
            if (!OPOS.DeviceEnabled)
            {
                logger.LogError(ExtcerLogger.logErr("Opos: Device not enabled!", FiscalName));
                OPOS.DeviceEnabled = true;
                logger.LogInformation(ExtcerLogger.Log("Opos: Device enabled OK!", FiscalName));
            }
            if (OPOS.CoverOpen)
            {
                logger.LogInformation(ExtcerLogger.Log("Opos: Cover is open!", FiscalName));
            }
        }

        /// <summary>
        /// Prints receipt comments
        /// Comments must be EXCACTLY 3 Rows, with format:
        /// line 1 -> Y/0/ text /
        /// line 2 -> 0/ text /
        /// line 3 -> 0/ text /
        /// </summary>
        /// <param name="receiptModel"></param>
        private void PrintComments(ReceiptModel receiptModel)
        {
            List<string> comments = GetReceiptCommentsText(receiptModel, PrinterTypeEnum.Receipt).Take(3).ToList();
            if (comments.Count > 0)
            {
                if (InstallationData.OposSupportOldERGO != null && InstallationData.OposSupportOldERGO == true)
                {
                    foreach (var item in comments)
                    {
                        OPOS.PrintRecMessage(item.Replace('/', '-'));
                    }
                }
                else
                {
                    comments[0] = comments[0].Replace('/', '-');
                    // choose footer command (Y or m), from v.2.2.20.0 
                    string footerOposCommand = configuration.GetValue<string>("FooterOposCommand");
                    if (footerOposCommand == "m")
                        comments[0] = "m/" + comments[0] + "/";
                    else if (footerOposCommand == "Y")
                        comments[0] = "Y/0/" + comments[0] + "/";
                    else
                        throw new Exception("Footer command must be Y or m  (Commands are CASE SENSITIVE).");
                    if (comments.Count >= 2)
                    {
                        comments[1] = comments[1].Replace('/', '-');
                        if (footerOposCommand == "Y")
                            comments[1] = "0/" + comments[1] + "/";
                        else
                            comments[1] = comments[1] + "/";
                    }
                    else
                    {
                        if (footerOposCommand == "m")
                            comments.Add("/");
                        else
                            comments.Add("0/ /");
                    }
                    if (comments.Count >= 3)
                    {
                        comments[2] = comments[2].Replace('/', '-');
                        if (footerOposCommand == "Y")
                            comments[2] = "0/" + comments[2] + "/";
                        else
                            comments[2] = comments[2] + "/";
                    }
                    else
                    {
                        if (footerOposCommand == "m")
                            comments.Add("/");
                        else
                            comments.Add("0/ /");
                    }
                    // create the string to print
                    string result = comments.Aggregate(new StringBuilder(""), (current, next) => current.Append(next)).ToString();
                    logger.LogDebug("Comments:" + result);
                    int res = 0;
                    var ss = OPOS.DirectIO(0, res, result);
                    if (ss != 0)
                    {
                        var s = OPOS.ErrorString;
                        logger.LogInformation("In OPOS PrintComments: " + s);
                    }
                }
            }
            else
            {
                int res = 0;
                OPOS.DirectIO(0, res, "m///");
            }
        }

        /// <summary>
        /// In case of error code (resCode!=0), sends OPOS a void command and returns false, else returns true.
        /// </summary>
        /// <param name="resCode"></param>
        /// <returns></returns>
        private bool CloseReceiptValidationOk(int resCode)
        {
            if (resCode != 0)
            {
                logger.LogWarning("CANCELING RECEIPT");
                OPOS.PrintRecVoid("ΑΚΥΡΩΣΗ");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Append to log file the latest OPOS error. Returns the error description.
        /// </summary>
        /// <param name="message">The application message (such as caller or app region, etc).</param>
        /// <param name="resCode">The result code (error code number).</param>
        /// <param name="intExtendedResultCode">In case the resCode equals 114 (OPOS_E_EXTENDED), this is the extended result code.</param>
        /// <returns></returns>
        private string LogOposError(string message, int resCode, int intExtendedResultCode = 0)
        {
            if (resCode == 0)
            {
                return string.Empty;
            }
            string err = "    OPOS error: " + message + " ErrorCode- " + resCode + " Message- " + OPOS.ErrorString;
            if (OPOS.ErrorString.Contains("Field too long"))
                err = err + "  -->>  TRY redusing the Max length in 'Installation Data' <<--";
            string err2 = string.Empty;
            string errorToPOS = string.Empty;
            if (intExtendedResultCode == 0)
            {
                err2 = "\r\n    WITH GETERROR: " + message + " errorCode- " + resCode + " message- " + OposErrorHelper.GetStandardErrorMessage(resCode) + "\r\n";
                errorToPOS = OposErrorHelper.GetStandardErrorMessage(resCode);
            }
            else
            {
                err2 = "\r\n    WITH GETERROR: " + message + " errorCode- " + resCode + " message- " + OposErrorHelper.GetStandardErrorMessage(resCode) + "\r\n     extended errorCode- " + intExtendedResultCode + " message- " + OposErrorHelper.GetExtendedErrorMessage(intExtendedResultCode) + "\r\n";
                errorToPOS = OposErrorHelper.GetStandardErrorMessage(resCode) + "\r\n" + OposErrorHelper.GetExtendedErrorMessage(intExtendedResultCode);
            }
            logger.LogError(ExtcerLogger.logErr("\r\n    ================================================\r\n" + err, FiscalName));
            logger.LogError(ExtcerLogger.logErr(err2 + "\r\n    ================================================\r\n", FiscalName));
            logger.LogWarning("CANCELING RECEIPT");
            OPOS.PrintRecVoid("ΑΚΥΡΩΣΗ");
            if (printResult != null)
            {
                printResult.ErrorDescription = errorToPOS;
                return printResult.ErrorDescription;
            }
            return ExtcerErrorHelper.GetErrorDescription(ExtcerErrorHelper.GENERAL_ERROR);
        }

        /// <summary>
        /// Get data for journal from fiscal device
        /// </summary>
        private void GetDataForJournalFromFiscalDevice()
        {
            try
            {
                string ergoresponse = string.Empty;
                int timesTried = 0;
                int res = 0;
                while (true)
                {
                    // send Q command
                    ergoresponse = "Q/";
                    var intres = OPOS.DirectIO(0, ref res, ref ergoresponse);
                    if (intres != 0)
                    {
                        // directIO returned error
                        logger.LogError("In Receipt. Could not empty journal");
                        break;
                    }
                    // response from OPOS is / separated
                    var splitted = ergoresponse.Split(new Char[] { '/' });
                    // check if response has valid length
                    if (splitted.Length >= 3)
                    {
                        // check for "*_a.txt" file name
                        if (splitted[2].EndsWith("_a.txt"))
                        {
                            ReadJournalFileFromFiscal(splitted[2]);
                        }
                        // check for "*_b.txt" file name
                        else if (splitted[2].EndsWith("_b.txt"))
                        {
                            ReadJournalFileFromFiscal(splitted[2]);
                        }
                        // check for "*_e.txt" file name
                        else if (splitted[2].EndsWith("_e.txt"))
                        {
                            ReadJournalFileFromFiscal(splitted[2]);
                        }
                    }
                    else
                    {
                        // if 1st argument is 3 then receipt retrieval is over
                        if (splitted[0].Equals("3"))
                        {
                            logger.LogInformation("In print Receipt. Emptied journal for receipt: " + printResult.ReceiptNo);
                            break;
                        }
                    }
                    timesTried++;
                    if (timesTried > 40)
                    {
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("Error printing OPOS receipt: #" + printResult.ReceiptNo + "\r\nError Description: ", exception, FiscalName));
            }
        }

        /// <summary>
        /// Reads from the fiscal device the selected file ( "*_a.txt" or "*_b.txt" or "*_e.txt" ) and stores it to the predefined path within the hard disk.
        /// </summary>
        /// <param name="strFileName">The file name to use for storage within hard disk.</param>
        private void ReadJournalFileFromFiscal(String strFileName)
        {
            String strErgoResponse = "Q/";
            String[] strSplitted;
            int res = 0;
            int intResult;
            // read until end of file <=> first 2 characters are "2/"
            StringBuilder strFileContents = new StringBuilder();
            do
            {
                // send Q command
                strErgoResponse = "Q/";
                intResult = OPOS.DirectIO(0, ref res, ref strErgoResponse);
                if (intResult != 0)
                {
                    // directIO returned error
                    logger.LogError("Break within '_a.txt' file read. Could not empty journal");
                    break;
                }
                // response from OPOS is / separated
                strSplitted = strErgoResponse.Split(new Char[] { '/' });
                strFileContents.Append(strSplitted[1]);
            }
            while (!strErgoResponse.StartsWith("2/"));
            // if the predefined path does not exist, create it
            DirectoryInfo di_JournalRoot = new DirectoryInfo(InstallationData.JournalPath);
            if (!di_JournalRoot.Exists)
            {
                di_JournalRoot.Create();
            }
            // store strFileContents to a file at predefined path for journal
            using (StreamWriter streamWriter = File.CreateText(InstallationData.JournalPath + '\\' + strFileName))
            {
                streamWriter.Write(strFileContents.ToString());
            }
        }

        /// <summary>
        /// Create a new work-day-log directory within journal, copy all files from journal-root to our new directory, delete all files from journal-root.
        /// </summary>
        private void CleanUpJournalFiles()
        {
            logger.LogInformation(ExtcerLogger.Log("Cleaning Up Journal Files...", FiscalName));
            try
            {
                // Create a reference to a directory.
                DirectoryInfo di_JournalRoot = new DirectoryInfo(InstallationData.JournalPath);
                // Create the directory only if it does not already exist.
                if (!di_JournalRoot.Exists)
                {
                    di_JournalRoot.Create();
                }
                String strFiscalDate = "";
                logger.LogInformation(ExtcerLogger.Log("Getting Date...", FiscalName));
                if (OPOS.GetDate(out strFiscalDate) != 0)
                {
                    logger.LogError(ExtcerLogger.logErr("Error, cannot read fiscal device date ", FiscalName));
                    return;
                }
                String strSubDirectoryName = String.Format("{0}_{1}", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.ParseExact(strFiscalDate, "ddMMyyyyHHmm", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"));
                // Create a subdirectory in the directory just created.
                logger.LogInformation(ExtcerLogger.Log("Creatting Subdirectory...", FiscalName));
                DirectoryInfo di_WorkDay = di_JournalRoot.CreateSubdirectory(strSubDirectoryName);
                // Copy all files from journal-root to our new subdirectory
                String strSourcePath = InstallationData.JournalPath;
                String strDestPath = InstallationData.JournalPath + '\\' + strSubDirectoryName;
                string[] strTextFilesList = Directory.GetFiles(strSourcePath, "*.txt");
                foreach (String f in strTextFilesList)
                {
                    // Remove path from the file name.
                    String strFileName = f.Substring(strSourcePath.Length + 1);
                    // Use the Path.Combine method to safely append the file name to the path.
                    // Will overwrite if the destination file already exists.
                    File.Copy(Path.Combine(strSourcePath, strFileName), Path.Combine(strDestPath, strFileName), true);
                }
                // Delete all files from journal-root
                logger.LogInformation(ExtcerLogger.Log("Deleting all files from journal-root...", FiscalName));
                foreach (String f in strTextFilesList)
                {
                    File.Delete(f);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("Error at journal cleanup: ", exception, FiscalName));
            }
        }

        /// <summary>
        /// case invoice index == 1
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="fiscalName"></param>
        /// <returns></returns>
        private PrintResultModel PrintOposReceipt(ReceiptModel receiptModel, string fiscalName)
        {
            int intExtendedResultCode;
            int? maxPrintableChars = InstallationData.OposMaxString;
            CheckDeviceOk();
            printResult = new PrintResultModel();
            printResult.InvoiceIndex = receiptModel.InvoiceIndex;
            printResult.ExtcerType = ExtcerTypesEnum.Opos;
            receiptModel.FiscalType = FiscalTypeEnum.Opos;
            try
            {
                if (receiptModel.IsVoid)
                {
                    printResult = PrintVoidReceipt(receiptModel, fiscalName);
                    return printResult;
                }
                printResult.ReceiptNo = receiptModel.ReceiptNo;
                printResult.ReceiptData = GetReceiptPreviewText(receiptModel);
                ErrorCode = OPOS.BeginFiscalReceipt(true);
                intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                printResult.ErrorDescription = LogOposError("at beginFiscalReceipt", ErrorCode, intExtendedResultCode);
                if (ErrorCode > 0)
                {
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ReceiptNo = receiptModel.ReceiptNo;
                    return printResult;
                }
                decimal total = 0;
                logger.LogInformation(ExtcerLogger.Log("------------------ RECEIPT #" + receiptModel.ReceiptNo + " -------------------", FiscalName));
                foreach (var item in receiptModel.Details)
                {
                    string descr = string.IsNullOrEmpty(item.ItemDescr) ? "    " : item.ItemDescr.Replace('/', '-');
                    descr = descr.Replace("\t", " ");
                    descr = new string(descr.Where(c => !char.IsPunctuation(c)).ToArray());
                    if (maxPrintableChars != null && item.ItemDescr.Length > maxPrintableChars)
                    {
                        descr = descr.Substring(0, (int)maxPrintableChars - 1);
                    }
                    if (item.IsVoid)
                    {
                        total = total - (item.ItemPrice * item.ItemQty);
                        ErrorCode = OPOS.PrintRecVoidItem(descr, item.ItemPrice, (Int32)(item.ItemQty * 1000), item.ItemVatRate, item.ItemPrice * (-1), 1);
                        intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                        printResult.ErrorDescription = LogOposError("at void item", ErrorCode, intExtendedResultCode);
                        if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                        {
                            printResult.Status = PrintStatusEnum.Failed;
                            printResult.ReceiptNo = receiptModel.ReceiptNo;
                            return printResult;
                        }
                    }
                    else
                    {
                        if (item.IsChangeItem)
                        {
                            total = total - (item.ItemGross * item.ItemQty * -1);
                            ErrorCode = OPOS.PrintRecRefund(descr, (Math.Abs(item.ItemGross) * item.ItemQty), item.ItemVatRate);
                            intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                            printResult.ErrorDescription = LogOposError("at PrintRec refund item", ErrorCode, intExtendedResultCode);
                            if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                            {
                                printResult.Status = PrintStatusEnum.Failed;
                                printResult.ReceiptNo = receiptModel.ReceiptNo;
                                return printResult;
                            }
                        }
                        else
                        {
                            if (item.ItemPrice != null && item.ItemPrice > 0)
                            {
                                total = total + (item.ItemPrice * item.ItemQty);
                                ErrorCode = OPOS.PrintRecItem(descr, item.ItemPrice, (Int32)(item.ItemQty * 1000), item.ItemVatRate, item.ItemPrice, "Euro");
                                intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                                printResult.ErrorDescription = LogOposError("at PrintRec item", ErrorCode, intExtendedResultCode);
                                logger.LogInformation(ExtcerLogger.Log("ITEM --  Description: " + item.ItemDescr + " QTY: " + item.ItemQty + " Price: " + item.ItemPrice, FiscalName));
                                if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                                {
                                    printResult.Status = PrintStatusEnum.Failed;
                                    printResult.ReceiptNo = receiptModel.ReceiptNo;
                                    return printResult;
                                }
                                if (item.ItemDiscount != null && item.ItemDiscount > 0)
                                {
                                    total = total - (decimal)(item.ItemDiscount * item.ItemQty);
                                    string discdescr = "Έκπτωση " + item.ItemDescr;
                                    discdescr = new string(discdescr.Where(c => !char.IsPunctuation(c)).ToArray());
                                    if (maxPrintableChars != null && discdescr.Length > maxPrintableChars)
                                    {
                                        discdescr = discdescr.Substring(0, (int)maxPrintableChars - 1);
                                    }
                                    ErrorCode = OPOS.PrintRecItemAdjustment(OposErrorHelper.FPTR_AT_AMOUNT_DISCOUNT, discdescr, (decimal)item.ItemDiscount, item.ItemVatRate);
                                    intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                                    printResult.ErrorDescription = LogOposError("at PrintRec discount item: " + discdescr, ErrorCode, intExtendedResultCode);
                                    logger.LogInformation(ExtcerLogger.Log("Item DISCOUNT -- : " + (decimal)item.ItemDiscount, FiscalName));
                                    if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                                    {
                                        printResult.Status = PrintStatusEnum.Failed;
                                        printResult.ReceiptNo = receiptModel.ReceiptNo;
                                        return printResult;
                                    }
                                }
                            }
                        }
                        if (item.Extras.Count > 0)
                        {
                            if (PrintOposReceipt_ProcessExtras(item, maxPrintableChars, receiptModel, ref total) != null)
                            {
                                return printResult;
                            }
                        }
                    }
                }
                logger.LogInformation("IS TOTAL DISCOUNT: " + (receiptModel.TotalDiscount != null) + " Value: " + (receiptModel.TotalDiscount ?? -1));
                if (receiptModel.TotalDiscount != null && receiptModel.TotalDiscount > 0)
                {
                    ErrorCode = OPOS.PrintRecSubtotalAdjustment(OposErrorHelper.FPTR_AT_AMOUNT_DISCOUNT, "Έκπτωση", (decimal)receiptModel.TotalDiscount);
                    intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                    printResult.ErrorDescription = LogOposError("at print total receipt discount ", ErrorCode, intExtendedResultCode);
                    logger.LogInformation(ExtcerLogger.Log("RECEIPT DISCOUNT -- : " + (decimal)receiptModel.TotalDiscount, FiscalName));
                    if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                    {
                        printResult.Status = PrintStatusEnum.Failed;
                        printResult.ReceiptNo = receiptModel.ReceiptNo;
                        return printResult;
                    }
                }
                logger.LogInformation(ExtcerLogger.Log("------------ END RECEIPT -------------\r\n", FiscalName));
                // get receipt number
                int args;
                string receiptTotal;
                OPOS.GetData(OposErrorHelper.FPTR_GD_CURRENT_TOTAL, out args, out receiptTotal);
                var ss = OPOS.ErrorString;
                var voucher = receiptModel.PaymentsList.Where(f => (int)f.AccountType == 6).FirstOrDefault();
                if (voucher != null)
                {
                    logger.LogInformation("Voucher - Tichet Restaurant");
                    ErrorCode = OPOS.PrintRecNotPaid(voucher.Description, (decimal)(voucher.Amount));
                    intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                    LogOposError("at Voucher", ErrorCode, intExtendedResultCode);
                    if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                    {
                        printResult.Status = PrintStatusEnum.Failed;
                        printResult.ReceiptNo = receiptModel.ReceiptNo;
                        return printResult;
                    }
                }
                logger.LogInformation("Total is: " + total);
                bool blnUseChanges = false;
                decimal dcmlCashAmount = 0;
                string strCashAmount = receiptModel.CashAmount;
                if (!string.IsNullOrEmpty(strCashAmount))
                {
                    blnUseChanges = true;
                    dcmlCashAmount = Convert.ToDecimal(strCashAmount, CultureInfo.InvariantCulture);
                }
                if (receiptModel.PaymentsList == null || receiptModel.PaymentsList.Count == 0)
                {
                    receiptModel.PaymentsList = new List<PaymentTypeModel>();
                    receiptModel.PaymentsList.Add(new PaymentTypeModel() { Description = "Cash", AccountType = 1, Amount = receiptModel.Total });
                    logger.LogWarning("     ---->>      PaymentsList is EMPTY.   Printing as CASH   (AccountType=1, Description=Cash)       <<---");
                }
                if (receiptModel.PaymentsList.Count > 0)
                {
                    foreach (var pw in receiptModel.PaymentsList)
                    {
                        bool blnIsLastItem = pw.Equals(receiptModel.PaymentsList.Last());
                        if (pw.AccountType != null)
                        {
                            if (pw.AccountType != 6)
                            {
                                OPOS.PreLine = pw.Description;
                            }
                            if (InstallationData.OposMethodOfPaymentCASH != null && CashAccountIds.Contains((int)pw.AccountType))
                            {
                                // This is cash type handling
                                if (blnUseChanges)
                                {
                                    ErrorCode = OPOS.PrintRecTotal(0, dcmlCashAmount, "a");
                                }
                                else
                                {
                                    ErrorCode = OPOS.PrintRecTotal(0, blnIsLastItem ? 0 : pw.Amount ?? 0, "a");
                                }
                            }
                            else if (InstallationData.OposMethodOfPaymentCC != null && CCAccountIds.Contains((int)pw.AccountType))
                            {
                                // This is credit card type handling
                                ErrorCode = OPOS.PrintRecTotal(0, blnIsLastItem ? 0 : pw.Amount ?? 0, "b");
                            }
                            else if (InstallationData.OposMethodOfPaymentCREDIT != null && CreditAccountIds.Contains((int)pw.AccountType))
                            {
                                // This is credit type handling
                                ErrorCode = OPOS.PrintRecTotal(0, blnIsLastItem ? 0 : pw.Amount ?? 0, "c");
                            }
                            else if ((int)pw.AccountType == 6)
                            {
                                // This is ticket restaurant / voucher type handling
                            }
                            else
                            {
                                if (blnUseChanges)
                                {
                                    ErrorCode = OPOS.PrintRecTotal(0, dcmlCashAmount, "a");
                                }
                                else
                                {
                                    ErrorCode = OPOS.PrintRecTotal(0, blnIsLastItem ? 0 : pw.Amount ?? 0, "a");
                                }
                            }
                        }
                    }
                }
                else
                {
                    logger.LogInformation("NO PAYMENT TYPE FOUND -- PRINTING AS CASH");
                    if (blnUseChanges)
                    {
                        ErrorCode = OPOS.PrintRecTotal(0, dcmlCashAmount, "a");
                    }
                    else
                    {
                        ErrorCode = OPOS.PrintRecTotal(0, 0, "Μετρητά");
                    }
                    if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                    {
                        printResult.Status = PrintStatusEnum.Failed;
                        printResult.ReceiptNo = receiptModel.ReceiptNo;
                        return printResult;
                    }
                }
                intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                LogOposError("at PrintRecTotal", ErrorCode, intExtendedResultCode);
                if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                {
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ReceiptNo = receiptModel.ReceiptNo;
                    return printResult;
                }
                if (!CloseReceiptValidationOk(ErrorCode))
                {
                }
                if (receiptModel.IsVoid)
                {
                    ErrorCode = OPOS.PrintRecVoid("Ακύρωση");
                    intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                    printResult.ErrorDescription = LogOposError("Canceling receipt", ErrorCode, intExtendedResultCode);
                    if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                    {
                        printResult.Status = PrintStatusEnum.Failed;
                        printResult.ReceiptNo = receiptModel.ReceiptNo;
                        return printResult;
                    }
                }
                else
                {
                    string receiptNo;
                    ErrorCode = OPOS.GetData(OposErrorHelper.FPTR_GD_RECEIPT_NUMBER, out args, out receiptNo);
                    intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                    LogOposError("at GetReceiptNumber", ErrorCode, intExtendedResultCode);
                    receiptModel.ReceiptNo = receiptNo;
                    printResult.ReceiptNo = receiptNo;
                    logger.LogInformation("Receipt Number From OPOS = #" + printResult.ReceiptNo);
                    // print receipt comments
                    PrintComments(receiptModel);
                }
                ErrorCode = OPOS.EndFiscalReceipt(false);
                intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                printResult.ErrorDescription = LogOposError("at EndFiscalReceipt", ErrorCode, intExtendedResultCode);
                if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                {
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ReceiptNo = receiptModel.ReceiptNo;
                    return printResult;
                }
                if (!CloseReceiptValidationOk(ErrorCode))
                {
                }
                // if move receipt to journal is activated
                if (InstallationData.ErgoSpd3EmptyJournal != null && InstallationData.ErgoSpd3EmptyJournal == true)
                {
                    logger.LogInformation("Receipt Number From OPOS = #" + printResult.ReceiptNo);
                    GetDataForJournalFromFiscalDevice();
                    logger.LogInformation("Receipt Number From OPOS = #" + printResult.ReceiptNo);
                }
                printResult.Status = PrintStatusEnum.Failed;
                printResult.OrderNo = receiptModel.OrderNo;
                printResult.ExtcerType = ExtcerTypesEnum.Opos;
                printResult.ReceiptType = PrintModeEnum.Receipt;
                if (ErrorCode == 0)
                {
                    printResult.Status = PrintStatusEnum.Printed;
                }
                else
                {
                    printResult.Status = PrintStatusEnum.Failed;
                }
                logger.LogInformation("Receipt Number From OPOS = #" + printResult.ReceiptNo);
                ArtcasLogger.LogArtCasXML(receiptModel, DateTime.Now, LogOposError(" ", ErrorCode), new List<string>(), ReceiptReceiveTypeEnum.WEB, printResult.Status, fiscalName, printResult.Id);
            }
            catch (CustomException exception)
            {
                printResult.ErrorDescription = exception.Message;
                logger.LogError(ExtcerLogger.logErr("Error printing OPOS receipt: #" + printResult.ReceiptNo + "\r\nError Description: ", exception, FiscalName));
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("Error printing OPOS receipt (2): #" + printResult.ReceiptNo + "\r\nError Description: ", exception, FiscalName));
            }
            return printResult;
        }

        /// <summary>
        /// Print receipt extras
        /// </summary>
        /// <param name="item"></param>
        /// <param name="maxPrintableChars"></param>
        /// <param name="receiptModel"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        private PrintResultModel PrintOposReceipt_ProcessExtras(ReceiptItemsModel item, int? maxPrintableChars, ReceiptModel receiptModel, ref decimal total)
        {
            int intExtendedResultCode;
            foreach (var extra in item.Extras)
            {
                string extrdescr = String.IsNullOrEmpty(extra.ItemDescr) ? "    " : extra.ItemDescr;
                extrdescr = extrdescr.Replace("\t", " ");
                extrdescr = new string(extrdescr.Where(c => !char.IsPunctuation(c)).ToArray());
                if (maxPrintableChars != null && extrdescr.Length > maxPrintableChars)
                {
                    extrdescr = extrdescr.Substring(0, (int)maxPrintableChars - 1);
                }
                if (extra.ItemPrice != null && extra.ItemPrice > 0)
                {
                    var itemPrice = (decimal)extra.ItemPrice;
                    var itemGross = (decimal)extra.ItemGross;
                    if (extra.IsChangeItem)
                    {
                        total = total - item.ItemGross;
                        ErrorCode = OPOS.PrintRecRefund(extrdescr, itemGross, extra.ItemVatRate);
                        intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                        printResult.ErrorDescription = LogOposError("at PrintRec discount item", ErrorCode, intExtendedResultCode);
                        if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                        {
                            printResult.Status = PrintStatusEnum.Failed;
                            printResult.ReceiptNo = receiptModel.ReceiptNo;
                            return printResult;
                        }
                    }
                    else
                    {
                        total = total + itemPrice;
                        ErrorCode = OPOS.PrintRecItem(extrdescr, itemPrice, (Int32)(extra.ItemQty * 1000), extra.ItemVatRate, itemPrice, "Euro");
                        intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                        printResult.ErrorDescription = LogOposError("at PrintRec extras item", ErrorCode, intExtendedResultCode);
                        logger.LogInformation(ExtcerLogger.Log("  EXTRA -- Description: " + extra.ItemDescr + " QTY: " + extra.ItemQty + " Price: " + extra.ItemPrice, FiscalName));
                        if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                        {
                            printResult.Status = PrintStatusEnum.Failed;
                            printResult.ReceiptNo = receiptModel.ReceiptNo;
                            return printResult;
                        }
                    }
                    if (extra.ItemDiscount != null && extra.ItemDiscount > 0)
                    {
                        total = total - (decimal)extra.ItemDiscount;
                        ErrorCode = OPOS.PrintRecItemAdjustment(OposErrorHelper.FPTR_AT_AMOUNT_DISCOUNT, "Έκπτωση " + extrdescr, (decimal)extra.ItemDiscount, extra.ItemVatRate);
                        intExtendedResultCode = (114 == ErrorCode) ? OPOS.ResultCodeExtended : 0;
                        printResult.ErrorDescription = LogOposError("at PrintRec discount extra", ErrorCode, intExtendedResultCode);
                        logger.LogInformation(ExtcerLogger.Log("  EXTRA DISCOUNT -- : " + (decimal)extra.ItemDiscount, FiscalName));
                        if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                        {
                            printResult.Status = PrintStatusEnum.Failed;
                            printResult.ReceiptNo = receiptModel.ReceiptNo;
                            return printResult;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Receipt void is printed in generic printer
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="fiscalName"></param>
        /// <returns></returns>
        private PrintResultModel PrintVoidReceipt(ReceiptModel receiptModel, string fiscalName)
        {
            PrintResultModel printResult = new PrintResultModel();
            ReceiptModel currentReceiptData = receiptModel;
            string error = string.Empty;
            try
            {
                printResult.OrderNo = receiptModel.OrderId.ToString();
                printResult.ReceiptNo = receiptModel.OrderNo;
                printResult.ExtcerType = ExtcerTypesEnum.Opos;
                printResult.ReceiptType = PrintModeEnum.Void;
                RollerTypeReportModel template;
                KitchenPrinterModel printerSettings;
                // get printer for void
                printerSettings = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Void);
                // get receipt printer template
                template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.Void).Value.FirstOrDefault().Value;
                if (template == null)
                    logger.LogInformation(ExtcerLogger.Log("Generic void template not found", FiscalName));
                else
                    logger.LogInformation(ExtcerLogger.Log("Template: " + (template.ReportName ?? "<null>"), FiscalName));
                printResult.ReceiptData = ProcessVoidReceiptTemplate(template, receiptModel, printerSettings);
                printResult.Status = PrintStatusEnum.Printed;
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("Error printing OPOS void: ", exception, FiscalName));
                printResult.Status = PrintStatusEnum.Failed;
                error = "Error printing OPOS void: " + exception.Message + " StackTRace: " + exception.StackTrace;
            }
            ArtcasLogger.LogArtCasXML(receiptModel, DateTime.Now, error, new List<string>(), ReceiptReceiveTypeEnum.WEB, printResult.Status, fiscalName, printResult.Id);
            return printResult;
        }

        /// <summary>
        /// Get receipt text for preview
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <returns></returns>
        private List<string> GetReceiptPreviewText(ReceiptModel receiptModel)
        {
            List<string> res = new List<string>();
            try
            {
                printResult.ReceiptData = new List<string>();
                var recStr = GetReceiptText(receiptModel, PrinterTypeEnum.OposPreview);
                if (recStr.Count > 0)
                {
                    res.AddRange(recStr);
                }
                var recStrComments = GetReceiptText(receiptModel, PrinterTypeEnum.Receipt);
                if (recStrComments.Count > 0)
                {
                    res.AddRange(recStrComments);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("Error generating OPOS  receipt text: " + exception.ToString(), FiscalName));
            }
            return res;
        }

        /// <summary>
        /// Get receipt text
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="printType"></param>
        /// <returns></returns>
        private List<string> GetReceiptText(ReceiptModel receiptModel, PrinterTypeEnum printType)
        {
            List<string> commentsStr = new List<string>();
            ReceiptModel currentReceiptData = receiptModel;
            currentReceiptData.FiscalType = FiscalTypeEnum.Opos;
            string result = string.Empty;
            try
            {
                logger.LogInformation(ExtcerLogger.Log("PrintType: " + printType.ToString(), FiscalName));
                // get printer for receipt
                logger.LogInformation(ExtcerLogger.Log("Selecting printer...", FiscalName));
                var printerSettings = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Receipt && f.SlotIndex == "1");
                if (printerSettings == null)
                    logger.LogError(ExtcerLogger.Log("Receipt printer NOT FOUND into printers list. Check SlotIndex value (must be a printer with SlotIndex=1 and PrinterType = Receipt)", FiscalName));
                else
                    logger.LogInformation(ExtcerLogger.Log("Printer: " + (printerSettings.Name ?? "<null>"), FiscalName));
                // get receipt printer template
                logger.LogInformation(ExtcerLogger.Log("Selecting template...", FiscalName));
                var template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == printType).Value.FirstOrDefault().Value;
                if (template == null)
                    logger.LogError(ExtcerLogger.Log("No PrinterTemplate found.", FiscalName));
                else
                    logger.LogInformation(ExtcerLogger.Log("Template: " + (template.ReportName ?? "<null>"), FiscalName));
                if (printType == PrinterTypeEnum.OposPreview)
                {
                    commentsStr = ProcessReceiptTemplate(template, currentReceiptData, printerSettings, false);
                }
                // create the string to print
                result = commentsStr.Aggregate(new StringBuilder(""), (current, next) => current.Append("\r\n").Append(next)).ToString();
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("Error creating OPOS preview: ", exception, FiscalName));
            }
            return commentsStr;
        }

        /// <summary>
        /// Get the comment lines to be used with the OPOS receipt printout.
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="printType"></param>
        /// <returns></returns>
        private List<string> GetReceiptCommentsText(ReceiptModel receiptModel, PrinterTypeEnum printType)
        {
            List<string> commentsStr = new List<string>();
            ReceiptModel currentReceiptData = receiptModel;
            currentReceiptData.FiscalType = FiscalTypeEnum.Opos;
            try
            {
                // get printer for receipt
                var printerSettings = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Receipt && f.SlotIndex == "1");
                if (printerSettings == null)
                {
                    logger.LogError(ExtcerLogger.Log("Receipt printer NOT FOUND into printers list. Check SlotIndex value (must be a printer with SlotIndex=1 for printing receipt) ", FiscalName));
                    throw new CustomException(ExtcerErrorHelper.INVOICE_NOT_FOUND);
                }
                // get receipt printer template
                var template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Value.ReportName == printerSettings.TemplateShortName).Value.FirstOrDefault().Value;
                commentsStr = ProcessReceiptCommentTemplate(template, currentReceiptData, printerSettings);
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("Error creating OPOS comment lines: ", exception, FiscalName));
            }
            return commentsStr;
        }

        /// <summary>
        /// Get Z text
        /// </summary>
        /// <param name="zData"></param>
        /// <returns></returns>
        private List<string> GetZText(ZReportModel zData)
        {
            List<string> zStr = new List<string>();
            try
            {
                logger.LogInformation(ExtcerLogger.Log("Getting Z printer", FiscalName));
                // find the printer that is set for report printing
                var zPrinter = availablePrinters.Where(f => f.PrinterType == PrinterTypeEnum.ZReport).FirstOrDefault();
                if (zPrinter == null)
                {
                    logger.LogWarning(ExtcerLogger.Log("ZReport printer not found for PrinterType = ZReport ", FiscalName));
                }
                // get receipt printer template
                var template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.ZReport).Value.FirstOrDefault().Value;
                if (template == null)
                {
                    logger.LogWarning(ExtcerLogger.Log("Generic Z template not found", FiscalName));
                }
                // get text to print
                logger.LogInformation(ExtcerLogger.Log("Getting text to print...", FiscalName));
                zStr = ProcessZReportTemplate(template, zData, zPrinter);
                return zStr;
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("OPOS: Error in GetZText Error: ", exception, FiscalName));
                return zStr;
            }
        }

        /// <summary>
        /// Process receipt template.
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentReceipt"></param>
        /// <param name="printerSettings"></param>
        /// <param name="allowPrint"></param>
        /// <returns></returns>
        private List<string> ProcessReceiptTemplate(RollerTypeReportModel repTemplate, ReceiptModel currentReceipt, KitchenPrinterModel printerSettings, bool allowPrint = false)
        {
            List<string> result = new List<string>();
            KitchenPrinterModel receiptPrinter = new KitchenPrinterModel();
            receiptPrinter = printerSettings;
            if (receiptPrinter.Regions.Count > 0)
            {
                if (receiptPrinter.Regions[0] > 0)
                {
                    long regionId = currentReceipt.RegionId.GetValueOrDefault();
                    if (regionId > 0)
                    {
                        receiptPrinter = availablePrinters.Where(f => f.Name == printerSettings.Name && f.Regions.Contains(regionId)).FirstOrDefault();
                    }
                }
            }
            Printer printer = PrintersEscList.Where(f => f.Name == receiptPrinter.EscapeCharsTemplate).FirstOrDefault();
            var sections = repTemplate.Sections;
            foreach (var section in sections.Where(f => f.SectionType != (int)SectionTypeEnums.Extras))
            {
                if (section != null)
                {
                    if (section.SectionType == (int)SectionTypeEnums.Details)
                    {
                        foreach (var itemData in currentReceipt.Details)
                        {
                            if (itemData.IsChangeItem)
                            {
                                if (itemData.ItemPrice != null)
                                {
                                    var price_original = itemData.ItemPrice;
                                    itemData.ItemPrice = Math.Abs(itemData.ItemPrice);
                                    result.AddRange(ProcessSection(section.SectionRows, itemData, printer));
                                    // restore the original amount as positive
                                    itemData.ItemPrice = price_original;
                                }
                            }
                            else
                            {
                                result.AddRange(ProcessSection(section.SectionRows, itemData, printer));
                            }
                            if (itemData.ItemDiscount != null && itemData.ItemDiscount > 0)
                            {
                                var discountDetailsSection = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.DiscountDetails);
                                if (discountDetailsSection != null)
                                {
                                    itemData.ItemDiscount = itemData.ItemDiscount * (-1);
                                    result.AddRange(ProcessSection(discountDetailsSection.SectionRows, itemData, printer));
                                    // restore the original amount as positive
                                    itemData.ItemDiscount = itemData.ItemDiscount * (-1);
                                }
                            }
                            if (itemData.Extras.Count > 0)
                            {
                                var extrasDoc = repTemplate.Sections.Where(w => w.SectionType == (int)SectionTypeEnums.Extras).FirstOrDefault();
                                if (extrasDoc != null)
                                {
                                    foreach (var extra in itemData.Extras)
                                    {
                                        extra.ItemPrice = extra.ItemPrice ?? 0;
                                        if (extra.IsChangeItem)
                                        {
                                            if (extra.ItemPrice != null)
                                            {
                                                extra.ItemPrice = extra.ItemPrice * -1;
                                                result.AddRange(ProcessSection(extrasDoc.SectionRows, extra, printer));
                                                // restore the original amount as positive
                                                extra.ItemPrice = extra.ItemPrice * -1;
                                            }
                                        }
                                        else
                                        {
                                            result.AddRange(ProcessSection(extrasDoc.SectionRows, extra, printer));
                                        }
                                        if (extra.ItemDiscount != null && extra.ItemDiscount > 0)
                                        {
                                            var discountDetailsSection = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.DiscountDetails);
                                            if (discountDetailsSection != null)
                                            {
                                                extra.ItemDiscount = extra.ItemDiscount * (-1);
                                                result.AddRange(ProcessSection(discountDetailsSection.SectionRows, extra, printer));
                                                // restore the original amount as positive
                                                extra.ItemDiscount = extra.ItemDiscount * (-1);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (currentReceipt.TotalDiscount != null && currentReceipt.TotalDiscount > 0)
                        {
                            var totalDiscountSection = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.Discount);
                            if (totalDiscountSection != null)
                            {
                                currentReceipt.TotalDiscount = currentReceipt.TotalDiscount * (-1);
                                result.AddRange(ProcessSection(totalDiscountSection.SectionRows, currentReceipt, printer));
                                // restore the original amount as positive
                                currentReceipt.TotalDiscount = currentReceipt.TotalDiscount * (-1);
                            }
                        }
                    }
                    else if (section.SectionType == (int)SectionTypeEnums.Customer)
                    {
                        if (!string.IsNullOrEmpty(currentReceipt.RoomNo))
                        {
                            result.AddRange(ProcessSection(section.SectionRows, currentReceipt, printer));
                        }
                    }
                    else if (section.SectionType == (int)SectionTypeEnums.VatAnalysis)
                    {
                        var detvat = currentReceipt.Details.Select(f => new Vat
                        {
                            VatRate = f.ItemVatRate,
                            Gross = (f.ItemNet + f.ItemVatValue),
                            Net = f.ItemNet,
                            VatAmount = f.ItemVatValue,
                            VatDesc = f.ItemVatDesc
                        }).AsEnumerable();
                        var extvat = currentReceipt.Details.SelectMany(f => f.Extras).Select(w => new Vat
                        {
                            VatRate = w.ItemVatRate,
                            VatAmount = w.ItemVatValue,
                            Gross = (w.ItemNet + w.ItemVatValue),
                            Net = w.ItemNet,
                            VatDesc = w.ItemVatDesc
                        }).AsEnumerable();
                        var union = detvat.Union(extvat);
                        var grbyvatrate = union.GroupBy(f => f.VatRate).Select(f => new Vat
                        {
                            VatRate = f.Key.Value,
                            Gross = f.Sum(s => s.Gross),
                            Net = f.Sum(s => s.Net),
                            VatAmount = f.Sum(s => s.VatAmount),
                            VatDesc = f.FirstOrDefault().VatDesc
                        });
                        foreach (var item in grbyvatrate)
                        {
                            result.AddRange(ProcessSection(section.SectionRows, item, printer));
                        }
                    }
                    else if (section.SectionType != (int)SectionTypeEnums.Discount && section.SectionType != (int)SectionTypeEnums.DiscountDetails)
                    {
                        result.AddRange(ProcessSection(section.SectionRows, currentReceipt, printer));
                    }
                }
            }
            if ((receiptPrinter.PrintKitchenOnly == null || (bool)receiptPrinter.PrintKitchenOnly == false) && allowPrint)
            {
                Task task = Task.Run(() => SendTextToPrinter(result, printer, printerSettings));
            }
            else
            {
                if (!allowPrint)
                {
                    result.Insert(0, "--PREVIEW OF RECEIPT--");
                }
                else
                {
                    result.Insert(0, "--SETTED NOT TO PRINT--");
                }
            }
            return result;
        }

        /// <summary>
        /// Process void receipt template.
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentReceipt"></param>
        /// <param name="printerSettings"></param>
        /// <returns></returns>
        private List<string> ProcessVoidReceiptTemplate(RollerTypeReportModel repTemplate, ReceiptModel currentReceipt, KitchenPrinterModel printerSettings)
        {
            List<string> result = new List<string>();
            var receiptPrinter = availablePrinters.Where(f => f.Name == printerSettings.Name && f.PrinterType == PrinterTypeEnum.Void).FirstOrDefault();
            Printer printer = PrintersEscList.Where(f => f.Name == receiptPrinter.EscapeCharsTemplate).FirstOrDefault();
            var sections = repTemplate.Sections;
            foreach (var section in sections.Where(f => f.SectionType != (int)SectionTypeEnums.Extras))
            {
                if (section != null)
                {
                    if (section.SectionType == (int)SectionTypeEnums.Details)
                    {
                        foreach (var items in currentReceipt.Details)
                        {
                            result.AddRange(ProcessSection(section.SectionRows, items, printer));
                            if (items.Extras.Count > 0)
                            {
                                var extrasDoc = repTemplate.Sections.Where(w => w.SectionType == (int)SectionTypeEnums.Extras).FirstOrDefault();
                                if (extrasDoc != null)
                                {
                                    foreach (var extra in items.Extras)
                                    {
                                        extra.ItemPrice = extra.ItemPrice ?? 0;
                                        if (extra.IsChangeItem)
                                        {
                                            if (extra.ItemPrice != null)
                                            {
                                                extra.ItemPrice = extra.ItemPrice * -1;
                                            }
                                        }
                                        result.AddRange(ProcessSection(extrasDoc.SectionRows, extra, printer));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        result.AddRange(ProcessSection(section.SectionRows, currentReceipt, printer));
                    }
                }
            }
            Task task = Task.Run(() => SendTextToPrinter(result, printer, printerSettings));
            return result;
        }

        /// <summary>
        /// Process receipt comments template.
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentReceipt"></param>
        /// <param name="printerSettings"></param>
        /// <returns></returns>
        private List<string> ProcessReceiptCommentTemplate(RollerTypeReportModel repTemplate, ReceiptModel currentReceipt, KitchenPrinterModel printerSettings)
        {
            List<string> result = new List<string>();
            var receiptPrinter = availablePrinters.Where(f => f.Name == printerSettings.Name && f.PrinterType == PrinterTypeEnum.Receipt).FirstOrDefault();
            Printer printer = PrintersEscList.Where(f => f.Name == receiptPrinter.EscapeCharsTemplate).FirstOrDefault();
            var sections = repTemplate.Sections;
            foreach (var section in sections.Where(f => f.SectionType == (int)SectionTypeEnums.OposComments))
            {
                if (section != null)
                {
                    result.AddRange(ProcessSection(section.SectionRows, currentReceipt, printer, false, true));
                }
            }
            return result;
        }

        /// <summary>
        /// Process receipt summary template.
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentReceipt"></param>
        /// <param name="printersettings"></param>
        /// <returns></returns>
        private List<string> ProcessReceiptSumTemplate(RollerTypeReportModel repTemplate, ReceiptModel currentReceipt, KitchenPrinterModel printersettings)
        {
            List<string> result = new List<string>();
            var kitchenPrinter = availablePrinters.Where(f => f.Name == printersettings.Name && f.PrinterType == PrinterTypeEnum.Report).FirstOrDefault();
            Printer printer = PrintersEscList.Where(f => f.Name == kitchenPrinter.EscapeCharsTemplate).FirstOrDefault();
            var sections = repTemplate.Sections;
            var availsections = sections.Where(f => f.SectionType != (int)SectionTypeEnums.Extras && f.SectionType != (int)SectionTypeEnums.ReceiptSumHeader && f.SectionType != (int)SectionTypeEnums.ReceiptSumFooter);
            foreach (var section in availsections)
            {
                if (section != null)
                {
                    if (section.SectionType == (int)SectionTypeEnums.Details)
                    {
                        var invGroups = from d in currentReceipt.Details
                                        group d by d.InvoiceNo into dd
                                        select new
                                        {
                                            RegionName = dd.Key,
                                            InvPosition = dd.Select(f => f.InvoiceNo).FirstOrDefault(),
                                            RegionItems = dd.Select(f => f)
                                        };
                        string CurInvoiceNo = "";
                        foreach (var region in invGroups.OrderBy(f => f.InvPosition))
                        {
                            var regionitemsCount = (region.RegionItems != null) ? region.RegionItems.Count() : 0;
                            var regionCounter = 0;
                            foreach (var items in region.RegionItems)
                            {
                                if (CurInvoiceNo != items.InvoiceNo)
                                {
                                    // new region item
                                    regionCounter = 1;
                                    // set current region name to items region
                                    CurInvoiceNo = items.InvoiceNo;
                                    List<ReportSectionsRowsModel> headerrow = new List<ReportSectionsRowsModel>();
                                    var header = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.ReceiptSumHeader);
                                    if (header != null)
                                    {
                                        headerrow = header.SectionRows;
                                        // add the receipt sum header
                                        result.AddRange(ProcessSection(headerrow, items, printer, false));
                                    }
                                    // add the item
                                    result.AddRange(ProcessSection(section.SectionRows, items, printer, false));
                                }
                                else
                                {
                                    // item is in current region
                                    regionCounter++;
                                    // add line but ignore region info
                                    result.AddRange(ProcessSection(section.SectionRows, items, printer, false));
                                }
                                if (items.ItemDiscount != null && items.ItemDiscount > 0)
                                {
                                    var discountDetailsSection = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.DiscountDetails);
                                    if (discountDetailsSection != null)
                                    {
                                        logger.LogInformation(ExtcerLogger.Log("ITEM DISCOUNT: " + items.ItemDiscount, FiscalName));
                                        items.ItemDiscount = items.ItemDiscount * (-1);
                                        result.AddRange(ProcessSection(discountDetailsSection.SectionRows, items, printer));
                                    }
                                }
                                if (items.Extras.Count > 0)
                                {
                                    var extrasDoc = repTemplate.Sections.Where(w => w.SectionType == (int)SectionTypeEnums.Extras).FirstOrDefault();
                                    if (extrasDoc != null)
                                    {
                                        foreach (var extra in items.Extras)
                                        {
                                            extra.ItemPrice = extra.ItemPrice ?? 0;
                                            if (extra.IsChangeItem)
                                            {
                                                if (extra.ItemPrice != null)
                                                {
                                                    extra.ItemPrice = extra.ItemPrice * -1;
                                                }
                                            }
                                            result.AddRange(ProcessSection(extrasDoc.SectionRows, extra, printer));
                                            if (extra.ItemDiscount != null && extra.ItemDiscount > 0)
                                            {
                                                var discountDetailsSection = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.DiscountDetails);
                                                if (discountDetailsSection != null)
                                                {
                                                    logger.LogInformation(ExtcerLogger.Log("EXTRA DISCOUNT: " + extra.ItemDiscount, FiscalName));
                                                    extra.ItemDiscount = extra.ItemDiscount * (-1);
                                                    result.AddRange(ProcessSection(discountDetailsSection.SectionRows, extra, printer));
                                                }
                                            }
                                        }
                                    }
                                }
                                // is end of current invoice group
                                if (regionCounter == regionitemsCount)
                                {
                                    List<ReportSectionsRowsModel> footerRow = new List<ReportSectionsRowsModel>();
                                    var footer = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.ReceiptSumFooter);
                                    if (footer != null)
                                    {
                                        footerRow = footer.SectionRows;
                                        // add the receipt sum footer
                                        result.AddRange(ProcessSection(footerRow, items, printer, false));
                                    }
                                }
                            }
                        }
                        if (currentReceipt.TotalDiscount != null && currentReceipt.TotalDiscount > 0)
                        {
                            var totalDiscountSection = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.Discount);
                            if (totalDiscountSection != null)
                            {
                                currentReceipt.TotalDiscount = currentReceipt.TotalDiscount * (-1);
                                result.AddRange(ProcessSection(totalDiscountSection.SectionRows, currentReceipt, printer));
                            }
                        }
                    }
                    else if (section.SectionType == (int)SectionTypeEnums.Customer)
                    {
                        if (!string.IsNullOrEmpty(currentReceipt.RoomNo))
                        {
                            result.AddRange(ProcessSection(section.SectionRows, currentReceipt, printer));
                        }
                    }
                    else if (section.SectionType == (int)SectionTypeEnums.PaymentMethods)
                    {
                        foreach (var item in currentReceipt.PaymentsList)
                        {
                            result.AddRange(ProcessSection(section.SectionRows, item, printer));
                        }
                    }
                    else if (section.SectionType != (int)SectionTypeEnums.Discount && section.SectionType != (int)SectionTypeEnums.DiscountDetails)
                    {
                        result.AddRange(ProcessSection(section.SectionRows, currentReceipt, printer));
                    }
                }
            }
            logger.LogInformation("in  ProcessKitchenTemplate Sending kitchen to printer delegate");
            Task task = Task.Run(() => SendTextToPrinter(result, printer, printersettings));
            return result;
        }

        /// <summary>
        /// Process Z template.
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentZData"></param>
        /// <param name="printerSettings"></param>
        /// <returns></returns>
        private List<string> ProcessZReportTemplate(RollerTypeReportModel repTemplate, ZReportModel currentZData, KitchenPrinterModel printerSettings)
        {
            List<string> result = new List<string>();
            var zPrinter = availablePrinters.Where(f => f.Name == printerSettings.Name && f.PrinterType == PrinterTypeEnum.ZReport).FirstOrDefault();
            if (zPrinter == null)
            {
                logger.LogError(ExtcerLogger.Log("ZReport printer NOT FOUND into printers list. Check Printers Settins, SlotIndex values etc.", FiscalName));
            }
            Printer printer = PrintersEscList.Where(f => f.Name == printerSettings.EscapeCharsTemplate).FirstOrDefault();
            var sections = repTemplate.Sections;
            foreach (var section in sections)
            {
                if (section != null)
                {
                    if (section.SectionType == (int)SectionTypeEnums.PaymentAnalysis)
                    {
                        if (currentZData.PaymentAnalysis != null && currentZData.PaymentAnalysis.Count > 0)
                        {
                            foreach (var item in currentZData.PaymentAnalysis)
                            {
                                result.AddRange(ProcessSection(section.SectionRows, item, printer));
                            }
                        }
                    }
                    else if (section.SectionType == (int)SectionTypeEnums.VatAnalysis)
                    {
                        if (currentZData.VatAnalysis != null && currentZData.VatAnalysis.Count > 0)
                        {
                            foreach (var item in currentZData.VatAnalysis)
                            {
                                result.AddRange(ProcessSection(section.SectionRows, item, printer));
                            }
                        }
                    }
                    else if (section.SectionType == (int)SectionTypeEnums.VoidAnalysis)
                    {
                        if (currentZData.VoidAnalysis != null && currentZData.VoidAnalysis.Count > 0)
                        {
                            foreach (var item in currentZData.VoidAnalysis)
                            {
                                result.AddRange(ProcessSection(section.SectionRows, item, printer));
                            }
                        }
                    }
                    else if (section.SectionType == (int)SectionTypeEnums.CardAnalysis)
                    {
                        if (currentZData.CardAnalysis != null && currentZData.CardAnalysis.Count > 0)
                        {
                            foreach (var item in currentZData.CardAnalysis)
                            {
                                result.AddRange(ProcessSection(section.SectionRows, item, printer));
                            }
                        }
                    }
                    else
                    {
                        result.AddRange(ProcessSection(section.SectionRows, currentZData, printer));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Process the given section and create the string to print.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="obj"></param>
        /// <param name="printer"></param>
        /// <param name="ignoreRegionInfo"></param>
        /// <param name="isOposComments"></param>
        /// <returns></returns>
        private List<string> ProcessSection(List<ReportSectionsRowsModel> section, object obj, Printer printer, bool ignoreRegionInfo = false, bool isOposComments = false)
        {
            int? intMaxWidth = null;
            List<string> result = new List<string>();
            // loop through lines of the section
            foreach (var line in section)
            {
                if (!ignoreRegionInfo || (ignoreRegionInfo && line.SectionColumns.Count(f => f.ColumnText == "@ItemRegion") == 0))
                {
                    string str = string.Empty;
                    bool add = true;
                    // loop  through columns of the current line
                    foreach (var col in line.SectionColumns)
                    {
                        var tempStr = string.Empty;
                        string colText = col.ColumnText ?? string.Empty;
                        int colWidth = Convert.ToInt32(col.Width);
                        intMaxWidth = colWidth;
                        string data = GenericCommon.ReplacePatterns(obj, colText);
                        switch (colText)
                        {
                            case "@ItemTotal":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null && model.CreditTransactions.Count() > 0)
                                    {
                                        data = (model.CreditTransactions.FirstOrDefault().Amount ?? 0).ToString("0.00");
                                    }
                                }
                                break;
                            case "@InvoiceCustomerName":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        data = model.CustomerName;
                                    }
                                }
                                break;
                            case "@CustomerName":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        var guestdata = model.PaymentsList.Where(f => f.Guest != null && f.Guest.LastName != null && f.Guest.LastName.Length > 0).Select(ff => ff.Guest.LastName);
                                        var res = string.Empty;
                                        if (guestdata != null && guestdata.Count() > 0)
                                        {
                                            res = guestdata.Aggregate((current, next) => current + ", " + next);
                                        }
                                        data = res;
                                    }
                                }
                                break;
                            case "@RoomNo":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        var guestdata = model.PaymentsList.Where(f => f.Guest != null && f.Guest.Room != null && f.Guest.Room.Length > 0).Select(ff => ff.Guest.Room);
                                        var res = "";
                                        if (guestdata != null && guestdata.Count() > 0)
                                        {
                                            res = guestdata.Aggregate((current, next) => current + ", " + next);
                                        }
                                        data = res;
                                    }
                                }
                                break;
                            case "@SystemDate":
                                {
                                    var date = DateTime.Now;
                                    if (obj.GetType() == typeof(ZReportModel))
                                    {
                                        var d = obj as ZReportModel;
                                        if (d != null)
                                        {
                                            date = d.Day;
                                        }
                                    }
                                    if (!String.IsNullOrEmpty(col.FormatOption))
                                    {
                                        data = String.Format(col.FormatOption, date).ToUpper();
                                    }
                                    else
                                    {
                                        // set default format
                                        data = String.Format("{0:dd/MM/yyyy}", date).ToUpper();
                                    }
                                }
                                break;
                            case "@SystemTime":
                                {
                                    var date = DateTime.Now;
                                    if (obj.GetType() == typeof(ZReportModel))
                                    {
                                        var d = obj as ZReportModel;
                                        if (d != null)
                                        {
                                            date = d.Day;
                                        }
                                    }
                                    if (!String.IsNullOrEmpty(col.FormatOption))
                                    {
                                        data = String.Format(col.FormatOption, date).ToUpper();
                                    }
                                    else
                                    {
                                        // set default format
                                        data = String.Format("{0:HH:mm}", date).ToUpper();
                                    }
                                }
                                break;
                            case "@Day":
                                {
                                    if (!String.IsNullOrEmpty(col.FormatOption))
                                    {
                                        data = String.Format(col.FormatOption, data).ToUpper();
                                    }
                                    else
                                    {
                                        // set default format
                                        data = String.Format("{0:dd/MM/yyyy}", data).ToUpper();
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        if (colWidth <= data.Length)
                        {
                            tempStr += data.Substring(0, colWidth);
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(col.AlignOption))
                            {
                                tempStr += data.PadRight(colWidth, ' ');
                            }
                            else
                            {
                                TextAlignEnum alignment = (TextAlignEnum)Enum.Parse(typeof(TextAlignEnum), col.AlignOption, true);
                                switch (alignment)
                                {
                                    case TextAlignEnum.Middle:
                                        tempStr += GenericCommon.CenteredString(data, colWidth);
                                        break;
                                    case TextAlignEnum.Right:
                                        tempStr += data.PadLeft(colWidth, ' ');
                                        break;
                                    case TextAlignEnum.Left:
                                    default:
                                        tempStr += data.PadRight(colWidth, ' ');
                                        break;
                                }
                            }
                        }
                        if (colText == "@ItemCustomRemark")
                        {
                            if (string.IsNullOrEmpty(data))
                            {
                                add = false;
                            }
                            else
                            {
                                add = true;
                                tempStr = " " + data.TrimEnd();
                                tempStr = GenericCommon.SetStringEscChars(printer, tempStr, col.IsBold, col.IsItalic, col.IsUnderline, col.IsDoubleSize);
                            }
                        }
                        else
                        {
                            tempStr = GenericCommon.SetStringEscChars(printer, tempStr, col.IsBold, col.IsItalic, col.IsUnderline, col.IsDoubleSize);
                        }
                        if (line.SectionColumns.Select(f => f.ColumnText).Contains("@ItemRegion") && tempStr.Length > 0)
                        {
                            tempStr += "\n----------------------------------------";
                        }
                        str += tempStr;
                    }
                    if (add)
                    {
                        result.Add(str);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Send given text to the raw printer.
        /// </summary>
        /// <param name="stringToPrint"></param>
        /// <param name="printer"></param>
        /// <param name="printerSettings"></param>
        private void SendTextToPrinter(List<string> stringToPrint, Printer printer, KitchenPrinterModel printerSettings)
        {
            // print to kitchen multiple times
            int printTimes = 1;
            if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen || printerSettings.PrinterType == PrinterTypeEnum.Receipt)
            {
                if (printerSettings.KitchenPrintTimes == null || printerSettings.KitchenPrintTimes == 0)
                {
                    printerSettings.KitchenPrintTimes = 1;
                }
                printTimes = (int)printerSettings.KitchenPrintTimes;
            }
            int intKitchenHeaderGapLines = 7;
            if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen)
            {
                if (printerSettings.KitchenHeaderGapLines == null)
                {
                    printerSettings.KitchenHeaderGapLines = 7;
                }
                intKitchenHeaderGapLines = (int)printerSettings.KitchenHeaderGapLines;
            }
            // get cutter escape chars
            var cutterEscChars = GenericCommon.GetEscapeChars(printer.Cutter);
            // get the init chars
            var initChar = GenericCommon.GetEscapeChars(printer.InitChar);
            // create the string to print
            string str = stringToPrint.Aggregate(new StringBuilder(""), (current, next) => current.Append("\r\n").Append(next)).ToString();
            string buzzerEscChars = GenericCommon.GetEscapeChars(printer.Buzzer);
            switch (printerSettings.PrinterCharsFormat)
            {
                case PrintCharsFormatEnum.OEM:
                    for (int i = 1; i <= printTimes; i++)
                    {
                        logger.LogInformation(ExtcerLogger.Log("Printing OEM ", printerSettings.FiscalName));
                        Encoding utf8 = new UTF8Encoding();
                        Encoding oem737 = Encoding.GetEncoding(737);
                        str = str + new string('\n', intKitchenHeaderGapLines) + '\r';
                        initChar = string.Empty;
                        if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen)
                        {
                            if (printerSettings.UseBuzzer != null && printerSettings.UseBuzzer == true)
                            {
                                str = buzzerEscChars + str;
                            }
                        }
                        if (printerSettings.UseCutter == null || printerSettings.UseCutter == true)
                        {
                            str = str + cutterEscChars;
                        }
                        SendBytesToPrinter(str, printerSettings.Name, 737);
                    }
                    break;
                case PrintCharsFormatEnum.ANSI:
                    {
                        logger.LogInformation(ExtcerLogger.Log("Printing ANSI ", printerSettings.FiscalName));
                        var toPrint = initChar + str + new string('\n', intKitchenHeaderGapLines) + '\r';
                        for (int i = 1; i <= printTimes; i++)
                        {
                            if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen && printerSettings.UseBuzzer == true)
                            {
                                RawPrinterHelper.SendStringToPrinter(printerSettings.Name, buzzerEscChars);
                            }
                            RawPrinterHelper.SendStringToPrinter(printerSettings.Name, toPrint);
                            if (printerSettings.UseCutter == null || printerSettings.UseCutter == true)
                            {
                                RawPrinterHelper.SendStringToPrinter(printerSettings.Name, cutterEscChars);
                            }
                        }
                    }
                    break;
                case PrintCharsFormatEnum.GRAPHIC:
                    for (int i = 1; i <= printTimes; i++)
                    {
                        if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen && printerSettings.UseBuzzer == true)
                        {
                            RawPrinterHelper.SendStringToPrinter(printerSettings.Name, buzzerEscChars);
                        }
                        var toPrint = initChar + str + new string('\n', intKitchenHeaderGapLines) + '\r';
                        PrintGraphic(printerSettings.Name, toPrint, (7 == intKitchenHeaderGapLines));
                        if (printerSettings.UseCutter == null || printerSettings.UseCutter == true)
                        {
                            RawPrinterHelper.SendStringToPrinter(printerSettings.Name, cutterEscChars);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Send given bytes to the raw printer.
        /// </summary>
        /// <param name="toSend"></param>
        /// <param name="printerName"></param>
        /// <param name="codePage"></param>
        private void SendBytesToPrinter(string toSend, string printerName, int codePage)
        {
            Encoding utf8 = new UTF8Encoding();
            Encoding destcodepage = Encoding.GetEncoding(codePage);
            byte[] input_utf8 = utf8.GetBytes(toSend);
            byte[] output_dest = Encoding.Convert(utf8, destcodepage, input_utf8);
            int nLength = Convert.ToInt32(output_dest.Length);
            // Allocate some unmanaged memory for those bytes.
            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.
            Marshal.Copy(output_dest, 0, pUnmanagedBytes, nLength);
            if (!RawPrinterHelper.SendBytesToPrinter(printerName, pUnmanagedBytes, nLength))
            {
                logger.LogInformation("SendBytesToPrinter: " + printerName + " -> Failed!");
            }
        }

        /// <summary>
        /// The PrintPage event is raised for each page to be printed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = 2;
            float topMargin = ev.MarginBounds.Top;
            String line = null;
            // Calculate the number of lines per page.
            linesPerPage = ev.MarginBounds.Height / printFont.GetHeight(ev.Graphics);
            // Iterate over the file, printing each line. 
            while (count < linesPerPage && ((line = streamToPrint.ReadLine()) != null))
            {
                yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                ev.Graphics.DrawString(line, printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                count++;
            }
            // If more lines exist, print another page. 
            if (line != null)
            {
                ev.HasMorePages = true;
            }
            else
            {
                ev.HasMorePages = false;
            }
        }

        /// <summary>
        /// The PrintPage event is raised for each page to be printed.
        /// Special version for LABEL printer Brother QL-710W
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        private void pd_PrintPage_Custom(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.PageBounds.Left;
            float topMargin = ev.PageBounds.Top;
            String line = null;
            // Calculate the number of lines per page.
            linesPerPage = ev.PageBounds.Height / printFont.GetHeight(ev.Graphics);
            // Iterate over the file, printing each line. 
            while (count < linesPerPage && ((line = streamToPrint.ReadLine()) != null))
            {
                yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                ev.Graphics.DrawString(line, printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                count++;
            }
            // If more lines exist, print another page. 
            if (line != null)
            {
                ev.HasMorePages = true;
            }
            else
            {
                ev.HasMorePages = false;
            }
        }

        #endregion
    }
}