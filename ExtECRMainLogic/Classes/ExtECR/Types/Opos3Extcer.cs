using ExtECRMainLogic.Classes.Extenders;
using ExtECRMainLogic.Classes.Helpers;
using ExtECRMainLogic.Classes.Loggers;
using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Enumerators.OPOS3;
using ExtECRMainLogic.Exceptions;
using ExtECRMainLogic.Hubs;
using ExtECRMainLogic.Models.CommunicationModels;
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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExtECRMainLogic.Classes.ExtECR.Types
{
    /// <summary>
    /// OPOS3 Instance
    /// </summary>
    public class Opos3Extcer : FiscalManager
    {
        #region Properties
        /// <summary>
        /// 9600; 230400; 115200;
        /// </summary>
        private const int intBaudRate = 115200;
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
        /// true if port is open
        /// </summary>
        Boolean PortOpened;
        /// <summary>
        /// Format and Send Commands to an OPOS3 fiscal devise through COM Port
        /// </summary>
        MATCMD OPOS3;
        /// <summary>
        /// 
        /// </summary>
        int ErrorCode;
        /// <summary>
        /// 
        /// </summary>
        decimal total;
        /// <summary>
        /// 
        /// </summary>
        private PrintResultModel printResult;
        /// <summary>
        /// 
        /// </summary>
        StreamReader streamToPrint;
        /// <summary>
        /// 
        /// </summary>
        private Font printFont;
        /// <summary>
        /// Server communication model
        /// </summary>
        private ServerCommunicationModel serverCommunication;
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
        private readonly ILogger<Opos3Extcer> logger;
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
        public Opos3Extcer(Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>> printerTemplatesList, List<Printer> printerEscList, List<KitchenPrinterModel> availablePrinters, InstallationDataModel instData, string fiscName, ServerCommunicationModel serverCommunication, string applicationPath, IConfiguration configuration, IApplicationBuilder applicationBuilder)
        {
            // instance identification
            this.InstanceID = ExtcerTypesEnum.Opos3;
            // initialization of variable to be used with lock on printouts
            this.thisLock = new object();
            this.PrintersTemplates = printerTemplatesList;
            this.PrintersEscList = printerEscList;
            this.availablePrinters = availablePrinters;
            this.InstallationData = instData;
            this.FiscalName = fiscName;
            this.serverCommunication = serverCommunication;
            this.applicationPath = applicationPath;
            this.configuration = configuration;
            this.CashAccountIds = new List<int>();
            this.CCAccountIds = new List<int>();
            this.CreditAccountIds = new List<int>();
            this.PortOpened = false;
            this.OPOS3 = new MATCMD();
            this.ErrorCode = 0;
            this.total = 0;
            this.logger = (ILogger<Opos3Extcer>)applicationBuilder.ApplicationServices.GetService(typeof(ILogger<Opos3Extcer>));
            this.localHubInvoker = (LocalHubInvoker)applicationBuilder.ApplicationServices.GetService(typeof(LocalHubInvoker));
            setupAccountIds();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Opos3Extcer()
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
            if (string.IsNullOrEmpty(receiptModel.InvoiceIndex) || receiptModel.InvoiceIndex == "1" || receiptModel.InvoiceType == 7)
            {
                // print to OPOS3
                return PrintOpos3Receipt(receiptModel, fiscalName);
            }
            else
            {
                // print to generic
                logger.LogInformation(">>> Using Generic Printer instead of OPOS3...");
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
                receiptModel.FiscalType = FiscalTypeEnum.Generic;
                // get printer for receipt summary
                KitchenPrinterModel printerToPrint;
                printerToPrint = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Report);
                if (printerToPrint == null)
                {
                    logger.LogError(ExtcerLogger.Log("Printer NOT FOUND into printers list with Type='Report'. Check Printers Settins, SlotIndex values etc.", FiscalName));
                }
                // get receipt printer template
                RollerTypeReportModel template;
                if (PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Value.ReportName == printerToPrint.TemplateShortName).Value == null)
                {
                    logger.LogError(ExtcerLogger.Log("No PrinterTemplate found with Name=" + (printerToPrint.TemplateShortName ?? "<null>"), FiscalName));
                }
                template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Value.ReportName == printerToPrint.TemplateShortName).Value.FirstOrDefault().Value;
                if (template == null)
                {
                    logger.LogInformation(ExtcerLogger.Log("No PrinterTemplate found", FiscalName));
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
            logger.LogInformation(ExtcerLogger.Log("Checking device availability...", FiscalName));
            CheckDeviceOk();
            printResult = new PrintResultModel();
            printResult.ExtcerType = ExtcerTypesEnum.Opos3;
            printResult.ReceiptType = PrintModeEnum.XReport;
            printResult.Status = PrintStatusEnum.Failed;
            logger.LogInformation(ExtcerLogger.Log("Printing X Report...", FiscalName));
            ErrorCode = OPOS3.PrintXreport();
            printResult.ErrorDescription = LogOpos3Error("at PrintXReport", ErrorCode);
            if (ErrorCode != 0)
            {
                logger.LogError(ExtcerLogger.Log("PrintXReport Failure", FiscalName));
                printResult.Status = PrintStatusEnum.Failed;
                return printResult;
            }
            else
            {
                printResult.Status = PrintStatusEnum.Printed;
            }
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
            logger.LogInformation(ExtcerLogger.Log("Checking device availability...", FiscalName));
            CheckDeviceOk();
            // append ZLogger file
            string zlogger = configuration.GetValue<string>("ZLoggerFile").Trim();
            if (zlogger != "")
            {
                logger.LogInformation(ExtcerLogger.Log("Appending ZLogger file...", FiscalName));
                string newLine = OPOS3.ReadDailyTotals();
                newLine = DateTime.Now.ToString("yyyy-MM-dd") + "/" + zData.PosInfoId.ToString() + "/" + newLine;
                ApendZLogger(zlogger, newLine);
                newLine = (zData.PosInfoId.ToString() + "/" + OPOS3.ReadDailyTotals()).Replace("/", "_");
                newLine = newLine.Replace(".", "$");
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                string ApplicationVersion = fvi.FileVersion;
                InsertZlogger(newLine);
            }
            CheckDeviceOk();
            printResult = new PrintResultModel();
            printResult.ExtcerType = ExtcerTypesEnum.Opos3;
            printResult.ReceiptType = PrintModeEnum.ZReport;
            printResult.Status = PrintStatusEnum.Failed;
            printResult.ReceiptNo = zData.ReportNo.ToString();
            printResult.ReceiptData = GetZText(zData);
            ErrorCode = OPOS3.PrintZreport(FiscalName, InstallationData.JournalPath);
            printResult.ErrorDescription = LogOpos3Error("at PrintZreport", ErrorCode);
            if (ErrorCode != 0)
            {
                printResult.Status = PrintStatusEnum.Failed;
            }
            else
            {
                printResult.Status = PrintStatusEnum.Printed;
                // get receipt no
                int intTmp, intReceiptNo;
                string strTmp1, strTmp2, strTmp3, strTmp4;
                ErrorCode = OPOS3.GetFiscalDeviceLastPrintsInfo(FiscalName, out intReceiptNo, out strTmp1, out strTmp2, out intTmp, out strTmp3, out strTmp4);
                LogOpos3Error("at GetZreportNo", ErrorCode);
                printResult.ReceiptNo = intReceiptNo.ToString();
                logger.LogInformation(ExtcerLogger.Log("ZReport is Printed.", FiscalName));
            }
            CloseFiscal();
            return printResult;
        }

        /// <summary>
        /// Get Z total interface
        /// </summary>
        /// <returns></returns>
        public override PrintResultModel GetZTotal()
        {
            logger.LogInformation("In Opos3ExtECR GetZTotals");
            printResult = new PrintResultModel();
            printResult.ExtcerType = ExtcerTypesEnum.Opos3;
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
            logger.LogInformation("Starting 'PrintGraphic' within Opos3Extcer.");
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
            return;
        }

        /// <summary>
        /// Open drawer interface
        /// </summary>
        public override void OpenDrawer()
        {
            CheckDeviceOk();
            if ((ErrorCode = OPOS3.OpenDrawer()) != 0)
            {
                LogOpos3Error("at OpenDrawer", ErrorCode);
            }
            CloseFiscal();
            return;
        }

        /// <summary>
        /// Printer connectivity interface
        /// </summary>
        public override void PrinterConnectivity()
        {
            bool result = false;
            short StatusCode;
            int tries = 0;
            int totaltries = 2;
            int totalStatusCode = 0;
            if (!PortOpened)
                OpenFiscal();
            do
            {
                result = OPOS3.IsDeviceStatusOK(++tries, out StatusCode, totaltries);
                totalStatusCode = totalStatusCode + StatusCode;
                if (StatusCode == 999 && !result)
                {
                    // Port is closed. Open it.
                    CloseFiscal();
                    Thread.Sleep(600);
                    OpenFiscal();
                }
                else if (StatusCode == 256 && !result)
                {
                    logger.LogError(" PrinterConnectivity failed with error code 256. Throwing exception... ");
                    throw new PrinterConnectivityException("Printer is not available.", true);
                }
            } while (!result && tries < totaltries);
            if (!result)
            {
                logger.LogError("PrinterConnectivity Checker reached try limit. Throwing exception... ");
                throw new PrinterConnectivityException("Printer is not available.", (256 * totaltries) == totalStatusCode);
            }
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
        /// Open the selected COM port and Read the VAT rates that have been set in the fiscal device memory. Set PortOpened = true if devide is open.
        /// </summary>
        private void OpenFiscal()
        {
            logger.LogInformation(ExtcerLogger.Log(" ", FiscalName));
            logger.LogDebug(ExtcerLogger.Log("=================== " + FiscalName + " ===================", FiscalName));
            logger.LogInformation(ExtcerLogger.Log("OPOS3: Initializing device! Opening Serial... -> " + InstallationData.Opos3Com, FiscalName));
            if (OPOS3 != null && OPOS3.SerialCM == null)
                OPOS3.SerialCM = new Serial();
            int opos3ReadBufferSize = configuration.GetValue<int>("OPOS3ReadBufferSize");
            int opos3WriteBufferSize = configuration.GetValue<int>("OPOS3WriteBufferSize");
            PortOpened = OPOS3.SerialCM.OpenPort(InstallationData.Opos3Com, intBaudRate, System.IO.Ports.Handshake.None, opos3ReadBufferSize, opos3WriteBufferSize);
            if (!PortOpened)
            {
                logger.LogError(ExtcerLogger.logErr("OPOS3: Opening Serial Failed", FiscalName));
                return;
            }
            logger.LogInformation(ExtcerLogger.Log("OPOS3: Serial is opened", FiscalName));
        }

        /// <summary>
        /// Close com port
        /// </summary>
        private void CloseFiscal()
        {
            try
            {
                logger.LogInformation(ExtcerLogger.Log("OPOS3: Closing Serial", FiscalName));
                OPOS3.SerialCM.ClosePort();
                PortOpened = false;
            }
            catch
            {
            }
        }

        /// <summary>
        /// Check the device status. If device is not opened then open it and and Read the VAT rates (reinitializition).
        /// </summary>
        private void CheckDeviceOk()
        {
            if (!PortOpened)
                OpenFiscal();
        }

        /// <summary>
        /// Read VAT rates. Results are stored into array VATS.
        /// </summary>
        private void ReadVatRates()
        {
            logger.LogInformation(ExtcerLogger.Log("OPOS3: Reading VAT Rates...", FiscalName));
            OPOS3.GetVatRates();
            logger.LogInformation(ExtcerLogger.Log("OPOS3: VAT Rates: " + VatsToString(), FiscalName));
        }

        /// <summary>
        /// Return the stored VATS as string
        /// </summary>
        /// <returns></returns>
        private string VatsToString()
        {
            string str = "";
            for (int i = 0; i < OPOS3.VATS.Length; i++)
            {
                str = str + OPOS3.VATS[i].ToString() + " ";
            }
            return str;
        }

        /// <summary>
        /// Print receipt's header for a print without ADHME
        /// </summary>
        /// <param name="receiptModel"></param>
        private void tempPrintHeader(ReceiptModel receiptModel)
        {
            logger.LogInformation(ExtcerLogger.Log("Printing temp...", FiscalName));
            ErrorCode = OPOS3.PrintLine(receiptModel.DepartmentTypeDescription);
            if (ErrorCode != 0)
                printResult.ErrorDescription = LogOpos3Error("at tempPrintHeader item (1) ", ErrorCode);
            ErrorCode = OPOS3.PrintLine(" ");
            ErrorCode = OPOS3.PrintLine(DateTime.Now.ToString("dd-MM-yyyy HH:dd"));
            if (ErrorCode != 0)
                printResult.ErrorDescription = LogOpos3Error("at tempPrintHeader item (2) ", ErrorCode);
            ErrorCode = OPOS3.PrintLine("TAMEIO " + receiptModel.Pos.ToString() + "   ΤΑΜΕΙΑΣ " + receiptModel.WaiterNo);
            if (ErrorCode != 0)
                printResult.ErrorDescription = LogOpos3Error("at tempPrintHeader item (3) ", ErrorCode);
            ErrorCode = OPOS3.PrintLine(" ");
            printResult.ErrorDescription = LogOpos3Error("at tempPrintHeader item (4) ", ErrorCode);
        }

        /// <summary>
        /// Print an item (normal item, item discount, canceled item, ALLAGI PROIONTOS) into OPOS3 printer
        /// </summary>
        /// <param name="item"></param>
        /// <param name="maxPrintableChars"></param>
        /// <param name="total"></param>
        /// <param name="printResult"></param>
        /// <param name="receiptModel"></param>
        private void printReceiptItem(ReceiptItemsModel item, int? maxPrintableChars, ref decimal total, PrintResultModel printResult, ReceiptModel receiptModel)
        {
            // get item's second line (additional info)
            string additionalInfo = getItemAdditionalInfo(item, receiptModel);
            // get item's desctiption
            string descr = getItemDescription(item, maxPrintableChars);
            descr = descr.Replace("\t", " ");
            descr = new string(descr.Where(c => !char.IsPunctuation(c)).ToArray());
            if (receiptModel.TempPrint)
            {
                decimal price = 0.0M;
                if (item.IsVoid || item.IsChangeItem)
                {
                    price = -item.ItemPrice * item.ItemQty;
                    ErrorCode = OPOS3.PrintLine("-- ΑΚΥΡΩΣΗ ΕΙΔΟΥΣ --");
                }
                else if (item.ItemPrice > 0)
                {
                    price = item.ItemPrice * item.ItemQty;
                    logger.LogInformation(ExtcerLogger.Log("ITEM --------> " + item.ItemDescr + " QTY: " + item.ItemQty + " Price: " + item.ItemPrice, FiscalName));
                }
                ErrorCode = OPOS3.PrintLine(descr + "  " + price.ToString("#0.00") + "   " + item.ItemVatDesc + "%");
                printResult.ErrorDescription = LogOpos3Error("at TempPrint item", ErrorCode);
                total = total + price;
                if (additionalInfo != string.Empty)
                {
                    ErrorCode = OPOS3.PrintLine(additionalInfo);
                    printResult.ErrorDescription = LogOpos3Error("at TempPrint item (additionalInfo)", ErrorCode);
                }
            }
            else
            {
                if (item.IsVoid)
                {
                    logger.LogInformation(ExtcerLogger.Log("VOID ITEM --------> " + item.ItemDescr + " QTY: " + item.ItemQty + " Price: " + item.ItemPrice, FiscalName));
                    total = total - (item.ItemPrice * item.ItemQty);
                    ErrorCode = OPOS3.PrintRecItemVoid(descr, additionalInfo, item.ItemQty, item.ItemVatDesc, item.ItemPrice);
                    printResult.ErrorDescription = LogOpos3Error("at Void Item", ErrorCode);
                    foreach (ReceiptExtrasModel extra in item.Extras)
                    {
                        if (extra.ItemPrice > 0)
                        {
                            total = total - (extra.ItemPrice ?? 0) * extra.ItemQty;
                            ErrorCode = OPOS3.PrintRecRefund(extra.ItemDescr, string.Empty, extra.ItemQty, extra.ItemVatDesc, extra.ItemPrice ?? 0);
                            printResult.ErrorDescription = LogOpos3Error("at Void Extra", ErrorCode);
                        }
                    }
                }
                else if (item.IsChangeItem)
                {
                    total = total - (item.ItemGross * item.ItemQty * -1);
                    logger.LogInformation(ExtcerLogger.Log("CHANGE ITEM --------> " + item.ItemDescr + " QTY: " + item.ItemQty + " Price: " + item.ItemPrice, FiscalName));
                    ErrorCode = OPOS3.PrintRecRefund(descr, additionalInfo, item.ItemQty, item.ItemVatDesc, Math.Abs(item.ItemGross));
                    printResult.ErrorDescription = LogOpos3Error("at PrintRec refund item", ErrorCode);
                }
                else if (item.ItemPrice > 0)
                {
                    total = total + (item.ItemTotal + Convert.ToDecimal(item.ItemDiscount));
                    logger.LogInformation(ExtcerLogger.Log("ITEM --------> " + item.ItemDescr + " QTY: " + item.ItemQty + " Price: " + item.ItemPrice, FiscalName));
                    ErrorCode = OPOS3.PrintRecItem(descr, additionalInfo, item.ItemQty, item.ItemVatDesc, item.ItemPrice);
                    printResult.ErrorDescription = LogOpos3Error("at PrintRec item", ErrorCode);
                    logger.LogInformation(ExtcerLogger.Log("ITEM's printing process ends.", FiscalName));
                }
                if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                {
                    printResult.Status = PrintStatusEnum.Failed;
                    CloseFiscal();
                }
            }
        }

        /// <summary>
        /// Print item's discount
        /// </summary>
        /// <param name="item"></param>
        /// <param name="maxPrintableChars"></param>
        /// <param name="total"></param>
        /// <param name="printResult"></param>
        /// <param name="receiptModel"></param>
        private void printItemDiscount(ReceiptItemsModel item, int? maxPrintableChars, ref decimal total, PrintResultModel printResult, ReceiptModel receiptModel)
        {
            if (item.ItemDiscount != null && item.ItemDiscount > 0)
            {
                decimal discount = (decimal)item.ItemDiscount * item.ItemQty;
                total = total - discount;
                string discountDescription = "Έκπτωση " + item.ItemDescr;
                discountDescription = new string(discountDescription.Where(c => !char.IsPunctuation(c)).ToArray());
                if (maxPrintableChars != null && discountDescription.Length > maxPrintableChars)
                {
                    discountDescription = discountDescription.Substring(0, (int)maxPrintableChars - 1);
                }
                if (!receiptModel.TempPrint)
                {
                    ErrorCode = OPOS3.PrintRecItemAdjustment(AdjustmentType.SalesDiscount, (decimal)item.ItemDiscount, discountDescription, item.ItemVatDesc);
                    printResult.ErrorDescription = LogOpos3Error("at PrintRec discount item: " + discountDescription, ErrorCode);
                }
                else
                {
                    ErrorCode = OPOS3.PrintLine(discountDescription + " " + discount.ToString("#.00") + " " + item.ItemVatValue.ToString("#.00") + "%");
                    printResult.ErrorDescription = LogOpos3Error("at TempPrint discount item: " + discountDescription, ErrorCode);
                }
                logger.LogInformation(ExtcerLogger.Log("Item DISCOUNT : " + (decimal)discount, FiscalName));
            }
        }

        /// <summary>
        /// Print item's extra
        /// </summary>
        /// <param name="item"></param>
        /// <param name="maxPrintableChars"></param>
        /// <param name="total"></param>
        /// <param name="printResult"></param>
        /// <param name="receiptModel"></param>
        private void printExtra(ReceiptItemsModel item, int? maxPrintableChars, ref decimal total, PrintResultModel printResult, ReceiptModel receiptModel)
        {
            if (item.Extras.Count > 0)
            {
                //if PrintType == PrintWhole then print all extras
                if (receiptModel.PrintType == PrintType.PrintWhole)
                {
                    foreach (var extra in item.Extras)
                    {
                        PrintItemExtras(extra, maxPrintableChars, receiptModel, ref total);
                    }
                }
                //if PrintType != PrintWhole then print only the last extra in the item
                if (receiptModel.PrintType == PrintType.PrintExtra)
                {
                    PrintItemExtras(item.Extras[item.Extras.Count - 1], maxPrintableChars, receiptModel, ref total);
                }
            }
        }

        /// <summary>
        /// Print dicounts, totals, etc....
        /// </summary>
        /// <param name="fiscalName"></param>
        /// <param name="maxPrintableChars"></param>
        /// <param name="total"></param>
        /// <param name="printResult"></param>
        /// <param name="receiptModel"></param>
        private void printReceiptEnd(string fiscalName, int? maxPrintableChars, ref decimal total, PrintResultModel printResult, ReceiptModel receiptModel)
        {
            // print total receipt discount
            printReceiptDiscount(printResult, receiptModel);
            logger.LogInformation(ExtcerLogger.Log("------------ END RECEIPT -------------\r\n", FiscalName));
            // get receipt number
            printResult.ReceiptNo = GetReceiptNumber();
            // print subtotal
            if (!receiptModel.TempPrint)
            {
                ErrorCode = OPOS3.PrintSubtotal();
            }
            else
            {
                ErrorCode = OPOS3.PrintLine(" ");
                ErrorCode = OPOS3.PrintLine("ΜΕΡΙΚΟ ΣΥΝΟΛΟ" + " " + total.ToString("#.00"));
                ErrorCode = OPOS3.PrintLine(" ");
            }
            printResult.ErrorDescription = LogOpos3Error("at PrintSubtotal", ErrorCode);
            if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
            {
                printResult.Status = PrintStatusEnum.Failed;
                printResult.ReceiptNo = receiptModel.ReceiptNo;
                CloseFiscal();
                return;
            }
            var voucher = receiptModel.PaymentsList.Where(f => (int)f.AccountType == 6).FirstOrDefault();
            if (voucher != null && !receiptModel.TempPrint)
            {
                logger.LogInformation("Voucher - Tichet Restaurant");
                ErrorCode = OPOS3.PrintRecNotPaid((decimal)voucher.Amount, voucher.Description);
                printResult.ErrorDescription = LogOpos3Error("at Voucher", ErrorCode);
                total = total - (decimal)voucher.Amount;
                if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                {
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ReceiptNo = receiptModel.ReceiptNo;
                    CloseFiscal();
                    logger.LogInformation(ExtcerLogger.Log("Total is: " + total, FiscalName));
                    return;
                }
            }
            if (receiptModel.PaymentsList.Count > 0)
            {
                foreach (var pw in receiptModel.PaymentsList.Where(p => p != voucher))
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
                            if (!receiptModel.TempPrint)
                                ErrorCode = OPOS3.PrintRecTotal(PaymentType.Cash, blnIsLastItem ? Math.Max((decimal)pw.Amount, decimal.Parse(receiptModel.CashAmount ?? "0", CultureInfo.InvariantCulture)) : Math.Max((decimal)pw.Amount, decimal.Parse(receiptModel.CashAmount ?? "0", CultureInfo.InvariantCulture)), pw.Description ?? "Μετρητά");
                            else
                                ErrorCode = OPOS3.PrintLine((pw.Description ?? "Μετρητά") + "   " + total.ToString("#.00"), 4);
                        }
                        else if (InstallationData.OposMethodOfPaymentCC != null && CCAccountIds.Contains((int)pw.AccountType))
                        {
                            // This is credit card type handling
                            if (!receiptModel.TempPrint)
                                ErrorCode = OPOS3.PrintRecTotal(PaymentType.Cards, blnIsLastItem ? 0 : pw.Amount ?? 0, pw.Description ?? "Πιστωτικές Κάρτες");
                            else
                                ErrorCode = OPOS3.PrintLine((pw.Description ?? "Πιστωτικές Κάρτες") + "   " + total.ToString("#.00"), 4);
                        }
                        else if (InstallationData.OposMethodOfPaymentCREDIT != null && CreditAccountIds.Contains((int)pw.AccountType))
                        {
                            // This is credit type handling
                            if (!receiptModel.TempPrint)
                                ErrorCode = OPOS3.PrintRecTotal(PaymentType.Credit, blnIsLastItem ? 0 : pw.Amount ?? 0, pw.Description ?? "CREDIT");
                            else
                                ErrorCode = OPOS3.PrintLine((pw.Description ?? "CREDIT") + "   " + total.ToString("#0.00"), 4);
                        }
                        else if ((int)pw.AccountType == 6)
                        {
                            // This is ticket restaurant / voucher type handling
                        }
                        else
                        {
                            if (!receiptModel.TempPrint)
                                ErrorCode = OPOS3.PrintRecTotal(PaymentType.Cash, blnIsLastItem ? 0 : pw.Amount ?? 0, pw.Description ?? "Μετρητά");
                            else
                                ErrorCode = OPOS3.PrintLine((pw.Description ?? "Μετρητά") + "   " + total.ToString("#.00"), 4);
                        }
                    }
                    total = total - (pw.Amount ?? 0);
                }
            }
            else
            {
                logger.LogInformation(ExtcerLogger.Log("NO PAYMENT TYPE FOUND -- PRINTING AS CASH", FiscalName));
                if (!receiptModel.TempPrint)
                    ErrorCode = OPOS3.PrintRecTotal(PaymentType.Cash, 0, "Μετρητά");
                else
                    ErrorCode = OPOS3.PrintLine("Μετρητά" + "   " + total.ToString("#.00"), 4);
                if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                {
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ReceiptNo = receiptModel.ReceiptNo;
                    CloseFiscal();
                    return;
                }
            }
            LogOpos3Error("at PrintRecTotal", ErrorCode);
            if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
            {
                printResult.Status = PrintStatusEnum.Failed;
                printResult.ReceiptNo = receiptModel.ReceiptNo;
                CloseFiscal();
                return;
            }
            if (receiptModel.TempPrint)
            {
                int printLineFeed = configuration.GetValue<int>("TempPrintLineFeed");
                OPOS3.LineFeed(printLineFeed);
                OPOS3.Cutter();
                OPOS3.OpenDrawer();
                return;
            }
            if (CloseReceiptValidationOk(ErrorCode))
            {
            }
            if (receiptModel.IsVoid)
            {
                ErrorCode = OPOS3.PrintRecVoid("");
                printResult.ErrorDescription = LogOpos3Error("Canceling receipt", ErrorCode);
                if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                {
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ReceiptNo = receiptModel.ReceiptNo;
                    CloseFiscal();
                    return;
                }
            }
            else
            {
                printResult.ReceiptNo = receiptModel.ReceiptNo = GetReceiptNumber();
                // print receipt comments
                PrintComments(receiptModel);
                // print receipt footer
                PrintFooters(receiptModel);
            }
            // send a command to the OPOS3 fiscal device to close an open transaction/receipt
            ErrorCode = OPOS3.EndFiscalReceipt();
            OPOS3.blnEndFiscalReceiptOk = (ErrorCode == 0) ? true : false;
            printResult.ErrorDescription = LogOpos3Error("at EndFiscalReceipt", ErrorCode);
            if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
            {
                printResult.Status = PrintStatusEnum.Failed;
                printResult.ReceiptNo = receiptModel.ReceiptNo;
                CloseFiscal();
                return;
            }
            if (CloseReceiptValidationOk(ErrorCode))
            {
                OPOS3.CloseTransaction();
                // move receipt to journal
                if (!OPOS3.ReadDeviceFlashMemoryFiles(fiscalName, InstallationData.JournalPath))
                {
                    logger.LogInformation(ExtcerLogger.Log("In Receipt. Could not empty journal", FiscalName));
                }
                else
                {
                    printResult.Status = PrintStatusEnum.Printed;
                }
            }
            printResult.OrderNo = receiptModel.OrderNo;
            printResult.ExtcerType = ExtcerTypesEnum.Opos3;
            printResult.ReceiptType = PrintModeEnum.Receipt;
            ArtcasLogger.LogArtCasXML(receiptModel, DateTime.Now, LogOpos3Error(" ", ErrorCode), new List<string>(), ReceiptReceiveTypeEnum.WEB, printResult.Status, fiscalName, printResult.Id);
        }

        /// <summary>
        /// Print total receipt discount
        /// </summary>
        /// <param name="printResult"></param>
        /// <param name="receiptModel"></param>
        private void printReceiptDiscount(PrintResultModel printResult, ReceiptModel receiptModel)
        {
            total = total - (decimal)receiptModel.TotalDiscount;
            if (receiptModel.TotalDiscount != null && receiptModel.TotalDiscount > 0 && (decimal)receiptModel.TotalDiscount > 0.01m)
            {
                ErrorCode = OPOS3.PrintRecItemAdjustment(AdjustmentType.SubtotalDiscount, (decimal)receiptModel.TotalDiscount, "Eκπτωση", "1");
                printResult.ErrorDescription = LogOpos3Error("at print total receipt discount ", ErrorCode);
                logger.LogInformation(ExtcerLogger.Log("RECEIPT DISCOUNT : " + (decimal)receiptModel.TotalDiscount, FiscalName));
                if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                {
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ReceiptNo = receiptModel.ReceiptNo;
                    CloseFiscal();
                }
            }
        }

        /// <summary>
        /// Print receipt comments.
        /// Comments must be EXCACTLY 3 rows, with format: "m/text_line_1/text_line_2/text_line_3/" ???
        /// </summary>
        /// <param name="receiptModel"></param>
        private void PrintComments(ReceiptModel receiptModel)
        {
            int? intMaxPrintableChars = InstallationData.OposMaxString;
            // get comments text list
            List<string> comments = GetReceiptText(receiptModel, PrinterTypeEnum.Receipt).Take(3).ToList();
            if (comments.Count > 0)
            {
                while (comments.Count < 3)
                {
                    logger.LogInformation(ExtcerLogger.Log("OPOS3: Adding an empty comment...", FiscalName));
                    comments.Add("");
                }
                comments[0] = comments[0].Replace('/', '-');
                if (comments.Count >= 2)
                {
                    comments[1] = comments[1].Replace('/', '-');
                    if (comments.Count >= 3)
                    {
                        comments[2] = comments[2].Replace('/', '-');
                    }
                }
                // create the string to print
                string result = comments.Aggregate(new StringBuilder(""), (current, next) => current.Append(next)).ToString();
                // print comment lines
                ErrorCode = OPOS3.PrintFooterComments(comments[0], comments[1], comments[2], intMaxPrintableChars);
                printResult.ErrorDescription = LogOpos3Error("at print receipt comments ", ErrorCode);
                if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                {
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ReceiptNo = receiptModel.ReceiptNo;
                    CloseFiscal();
                }
            }
        }

        /// <summary>
        /// Print receipt footer (Y command) (footer is defined in confog file).
        /// </summary>
        /// <param name="receiptModel"></param>
        private void PrintFooters(ReceiptModel receiptModel)
        {
            int? intMaxPrintableChars = InstallationData.OposMaxString;
            // get comments text list
            string footer = configuration.GetValue<string>("FooterOpos3");
            if (footer == "")
                return;
            // print comment lines here
            ErrorCode = OPOS3.PrintFooter(footer);
            printResult.ErrorDescription = LogOpos3Error("at print receipt comments ", ErrorCode);
            if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
            {
                printResult.Status = PrintStatusEnum.Failed;
                printResult.ReceiptNo = receiptModel.ReceiptNo;
                CloseFiscal();
            }
        }

        /// <summary>
        /// In case of error condition (parameter value not zero), issue a command to cancel the currently open receipt.
        /// Returns true if no error condition in the entry parameter (parameter value is zero), else false.
        /// </summary>
        /// <param name="resCode">The result (error) code from previous OPOS3 command call.</param>
        /// <returns></returns>
        private bool CloseReceiptValidationOk(int resCode)
        {
            if (resCode != 0)
            {
                logger.LogInformation(ExtcerLogger.Log("CANCELING RECEIPT", FiscalName));
                if (OPOS3.PrintRecVoid("") != 0)
                {
                    logger.LogInformation(ExtcerLogger.Log("CANCELING RECEIPT FAILED!", FiscalName));
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get the number of the latest issued receipt.
        /// </summary>
        /// <returns></returns>
        private string GetReceiptNumber()
        {
            int intTmp, intLastReceiptNo;
            string strTmp1, strTmp2, strTmp3, strTmp4;
            ErrorCode = OPOS3.GetFiscalDeviceLastPrintsInfo(FiscalName, out intTmp, out strTmp1, out strTmp2, out intLastReceiptNo, out strTmp3, out strTmp4);
            LogOpos3Error("at GetReceiptNo", ErrorCode);
            return intLastReceiptNo.ToString();
        }

        /// <summary>
        /// Append the ZLogger file with the daily totals
        /// </summary>
        /// <param name="file"></param>
        /// <param name="newLine">
        /// every line contains the data:
        /// Date/PosId/Daily VAT A/Daily VAT B/Daily VAT C/Daily VAT D/Daily VAT E/Daily total/Legal receipts total/Illegal receipts total/Voids total/Refunds total/Cancels total/CASH(Type of payment)/CARD(Type of payment)/CREDIT(Type of payment)/Total of Amount Discounts/Total of Amount Markups/Total of Subtotal Discounts/Total of Subtotal Amount Markups
        /// ex:2017-07-17/10/0.00/19.89/0.00/0.00/0.00/19.89/2/3/0.00/0.00/0.00/18.50/0.00/1.39/0.00/1.38/0.00/0.00
        /// </param>
        private void ApendZLogger(string file, string newLine)
        {
            try
            {
                using (StreamWriter w = File.AppendText(file))
                {
                    w.WriteLine(newLine);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.Log("ZLogger file Failure: " + exception.ToString(), FiscalName));
            }
        }

        /// <summary>
        /// Append to log file the latest OPOS error. Returns the error description.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="resCode"></param>
        /// <param name="blnCancelNeeded"></param>
        /// <returns></returns>
        private string LogOpos3Error(string message, int resCode, bool blnCancelNeeded = true)
        {
            if (resCode == 0)
                return string.Empty;
            if (resCode == -10)
                return "Vat Value did not find into printer's VAT list";
            // get last error recorded
            string strSuggestedAction;
            string strErrorMessageToPOS = OPOS3.GetLastError(out strSuggestedAction);
            if (strErrorMessageToPOS.Contains("Field too long"))
                strErrorMessageToPOS = strErrorMessageToPOS + "  -->>  TRY redusing the Max length in 'Installation Data' <<--";
            if (blnCancelNeeded || resCode == 65 || resCode == 69)
            {
                // cancel last receipt issue
                logger.LogInformation(ExtcerLogger.Log("CANCELING RECEIPT", FiscalName));
                OPOS3.PrintRecCancel();
                OPOS3.CloseTransaction();
            }
            else if (resCode == 61)
            {
                ErrorCode = OPOS3.PrintZreport(FiscalName, InstallationData.JournalPath);
                if (ErrorCode == 0)
                    return string.Empty;
                else
                    printResult.ErrorDescription = LogOpos3Error("at PrintZreport", ErrorCode);
            }
            else
            {
                if (resCode == 256)
                {
                    strErrorMessageToPOS = "OPOS3 device communication problem.";
                    strSuggestedAction = "Check cables/COM port settings/driver settings and retry.";
                }
            }
            if (printResult != null)
            {
                printResult.ErrorDescription = strErrorMessageToPOS + "\t \r\n" + strSuggestedAction;
                return printResult.ErrorDescription;
            }
            return ExtcerErrorHelper.GetErrorDescription(ExtcerErrorHelper.GENERAL_ERROR);
        }

        /// <summary>
        /// Insert zlogger to endofday table
        /// </summary>
        /// <param name="zlogger"></param>
        private void InsertZlogger(string zlogger)
        {
            string result = "";
            try
            {
                int loop = 0;
                do
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(serverCommunication.connectionUrl);
                        AuthenticationHeaderValue authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(new ASCIIEncoding().GetBytes(string.Format("{0}:{1}", serverCommunication.authorizationUsername, serverCommunication.authorizationPassword))));
                        client.DefaultRequestHeaders.Authorization = authHeader;
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        string request = "api/v3/EndOfDay/InsertZlogger/" + zlogger;
                        logger.LogInformation("Requesting Server: " + request);
                        HttpResponseMessage response = client.GetAsync(request).Result;
                        result = response.Content.ReadAsStringAsync().Result;
                    }
                    if (!result.Contains("An error has occurred"))
                    {
                        loop = 1000;
                    }
                    else
                    {
                        loop++;
                        logger.LogInformation("Server Returned Error: " + result.Substring(0, 130) + "...");
                        if (loop <= 3)
                            Thread.Sleep(1200 * loop);
                    }
                } while (loop <= 3);
                zlogger = zlogger.Replace("_", "/");
                zlogger = zlogger.Replace("$", ".");
                //TODO GEO
                //Received("Zlogger:" + zlogger.Replace("_", "/") + ':' + result, null, this);
            }
            catch (Exception exception)
            {
                logger.LogError("at OnHub_InsertZlogger. Error: " + exception.ToString());
            }
        }

        /// <summary>
        /// Initialize a new PrintResultModel
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <returns></returns>
        private PrintResultModel initPrintResultModel(ReceiptModel receiptModel)
        {
            PrintResultModel printResult = new PrintResultModel();
            printResult.InvoiceIndex = receiptModel.InvoiceIndex;
            printResult.ExtcerType = ExtcerTypesEnum.Opos3;
            printResult.ReceiptNo = receiptModel.ReceiptNo;
            receiptModel.FiscalType = FiscalTypeEnum.Opos;
            return printResult;
        }

        /// <summary>
        /// Print Receipt or cancel the current receipt, Case InvoiceIndex = 1.
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="fiscalName"></param>
        /// <returns></returns>
        private PrintResultModel PrintOpos3Receipt(ReceiptModel receiptModel, string fiscalName)
        {
            int? maxPrintableChars = InstallationData.OposMaxString;
            CheckDeviceOk();
            if (receiptModel.PrintType == PrintType.PrintWhole)
                return printWholeReceipt(receiptModel, fiscalName, maxPrintableChars);
            else if (receiptModel.PrintType == PrintType.CancelCurrentReceipt)
                return voidCurrentReceipt(receiptModel);
            else
                return printPartial(receiptModel, fiscalName, maxPrintableChars);
        }

        /// <summary>
        /// Print all sections of the receipt.
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="fiscalName"></param>
        /// <param name="maxPrintableChars"></param>
        /// <returns></returns>
        private PrintResultModel printWholeReceipt(ReceiptModel receiptModel, string fiscalName, int? maxPrintableChars)
        {
            // create new PrintResultModel
            printResult = initPrintResultModel(receiptModel);
            // return the PrintType
            printResult.PrintType = receiptModel.PrintType;
            // read VAT rates
            ReadVatRates();
            try
            {
                // print the void in different printer
                if (receiptModel.IsVoid)
                {
                    printResult = PrintVoidReceipt(receiptModel, fiscalName);
                    CloseFiscal();
                    return printResult;
                }
                // get preview string list
                printResult.ReceiptData = GetReceiptPreviewText(receiptModel);
                logger.LogInformation(ExtcerLogger.Log("============ NEW RECEIPT (whole print) ============", FiscalName));
                //1. initialize new receipt
                logger.LogInformation(ExtcerLogger.Log("InvoiceIndex: " + (receiptModel.InvoiceIndex ?? "null") + ", InvoiceType: " + receiptModel.InvoiceType, FiscalName));
                if (receiptModel.InvoiceIndex != null && receiptModel.InvoiceType == 7)
                {
                    ErrorCode = OPOS3.PrintInvoice(receiptModel.BillingVatNo, receiptModel.CustomerName, receiptModel.BillingJob, receiptModel.BillingName, receiptModel.CustomerDeliveryAddress, receiptModel.BillingDOY, receiptModel.CustomerPhone);
                    printResult.ErrorDescription = LogOpos3Error("at Print Invoice", ErrorCode);
                    if (ErrorCode != 0)
                    {
                        printResult.Status = PrintStatusEnum.Failed;
                        CloseFiscal();
                        return printResult;
                    }
                    logger.LogInformation(ExtcerLogger.Log("------------- RECEIPT #" + receiptModel.ReceiptNo + " -------------", FiscalName));
                }
                else
                {
                    if (!receiptModel.TempPrint)
                    {
                        // send a command to the OPOS3 fiscal device to start a new receipt
                        ErrorCode = OPOS3.BeginFiscalReceipt(true);
                        printResult.ErrorDescription = LogOpos3Error("at BeginFiscalReceipt", ErrorCode, false);
                        if (ErrorCode != 0)
                        {
                            printResult.Status = PrintStatusEnum.Failed;
                            CloseFiscal();
                            return printResult;
                        }
                        logger.LogInformation(ExtcerLogger.Log("------------- RECEIPT #" + receiptModel.ReceiptNo + " -------------", FiscalName));
                    }
                    else
                    {
                        // or print receipt's header for a print without ADHME
                        tempPrintHeader(receiptModel);
                    }
                }
                total = 0;
                //2. print receipt items
                foreach (var item in receiptModel.Details)
                {
                    printReceiptItem(item, maxPrintableChars, ref total, printResult, receiptModel);
                    if (printResult.Status == PrintStatusEnum.Failed)
                        return printResult;
                    printItemDiscount(item, maxPrintableChars, ref total, printResult, receiptModel);
                    if (printResult.Status == PrintStatusEnum.Failed)
                        return printResult;
                    printExtra(item, maxPrintableChars, ref total, printResult, receiptModel);
                    if (printResult.Status == PrintStatusEnum.Failed)
                        return printResult;
                }
                //3. print receipt's last sections like discounts, total, etc...
                printReceiptEnd(fiscalName, maxPrintableChars, ref total, printResult, receiptModel);
            }
            catch (CustomException exception)
            {
                printResult.ErrorDescription = exception.Message;
                logger.LogError(ExtcerLogger.logErr("Error printing OPOS3 receipt (3): #" + printResult.ReceiptNo + "  ERRORDESCR: ", exception, FiscalName));
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("Error printing OPOS3 receipt (4): #" + printResult.ReceiptNo + "  ERRORDESCR: ", exception, FiscalName));
            }
            //4. close com port
            CloseFiscal();
            return printResult;
        }

        /// <summary>
        /// Print receipt's one item only or header or footer
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="fiscalName"></param>
        /// <param name="maxPrintableChars"></param>
        /// <returns></returns>
        private PrintResultModel printPartial(ReceiptModel receiptModel, string fiscalName, int? maxPrintableChars)
        {
            // create new PrintResultModel
            printResult = initPrintResultModel(receiptModel);
            // return the PrintType
            printResult.PrintType = receiptModel.PrintType;
            try
            {
                if (receiptModel.PrintType == PrintType.PrintItem)
                {
                    // start a new receipt if the item into the receiptModel hasItemSort = 0
                    if (receiptModel.Details[0].ItemSort == 0)
                    {
                        // read VAT rates
                        ReadVatRates();
                        total = 0;
                        //1. send a command to the OPOS3 fiscal device to start a new receipt
                        if (!receiptModel.TempPrint)
                        {
                            logger.LogInformation(ExtcerLogger.Log("============ NEW RECEIPT (partial print) ============", FiscalName));
                            ErrorCode = OPOS3.BeginFiscalReceipt(true);
                            printResult.ErrorDescription = LogOpos3Error("at BeginFiscalReceipt", ErrorCode, false);
                            if (ErrorCode != 0)
                            {
                                printResult.Status = PrintStatusEnum.Failed;
                                printResult.ErrorDescription = OPOS3.getErrorDescription(ErrorCode);
                                CloseFiscal();
                                return printResult;
                            }
                        }
                        else
                        {
                            tempPrintHeader(receiptModel);
                        }
                    }
                    //2. print the last item from receiptModel's details list
                    logger.LogInformation(ExtcerLogger.Log("Printing item: #" + (receiptModel.Details[0].ItemSort + 1).ToString(), FiscalName));
                    printReceiptItem(receiptModel.Details[receiptModel.Details.Count() - 1], maxPrintableChars, ref total, printResult, receiptModel);
                    if (printResult.Status == PrintStatusEnum.Failed)
                    {
                        printResult.ErrorDescription = OPOS3.getErrorDescription(ErrorCode);
                        return printResult;
                    }
                }
                // print item's discount
                if (receiptModel.PrintType == PrintType.PrintItemDiscount)
                {
                    printItemDiscount(receiptModel.Details[receiptModel.Details.Count() - 1], maxPrintableChars, ref total, printResult, receiptModel);
                }
                // print item's extra
                if (receiptModel.PrintType == PrintType.PrintExtra)
                {
                    printExtra(receiptModel.Details[receiptModel.Details.Count() - 1], maxPrintableChars, ref total, printResult, receiptModel);
                }
                //3. if the receiptModel's PrintType is marked as 'PrintEnd' then finish printing
                if (receiptModel.PrintType == PrintType.PrintEnd)
                {
                    if (receiptModel.InvoiceIndex != null && receiptModel.InvoiceType == 7)
                    {
                        ErrorCode = OPOS3.PrintInvoice(receiptModel.BillingVatNo, receiptModel.CustomerName, receiptModel.BillingJob, receiptModel.BillingName, receiptModel.CustomerDeliveryAddress, receiptModel.BillingDOY, receiptModel.CustomerPhone);
                        printResult.ErrorDescription = LogOpos3Error("at Print Invoice", ErrorCode);
                        if (ErrorCode != 0)
                        {
                            printResult.Status = PrintStatusEnum.Failed;
                            CloseFiscal();
                            return printResult;
                        }
                        logger.LogInformation(ExtcerLogger.Log("------------- RECEIPT #" + receiptModel.ReceiptNo + " -------------", FiscalName));
                    }
                    // print receipt's last sections like discounts, total, etc...
                    printReceiptEnd(fiscalName, maxPrintableChars, ref total, printResult, receiptModel);
                    // close com port
                    CloseFiscal();
                    // get preview string list
                    printResult.ReceiptData = GetReceiptPreviewText(receiptModel);
                }
            }
            catch (CustomException exception)
            {
                printResult.ErrorDescription = exception.Message;
                logger.LogError(ExtcerLogger.logErr("Error printing OPOS3 receipt (1): #" + printResult.ReceiptNo + ": ", exception, FiscalName));
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("Error printing OPOS3 receipt (2): #" + printResult.ReceiptNo + ": ", exception, FiscalName));
            }
            return printResult;
        }

        /// <summary>
        /// Cancel the current receipt
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <returns></returns>
        private PrintResultModel voidCurrentReceipt(ReceiptModel receiptModel)
        {
            if (!receiptModel.TempPrint)
            {
                logger.LogInformation(ExtcerLogger.Log("Voiding Receipt...", FiscalName));
                ErrorCode = OPOS3.PrintRecVoid("");
                printResult.ErrorDescription = LogOpos3Error("at PrintRec void", ErrorCode, false);
                if (ErrorCode != 0)
                {
                    printResult.Status = PrintStatusEnum.Failed;
                    CloseFiscal();
                    return printResult;
                }
            }
            else
            {
                ErrorCode = OPOS3.PrintLine("------- ΑΚΥΡΩΣΗ ----------");
                OPOS3.LineFeed(4);
                OPOS3.Cutter();
                OPOS3.OpenDrawer();
            }
            printResult.ErrorDescription = LogOpos3Error("at PrintRec void ", ErrorCode);
            return printResult;
        }

        /// <summary>
        /// Print one extra.
        /// </summary>
        /// <param name="extra"></param>
        /// <param name="maxPrintableChars"></param>
        /// <param name="receiptModel"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        private PrintResultModel PrintItemExtras(ReceiptExtrasModel extra, int? maxPrintableChars, ReceiptModel receiptModel, ref decimal total)
        {
            string extrdescr = String.IsNullOrEmpty(extra.ItemDescr) ? "    " : "+ " + extra.ItemDescr.Replace('/', '-');
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
                if (extra.IsChangeItem == true)
                {
                    decimal price = (decimal)(extra.ItemGross * extra.ItemQty * -1);
                    total = total - price;
                    if (!receiptModel.TempPrint)
                    {
                        ErrorCode = OPOS3.PrintRecRefund(extrdescr, "", extra.ItemQty, extra.ItemVatDesc, itemGross);
                    }
                    else
                    {
                        ErrorCode = OPOS3.PrintLine(extrdescr + " " + price.ToString("#.00") + " " + extra.ItemVatValue.ToString("#.00") + "%");
                    }
                    printResult.ErrorDescription = LogOpos3Error("at PrintRec discount item", ErrorCode);
                    if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                    {
                        printResult.Status = PrintStatusEnum.Failed;
                        printResult.ReceiptNo = receiptModel.ReceiptNo;
                        CloseFiscal();
                        return printResult;
                    }
                }
                else
                {
                    total = total + (itemPrice * extra.ItemQty);
                    if (extra.ItemQty > 0)
                    {
                        if (!receiptModel.TempPrint)
                        {
                            ErrorCode = OPOS3.PrintRecItem(extrdescr, "", extra.ItemQty, extra.ItemVatDesc, itemPrice);
                        }
                        else
                        {
                            ErrorCode = OPOS3.PrintLine(extrdescr + " " + (itemPrice * extra.ItemQty).ToString("#0.00") + " " + extra.ItemVatValue.ToString("#0.00") + "%");
                        }
                        printResult.ErrorDescription = LogOpos3Error("at PrintRec extras item", ErrorCode);
                        logger.LogInformation(ExtcerLogger.Log("  EXTRA -- Description: " + extra.ItemDescr + " QTY: " + extra.ItemQty + " Price: " + extra.ItemPrice, FiscalName));
                        if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                        {
                            printResult.Status = PrintStatusEnum.Failed;
                            printResult.ReceiptNo = receiptModel.ReceiptNo;
                            Close();
                            return printResult;
                        }
                    }
                }
                if (extra.ItemDiscount != null && extra.ItemDiscount > 0)
                {
                    total = total - (decimal)(extra.ItemDiscount * extra.ItemQty);
                    string extraDiscountDescription = "Έκπτωση " + extrdescr;
                    if (maxPrintableChars != null && extraDiscountDescription.Length > maxPrintableChars)
                    {
                        extraDiscountDescription = extraDiscountDescription.Substring(0, (int)maxPrintableChars - 1);
                    }
                    ErrorCode = OPOS3.PrintRecItemAdjustment(AdjustmentType.SalesDiscount, (decimal)extra.ItemDiscount, extraDiscountDescription, extra.ItemVatDesc);
                    printResult.ErrorDescription = LogOpos3Error("at PrintRec discount extra", ErrorCode);
                    logger.LogInformation(ExtcerLogger.Log("  EXTRA DISCOUNT -- : " + (decimal)extra.ItemDiscount, FiscalName));
                    if (GenericCommon.CheckHasErrors(printResult.ErrorDescription))
                    {
                        printResult.Status = PrintStatusEnum.Failed;
                        printResult.ReceiptNo = receiptModel.ReceiptNo;
                        CloseFiscal();
                        return printResult;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Receipt void is printed in generic printer.
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
                // print receipt
                printResult.OrderNo = receiptModel.OrderId.ToString();
                printResult.ReceiptNo = receiptModel.OrderNo;
                printResult.ExtcerType = ExtcerTypesEnum.Opos3;
                printResult.ReceiptType = PrintModeEnum.Void;
                // get printer for void
                KitchenPrinterModel printersettings;
                printersettings = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Void);
                if (printersettings == null)
                {
                    logger.LogWarning(ExtcerLogger.Log("Void printer NOT FOUND", FiscalName));
                }
                // get receipt printer template
                RollerTypeReportModel template;
                if (PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.Void).Value == null)
                {
                    logger.LogWarning(ExtcerLogger.Log("Void template NOT FOUND", FiscalName));
                }
                template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.Void).Value.FirstOrDefault().Value;
                if (template == null)
                {
                    logger.LogWarning(ExtcerLogger.Log("Void template NOT FOUND", FiscalName));
                }
                printResult.ReceiptData = ProcessVoidReceiptTemplate(template, receiptModel, printersettings);
                printResult.Status = PrintStatusEnum.Printed;
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("Error printing OPOS3 Void: ", exception, FiscalName));
                printResult.Status = PrintStatusEnum.Failed;
                error = "Error printing OPOS3 Void: " + exception.Message + " StackTRace: " + exception.StackTrace;
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
                var recstr = GetReceiptText(receiptModel, PrinterTypeEnum.OposPreview);
                if (recstr.Count > 0)
                    res.AddRange(recstr);
                var recstrcomments = GetReceiptText(receiptModel, PrinterTypeEnum.Receipt);
                if (recstrcomments.Count > 0)
                    res.AddRange(recstrcomments);
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("Error generating OPOS3 preview receipt text", exception, FiscalName));
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
                var printersettings = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Receipt && f.SlotIndex == "1");
                if (printersettings == null)
                {
                    logger.LogError(ExtcerLogger.Log("Receipt printer NOT FOUND into printers list. Check SlotIndex value (must be a printer with SlotIndex=1 for printing receipt) ", FiscalName));
                }
                else
                {
                    logger.LogInformation(ExtcerLogger.Log("Printer for Receipt and SlotIndex=1: " + (printersettings.Name ?? "<null>"), FiscalName));
                }
                // get receipt printer template
                if (PrintersTemplates.FirstOrDefault(f => f.Key == printersettings.TemplateShortName).Value == null)
                {
                    logger.LogWarning(ExtcerLogger.Log("No PrinterTemplate found for Type=" + printersettings.TemplateShortName + ".", FiscalName));
                    return new List<string>();
                }
                var template = PrintersTemplates.FirstOrDefault(f => f.Key == printersettings.TemplateShortName).Value.FirstOrDefault().Value;
                if (template == null)
                {
                    logger.LogWarning(ExtcerLogger.Log("No PrinterTemplate found for Type=" + printersettings.TemplateShortName + ".", FiscalName));
                }
                else
                {
                    logger.LogInformation(ExtcerLogger.Log("Template: " + (template.ReportName ?? "<null>"), FiscalName));
                }
                if (printType == PrinterTypeEnum.OposPreview)
                {
                    commentsStr = ProcessReceiptTemplate(template, currentReceiptData, printersettings, false);
                }
                else
                {
                    commentsStr = ProcessReceiptCommentTemplate(template, currentReceiptData, printersettings);
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
        /// Return Ztext to print
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
                if (zPrinter == null)
                {
                    logger.LogError(ExtcerLogger.Log("ZReport printer NOT FOUND into printers list with  PrinterType = ZReport. Check Printers Settins, SlotIndex values etc.", FiscalName));
                }
                else
                {
                    logger.LogInformation(ExtcerLogger.Log("Printer with  PrinterType = ZReport: " + (zPrinter.Name ?? "<null>"), FiscalName));
                }
                logger.LogInformation(ExtcerLogger.Log("GetZText: z printer PrintersEscList", FiscalName));
                // get receipt printer template
                if (PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.ZReport).Value == null)
                {
                    logger.LogWarning(ExtcerLogger.Log("ZReport template NOT FOUND", FiscalName));
                }
                var template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.ZReport).Value.FirstOrDefault().Value;
                if (template == null)
                {
                    logger.LogWarning(ExtcerLogger.Log("Z template NOT FOUND", FiscalName));
                }
                else
                {
                    logger.LogInformation(ExtcerLogger.Log("Template for Z report: " + (template.ReportName ?? "<null>"), FiscalName));
                }
                // get text to print
                zStr = ProcessZReportTemplate(template, zData, zPrinter);
                return zStr;
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("OPOS3: Error in  GetZText Error: ", exception, FiscalName));
                return zStr;
            }
        }

        /// <summary>
        /// Get the string for item's additional info. (form the second line of an item)
        /// </summary>
        /// <param name="item"></param>
        /// <param name="receiptModel"></param>
        /// <returns></returns>
        private string getItemAdditionalInfo(ReceiptItemsModel item, ReceiptModel receiptModel)
        {
            string additionalInfo = string.Empty;
            if (!string.IsNullOrEmpty(receiptModel.ItemAdditionalInfo))
            {
                additionalInfo = (item.GetType().GetProperty(receiptModel.ItemAdditionalInfo).GetValue(item, null) ?? "").ToString();
            }
            return additionalInfo;
        }

        /// <summary>
        /// Get the string for item's description.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="maxPrintableChars"></param>
        /// <returns></returns>
        private string getItemDescription(ReceiptItemsModel item, int? maxPrintableChars)
        {
            string descr = String.IsNullOrEmpty(item.ItemDescr) ? "    " : item.ItemDescr.Replace('/', '-');
            if (maxPrintableChars != null && item.ItemDescr.Length > maxPrintableChars)
            {
                descr = descr.Substring(0, (int)maxPrintableChars - 1);
            }
            return descr;
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
            KitchenPrinterModel kitchenPrinterModel = new KitchenPrinterModel();
            kitchenPrinterModel = printerSettings;
            if (kitchenPrinterModel.Regions.Count > 0)
            {
                if (kitchenPrinterModel.Regions[0] > 0)
                {
                    long regionId = currentReceipt.RegionId.GetValueOrDefault();
                    if (regionId > 0)
                    {
                        kitchenPrinterModel = availablePrinters.Where(f => f.Name == printerSettings.Name && f.Regions.Contains(regionId)).FirstOrDefault();
                    }
                }
            }
            if (kitchenPrinterModel == null)
            {
                logger.LogError(ExtcerLogger.Log("Printer NOT FOUND into printers list with name='" + (printerSettings.Name ?? "<null>") + "'. Check Printers Settins, SlotIndex values etc.", FiscalName));
            }
            Printer printer = PrintersEscList.Where(f => f.Name == kitchenPrinterModel.EscapeCharsTemplate).FirstOrDefault();
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
                                var price_original = itemData.ItemPrice;
                                itemData.ItemPrice = Math.Abs(itemData.ItemPrice);
                                result.AddRange(ProcessSection(section.SectionRows, itemData, printer));
                                // restore the original amount as positive
                                itemData.ItemPrice = price_original;
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
                        var detailsVat = currentReceipt.Details.Select(f => new Vat
                        {
                            VatRate = f.ItemVatRate,
                            Gross = (f.ItemNet + f.ItemVatValue),
                            Net = f.ItemNet,
                            VatAmount = f.ItemVatValue,
                            VatDesc = f.ItemVatDesc
                        }).AsEnumerable();
                        var extrasVat = currentReceipt.Details.SelectMany(f => f.Extras).Select(w => new Vat
                        {
                            VatRate = w.ItemVatRate,
                            VatAmount = w.ItemVatValue,
                            Gross = (w.ItemNet + w.ItemVatValue),
                            Net = w.ItemNet,
                            VatDesc = w.ItemVatDesc
                        }).AsEnumerable();
                        var union = detailsVat.Union(extrasVat);
                        var groupByVatRate = union.GroupBy(f => f.VatRate).Select(f => new Vat
                        {
                            VatRate = f.Key.Value,
                            Gross = f.Sum(s => s.Gross),
                            Net = f.Sum(s => s.Net),
                            VatAmount = f.Sum(s => s.VatAmount),
                            VatDesc = f.FirstOrDefault().VatDesc
                        });
                        foreach (var item in groupByVatRate)
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
            if ((kitchenPrinterModel.PrintKitchenOnly == null || (bool)kitchenPrinterModel.PrintKitchenOnly == false) && allowPrint)
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
        /// Process void receipt template and print at generic printer.
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentReceipt"></param>
        /// <param name="printerSettings"></param>
        /// <returns></returns>
        private List<string> ProcessVoidReceiptTemplate(RollerTypeReportModel repTemplate, ReceiptModel currentReceipt, KitchenPrinterModel printerSettings)
        {
            List<string> result = new List<string>();
            var receiptPrinter = availablePrinters.Where(f => f.Name == printerSettings.Name).FirstOrDefault();
            if (receiptPrinter == null)
            {
                logger.LogError(ExtcerLogger.Log("Printer NOT FOUND into printers list with name='" + (printerSettings.Name ?? "<null>") + "'. Check Printers Settins, SlotIndex values etc.", FiscalName));
            }
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
        /// Process the receipt comment template.
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentReceipt"></param>
        /// <param name="printerSettings"></param>
        /// <returns></returns>
        private List<string> ProcessReceiptCommentTemplate(RollerTypeReportModel repTemplate, ReceiptModel currentReceipt, KitchenPrinterModel printerSettings)
        {
            List<string> result = new List<string>();
            var receiptPrinter = availablePrinters.Where(f => f.Name == printerSettings.Name).FirstOrDefault();
            if (receiptPrinter == null)
            {
                logger.LogError(ExtcerLogger.Log("Printer NOT FOUND into printers list with name='" + (printerSettings.Name ?? "<null>") + "'. Check Printers Settins, SlotIndex values etc.", FiscalName));
            }
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
        /// Process receipt sum template.
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentReceipt"></param>
        /// <param name="printersettings"></param>
        /// <returns></returns>
        private List<string> ProcessReceiptSumTemplate(RollerTypeReportModel repTemplate, ReceiptModel currentReceipt, KitchenPrinterModel printersettings)
        {
            List<string> result = new List<string>();
            var kitchenPrinter = availablePrinters.Where(f => f.Name == printersettings.Name).FirstOrDefault();
            if (kitchenPrinter == null)
            {
                logger.LogError(ExtcerLogger.Log("kitchenPrinter NOT FOUND into printers list with name='" + (printersettings.Name ?? "<null>") + "'. Check Printers Settins, SlotIndex values etc.", FiscalName));
            }
            Printer printer = PrintersEscList.Where(f => f.Name == kitchenPrinter.EscapeCharsTemplate).FirstOrDefault();
            var sections = repTemplate.Sections;
            var availableSections = sections.Where(f => f.SectionType != (int)SectionTypeEnums.Extras && f.SectionType != (int)SectionTypeEnums.ReceiptSumHeader && f.SectionType != (int)SectionTypeEnums.ReceiptSumFooter);
            foreach (var section in availableSections)
            {
                if (section != null)
                {
                    if (section.SectionType == (int)SectionTypeEnums.Details)
                    {
                        var invoiceGroups = from d in currentReceipt.Details
                                            group d by d.InvoiceNo into dd
                                            select new
                                            {
                                                RegionName = dd.Key,
                                                InvPosition = dd.Select(f => f.InvoiceNo).FirstOrDefault(),
                                                RegionItems = dd.Select(f => f)
                                            };
                        string currentInvoiceNo = string.Empty;
                        foreach (var region in invoiceGroups.OrderBy(f => f.InvPosition))
                        {
                            var regionitemsCount = (region.RegionItems != null) ? region.RegionItems.Count() : 0;
                            var regionCounter = 0;
                            foreach (var items in region.RegionItems)
                            {
                                if (currentInvoiceNo != items.InvoiceNo)
                                {
                                    // new region item
                                    regionCounter = 1;
                                    // set current region name to items region
                                    currentInvoiceNo = items.InvoiceNo;
                                    List<ReportSectionsRowsModel> headerRow = new List<ReportSectionsRowsModel>();
                                    var header = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.ReceiptSumHeader);
                                    if (header != null)
                                    {
                                        headerRow = header.SectionRows;
                                        // add the receiptSum header
                                        result.AddRange(ProcessSection(headerRow, items, printer, false));
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
                                if (regionCounter == regionitemsCount)
                                {
                                    List<ReportSectionsRowsModel> footerrow = new List<ReportSectionsRowsModel>();
                                    var footer = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.ReceiptSumFooter);
                                    if (footer != null)
                                    {
                                        footerrow = footer.SectionRows;
                                        // add the receiptSum header
                                        result.AddRange(ProcessSection(footerrow, items, printer, false));
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
            logger.LogInformation(ExtcerLogger.Log("in ProcessReceiptSumTemplate Sending kitchen to printer delegate", FiscalName));
            Task task = Task.Run(() => SendTextToPrinter(result, printer, printersettings));
            return result;
        }

        /// <summary>
        /// Process Z report template.
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
            else
            {
                logger.LogInformation(ExtcerLogger.Log("Printer with  PrinterType = ZReport: " + (zPrinter.Name ?? "<null>"), FiscalName));
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
            // loop through the lines of the section
            foreach (var line in section)
            {
                if (!ignoreRegionInfo || (ignoreRegionInfo && line.SectionColumns.Count(f => f.ColumnText == "@ItemRegion") == 0))
                {
                    string str = string.Empty;
                    bool add = true;
                    // loop  through the columns of the current line
                    foreach (var col in line.SectionColumns)
                    {
                        var tempStr = string.Empty;
                        string colText = col.ColumnText != null ? col.ColumnText : "";
                        int colWidth = Convert.ToInt32(col.Width);
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
                        }
                        if (colWidth > data.Length)
                        {
                            if (!String.IsNullOrEmpty(col.AlignOption))
                            {
                                TextAlignEnum alignment = (TextAlignEnum)Enum.Parse(typeof(TextAlignEnum), col.AlignOption, true);
                                switch (alignment)
                                {
                                    case TextAlignEnum.Left:
                                        tempStr += data.PadRight(colWidth, ' ');
                                        break;
                                    case TextAlignEnum.Middle:
                                        tempStr += GenericCommon.CenteredString(data, colWidth);
                                        break;
                                    case TextAlignEnum.Right:
                                        tempStr += data.PadLeft(colWidth, ' ');
                                        break;
                                    default:
                                        tempStr += data.PadRight(colWidth, ' ');
                                        break;
                                }
                            }
                            else
                            {
                                tempStr += data.PadRight(colWidth, ' ');
                            }
                        }
                        else
                        {
                            tempStr += data.Substring(0, colWidth);
                        }
                        if (colText == "@ItemCustomRemark")
                        {
                            if (!string.IsNullOrEmpty(data))
                            {
                                add = true;
                                tempStr = " " + data.TrimEnd();
                                tempStr = GenericCommon.SetStringEscChars(printer, tempStr, col.IsBold, col.IsItalic, col.IsUnderline, col.IsDoubleSize);
                            }
                            else
                            {
                                add = false;
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
        private void SendTextToPrinter(List<String> stringToPrint, Printer printer, KitchenPrinterModel printerSettings)
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
                        for (int i = 1; i <= printTimes; i++)
                        {
                            logger.LogInformation(ExtcerLogger.Log("Printing OEM", printerSettings.FiscalName));
                            Encoding utf8 = new UTF8Encoding();
                            Encoding oem737 = Encoding.GetEncoding(737);
                            str = str + new string('\n', intKitchenHeaderGapLines) + '\r';
                            initChar = string.Empty;
                            if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen)
                            {
                                if (printerSettings.UseBuzzer == true)
                                {
                                    SendBytesToPrinter(buzzerEscChars, printerSettings.Name, 737);
                                }
                            }
                            if (printerSettings.UseCutter == null || printerSettings.UseCutter == true)
                            {
                                str = str + cutterEscChars;
                            }
                            SendBytesToPrinter(str, printerSettings.Name, 737);
                        }
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
                            if (printerSettings.UseCutter == null || printerSettings.UseCutter == true)
                            {
                                toPrint = toPrint + cutterEscChars;
                            }
                            RawPrinterHelper.SendStringToPrinter(printerSettings.Name, toPrint);
                        }
                    }
                    break;
                case PrintCharsFormatEnum.GRAPHIC:
                    {
                        for (int i = 1; i <= printTimes; i++)
                        {
                            if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen && printerSettings.UseBuzzer == true)
                            {
                                logger.LogInformation("In OEM graphic: " + buzzerEscChars.ToString());
                                RawPrinterHelper.SendStringToPrinter(printerSettings.Name, buzzerEscChars);
                            }
                            var toprint = initChar + str + new string('\n', intKitchenHeaderGapLines) + '\r';
                            PrintGraphic(printerSettings.Name, toprint, (7 == intKitchenHeaderGapLines));
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
        /// <param name="strToSend"></param>
        /// <param name="strPrinterName"></param>
        /// <param name="codePage"></param>
        private void SendBytesToPrinter(String strToSend, String strPrinterName, int codePage)
        {
            Encoding utf8 = new UTF8Encoding();
            Encoding destcodepage = Encoding.GetEncoding(codePage);
            byte[] input_utf8 = utf8.GetBytes(strToSend);
            byte[] output_dest = Encoding.Convert(utf8, destcodepage, input_utf8);
            int nLength = Convert.ToInt32(output_dest.Length);
            // Allocate some unmanaged memory for those bytes.
            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.
            Marshal.Copy(output_dest, 0, pUnmanagedBytes, nLength);
            if (!RawPrinterHelper.SendBytesToPrinter(strPrinterName, pUnmanagedBytes, nLength))
            {
                logger.LogInformation("SendBytesToPrinter:" + strPrinterName + " -> Failed!");
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