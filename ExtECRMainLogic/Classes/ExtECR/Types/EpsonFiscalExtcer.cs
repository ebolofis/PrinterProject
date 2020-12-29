using ExtECRMainLogic.Classes.Extenders;
using ExtECRMainLogic.Classes.Helpers;
using ExtECRMainLogic.Classes.Loggers;
using ExtECRMainLogic.Enumerators.Epson;
using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Exceptions;
using ExtECRMainLogic.Hubs;
using ExtECRMainLogic.Models.ConfigurationModels;
using ExtECRMainLogic.Models.ExtECRModels;
using ExtECRMainLogic.Models.PrinterModels;
using ExtECRMainLogic.Models.ReceiptModels;
using ExtECRMainLogic.Models.TemplateModels;
using ExtECRMainLogic.Models.ZReportModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExtECRMainLogic.Classes.ExtECR.Types
{
    /// <summary>
    /// Epson Instance
    /// </summary>
    public class EpsonFiscalExtcer : FiscalManager
    {
        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public const int FD_SUCCESS = 0;
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
        /// The number of COM port to communicate Fiscal device.
        /// Values: 1 = COM1, . . .
        /// </summary>
        private int intCOMportToFiscalDevice;
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
        private decimal[] arrayOfVats;
        /// <summary>
        /// 
        /// </summary>
        private int ErrorCode;
        /// <summary>
        /// 
        /// </summary>
        private PrintResultModel printResult;
        /// <summary>
        /// 
        /// </summary>
        private TTotalRec subtotalRec;
        /// <summary>
        /// 
        /// </summary>
        private TTotalRec totalRec;
        /// <summary>
        /// 
        /// </summary>
        private StreamReader streamToPrint;
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
        private readonly ILogger<EpsonFiscalExtcer> logger;
        /// <summary>
        /// Local hub invoker
        /// </summary>
        private readonly LocalHubInvoker localHubInvoker;
        #region  DLL references
        [DllImport("epson210.dll", EntryPoint = "FDOPENDEVICE", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int FDOpenDevice(int deviceID, int portnumber);
        [DllImport("epson210.dll", EntryPoint = "FDGETLASTERROR", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe int FDGetLastError(int* errorID, sbyte* errorMessage);
        [DllImport("epson210.dll", EntryPoint = "FDCANCELTRANSACTION", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int FDCancelTransaction();
        [DllImport("epson210.dll", EntryPoint = "FDGETSTATUS", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe int FDGetStatus(int* statusID, sbyte* statusDescr);
        [DllImport("epson210.dll", EntryPoint = "FDINIT", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int FDInit();
        [DllImport("epson210.dll", EntryPoint = "FDCLOSEDEVICE", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int FDCloseDevice(int deviceID);
        [DllImport("epson210.dll", EntryPoint = "FDSENDANDGETDATA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe int FDSendAndGetData(sbyte* sData, sbyte* rData, sbyte* sInt, sbyte* rInt, uint handle);
        [DllImport("epson210.dll", EntryPoint = "FDGETRECEIPTNUMBER", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe int FDGetReceiptNumber(int* legalNo, int* illegalNo);
        [DllImport("epson210.dll", EntryPoint = "FDADDTRANSACTIONLINE", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe int FDAddTransactionLine(int* appLineID, sbyte* transactionLine);
        [DllImport("epson210.dll", EntryPoint = "FDDOTRANSACTION", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe int FDDoTransaction(int* appLineID, int* errorLine);
        [DllImport("epson210.dll", EntryPoint = "FDDRAWER", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern void FDDrawer();
        [DllImport("epson210.dll", EntryPoint = "FDPRINTREPORT", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int FDPrintReport(int reportId);
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="printerTemplatesList"></param>
        /// <param name="printerEscList"></param>
        /// <param name="availablePrinters"></param>
        /// <param name="installationData"></param>
        /// <param name="strFiscalName"></param>
        /// <param name="applicationPath"></param>
        /// <param name="configuration"></param>
        /// <param name="applicationBuilder"></param>
        public EpsonFiscalExtcer(Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>> printerTemplatesList, List<Printer> printerEscList, List<KitchenPrinterModel> availablePrinters, InstallationDataModel installationData, string strFiscalName, string applicationPath, IConfiguration configuration, IApplicationBuilder applicationBuilder)
        {
            // instance identification
            this.InstanceID = ExtcerTypesEnum.EpsonFiscal;
            // initialization of variable to be used with lock on printouts
            this.thisLock = new object();
            this.PrintersTemplates = printerTemplatesList;
            this.PrintersEscList = printerEscList;
            this.availablePrinters = availablePrinters;
            this.InstallationData = installationData;
            this.FiscalName = strFiscalName;
            this.applicationPath = applicationPath;
            this.configuration = configuration;
            this.intCOMportToFiscalDevice = installationData.EpsonFiscalComPort ?? 1;
            this.CashAccountIds = new List<int>();
            this.CCAccountIds = new List<int>();
            this.CreditAccountIds = new List<int>();
            this.arrayOfVats = new decimal[5];
            this.ErrorCode = 0;
            this.logger = (ILogger<EpsonFiscalExtcer>)applicationBuilder.ApplicationServices.GetService(typeof(ILogger<EpsonFiscalExtcer>));
            this.localHubInvoker = (LocalHubInvoker)applicationBuilder.ApplicationServices.GetService(typeof(LocalHubInvoker));
            SetupAccountIds();
            LoadDiskFileWithVATs();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~EpsonFiscalExtcer()
        {

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
                // print to Epson
                return EpsonFiscal_PrintReceipt(receiptModel, fiscalName);
            }
            else
            {
                // print to generic
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
            string error = string.Empty;
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
                    logger.LogWarning(ExtcerLogger.Log("Generic template not found", FiscalName));
                }
                else
                {
                    logger.LogInformation(ExtcerLogger.Log("Template: " + template.ReportName ?? "<null>", FiscalName));
                }
                printResult.ReceiptType = PrintModeEnum.InvoiceSum;
                printResult.ReceiptData = ProcessReceiptSumTemplate(template, receiptModel, printerToPrint);
                printResult.Status = PrintStatusEnum.Printed;
            }
            catch (CustomException exception)
            {
                printResult.ErrorDescription = exception.Message;
                logger.LogError(ExtcerLogger.logErr("Error printing generic ReceiptSum: ", exception, FiscalName));
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
            OpenFiscal();
            printResult = new PrintResultModel();
            printResult.ExtcerType = ExtcerTypesEnum.EpsonFiscal;
            printResult.ReceiptType = PrintModeEnum.XReport;
            printResult.Status = PrintStatusEnum.Failed;
            if (ErrorCode == 0)
            {
                if (FD_SUCCESS != (ErrorCode = FDPrintReport(1)))
                {
                    EpsonFiscal_LogError("at EpsonFiscal - X Report Failed. - ", ErrorCode, false);
                    return printResult;
                }
            }
            logger.LogInformation(ExtcerLogger.Log("at EpsonFiscal - X Report OK! - ", FiscalName));
            printResult.Status = PrintStatusEnum.Printed;
            CloseFiscal();
            return printResult;
        }

        /// <summary>
        /// Print Z report interface
        /// </summary>
        /// <param name="zData"></param>
        /// <returns></returns>
        public override PrintResultModel PrintZ(ZReportModel zData)
        {
            OpenFiscal();
            printResult = new PrintResultModel();
            printResult.ExtcerType = ExtcerTypesEnum.EpsonFiscal;
            printResult.ReceiptType = PrintModeEnum.ZReport;
            printResult.Status = PrintStatusEnum.Failed;
            printResult.ReceiptNo = zData.ReportNo.ToString();
            printResult.ReceiptData = GetZText(zData);
            if (ErrorCode == 0)
            {
                if (FD_SUCCESS != (ErrorCode = FDPrintReport(7)))
                {
                    printResult.ErrorDescription = EpsonFiscal_LogError("at PrintZ", ErrorCode, false);
                    EpsonFiscal_LogError("at EpsonFiscal - Z Report Failed. - ", ErrorCode);
                    return printResult;
                }
            }
            logger.LogInformation(ExtcerLogger.Log("at EpsonFiscal - Z Report OK! - ", FiscalName));
            printResult.Status = PrintStatusEnum.Printed;
            // get Z report ticket number
            int intLegalReceiptLastNumber;
            int intIllegalReceiptLastNumber;
            if ((ErrorCode = GetLastReceiptNumber(out intLegalReceiptLastNumber, out intIllegalReceiptLastNumber)) != 0)
            {
                printResult.ErrorDescription = EpsonFiscal_LogError(" at EpsonFiscal_PrintZ_Report->Cannot Get Receipt Number", ErrorCode, false);
            }
            printResult.ReceiptNo = intLegalReceiptLastNumber.ToString();
            CloseFiscal();
            return printResult;
        }

        /// <summary>
        /// Get Z total interface
        /// </summary>
        /// <returns></returns>
        public override PrintResultModel GetZTotal()
        {
            logger.LogInformation("In EpsonFiscalExtcer GetZTotals");
            printResult = new PrintResultModel();
            printResult.ExtcerType = ExtcerTypesEnum.EpsonFiscal;
            printResult.ReceiptType = PrintModeEnum.ZTotals;
            printResult.Status = PrintStatusEnum.Unknown;
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
            logger.LogInformation("Starting 'PrintGraphic' within EpsonFiscalExtcer.");
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
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Print image interface
        /// </summary>
        public override void PrintImage()
        {
            return;
        }

        /// <summary>
        /// Open drawer interface
        /// </summary>
        public override void OpenDrawer()
        {
            CheckDeviceOk();
            FDDrawer();
            return;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Payment types initialization.
        /// </summary>
        private void SetupAccountIds()
        {
            if (InstallationData != null)
            {
                // cash accounts
                var CashAccs = InstallationData.OposMethodOfPaymentCASH.Split(',');
                foreach (var item in CashAccs)
                {
                    int res = 0;
                    if (int.TryParse(item, out res))
                    {
                        CashAccountIds.Add(res);
                    }
                }
                // credit card accounts
                var CCAccs = InstallationData.OposMethodOfPaymentCC.Split(',');
                foreach (var item in CCAccs)
                {
                    int res = 0;
                    if (int.TryParse(item, out res))
                    {
                        CCAccountIds.Add(res);
                    }
                }
                // credit accounts
                var CreditAccs = InstallationData.OposMethodOfPaymentCREDIT.Split(',');
                foreach (var item in CreditAccs)
                {
                    int res = 0;
                    if (int.TryParse(item, out res))
                    {
                        CreditAccountIds.Add(res);
                    }
                }
            }
        }

        /// <summary>
        /// Initialize VATs array from disk file "ECRVAT.txt".
        /// </summary>
        private void LoadDiskFileWithVATs()
        {
            // get application path to a variable
            var pathComponents = new List<string>() { applicationPath, "Config", "VAT", "ECRVAT.txt" };
            var vatPath = Path.GetFullPath(Path.Combine(pathComponents.ToArray()));
            string fileToReadPath = vatPath;
            string strReadLine;
            // if VATs file exists
            if (File.Exists(fileToReadPath))
            {
                try
                {
                    using (TextReader textReader = new StreamReader(fileToReadPath))
                    {
                        int i = 0;
                        while ((strReadLine = textReader.ReadLine()) != null)
                        {
                            arrayOfVats[i++] = decimal.Parse(strReadLine.Split(',')[1].Replace('.', ','));
                        }
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError("Error reading 'ECRVAT.txt'.\r\nError Message: ", exception);
                }
            }
            else
            {
                logger.LogError("Error loading ECRVAT.txt: file not found at application location");
                localHubInvoker.SendError("Critical File Missing:\r\nCannot find 'ECRVAT.txt' file!");
            }
        }

        /// <summary>
        /// I doubt we need this!
        /// </summary>
        /// <param name="intVatCode"></param>
        private void GetVatAmountFromFiscal(int intVatCode)
        {
            // the command in byte array variable
            byte[] arrayOfBytesFromCommand = Encoding.ASCII.GetBytes("0/");
            // the VAT values we get from the fiscal device
            byte[] resultArray = new byte[9000];
            sbyte bt1 = 0;
            sbyte bt2 = 0;
            int intErrorCode;
            try
            {
                unsafe
                {
                    fixed (byte* pbcommandData = arrayOfBytesFromCommand, pbResults = &resultArray[0])
                    {
                        // the final command to give the fiscal device to execute
                        sbyte* commandData = (sbyte*)pbcommandData;
                        sbyte* getFields = (sbyte*)pbResults;
                        unchecked
                        {
                            if (FD_SUCCESS != (intErrorCode = FDSendAndGetData(commandData, getFields, &bt1, &bt2, (uint)-1)))
                            {
                                EpsonFiscal_LogError("Cannot Retrieve Totals From Fiscal.", intErrorCode, false);
                                return;
                            }
                            if (intVatCode == 0)
                            {
                                subtotalRec.Vat1 = StringToReal100(new string(getFields + 5 * 300));
                                subtotalRec.Vat2 = StringToReal100(new string(getFields + 6 * 300));
                                subtotalRec.Vat3 = StringToReal100(new string(getFields + 7 * 300));
                                subtotalRec.Vat4 = StringToReal100(new string(getFields + 8 * 300));
                                subtotalRec.Vat5 = StringToReal100(new string(getFields + 9 * 300));
                                subtotalRec.Total = StringToReal100(new string(getFields + 10 * 300));
                            }
                            else
                            {
                                totalRec.Vat1 = StringToReal100(new string(getFields + 5 * 300));
                                totalRec.Vat2 = StringToReal100(new string(getFields + 6 * 300));
                                totalRec.Vat3 = StringToReal100(new string(getFields + 7 * 300));
                                totalRec.Vat4 = StringToReal100(new string(getFields + 8 * 300));
                                totalRec.Vat5 = StringToReal100(new string(getFields + 9 * 300));
                                totalRec.Total = StringToReal100(new string(getFields + 10 * 300));
                                arrayOfVats[0] = (decimal)Math.Round(totalRec.Vat1 - subtotalRec.Vat1);
                                arrayOfVats[1] = (decimal)Math.Round(totalRec.Vat2 - subtotalRec.Vat2);
                                arrayOfVats[2] = (decimal)Math.Round(totalRec.Vat3 - subtotalRec.Vat3);
                                arrayOfVats[3] = (decimal)Math.Round(totalRec.Vat4 - subtotalRec.Vat4);
                                arrayOfVats[4] = (decimal)Math.Round(totalRec.Vat5 - subtotalRec.Vat5);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Gets a number in string form and return an integer in which last two digits correspond to the fractional part and the other digits to the integer part.
        /// </summary>
        /// <param name="strSource"></param>
        /// <returns></returns>
        private int StringToReal100(string strSource)
        {
            if (strSource.Length == 0)
            {
                return 0;
            }
            int dotPosition = strSource.IndexOf('.');
            if (dotPosition == -1)
            {
                // no dot found
                try
                {
                    return int.Parse(strSource) * 100;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            if (dotPosition == 0)
            {
                // the source has the form ".123"
                strSource = "0" + strSource;
            }
            // get integer part
            string strTmp = strSource.Substring(0, dotPosition);
            // get fractional part
            strSource = strSource.Substring(dotPosition + 1);
            try
            {
                return int.Parse(strTmp) * 100 + int.Parse(strSource) % 100;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Connect to EpsonFiscal fiscal device and get device status. In case the status is ok (0), we can continue to the rest of our code.
        /// </summary>
        private void OpenFiscal()
        {
            int intDeviceId = 1;
            logger.LogInformation(ExtcerLogger.Log("at EpsonFiscal - Connecting - ", FiscalName));
            try
            {
                if (FD_SUCCESS != (ErrorCode = FDOpenDevice(intDeviceId, intCOMportToFiscalDevice)))
                {
                    EpsonFiscal_LogError("at EpsonFiscal - Connect Failed. - ", ErrorCode, false);
                    return;
                }
                else
                {
                    logger.LogInformation(ExtcerLogger.Log("at EpsonFiscal - Connect OK! - ", FiscalName));
                }
                sbyte[] rsltMsg = new sbyte[306];
                unsafe
                {
                    int statusID;
                    fixed (sbyte* statusDescr = &rsltMsg[0])
                    {
                        if (FD_SUCCESS != (ErrorCode = FDGetStatus(&statusID, statusDescr)))
                        {
                            EpsonFiscal_LogError(" at EpsonFiscal_OpenFiscal->Cannot Open Device - ", ErrorCode, false, new string(statusDescr));
                            return;
                        }
                    }
                }
                if (FD_SUCCESS != (ErrorCode = FDInit()))
                {
                    EpsonFiscal_LogError("at EpsonFiscal - Init CMD - ", ErrorCode);
                    return;
                }
            }
            catch (Exception exception)
            {
                logger.LogError(" Error on EpsonFiscal CloseDevice : ", exception);
                ErrorCode = 1;
                return;
            }
            ErrorCode = 0;
        }

        /// <summary>
        /// Close connection to fiscal device.
        /// </summary>
        private void CloseFiscal()
        {
            try
            {
                if (FD_SUCCESS != (ErrorCode = FDCloseDevice(0)))
                {
                    EpsonFiscal_LogError("at EpsonFiscal - CloseDevice - ", ErrorCode, false);
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error on EpsonFiscal CloseDevice : ", exception);
            }
        }

        /// <summary>
        /// Checks the device status and reinitializes it
        /// </summary>
        private void CheckDeviceOk()
        {
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strCommand"></param>
        /// <returns></returns>
        private int AddTransactionLine(string strCommand)
        {
            // on EPSON fiscal we use codepage Windows1253 to support Greek characters
            byte[] arrayOfBytesFromTransmitLine = Encoding.GetEncoding(1253).GetBytes(strCommand);
            int intTmp = -1;
            int intResultCode;
            logger.LogInformation("Command to execute: [" + strCommand + "]");
            unsafe
            {
                fixed (byte* pbTransmitLine = arrayOfBytesFromTransmitLine)
                {
                    sbyte* transmitLine = (sbyte*)pbTransmitLine;
                    if (FD_SUCCESS == (intResultCode = FDAddTransactionLine(&intTmp, transmitLine)))
                    {
                        return 0;
                    }
                }
            }
            return intResultCode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="intLineWithError"></param>
        /// <returns></returns>
        private int DoTransaction(out int intLineWithError)
        {
            int intErrorCode;
            int intTmp = -1;
            int intErrorLine = -1;
            intLineWithError = -1;
            unsafe
            {
                if (FD_SUCCESS == (intErrorCode = FDDoTransaction(&intTmp, &intErrorLine)))
                {
                    return 0;
                }
            }
            intLineWithError = intErrorLine;
            return intErrorCode;
        }

        /// <summary>
        /// Sends a command to the OPOS3 fiscal device to open transaction/receipt.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <param name="strWaiterName"></param>
        /// <param name="strTable"></param>
        /// <returns></returns>
        private int BeginFiscalReceipt(string strWaiterName = "", string strTable = "")
        {
            GetVatAmountFromFiscal(0);
            int intLegalReceiptLastNumber;
            int intIllegalReceiptLastNumber;
            if ((ErrorCode = GetLastReceiptNumber(out intLegalReceiptLastNumber, out intIllegalReceiptLastNumber)) != 0)
            {
                printResult.ErrorDescription = EpsonFiscal_LogError(" at EpsonFiscal_BeginFiscalReceipt->Cannot Get Receipt Number", ErrorCode, false);
            }
            string ReceiptNo = intLegalReceiptLastNumber.ToString();
            return AddTransactionLine("0/1/1/1/" + strWaiterName + "  /TABLE " + strTable);
        }

        /// <summary>
        /// Sends a command to the OPOS3 fiscal device to close an open transaction/receipt.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <returns></returns>
        private int EndFiscalReceipt()
        {
            int intLineWithError;
            return DoTransaction(out intLineWithError);
        }

        /// <summary>
        /// Add an item within an open receipt.
        /// </summary>
        /// <param name="strDescription"></param>
        /// <param name="dmlPrice"></param>
        /// <param name="dcmlQuantity"></param>
        /// <param name="intVatRate"></param>
        /// <returns></returns>
        private int PrintReceipt_NormalItem(string strDescription, decimal dmlPrice, decimal dcmlQuantity, int intVatRate)
        {
            int intDescriptionLength = strDescription.Length;
            if (string.Empty != strDescription)
            {
                strDescription = strDescription.Replace('/', '-').Substring(0, (intDescriptionLength > 20) ? 20 : intDescriptionLength);
            }
            try
            {
                string prep = string.Format("1/S/{0}//{1}/{2}/0/{3}/", strDescription, dcmlQuantity.ToString("#0.000"), dmlPrice.ToString("0.###"), arrayOfVats[intVatRate - 1]).Replace(',', '.');
                logger.LogInformation("Command to execute: [" + prep + "]");
                return AddTransactionLine(prep);
            }
            catch (Exception exception)
            {
                logger.LogError("Error in PrintReceipt_NormalItem (catch) : ", exception);
            }
            return -1;
        }

        /// <summary>
        /// Refund an item within an open receipt.
        /// </summary>
        /// <param name="strDescription"></param>
        /// <param name="dcmlPrice"></param>
        /// <param name="dcmlQuantity"></param>
        /// <param name="intVatRate"></param>
        /// <param name="strAdditionalDescription"></param>
        /// <returns></returns>
        private int PrintRecRefund(string strDescription, decimal dcmlPrice, decimal dcmlQuantity, int intVatRate, string strAdditionalDescription = "")
        {
            string prep = string.Format("1/R/{0}/{1}/{2}/{3}/0/{4}/", strDescription, strAdditionalDescription, dcmlQuantity.ToString("#0.000").Replace(",", "."), dcmlPrice.ToString("0.###").Replace(",", "."), arrayOfVats[intVatRate - 1].ToString("0.##").Replace(",", "."));
            logger.LogInformation("Command to execute: [" + prep + "]");
            return AddTransactionLine(prep);
        }

        /// <summary>
        /// Cancel an item that has been added to the receipt and print a void description.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <param name="strDescription"></param>
        /// <param name="dcmlPrice"></param>
        /// <param name="dcmlQuantity"></param>
        /// <param name="intVatRate"></param>
        /// <param name="strAdditionalInfo"></param>
        /// <returns></returns>
        private int PrintReceipt_VoidItem(string strDescription, decimal dcmlPrice, decimal dcmlQuantity, int intVatRate, string strAdditionalInfo = "")
        {
            try
            {
                string prep = string.Format("1/V/{0}/{1}/{2}/{3}/0/{4}//", strDescription, strAdditionalInfo, dcmlQuantity.ToString("#0.000"), dcmlPrice.ToString("0.###"), arrayOfVats[intVatRate - 1]).Replace(',', '.');
                logger.LogInformation("Command to execute: [" + prep + "]");
                return AddTransactionLine(prep);
            }
            catch (Exception exception)
            {
                logger.LogError("Error in PrintReceipt_VoidItem (catch) : ", exception);
            }
            return -1;
        }

        /// <summary>
        /// Called to apply and print a discount or a surcharge to the last receipt item sold or the receipt (sub-)total.
        /// Returns the resulting error code from the device driver, or zero if the amount to be used is null or bellow or equal to zero.
        /// </summary>
        /// <param name="adjustmentType"></param>
        /// <param name="dcmlAmount"></param>
        /// <param name="strDescription">Optional adjustment description - up to 16 chars. If omitted the default is used (Discount/Markup).</param>
        /// <param name="strAdditionalDescription">Optional additional description - up to 30 chars. If has length greater than zero, it will be printed in a separate line next to the discount/markup.</param>
        /// <returns></returns>
        private int PrintReceiptAdjustment(AdjustmentType adjustmentType, decimal? dcmlAmount, string strDescription = "", string strAdditionalDescription = "")
        {
            int intDescriptionLength = strDescription.Length;
            int intAdditionalDescriptionLength = strAdditionalDescription.Length;
            if (dcmlAmount != null && dcmlAmount > 0)
            {
                string pout = string.Empty;
                if (strDescription != string.Empty)
                {
                    strDescription = strDescription.Replace('/', '-').Substring(0, (intDescriptionLength > 16) ? 16 : intDescriptionLength);
                }
                if (strAdditionalDescription != string.Empty)
                {
                    strAdditionalDescription = strAdditionalDescription.Replace('/', '-').Substring(0, (intAdditionalDescriptionLength > 30) ? 30 : intAdditionalDescriptionLength);
                }
                switch (adjustmentType)
                {
                    case AdjustmentType.SalesDiscount:
                        pout = string.Format("2/1/{0}/0.00/{1}/{2}/", dcmlAmount, strDescription, strAdditionalDescription).Replace(',', '.');
                        break;
                    case AdjustmentType.SubtotalDiscount:
                        pout = string.Format("2/2/{0}/0.00/{1}/{2}/", dcmlAmount, strDescription, strAdditionalDescription).Replace(',', '.');
                        break;
                    case AdjustmentType.SalesMarkup:
                        pout = string.Format("2/3/{0}/0.00/{1}/{2}/", dcmlAmount, strDescription, strAdditionalDescription).Replace(',', '.');
                        break;
                    case AdjustmentType.SubtotalMarkup:
                        pout = string.Format("2/4/{0}/0.00/{1}/{2}/", dcmlAmount, strDescription, strAdditionalDescription).Replace(',', '.');
                        break;
                    default:
                        break;
                }
                return AddTransactionLine(pout);
            }
            return 0;
        }

        /// <summary>
        /// Mainly used to implement Voucher entry within receipt, but can be used as Credit Cards also, given the appropriate description.
        /// </summary>
        /// <param name="dcmlAmount"></param>
        /// <param name="strDescription"></param>
        /// <returns></returns>
        private int PrintRecNotPaid(decimal? dcmlAmount, string strDescription = "Voucher")
        {
            string prep = string.Format("3/{0}/11/{1}///", dcmlAmount, strDescription);
            logger.LogInformation("Command to execute: [" + prep + "]");
            return AddTransactionLine(prep);
        }

        /// <summary>
        /// Print a payment within an open receipt.
        /// </summary>
        /// <param name="paymentType"></param>
        /// <param name="payAmount"></param>
        /// <param name="strPaymentDescription"></param>
        /// <returns></returns>
        private int PrintRecTotal(PaymentType paymentType, decimal? payAmount, string strPaymentDescription = "")
        {
            int intPaymentCode = 0;
            switch (paymentType)
            {
                case PaymentType.CreditCard:
                    if (strPaymentDescription == string.Empty)
                    {
                        strPaymentDescription = "Πιστωτική Κάρτα";
                    }
                    intPaymentCode = 11;
                    break;
                case PaymentType.Check:
                    if (strPaymentDescription == string.Empty)
                    {
                        strPaymentDescription = "Επιταγή";
                    }
                    intPaymentCode = 21;
                    break;
                case PaymentType.Cash:
                default:
                    if (strPaymentDescription == string.Empty)
                    {
                        strPaymentDescription = "Μετρητά";
                    }
                    intPaymentCode = 31;
                    break;
            }
            string prep = string.Format("3/{0}/{1}/{2}/", payAmount ?? 0, intPaymentCode, strPaymentDescription);
            logger.LogInformation("Command to execute: [" + prep + "]");
            return AddTransactionLine(prep);
        }

        /// <summary>
        /// Prints receipt comments.
        /// Comments must be EXCACTLY 3 rows, with the following format: "8/Comment Line1/Comment Line2/Comment Line3/".
        /// </summary>
        /// <param name="receiptModel"></param>
        private void PrintComments(ReceiptModel receiptModel)
        {
            List<string> comments = GetReceiptCommentsText(receiptModel, PrinterTypeEnum.Receipt).Take(3).ToList();
            if (comments.Count > 0)
            {
                string strCommentsLine1 = string.Empty;
                string strCommentsLine2 = string.Empty;
                string strCommentsLine3 = string.Empty;
                int intLengthLine1;
                int intLengthLine2;
                int intLengthLine3;
                switch (comments.Count)
                {
                    case 1:
                        intLengthLine1 = comments[0].Length;
                        if (intLengthLine1 > 0)
                        {
                            strCommentsLine1 = comments[0].Replace('/', '-').Substring(0, (intLengthLine1 > 32) ? 32 : intLengthLine1);
                        }
                        break;
                    case 2:
                        intLengthLine1 = comments[0].Length;
                        intLengthLine2 = comments[1].Length;
                        if (intLengthLine1 > 0)
                        {
                            strCommentsLine1 = comments[0].Replace('/', '-').Substring(0, (intLengthLine1 > 32) ? 32 : intLengthLine1);
                        }
                        if (intLengthLine2 > 0)
                        {
                            strCommentsLine2 = comments[1].Replace('/', '-').Substring(0, (intLengthLine2 > 32) ? 32 : intLengthLine2);
                        }
                        break;
                    case 3:
                        intLengthLine1 = comments[0].Length;
                        intLengthLine2 = comments[1].Length;
                        intLengthLine3 = comments[2].Length;
                        if (intLengthLine1 > 0)
                        {
                            strCommentsLine1 = comments[0].Replace('/', '-').Substring(0, (intLengthLine1 > 32) ? 32 : intLengthLine1);
                        }
                        if (intLengthLine2 > 0)
                        {
                            strCommentsLine2 = comments[1].Replace('/', '-').Substring(0, (intLengthLine2 > 32) ? 32 : intLengthLine2);
                        }
                        if (intLengthLine3 > 0)
                        {
                            strCommentsLine3 = comments[2].Replace('/', '-').Substring(0, (intLengthLine3 > 32) ? 32 : intLengthLine3);
                        }
                        break;
                    default:
                        break;
                }
                AddTransactionLine(string.Format("8/{0}/{1}/{2}/", strCommentsLine1, strCommentsLine2, strCommentsLine3));
            }
        }

        /// <summary>
        /// Cancel currently open receipt.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <returns></returns>
        private int PrintRecVoid()
        {
            int intErrorCode;
            if (FD_SUCCESS == (intErrorCode = FDCancelTransaction()))
            {
                return 0;
            }
            return intErrorCode;
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
                logger.LogInformation("CANCELING RECEIPT");
                if (FD_SUCCESS != PrintRecVoid())
                {
                    logger.LogError("CANCELING RECEIPT FAILED!");
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read from the device DLL (driver) the record for the last legal and illegal receipt numbers.
        /// </summary>
        /// <param name="intLegalReceiptLastNumber">If successful, it is the last legal receipt number.</param>
        /// <param name="intIllegalReceiptLastNumber">If successful, it is the last illegal receipt number.</param>
        /// <returns>Returns the error code, on success is zero (0), else the error code.</returns>
        private int GetLastReceiptNumber(out int intLegalReceiptLastNumber, out int intIllegalReceiptLastNumber)
        {
            int intErrorCode = 0;
            int intLegalReceiptLastNumber_Local;
            int intIllegalReceiptLastNumber_Local;
            intLegalReceiptLastNumber = -1;
            intIllegalReceiptLastNumber = -1;
            unsafe
            {
                if (FD_SUCCESS != (intErrorCode = FDGetReceiptNumber(&intLegalReceiptLastNumber_Local, &intIllegalReceiptLastNumber_Local)))
                {
                    return intErrorCode;
                }
            }
            intLegalReceiptLastNumber = intLegalReceiptLastNumber_Local;
            intIllegalReceiptLastNumber = intIllegalReceiptLastNumber_Local;
            logger.LogInformation("Currently Last Receipt Number: #" + intLegalReceiptLastNumber);
            return intErrorCode;
        }

        /// <summary>
        /// Append to log file the latest OPOS error. Returns the error description.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="resCode"></param>
        /// <param name="blnCancelNeeded"></param>
        /// <param name="strStatusDescription"></param>
        /// <returns></returns>
        private string EpsonFiscal_LogError(string message, int resCode, bool blnCancelNeeded = true, string strStatusDescription = "")
        {
            if (resCode == 0)
            {
                return string.Empty;
            }
            string strErrDescr;
            EpsonFiscal_GetDeviceError(out strErrDescr);
            string strErrMsg = "EpsonFiscal error: " + message + " ErrorCode- " + resCode;
            string strErrMsg1 = ("" != strStatusDescription) ? "\r\nWITH GETERROR: " + message + " errorCode - " + resCode + " message   - " + strErrDescr + "\r\n extended error- " + strStatusDescription + "\r\n" : "\r\nWITH GETERROR: " + message + " errorCode - " + resCode + " message   - " + strErrDescr + "\r\n";
            logger.LogError(ExtcerLogger.logErr("\r\n" + "================================================\r\n" + strErrMsg, FiscalName));
            logger.LogError(ExtcerLogger.logErr(strErrMsg1 + "\r\n" + "================================================\r\n", FiscalName));
            if (blnCancelNeeded)
            {
                logger.LogInformation("CANCELING RECEIPT");
                PrintRecVoid();
            }
            if (printResult != null)
            {
                printResult.ErrorDescription = strErrDescr;
                return printResult.ErrorDescription;
            }
            return ExtcerErrorHelper.GetErrorDescription(ExtcerErrorHelper.GENERAL_ERROR);
        }

        /// <summary>
        /// Get the description of the last error and the suggested action to recover from it.
        /// </summary>
        /// <param name="strErrMsg"></param>
        private void EpsonFiscal_GetDeviceError(out string strErrMsg)
        {
            int intErrorNumber;
            sbyte[] rsltMsg1 = new sbyte[306];
            strErrMsg = string.Empty;
            unsafe
            {
                fixed (sbyte* sbytPtrErrorMsg = &rsltMsg1[0])
                {
                    if (FD_SUCCESS == (ErrorCode = FDGetLastError(&intErrorNumber, sbytPtrErrorMsg)))
                    {
                        // we have to issue a Z report on the device (as far as I know).
                        if (intErrorNumber != 149)
                        {
                            strErrMsg = new string(sbytPtrErrorMsg);
                        }
                        else
                        {
                            strErrMsg = "You have to issue a Z report to continue with receipts!";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Case InvoiceIndex == 1.
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="strFiscalName"></param>
        /// <returns></returns>
        private PrintResultModel EpsonFiscal_PrintReceipt(ReceiptModel receiptModel, string strFiscalName)
        {
            int? maxPrintableChars = InstallationData.OposMaxString;
            OpenFiscal();
            printResult = new PrintResultModel();
            printResult.InvoiceIndex = receiptModel.InvoiceIndex;
            printResult.ExtcerType = ExtcerTypesEnum.EpsonFiscal;
            receiptModel.FiscalType = FiscalTypeEnum.Opos;
            try
            {
                if (receiptModel.IsVoid)
                {
                    CloseFiscal();
                    // print the void in different printer
                    printResult = PrintVoidReceipt(receiptModel, strFiscalName);
                    return printResult;
                }
                printResult.ReceiptNo = receiptModel.ReceiptNo;
                printResult.ReceiptData = GetReceiptPreviewText(receiptModel);
                ErrorCode = BeginFiscalReceipt();
                printResult.ErrorDescription = EpsonFiscal_LogError("at BeginFiscalReceipt", ErrorCode, false);
                if (ErrorCode > 0)
                {
                    CloseFiscal();
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ReceiptNo = receiptModel.ReceiptNo;
                    return printResult;
                }
                decimal total = 0;
                logger.LogInformation(ExtcerLogger.Log("------------- RECEIPT #" + receiptModel.ReceiptNo + " -------------", FiscalName));
                foreach (var item in receiptModel.Details)
                {
                    string description = string.IsNullOrEmpty(item.ItemDescr) ? "    " : item.ItemDescr.Replace('/', '-');
                    if (maxPrintableChars != null && item.ItemDescr.Length > maxPrintableChars)
                    {
                        description = description.Substring(0, (int)maxPrintableChars - 1);
                    }
                    if (item.IsVoid)
                    {
                        total = total - (item.ItemPrice * item.ItemQty);
                        ErrorCode = PrintReceipt_VoidItem(description, item.ItemPrice, item.ItemQty, item.ItemVatRate);
                        printResult.ErrorDescription = EpsonFiscal_LogError("at void item", ErrorCode);
                        if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                        {
                            CloseFiscal();
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
                            ErrorCode = PrintRecRefund(description, Math.Abs(item.ItemGross), item.ItemQty, item.ItemVatRate);
                            printResult.ErrorDescription = EpsonFiscal_LogError("at PrintReceipt refund item", ErrorCode);
                            if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                            {
                                CloseFiscal();
                                printResult.Status = PrintStatusEnum.Failed;
                                printResult.ReceiptNo = receiptModel.ReceiptNo;
                                return printResult;
                            }
                        }
                        else
                        {
                            if (EpsonFiscal_PrintReceipt_HandleNormalItem(receiptModel, item, maxPrintableChars, ref total, description) != null)
                            {
                                return printResult;
                            }
                        }
                        if (item.Extras.Count > 0)
                        {
                            if (EpsonFiscal_PrintReceipt_HandleExtras(receiptModel, item, maxPrintableChars, ref total) != null)
                            {
                                return printResult;
                            }
                        }
                    }
                }
                if (receiptModel.TotalDiscount != null && receiptModel.TotalDiscount > 0)
                {
                    ErrorCode = PrintReceiptAdjustment(AdjustmentType.SubtotalDiscount, (decimal)receiptModel.TotalDiscount, "ΈκπτωσηΑπόδειξης");
                    printResult.ErrorDescription = EpsonFiscal_LogError("at print total receipt discount ", ErrorCode);
                    logger.LogInformation(ExtcerLogger.Log("RECEIPT DISCOUNT -- : " + (decimal)receiptModel.TotalDiscount, FiscalName));
                    if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                    {
                        CloseFiscal();
                        printResult.Status = PrintStatusEnum.Failed;
                        printResult.ReceiptNo = receiptModel.ReceiptNo;
                        return printResult;
                    }
                }
                logger.LogInformation(ExtcerLogger.Log("------------ END RECEIPT -------------\r\n", FiscalName));
                PrintComments(receiptModel);
                var voucher = receiptModel.PaymentsList.Where(f => (int)f.AccountType == 6).FirstOrDefault();
                if (voucher != null)
                {
                    ErrorCode = PrintRecNotPaid((decimal)(voucher.Amount), voucher.Description);
                    EpsonFiscal_LogError("at Voucher", ErrorCode);
                    if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                    {
                        CloseFiscal();
                        printResult.Status = PrintStatusEnum.Failed;
                        printResult.ReceiptNo = receiptModel.ReceiptNo;
                        return printResult;
                    }
                }
                logger.LogInformation("Total is: " + total);
                if (receiptModel.PaymentsList.Count > 0)
                {
                    foreach (var pw in receiptModel.PaymentsList)
                    {
                        bool blnIsLastItem = pw.Equals(receiptModel.PaymentsList.Last());
                        if (pw.AccountType != null)
                        {
                            if (pw.AccountType != 6)
                            {
                            }
                            if (InstallationData.OposMethodOfPaymentCASH != null && CashAccountIds.Contains((int)pw.AccountType))
                            {
                                // This is cash type handling
                                ErrorCode = PrintRecTotal(PaymentType.Cash, blnIsLastItem ? 0 : pw.Amount);
                            }
                            else if (InstallationData.OposMethodOfPaymentCC != null && CCAccountIds.Contains((int)pw.AccountType))
                            {
                                // This is credit card type handling
                                ErrorCode = PrintRecTotal(PaymentType.CreditCard, blnIsLastItem ? 0 : pw.Amount);
                            }
                            else if (InstallationData.OposMethodOfPaymentCREDIT != null && CreditAccountIds.Contains((int)pw.AccountType))
                            {
                                // This is credit type handling
                                ErrorCode = PrintRecTotal(PaymentType.Check, blnIsLastItem ? 0 : pw.Amount);
                            }
                            else if ((int)pw.AccountType == 6)
                            {
                                // This is ticket restaurant / voucher type handling
                            }
                            else
                            {
                                ErrorCode = PrintRecTotal(PaymentType.Cash, blnIsLastItem ? 0 : pw.Amount);
                            }
                        }
                    }
                }
                else
                {
                    logger.LogInformation("NO PAYMENT TYPE FOUND -- PRINTING AS CASH");
                    ErrorCode = PrintRecTotal(PaymentType.Cash, 0);
                    if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                    {
                        CloseFiscal();
                        printResult.Status = PrintStatusEnum.Failed;
                        printResult.ReceiptNo = receiptModel.ReceiptNo;
                        return printResult;
                    }
                }
                if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                {
                    CloseFiscal();
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ReceiptNo = receiptModel.ReceiptNo;
                    return printResult;
                }
                if (!CloseReceiptValidationOk(ErrorCode))
                {
                }
                if (receiptModel.IsVoid)
                {
                    ErrorCode = PrintRecVoid();
                    printResult.ErrorDescription = EpsonFiscal_LogError("Canceling receipt", ErrorCode);
                    if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                    {
                        CloseFiscal();
                        printResult.Status = PrintStatusEnum.Failed;
                        printResult.ReceiptNo = receiptModel.ReceiptNo;
                        return printResult;
                    }
                }
                else
                {
                    int intLegalReceiptLastNumber;
                    int intIllegalReceiptLastNumber;
                    if ((ErrorCode = GetLastReceiptNumber(out intLegalReceiptLastNumber, out intIllegalReceiptLastNumber)) != 0)
                    {
                        printResult.ErrorDescription = EpsonFiscal_LogError(" at EpsonFiscal_PrintReceipt(1)->Cannot Get Receipt Number", ErrorCode, false);
                    }
                    receiptModel.ReceiptNo = printResult.ReceiptNo = intLegalReceiptLastNumber.ToString();
                }
                ErrorCode = EndFiscalReceipt();
                printResult.ErrorDescription = EpsonFiscal_LogError("at EndFiscalReceipt", ErrorCode);
                if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                {
                    CloseFiscal();
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ReceiptNo = receiptModel.ReceiptNo;
                    return printResult;
                }
                if (!CloseReceiptValidationOk(ErrorCode))
                {
                }
                printResult.Status = PrintStatusEnum.Failed;
                printResult.OrderNo = receiptModel.OrderNo;
                printResult.ExtcerType = ExtcerTypesEnum.EpsonFiscal;
                printResult.ReceiptType = PrintModeEnum.Receipt;
                printResult.ReceiptData = new List<string>() { "", "      In OPOS Fiscal", " preview is not available" };
                if (ErrorCode == 0)
                {
                    int intLegalReceiptLastNumber;
                    int intIllegalReceiptLastNumber;
                    if ((ErrorCode = GetLastReceiptNumber(out intLegalReceiptLastNumber, out intIllegalReceiptLastNumber)) != 0)
                    {
                        printResult.ErrorDescription = EpsonFiscal_LogError(" at EpsonFiscal_PrintReceipt(2)->Cannot Get Receipt Number", ErrorCode, false);
                    }
                    printResult.Status = PrintStatusEnum.Printed;
                    receiptModel.ReceiptNo = printResult.ReceiptNo = intLegalReceiptLastNumber.ToString();
                }
                else
                {
                    printResult.Status = PrintStatusEnum.Failed;
                }
                ArtcasLogger.LogArtCasXML(receiptModel, DateTime.Now, EpsonFiscal_LogError(" ", ErrorCode), new List<string>(), ReceiptReceiveTypeEnum.WEB, printResult.Status, strFiscalName, printResult.Id);
            }
            catch (CustomException exception)
            {
                printResult.ErrorDescription = exception.Message;
                logger.LogError(ExtcerLogger.logErr("Error printing EpsonFiscal receipt: #" + printResult.ReceiptNo + "\r\nError Description: ", exception, FiscalName));
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("Error printing EpsonFiscal receipt: #" + printResult.ReceiptNo + "\r\nError Description: ", exception, FiscalName));
            }
            CloseFiscal();
            return printResult;
        }

        /// <summary>
        /// Print receipt item
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="item"></param>
        /// <param name="maxPrintableChars"></param>
        /// <param name="total"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        private PrintResultModel EpsonFiscal_PrintReceipt_HandleNormalItem(ReceiptModel receiptModel, ReceiptItemsModel item, int? maxPrintableChars, ref decimal total, string description)
        {
            if (item.ItemPrice > 0)
            {
                total = total + (item.ItemPrice * item.ItemQty);
                ErrorCode = PrintReceipt_NormalItem(description, item.ItemPrice, item.ItemQty, item.ItemVatRate);
                printResult.ErrorDescription = EpsonFiscal_LogError("at PrintReceipt item", ErrorCode);
                logger.LogInformation(ExtcerLogger.Log("ITEM --  Description: " + item.ItemDescr + " QTY: " + item.ItemQty + " Price: " + item.ItemPrice, FiscalName));
                if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                {
                    CloseFiscal();
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ReceiptNo = receiptModel.ReceiptNo;
                    return printResult;
                }
                if (item.ItemDiscount != null && item.ItemDiscount > 0)
                {
                    total = total - (decimal)(item.ItemDiscount * item.ItemQty);
                    string discountDescription = "Έκπτωση " + item.ItemDescr;
                    if (maxPrintableChars != null && discountDescription.Length > maxPrintableChars)
                    {
                        discountDescription = discountDescription.Substring(0, (int)maxPrintableChars - 1);
                    }
                    ErrorCode = PrintReceiptAdjustment(AdjustmentType.SalesDiscount, item.ItemDiscount, discountDescription);
                    printResult.ErrorDescription = EpsonFiscal_LogError("at PrintReceipt discount item: " + discountDescription, ErrorCode);
                    logger.LogInformation(ExtcerLogger.Log("Item DISCOUNT -- : " + (decimal)item.ItemDiscount, FiscalName));
                    if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                    {
                        CloseFiscal();
                        printResult.Status = PrintStatusEnum.Failed;
                        printResult.ReceiptNo = receiptModel.ReceiptNo;
                        return printResult;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Print receipt extra
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="item"></param>
        /// <param name="maxPrintableChars"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        private PrintResultModel EpsonFiscal_PrintReceipt_HandleExtras(ReceiptModel receiptModel, ReceiptItemsModel item, int? maxPrintableChars, ref decimal total)
        {
            foreach (var extra in item.Extras)
            {
                string extrDescr = string.IsNullOrEmpty(extra.ItemDescr) ? "    " : "+ " + extra.ItemDescr;
                if (maxPrintableChars != null && extrDescr.Length > maxPrintableChars)
                {
                    extrDescr = extrDescr.Substring(0, (int)maxPrintableChars - 1);
                }
                if (extra.ItemPrice != null && extra.ItemPrice > 0)
                {
                    var itemPrice = (decimal)extra.ItemPrice;
                    var itemGross = (decimal)extra.ItemGross;
                    if (extra.IsChangeItem)
                    {
                        total = total - (item.ItemGross * extra.ItemQty);
                        ErrorCode = PrintReceiptAdjustment(AdjustmentType.SalesDiscount, extra.ItemPrice, extra.ItemDescr);
                        EpsonFiscal_LogError("at PrintRec discount item", ErrorCode);
                        ErrorCode = PrintRecRefund(extrDescr, itemGross, extra.ItemQty, extra.ItemVatRate);
                        printResult.ErrorDescription = EpsonFiscal_LogError("at printRec discount item", ErrorCode);
                        if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                        {
                            CloseFiscal();
                            printResult.Status = PrintStatusEnum.Failed;
                            printResult.ReceiptNo = receiptModel.ReceiptNo;
                            return printResult;
                        }
                    }
                    else
                    {
                        total = total + (itemPrice * extra.ItemQty);
                        ErrorCode = PrintReceipt_NormalItem(extrDescr, itemPrice, extra.ItemQty, extra.ItemVatRate);
                        printResult.ErrorDescription = EpsonFiscal_LogError("at printRec extras item", ErrorCode);
                        logger.LogInformation(ExtcerLogger.Log("  EXTRA -- Description: " + extra.ItemDescr + " QTY: " + extra.ItemQty + " Price: " + extra.ItemPrice, FiscalName));
                        if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                        {
                            CloseFiscal();
                            printResult.Status = PrintStatusEnum.Failed;
                            printResult.ReceiptNo = receiptModel.ReceiptNo;
                            return printResult;
                        }
                    }
                    if (extra.ItemDiscount != null && extra.ItemDiscount > 0)
                    {
                        total = total - (decimal)(extra.ItemDiscount * extra.ItemQty);
                        ErrorCode = PrintReceiptAdjustment(AdjustmentType.SalesDiscount, extra.ItemDiscount, "Έκπτωση " + extrDescr);
                        printResult.ErrorDescription = EpsonFiscal_LogError("at printRec discount extra", ErrorCode);
                        logger.LogInformation(ExtcerLogger.Log("  EXTRA DISCOUNT -- : " + (decimal)extra.ItemDiscount, FiscalName));
                        if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                        {
                            CloseFiscal();
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
                printResult.ExtcerType = ExtcerTypesEnum.EpsonFiscal;
                printResult.ReceiptType = PrintModeEnum.Void;
                // get printer for void
                logger.LogInformation(ExtcerLogger.Log("Getting printer for void...", FiscalName));
                KitchenPrinterModel printersettings;
                printersettings = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Void);
                // get receipt printer template
                logger.LogInformation(ExtcerLogger.Log("Getting printer template for void...", FiscalName));
                RollerTypeReportModel template;
                template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.Void).Value.FirstOrDefault().Value;
                if (template == null)
                {
                    logger.LogWarning(ExtcerLogger.Log("Generic void template not found", FiscalName));
                }
                else
                {
                    logger.LogWarning(ExtcerLogger.Log("Template: " + template.ReportName ?? "<null>", FiscalName));
                }
                printResult.ReceiptData = ProcessVoidReceiptTemplate(template, receiptModel, printersettings);
                printResult.Status = PrintStatusEnum.Printed;
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("Error printing OPOS void: " + exception.Message + " StackTRace: ", exception, FiscalName));
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
            catch (Exception)
            {
                logger.LogInformation(ExtcerLogger.Log("Error generating OPOS preview receipt text", FiscalName));
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
                // get printer for receipt
                logger.LogInformation("Getting printer for receipt...");
                var printerSettings = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Receipt && f.SlotIndex == "1");
                if (printerSettings == null)
                {
                    logger.LogError(ExtcerLogger.Log("Receipt printer NOT FOUND into printers list. Check SlotIndex value (must be a printer with SlotIndex=1 for printing receipt) ", FiscalName));
                }
                // get receipt printer template
                logger.LogInformation("Getting receipt printer template for type " + printType.ToString() + "...");
                var template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == printType).Value.FirstOrDefault().Value;
                if (template == null)
                {
                    logger.LogWarning(ExtcerLogger.Log("Template not found", FiscalName));
                }
                else
                {
                    logger.LogInformation(ExtcerLogger.Log("Template: " + template.ReportName ?? "<null>", FiscalName));
                }
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
                }
                // get receipt printer template
                var template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Value.ReportName == printerSettings.TemplateShortName).Value.FirstOrDefault().Value;
                if (template == null)
                {
                    logger.LogWarning(ExtcerLogger.Log("Template not found", FiscalName));
                }
                else
                {
                    logger.LogInformation(ExtcerLogger.Log("Template: " + template.ReportName ?? "<null>", FiscalName));
                }
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
                logger.LogInformation(ExtcerLogger.Log("GetZText: z printer availablePrinters", FiscalName));
                // find the printer that is set for report printing
                var zPrinter = availablePrinters.Where(f => f.PrinterType == PrinterTypeEnum.ZReport).FirstOrDefault();
                logger.LogInformation(ExtcerLogger.Log("GetZText: z printer PrintersEscList", FiscalName));
                // get receipt printer template
                var template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.ZReport).Value.FirstOrDefault().Value;
                if (template == null)
                {
                    logger.LogError(ExtcerLogger.Log("Generic Z template not found", FiscalName));
                }
                else
                {
                    logger.LogInformation(ExtcerLogger.Log("Template: " + template.ReportName ?? "<null>", FiscalName));
                }
                logger.LogInformation(ExtcerLogger.Log("GetZText: proccesZ report template", FiscalName));
                // get text to print
                zStr = ProcessZReportTemplate(template, zData, zPrinter);
                return zStr;
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("EpsonFiscal: Error in  GetZText\r\nError: ", exception, FiscalName));
                return zStr;
            }
        }

        /// <summary>
        /// Process receipt template 
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentReceipt"></param>
        /// <param name="printerSettings"></param>
        /// <param name="allowPrint"></param>
        /// <returns></returns>
        private List<string> ProcessReceiptTemplate(RollerTypeReportModel repTemplate, ReceiptModel currentReceipt, KitchenPrinterModel printerSettings, bool allowPrint = false)
        {
            List<string> result = new List<string>();
            var receiptPrinter = printerSettings;
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
        /// Process receipt summary template
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
                        string CurInvoiceNo = string.Empty;
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
            logger.LogInformation("in  ProcessReceiptSumTemplate Sending kitchen to printer delegate");
            Task task = Task.Run(() => SendTextToPrinter(result, printer, printersettings));
            return result;
        }

        /// <summary>
        /// Process z template
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentZData"></param>
        /// <param name="printerSettings"></param>
        /// <returns></returns>
        private List<string> ProcessZReportTemplate(RollerTypeReportModel repTemplate, ZReportModel currentZData, KitchenPrinterModel printerSettings)
        {
            List<string> result = new List<string>();
            logger.LogInformation(ExtcerLogger.Log("Getting printer for ZReport...", FiscalName));
            var zPrinter = availablePrinters.Where(f => f.Name == printerSettings.Name && f.PrinterType == PrinterTypeEnum.ZReport).FirstOrDefault();
            if (zPrinter == null)
            {
                logger.LogError("Printer for ZReport not found.");
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
                        string data = GenericCommon.ReplacePatterns(obj, colText);
                        switch (colText)
                        {
                            case "@ItemTotal":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        data = (model.CreditTransactions.FirstOrDefault().Amount ?? 0).ToString();
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
                                        var res = string.Empty;
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
                                    if (!string.IsNullOrEmpty(col.FormatOption))
                                    {
                                        data = string.Format(col.FormatOption, date).ToUpper();
                                    }
                                    else
                                    {
                                        // set default format
                                        data = string.Format("{0:dd/MM/yyyy}", date).ToUpper();
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
                                    if (!string.IsNullOrEmpty(col.FormatOption))
                                    {
                                        data = string.Format(col.FormatOption, date).ToUpper();
                                    }
                                    else
                                    {
                                        // set default format
                                        data = string.Format("{0:HH:mm}", date).ToUpper();
                                    }
                                }
                                break;
                            case "@Day":
                                {
                                    if (!string.IsNullOrEmpty(col.FormatOption))
                                    {
                                        data = string.Format(col.FormatOption, data).ToUpper();
                                    }
                                    else
                                    {
                                        // set default format
                                        data = string.Format("{0:dd/MM/yyyy}", data).ToUpper();
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
                            if (string.IsNullOrEmpty(col.AlignOption))
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
                    {
                        logger.LogInformation(ExtcerLogger.Log("Printing OEM", printerSettings.FiscalName));
                        for (int i = 1; i <= printTimes; i++)
                        {
                            Encoding utf8 = new UTF8Encoding();
                            Encoding oem737 = Encoding.GetEncoding(737);
                            str = str + new string('\n', intKitchenHeaderGapLines) + '\r';
                            initChar = string.Empty;
                            if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen)
                            {
                                if (printerSettings.UseBuzzer != null && printerSettings.UseBuzzer == true)
                                {
                                    RawPrinterHelper.SendStringToPrinter(printerSettings.Name, buzzerEscChars);
                                }
                            }
                            SendBytesToPrinter(str, printerSettings.Name, 737);
                            if (printerSettings.UseCutter == null || printerSettings.UseCutter == true)
                            {
                                SendBytesToPrinter(cutterEscChars, printerSettings.Name, 737);
                            }
                        }
                    }
                    break;
                case PrintCharsFormatEnum.ANSI:
                    {
                        logger.LogInformation(ExtcerLogger.Log("Printing ANSI", printerSettings.FiscalName));
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
                    {
                        var toPrint = initChar + str + new string('\n', intKitchenHeaderGapLines) + '\r';
                        for (int i = 1; i <= printTimes; i++)
                        {
                            if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen && printerSettings.UseBuzzer == true)
                            {
                                RawPrinterHelper.SendStringToPrinter(printerSettings.Name, buzzerEscChars);
                            }
                            PrintGraphic(printerSettings.Name, toPrint, (7 == intKitchenHeaderGapLines));
                            if (printerSettings.UseCutter == null || printerSettings.UseCutter == true)
                            {
                                RawPrinterHelper.SendStringToPrinter(printerSettings.Name, cutterEscChars);
                            }
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
            Encoding destCodePage = Encoding.GetEncoding(codePage);
            byte[] input_utf8 = utf8.GetBytes(toSend);
            byte[] output_dest = Encoding.Convert(utf8, destCodePage, input_utf8);
            int nLength = Convert.ToInt32(output_dest.Length);
            // Allocate some unmanaged memory for those bytes.
            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.
            Marshal.Copy(output_dest, 0, pUnmanagedBytes, nLength);
            if (!RawPrinterHelper.SendBytesToPrinter(printerName, pUnmanagedBytes, nLength))
            {
                logger.LogError("SendBytesToPrinter:" + printerName + " -> Failed!");
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
            string line = null;
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