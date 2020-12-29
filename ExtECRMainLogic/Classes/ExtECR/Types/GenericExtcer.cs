using ExtECRMainLogic.Classes.Extenders;
using ExtECRMainLogic.Classes.Helpers;
using ExtECRMainLogic.Classes.Loggers;
using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Exceptions;
using ExtECRMainLogic.Hubs;
using ExtECRMainLogic.Models.ConfigurationModels;
using ExtECRMainLogic.Models.ExtECRModels;
using ExtECRMainLogic.Models.KitchenModels;
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
    /// Generic Instance
    /// </summary>
    public class GenericExtcer : FiscalManager
    {
        #region Properties
        /// <summary>
        /// Used for the final printout thread lock, so we send to windows spooler only one printout at a time.
        /// </summary>
        private object thisLock;
        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>> PrintersTemplates;
        /// <summary>
        /// 
        /// </summary>
        protected List<Printer> PrintersEscList;
        /// <summary>
        /// 
        /// </summary>
        public List<KitchenPrinterModel> availablePrinters;
        /// <summary>
        /// 
        /// </summary>
        protected InstallationDataModel InstallationData;
        /// <summary>
        /// 
        /// </summary>
        protected string FiscalName;
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
        private readonly ILogger<GenericExtcer> logger;
        /// <summary>
        /// Local hub invoker
        /// </summary>
        private readonly LocalHubInvoker localHubInvoker;
        /// <summary>
        /// 
        /// </summary>
        public Func<string, List<string>> SetFooter { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="printerTemplatesList"></param>
        /// <param name="printerEscList"></param>
        /// <param name="theAvailablePrinters"></param>
        /// <param name="instData"></param>
        /// <param name="fiscName"></param>
        /// <param name="applicationPath"></param>
        /// <param name="configuration"></param>
        /// <param name="applicationBuilder"></param>
        public GenericExtcer(Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>> printerTemplatesList, List<Printer> printerEscList, List<KitchenPrinterModel> theAvailablePrinters, InstallationDataModel instData, string fiscName, string applicationPath, IConfiguration configuration, IApplicationBuilder applicationBuilder)
        {
            // instance identification
            this.InstanceID = ExtcerTypesEnum.Generic;
            // initialization of variable to be used with lock on printouts
            this.thisLock = new object();
            this.PrintersTemplates = printerTemplatesList;
            this.PrintersEscList = printerEscList;
            this.availablePrinters = theAvailablePrinters;
            this.InstallationData = instData;
            this.FiscalName = fiscName;
            this.applicationPath = applicationPath;
            this.configuration = configuration;
            this.logger = (ILogger<GenericExtcer>)applicationBuilder.ApplicationServices.GetService(typeof(ILogger<GenericExtcer>));
            this.localHubInvoker = (LocalHubInvoker)applicationBuilder.ApplicationServices.GetService(typeof(LocalHubInvoker));
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~GenericExtcer()
        {

        }

        #region Override Actions

        /// <summary>
        /// Print receipt interface
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="fiscalName"></param>
        /// <param name="strInvoiceNumber"></param>
        /// <returns></returns>
        public override PrintResultModel PrintReceipt(ReceiptModel receiptModel, string fiscalName, string strInvoiceNumber = "")
        {
            return PrintReceipt(receiptModel, fiscalName, strInvoiceNumber, null);
        }

        /// <summary>
        /// Print receipt interface
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="fiscalName"></param>
        /// <param name="strInvoiceNumber"></param>
        /// <param name="objReprintData"></param>
        /// <returns></returns>
        public override PrintResultModel PrintReceipt(ReceiptModel receiptModel, string fiscalName, string strInvoiceNumber = "", object objReprintData = null)
        {
            PrintResultModel printResult = new PrintResultModel();
            printResult.ReceiptCultureInfo = Thread.CurrentThread.CurrentCulture.Name;
            RollerTypeReportModel template;
            KitchenPrinterModel printerToPrint;
            ReceiptModel currentReceiptData = receiptModel;
            string error = string.Empty;
            try
            {
                printResult.OrderNo = receiptModel.OrderId.ToString();
                printResult.ReceiptNo = receiptModel.ReceiptNo;
                printResult.ExtcerType = ExtcerTypesEnum.Generic;
                receiptModel.FiscalType = FiscalTypeEnum.Generic;
                if (receiptModel.IsVoid)
                {
                    receiptModel.IsVoid = true;
                    // get printer for void
                    printerToPrint = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Void && f.SlotIndex == receiptModel.InvoiceIndex);
                    if (printerToPrint == null)
                    {
                        logger.LogError(ExtcerLogger.Log("Printer NOT FOUND into printers list with type='Void'. Check Printers settings, SlotIndex values etc.", FiscalName));
                    }
                    if (printerToPrint.IsCrystalReportsPrintout == null || printerToPrint.IsCrystalReportsPrintout == false)
                    {
                        logger.LogInformation("Available Templates: ");
                        foreach (var item in PrintersTemplates)
                        {
                            logger.LogInformation(" * Template : " + item.Key);
                            logger.LogInformation(" * Type     : " + item.Value.FirstOrDefault().Key);
                        }
                        // get receipt printer template
                        if (PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.Void).Value == null)
                        {
                            logger.LogError(ExtcerLogger.Log("Generic void template not found.", FiscalName));
                        }
                        template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.Void).Value.FirstOrDefault().Value;
                        if (template == null)
                        {
                            logger.LogWarning(ExtcerLogger.Log("Generic void template not found", FiscalName));
                        }
                        else
                        {
                            logger.LogInformation(ExtcerLogger.Log("Template: '" + (template.ReportName ?? "<null>") + "'", FiscalName));
                        }
                        printResult.InvoiceIndex = receiptModel.InvoiceIndex;
                        printResult.ReceiptType = PrintModeEnum.Void;
                        printResult.ReceiptData = ProcessReceiptTemplate(template, receiptModel, printerToPrint, objReprintData);
                    }
                    else
                    {
                        //TODO GEO
                        //GenericCommon.PrintCrystalReport(printerToPrint.Template, printerToPrint, strInvoiceNumber, null, null, null, SetFooter);
                    }
                }
                else
                {
                    receiptModel.IsVoid = false;
                    // get printer for receipt
                    if (receiptModel.InvoiceIndex == null)
                    {
                        receiptModel.InvoiceIndex = "1";
                    }
                    printerToPrint = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Receipt && f.SlotIndex == receiptModel.InvoiceIndex);
                    if (printerToPrint == null)
                    {
                        logger.LogError(ExtcerLogger.Log("Printer NOT FOUND into printers list with type='Receipt' and slotIndex='" + receiptModel.InvoiceIndex.ToString() + "'. Check Printers setings, SlotIndex values etc.", FiscalName));
                        throw new CustomException(string.Format("Invoice number {0} not registered, " + "or there are no invoices registered!", receiptModel.InvoiceIndex));
                    }
                    if (printerToPrint.IsCrystalReportsPrintout == null || printerToPrint.IsCrystalReportsPrintout == false)
                    {
                        // get receipt printer template
                        if (PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Value.ReportName == printerToPrint.TemplateShortName).Value == null)
                        {
                            logger.LogError(ExtcerLogger.Log("Generic template not found.", FiscalName));
                        }
                        template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Value.ReportName == printerToPrint.TemplateShortName).Value.FirstOrDefault().Value;
                        if (template == null)
                        {
                            logger.LogWarning(ExtcerLogger.Log("Generic template not found", FiscalName));
                        }
                        else
                        {
                            logger.LogInformation(ExtcerLogger.Log("Template: " + (template.ReportName ?? "<null>"), FiscalName));
                        }
                        printResult.InvoiceIndex = receiptModel.InvoiceIndex;
                        printResult.ReceiptType = PrintModeEnum.Receipt;
                        printResult.ReceiptData = ProcessReceiptTemplate(template, receiptModel, printerToPrint, objReprintData);
                    }
                    else
                    {
                        //TODO GEO
                        //GenericCommon.PrintCrystalReport(printerToPrint.Template, printerToPrint, strInvoiceNumber, null, null, null, SetFooter);
                    }
                }
                printResult.Status = PrintStatusEnum.Printed;
            }
            catch (CustomException exception)
            {
                printResult.ErrorDescription = exception.Message;
                logger.LogError(ExtcerLogger.logErr("Error printing generic receipt: ", exception, FiscalName));
                printResult.Status = PrintStatusEnum.Failed;
                error = "Error printing generic receipt: " + exception.Message + " StackTRace: " + exception.StackTrace;
            }
            catch (Exception exception)
            {
                printResult.ErrorDescription = ExtcerErrorHelper.GetErrorDescription(ExtcerErrorHelper.GENERAL_ERROR);
                logger.LogError(ExtcerLogger.logErr("Error printing generic receipt: ", exception, FiscalName));
                printResult.Status = PrintStatusEnum.Failed;
                error = "Error printing generic receipt: " + exception.Message + " StackTRace: " + exception.StackTrace;
            }
            ArtcasLogger.LogArtCasXML(receiptModel, DateTime.Now, error, printResult.ReceiptData, ReceiptReceiveTypeEnum.WEB, printResult.Status, fiscalName, printResult.Id);
            return printResult;
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
                    logger.LogError(ExtcerLogger.Log("Printer with Type='Report' NOT FOUND into printers list. Check Printers Settings, SlotIndex values etc.", FiscalName));
                }
                // get receipt printer template
                RollerTypeReportModel template;
                if (PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Value.ReportName == printerToPrint.TemplateShortName).Value == null)
                {
                    logger.LogError(ExtcerLogger.Log("Generic template not found", FiscalName));
                }
                template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Value.ReportName == printerToPrint.TemplateShortName).Value.FirstOrDefault().Value;
                if (template == null)
                {
                    logger.LogWarning(ExtcerLogger.Log("Generic template not found", FiscalName));
                }
                else
                {
                    logger.LogInformation(ExtcerLogger.Log("Template: " + (template.ReportName ?? "<null>"), FiscalName));
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
        /// Print kitchen interface
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="blnIsVoid"></param>
        /// <returns></returns>
        public override PrintResultModel PrintKitchen(ReceiptModel receiptModel, bool blnIsVoid)
        {
            PrintResultModel printResult = new PrintResultModel();
            printResult.SecondaryPrintersList = new List<PrinterResultModel>();
            long regionId = -1000;
            List<KpsSpooler> spooler = CalculateWebKps(receiptModel);
            ReceiptModel currentReceiptData = receiptModel;
            // get kitchen printers
            var groupedPrinters = spooler.Where(f => f.PrinterType == PrinterTypeEnum.Kitchen).GroupBy(f => f.PrinterName);
            printResult.SecondaryPrintersList = new List<PrinterResultModel>();
            // loop through kitchen printers
            foreach (var printerItems in groupedPrinters.ToList())
            {
                string curPrinterName = printerItems.Key;
                PrinterResultModel pRes = new PrinterResultModel();
                try
                {
                    pRes.PrinterName = curPrinterName;
                    pRes.PrinterType = PrinterTypeEnum.Kitchen;
                    // get current printer name
                    curPrinterName = printerItems.Key;
                    KitchenPrinterModel printerSettings = new KitchenPrinterModel();
                    printerSettings = availablePrinters.Where(f => f.Name == curPrinterName && f.Groups.Contains((int)printerItems.FirstOrDefault().KitchenId)).FirstOrDefault();
                    if (printerSettings.Regions.Count > 0)
                    {
                        if (printerSettings.Regions[0] > 0)
                        {
                            regionId = receiptModel.RegionId.GetValueOrDefault();
                            if (regionId > 0)
                            {
                                printerSettings = availablePrinters.Where(f => f.Name == curPrinterName && f.Regions.Contains(regionId) && f.Groups.Contains((int)printerItems.FirstOrDefault().KitchenId)).FirstOrDefault();
                            }
                        }
                    }
                    if (printerSettings == null)
                    {
                        logger.LogError(ExtcerLogger.Log(">> Printer NOT FOUND into printers list with name='" + (curPrinterName ?? "") + "'. Check Printers settings, SlotIndex values etc.", FiscalName));
                        logger.LogError(ExtcerLogger.Log(">>   printerItems:" + printerItems, FiscalName));
                        logger.LogError(ExtcerLogger.Log(">>   regionId:" + regionId.ToString(), FiscalName));
                        logger.LogError(ExtcerLogger.Log(">>   Available Printers:" + Environment.NewLine + KitchenPrinterModel.ListToString(availablePrinters), FiscalName));
                        logger.LogError(ExtcerLogger.Log(">>   Available KpsSpoolers:" + Environment.NewLine + KpsSpooler.ListToString(spooler), FiscalName));
                    }
                    // get current printer template
                    RollerTypeReportModel template = null;
                    var templByName = PrintersTemplates.Where(f => f.Key == printerSettings.TemplateShortName).FirstOrDefault();
                    if (!templByName.Equals(null))
                    {
                        var b = templByName.Value;
                        template = b[PrinterTypeEnum.Kitchen];
                    }
                    else
                    {
                        logger.LogError("Cannot find any template with name: " + printerSettings.TemplateShortName);
                    }
                    if (template == null)
                    {
                        logger.LogWarning(ExtcerLogger.Log("Generic kitchen template not found", FiscalName));
                    }
                    else
                    {
                        logger.LogInformation(ExtcerLogger.Log("Kitchen Template: " + (template.ReportName ?? printerSettings.TemplateShortName), FiscalName));
                    }
                    if (printerSettings.IsCrystalReportsPrintout == null || printerSettings.IsCrystalReportsPrintout == false)
                    {
                        // get the text to print
                        var kitchenReceiptString = ProcessKitchenTemplate(template, currentReceiptData, printerItems.ToList(), printerSettings, blnIsVoid);
                        pRes.ReceiptData = kitchenReceiptString;
                    }
                    else
                    {
                        //TODO GEO
                        //GenericCommon.PrintCrystalReport(printerSettings.Template, printerSettings, receiptModel.InvoiceIdStr, SetFooter);
                        pRes.ReceiptData = new List<string>();
                    }
                    pRes.PrintStatus = PrintStatusEnum.Printed;
                    pRes.PrinterName = printerItems.FirstOrDefault().PrinterName;
                    pRes.KitchenIdList = printerItems.Select(f => (int)f.KitchenId).Distinct().ToList();
                }
                catch (Exception exception)
                {
                    logger.LogError(ExtcerLogger.logErr("Error printing kitchen receipt: printerName:", exception, FiscalName));
                    pRes.PrintStatus = PrintStatusEnum.Failed;
                }
                // add result to printer secondary list
                printResult.SecondaryPrintersList.Add(pRes);
            }
            return printResult;
        }

        /// <summary>
        /// Print kitchen instruction interface
        /// </summary>
        /// <param name="kitchenInstruction"></param>
        /// <returns></returns>
        public override PrintResultModel PrintKitchenInstruction(KitchenInstructionModel kitchenInstruction)
        {
            try
            {
                var printersToPrint = availablePrinters.Where(f => f.Groups.Contains(kitchenInstruction.KitchenId));
                if (printersToPrint == null)
                {
                    logger.LogError(ExtcerLogger.Log("Kichen Printer for KitchenId='" + kitchenInstruction.KitchenId.ToString() + "' NOT FOUND into printers list. Check Printers Settings, SlotIndex values etc.", FiscalName));
                }
                var result = new List<string>() { kitchenInstruction.Message };
                if (!string.IsNullOrEmpty(kitchenInstruction.Waiter))
                {
                    result.Add("Waiter: " + kitchenInstruction.Waiter);
                }
                if (!string.IsNullOrEmpty(kitchenInstruction.Table))
                {
                    result.Add("Table: " + kitchenInstruction.Table + "\r" + new string('\n', 3));
                }
                if (!string.IsNullOrEmpty(kitchenInstruction.ReceivedTime) && !string.IsNullOrEmpty(kitchenInstruction.ReceivedTime))
                {
                    result.Add("Time: " + kitchenInstruction.ReceivedDate + " " + kitchenInstruction.ReceivedTime + "\r" + new string('\n', 7));
                }
                else if (kitchenInstruction.SendTS != null)
                {
                    result.Add("Time: " + kitchenInstruction.SendTS.ToString("dd/MM/yyyy HH:mm:ss") + "\r" + new string('\n', 7));
                }
                foreach (var ptr in printersToPrint)
                {
                    Printer printer = PrintersEscList.Where(f => f.Name == ptr.EscapeCharsTemplate).FirstOrDefault();
                    Task task = Task.Run(() => SendTextToPrinter(result, printer, ptr));
                }
                // while been here we did not get an error so we assume we are ok.
                return new PrintResultModel { Status = PrintStatusEnum.Printed };
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.logErr("Error on PrintKitchenInstruction: ", exception, FiscalName));
            }
            return new PrintResultModel { Status = PrintStatusEnum.Failed };
        }

        /// <summary>
        /// Print reservation interface
        /// </summary>
        /// <param name="reservationModel"></param>
        /// <param name="fiscalName"></param>
        /// <param name="objReprintData"></param>
        /// <returns></returns>
        public override PrintResultModel PrintReservations(ExtecrTableReservetionModel reservationModel, string fiscalName, object objReprintData = null)
        {
            PrintResultModel printResult = new PrintResultModel();
            printResult.ReceiptType = PrintModeEnum.Reservation;
            RollerTypeReportModel template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.Reservation).Value.FirstOrDefault().Value;
            printResult.ExtcerType = ExtcerTypesEnum.Generic;
            printResult.ReceiptData = ProcessReservation(template, reservationModel, printResult);
            return printResult;
        }

        /// <summary>
        /// Print report interface
        /// </summary>
        /// <param name="msgToPrint"></param>
        /// <param name="printerType"></param>
        public override void PrintReport(string msgToPrint, PrinterTypeEnum printerType)
        {
            logger.LogInformation("Printing report - start: " + DateTime.Now);
            KitchenPrinterModel printerToPrint = availablePrinters.FirstOrDefault(f => f.PrinterType == printerType);
            if (printerToPrint == null)
            {
                logger.LogError(ExtcerLogger.Log("Printer with Type='" + printerType.ToString() + "' NOT FOUND into printers list. Check Printers Settings, SlotIndex values etc.", FiscalName));
            }
            logger.LogInformation(ExtcerLogger.Log("Selecting printer...", FiscalName));
            var reportPrinter = availablePrinters.Where(f => f.Name == printerToPrint.Name).FirstOrDefault();
            if (printerToPrint == null)
            {
                logger.LogError(ExtcerLogger.Log("Printer with Name='" + (printerToPrint.Name ?? "") + "' NOT FOUND into printers list. Check Printers Settings, SlotIndex values etc.", FiscalName));
            }
            Printer printer = PrintersEscList.Where(f => f.Name == reportPrinter.EscapeCharsTemplate).FirstOrDefault();
            var result = new List<string>() { msgToPrint };
            logger.LogInformation("Begin Invoke : " + printer.Name + " " + DateTime.Now);
            Task task = Task.Run(() => SendTextToPrinter(result, printer, printerToPrint));
        }

        /// <summary>
        /// Print report interface
        /// </summary>
        /// <param name="reportModel"></param>
        /// <param name="fiscalName"></param>
        /// <param name="objReprintData"></param>
        /// <returns></returns>
        public override PrintResultModel PrintReports(ReportsModel reportModel, string fiscalName, object objReprintData = null)
        {
            return GenericCommon.PrintReports(reportModel, fiscalName, availablePrinters, PrintersEscList, PrintersTemplates, this, thisLock, objReprintData, SetFooter);
        }

        /// <summary>
        /// Print X report interface
        /// </summary>
        /// <param name="xData"></param>
        /// <param name="resultX"></param>
        public override void PrintX(ZReportModel xData, out PrintResultModel resultX)
        {
            // find the printer that is set for report printing
            var xPrinter = availablePrinters.Where(f => f.PrinterType == PrinterTypeEnum.XReport).FirstOrDefault();
            if (xPrinter == null)
            {
                logger.LogError(ExtcerLogger.Log("XPrinter NOT FOUND into printers list with Type='XReport'. Check Printers setings, SlotIndex values etc.", FiscalName));
            }
            // get the selected printer escape chars
            Printer printer = PrintersEscList.Where(f => f.Name == xPrinter.EscapeCharsTemplate).FirstOrDefault();
            string occuredError = null;
            resultX = new PrintResultModel(DateTime.Now);
            List<string> result = new List<string>();
            try
            {
                resultX.ReceiptNo = xData.ReportNo.ToString();
                resultX.ReceiptType = PrintModeEnum.XReport;
                resultX.Status = PrintStatusEnum.Printed;
                resultX.ExtcerType = ExtcerTypesEnum.Generic;
                if (xPrinter.IsCrystalReportsPrintout == null || xPrinter.IsCrystalReportsPrintout == false)
                {
                    // get X report printer template
                    if (PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.XReport).Value == null)
                    {
                        logger.LogError(ExtcerLogger.Log("Generic X template not found.", FiscalName));
                    }
                    var template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.XReport).Value.FirstOrDefault().Value;
                    if (template == null)
                    {
                        logger.LogWarning(ExtcerLogger.Log("Generic X template not found", FiscalName));
                    }
                    else
                    {
                        logger.LogInformation(ExtcerLogger.Log("X Template: " + (template.ReportName ?? "<null>"), FiscalName));
                    }
                    // get text to print
                    result = ProcessZReportTemplate(template, xData, xPrinter);
                    resultX.ReceiptData = result;
                }
                else
                {
                    //TODO GEO
                    //GenericCommon.PrintCrystalReport(xPrinter.Template, xPrinter, string.Empty, null, xData.EndOfDayId, xData.PosInfoId);
                }
                resultX.Status = PrintStatusEnum.Printed;
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.Log(exception.ToString(), FiscalName));
                resultX.ErrorDescription = ExtcerErrorHelper.GetErrorDescription(ExtcerErrorHelper.GENERAL_ERROR);
                resultX.Status = PrintStatusEnum.Failed;
                occuredError += "XPRINT ERROR  Message: " + exception.Message + "\nStackTrace: " + exception.StackTrace;
            }
        }

        /// <summary>
        /// Print Z report interface
        /// </summary>
        /// <param name="zData"></param>
        /// <returns></returns>
        public override PrintResultModel PrintZ(ZReportModel zData)
        {
            // find the printer that is set for report printing
            var zPrinter = availablePrinters.Where(f => f.PrinterType == PrinterTypeEnum.ZReport).FirstOrDefault();
            if (zPrinter == null)
            {
                logger.LogError(ExtcerLogger.Log("ZPrinter NOT FOUND into printers list with Type='ZReport'. Check Printers setings, SlotIndex values etc.", FiscalName));
            }
            // get the selected printer escape chars
            Printer printer = PrintersEscList.Where(f => f.Name == zPrinter.EscapeCharsTemplate).FirstOrDefault();
            string occuredError = null;
            PrintResultModel resultObj = new PrintResultModel(DateTime.Now);
            List<string> result = new List<string>();
            try
            {
                resultObj.ReceiptNo = zData.ReportNo.ToString();
                resultObj.ReceiptType = PrintModeEnum.ZReport;
                resultObj.Status = PrintStatusEnum.Printed;
                resultObj.ExtcerType = ExtcerTypesEnum.Generic;
                if (zPrinter.IsCrystalReportsPrintout == null || zPrinter.IsCrystalReportsPrintout == false)
                {
                    // get receipt printer template
                    if (PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.ZReport).Value == null)
                    {
                        logger.LogError(ExtcerLogger.Log("Generic Z Template not found.", FiscalName));
                    }
                    var template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.ZReport).Value.FirstOrDefault().Value;
                    if (template == null)
                    {
                        logger.LogWarning(ExtcerLogger.Log("Generic Z Template not found", FiscalName));
                    }
                    else
                    {
                        logger.LogInformation(ExtcerLogger.Log("Z Template: " + (template.ReportName ?? "<null>"), FiscalName));
                    }
                    // get text to print
                    result = ProcessZReportTemplate(template, zData, zPrinter);
                    resultObj.ReceiptData = result;
                }
                else
                {
                    //TODO GEO
                    //GenericCommon.PrintCrystalReport(zPrinter.Template, zPrinter, string.Empty, null, zData.EndOfDayId, zData.PosInfoId);
                }
                resultObj.Status = PrintStatusEnum.Printed;
            }
            catch (Exception exception)
            {
                logger.LogError(ExtcerLogger.Log(exception.ToString(), FiscalName));
                resultObj.ErrorDescription = ExtcerErrorHelper.GetErrorDescription(ExtcerErrorHelper.GENERAL_ERROR);
                resultObj.Status = PrintStatusEnum.Failed;
                occuredError += "Message: " + exception.Message + "\nStackTrace: " + exception.StackTrace;
            }
            return resultObj;
        }

        /// <summary>
        /// Get Z total interface
        /// </summary>
        /// <returns></returns>
        public override PrintResultModel GetZTotal()
        {
            PrintResultModel printResult = new PrintResultModel();
            printResult.ExtcerType = ExtcerTypesEnum.Generic;
            printResult.ReceiptType = PrintModeEnum.ZTotals;
            printResult.Status = PrintStatusEnum.Unknown;
            printResult.ResponseValue = string.Empty;
            return printResult;
        }

        /// <summary>
        /// Print graphic interface
        /// </summary>
        /// <param name="printerName"></param>
        /// <param name="strToPrint"></param>
        /// <param name="blnUseDefaultMargins"></param>
        public override void PrintGraphic(string printerName, string strToPrint, bool blnUseDefaultMargins = true)
        {
            logger.LogInformation("Starting 'PrintGraphic' within GenericExtcer.");
            try
            {
                Stream strm = GenericCommon.GenerateStreamFromString(strToPrint);
                streamToPrint = new StreamReader(strm);
                try
                {
                    if (InstallationData.GenericGraphicBold)
                        printFont = new Font("Courier New", InstallationData.GenericGraphicFontSize, FontStyle.Bold);
                    else
                        printFont = new Font("Courier New", InstallationData.GenericGraphicFontSize, FontStyle.Regular);
                    PrintDocument pd = new PrintDocument();
                    if (blnUseDefaultMargins)
                    {
                        pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
                    }
                    else
                    {
                        pd.PrintPage += new PrintPageEventHandler(pd_PrintPage_Custom);
                    }
                    pd.PrinterSettings.PrinterName = printerName;
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

        #endregion

        #region Private Methods

        /// <summary>
        /// Process receipt template.
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentReceipt"></param>
        /// <param name="printerSettings"></param>
        /// <param name="objReprintData"></param>
        /// <returns></returns>
        private List<string> ProcessReceiptTemplate(RollerTypeReportModel repTemplate, ReceiptModel currentReceipt, KitchenPrinterModel printerSettings, object objReprintData = null)
        {
            List<string> result = new List<string>();
            var receiptPrinter = printerSettings;
            // get esc characters
            Printer printer = PrintersEscList.Where(f => f.Name == receiptPrinter.EscapeCharsTemplate).FirstOrDefault();
            // get sections from template
            var sections = repTemplate.Sections;
            //for each section (except extras) create receipt's lines
            foreach (var section in sections.Where(f => f.SectionType != (int)SectionTypeEnums.Extras))
            {
                if (section != null)
                {
                    // print the fiscal sign in the receipt
                    if (section.SectionType == (int)SectionTypeEnums.FiscalSign)
                    {
                        if (currentReceipt.PrintFiscalSign != null && currentReceipt.PrintFiscalSign == false)
                        {
                            continue;
                        }
                    }
                    if (section.SectionType == (int)SectionTypeEnums.Details)
                    {
                        foreach (var itemData in currentReceipt.Details)
                        {
                            if (itemData.IsChangeItem)
                            {
                                itemData.ItemPrice = Math.Abs(itemData.ItemPrice) * -1;
                            }
                            result.AddRange(ProcessSection(section.SectionRows, itemData, printer, false, false, objReprintData));
                            string str = result[result.Count - 1];
                            if (str.Length > 1 && str[str.Length - 2] == ',')
                            {
                                str = str.Remove(str.Length - 2, 1);
                                result[result.Count - 1] = str;
                            }
                            if (itemData.Extras.Count > 0)
                            {
                                ProcessReceiptTemplateExtras(repTemplate, printer, result, itemData);
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
                    else if (section.SectionType == (int)SectionTypeEnums.RelatedReceipts)
                    {
                        if (currentReceipt.RelatedReceipts != null && currentReceipt.RelatedReceipts.Count > 0)
                        {
                            result.AddRange(ProcessSection(section.SectionRows, currentReceipt.RelatedReceipts, printer, false, false, null, repTemplate.MaxWidth));
                        }
                    }
                    else if (section.SectionType == (int)SectionTypeEnums.SalesTypeSection)
                    {
                        foreach (var items in currentReceipt.SalesTypeDescriptions)
                        {
                            result.AddRange(ProcessSection(section.SectionRows, items, printer));
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
                    else if (section.SectionType == (int)SectionTypeEnums.CreditTransactions)
                    {
                        foreach (var item in currentReceipt.CreditTransactions)
                        {
                            result.AddRange(ProcessSection(section.SectionRows, item, printer));
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
                        result.AddRange(ProcessSection(section.SectionRows, currentReceipt, printer, false, false, objReprintData));
                    }
                }
            }
            if (printerSettings.PrintKitchenOnly == null || (bool)printerSettings.PrintKitchenOnly == false)
            {
                Task task = Task.Run(() => SendTextToPrinter(result, printer, printerSettings));
            }
            else
            {
                result.Insert(0, "--SETTED NOT TO PRINT--");
            }
            return result;
        }

        /// <summary>
        /// Process the extras for the receipt template
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="printer"></param>
        /// <param name="result"></param>
        /// <param name="items"></param>
        private void ProcessReceiptTemplateExtras(RollerTypeReportModel repTemplate, Printer printer, List<string> result, ReceiptItemsModel items)
        {
            var extrasDoc = repTemplate.Sections.Where(w => w.SectionType == (int)SectionTypeEnums.Extras).FirstOrDefault();
            if (extrasDoc != null)
            {
                foreach (var extra in items.Extras)
                {
                    if (InstallationData.GenericExtraZeroPrice == true || (InstallationData.GenericExtraZeroPrice == false && extra.ItemPrice > 0))
                    {
                        if (extra.IsChangeItem)
                        {
                            if (extra.ItemPrice != null)
                            {
                                extra.ItemPrice = extra.ItemPrice * -1;
                            }
                        }
                        result.AddRange(ProcessSection(extrasDoc.SectionRows, extra, printer, false, true));
                        if (extra.ItemDiscount != null && extra.ItemDiscount > 0)
                        {
                            var discountDetailsSection = repTemplate.Sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.DiscountDetails);
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
        }

        /// <summary>
        /// Process ReceiptSum template
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
                                            InvPosition = dd.Select(f => string.IsNullOrEmpty(f.InvoiceNo) ? 0 : int.Parse(f.InvoiceNo)).FirstOrDefault(),
                                            RegionItems = dd.Select(f => f)
                                        };
                        string CurInvoiceNo = "";
                        foreach (var region in invGroups.OrderByDescending(f => f.RegionName))
                        {
                            var regionitemsCount = region.RegionItems != null ? region.RegionItems.Count() : 0;
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
                                // end of invoice group
                                if (regionCounter == regionitemsCount)
                                {
                                    List<ReportSectionsRowsModel> footerrow = new List<ReportSectionsRowsModel>();
                                    var footer = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.ReceiptSumFooter);
                                    if (footer != null)
                                    {
                                        footerrow = footer.SectionRows;
                                        // add the receipt sum header
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
            logger.LogInformation("in ProcessReceiptSumTemplate Sending kitchen to printer delegate");
            Task task = Task.Run(() => SendTextToPrinter(result, printer, printersettings));
            return result;
        }

        /// <summary>
        /// Process kitchen template.
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentReceipt"></param>
        /// <param name="spoolItemsG"></param>
        /// <param name="printerSettings"></param>
        /// <param name="blnIsVoid"></param>
        /// <returns></returns>
        private List<string> ProcessKitchenTemplate(RollerTypeReportModel repTemplate, ReceiptModel currentReceipt, List<KpsSpooler> spoolItemsG, KitchenPrinterModel printerSettings, bool blnIsVoid = false)
        {
            List<string> result = new List<string>();
            var kitchenPrinter = availablePrinters.Where(f => f.Name == printerSettings.Name && f.PrinterType == PrinterTypeEnum.Kitchen).FirstOrDefault();
            Printer printer = PrintersEscList.Where(f => f.Name == kitchenPrinter.EscapeCharsTemplate).FirstOrDefault();
            var sections = repTemplate.Sections;
            int intNumberOfTotalItems = 0;
            int totalItems = 0;
            if ((printerSettings.PrintSingleItem != null && printerSettings.PrintSingleItem == true) && (printerSettings.MergeSameCodes == null || printerSettings.MergeSameCodes == false))
            {
                // calculate total items within receipt
                foreach (var receiptItem in currentReceipt.Details)
                {
                    intNumberOfTotalItems += (Int32)receiptItem.ItemQty;
                }
            }
            else if ((printerSettings.PrintSingleItem != null && printerSettings.PrintSingleItem == true) && (printerSettings.MergeSameCodes != null && printerSettings.MergeSameCodes == true))
            {
                // get total number of different item codes
                totalItems = spoolItemsG.Count;
            }
            if (printerSettings.PrintSingleItem != null && printerSettings.PrintSingleItem == true)
            {
                ProcessKitchenTemplate_ItemByItem(repTemplate, currentReceipt, spoolItemsG, result, printerSettings, printer, sections, intNumberOfTotalItems, totalItems);
            }
            else
            {
                foreach (var section in sections.Where(f => f.SectionType != (int)SectionTypeEnums.Extras))
                {
                    if (section != null)
                    {
                        if (section.SectionType == (int)SectionTypeEnums.Details)
                        {
                            // group items by region group 
                            var spoolIds = spoolItemsG.Select(ff => ff.ItemDescription);
                            var flat = currentReceipt.Details.Where(f => spoolIds.Contains(f.ItemDescr));
                            var regionGroups = from d in flat
                                               group d by d.ItemRegion into dd
                                               select new
                                               {
                                                   RegionName = dd.Key,
                                                   RegionPosition = dd.Select(f => f.RegionPosition).FirstOrDefault(),
                                                   ItemSort = dd.Select(f => f.ItemSort).FirstOrDefault(),
                                                   RegionItems = dd.Select(f => f)
                                               };
                            string CurrentRegionName = string.Empty;
                            foreach (var region in regionGroups.OrderBy(f => f.RegionPosition).ThenBy(f => f.ItemSort))
                            {
                                var regionitemsCount = (region.RegionItems != null) ? region.RegionItems.Count() : 0;
                                var regionCounter = 0;
                                foreach (var items in region.RegionItems)
                                {
                                    if (CurrentRegionName != items.ItemRegion)
                                    {
                                        // new region item
                                        regionCounter = 1;
                                        // set current region name to items region
                                        CurrentRegionName = items.ItemRegion;
                                        if (string.IsNullOrEmpty(CurrentRegionName))
                                        {
                                            result.AddRange(ProcessSection(section.SectionRows, items, printer, true, false, null, 0.0, blnIsVoid));
                                        }
                                        else
                                        {
                                            result.AddRange(ProcessSection(section.SectionRows, items, printer, false, false, null, 0.0, blnIsVoid));
                                        }
                                    }
                                    else
                                    {
                                        // item is in current region
                                        regionCounter++;
                                        // add line but ignore region info
                                        result.AddRange(ProcessSection(section.SectionRows, items, printer, true, false, null, 0.0, blnIsVoid));
                                    }
                                    if (items.Extras.Count > 0)
                                    {
                                        var extrasDoc = repTemplate.Sections.Where(w => w.SectionType == (int)SectionTypeEnums.Extras).FirstOrDefault();
                                        if (extrasDoc != null)
                                        {
                                            foreach (var extra in items.Extras)
                                            {
                                                result.AddRange(ProcessSection(extrasDoc.SectionRows, extra, printer, false, false, null, 0.0, blnIsVoid));
                                            }
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(CurrentRegionName) && regionCounter == regionitemsCount && !string.IsNullOrEmpty(items.ItemRegion))
                                    {
                                        result.Add("----------------------------------------");
                                    }
                                }
                            }
                        }
                        else
                        {
                            result.AddRange(ProcessSection(section.SectionRows, currentReceipt, printer, false, false, null, 0.0, blnIsVoid));
                        }
                    }
                }
                logger.LogInformation("in ProcessKitchenTemplate Sending kitchen to printer delegate");
                Task task = Task.Run(() => SendTextToPrinter(result, printer, printerSettings));
            }
            return result;
        }

        /// <summary>
        /// Process kitchen template for the 'item by item' print, either for delivery or for standard kitchen mode.
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentReceipt"></param>
        /// <param name="spoolItemsG"></param>
        /// <param name="result"></param>
        /// <param name="printerSettings"></param>
        /// <param name="printer"></param>
        /// <param name="sections"></param>
        /// <param name="intNumberOfTotalItems"></param>
        /// <param name="totalItems"></param>
        private void ProcessKitchenTemplate_ItemByItem(RollerTypeReportModel repTemplate, ReceiptModel currentReceipt, List<KpsSpooler> spoolItemsG, List<string> result, KitchenPrinterModel printerSettings, Printer printer, List<ReportSectionsModel> sections, int intNumberOfTotalItems, int totalItems)
        {
            int counter = 0;
            if (printerSettings.MergeSameCodes == null || printerSettings.MergeSameCodes == false)
            {
                ProcessKitchenTemplate_DeliveryMode(repTemplate, currentReceipt, spoolItemsG, result, printerSettings, printer, sections, intNumberOfTotalItems, counter);
            }
            else if (printerSettings.MergeSameCodes != null && printerSettings.MergeSameCodes == true)
            {
                ProcessKitchenTemplate_KitchenMode(repTemplate, currentReceipt, spoolItemsG, result, printerSettings, printer, sections, totalItems, counter);
            }
        }

        /// <summary>
        /// kitchen - delivery mode
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentReceipt"></param>
        /// <param name="spoolItemsG"></param>
        /// <param name="result"></param>
        /// <param name="printerSettings"></param>
        /// <param name="printer"></param>
        /// <param name="sections"></param>
        /// <param name="intNumberOfTotalItems"></param>
        /// <param name="counter"></param>
        private void ProcessKitchenTemplate_DeliveryMode(RollerTypeReportModel repTemplate, ReceiptModel currentReceipt, List<KpsSpooler> spoolItemsG, List<string> result, KitchenPrinterModel printerSettings, Printer printer, List<ReportSectionsModel> sections, int intNumberOfTotalItems, int counter)
        {
            List<ClassUsedItems> defArray = spoolItemsG.GroupBy(g => g.ItemDescription).Select(s => new ClassUsedItems
            {
                ItemDescription = s.Key,
                Appears = s.Count(),
                Used = 0
            }).ToList();
            int? intItemQuantity = null;
            int rowstoskip = 0;
            foreach (var spoolItemsl in spoolItemsG)
            {
                bool blnFooterSectionExists = false;
                List<string> tempResult = new List<string>();
                counter++;
                List<KpsSpooler> spoolItems = new List<KpsSpooler>();
                spoolItems.Add(spoolItemsl);
                foreach (var section in sections.Where(f => f.SectionType != (int)SectionTypeEnums.Extras))
                {
                    if (section != null)
                    {
                        if (section.SectionType == (int)SectionTypeEnums.Details)
                        {
                            // group items by region group 
                            var spoolIds = spoolItems.Select(ff => ff.ItemDescription);
                            var current = defArray.Where(w => w.ItemDescription == spoolIds.FirstOrDefault()).First();
                            if (current.Appears >= (current.Used + 1))
                            {
                                rowstoskip = current.Used;
                                current.Used++;
                            }
                            var flat = currentReceipt.Details.Where(f => spoolIds.Contains(f.ItemDescr)).OrderBy(o => o.ItemCode).Skip(rowstoskip).Take(1);
                            intItemQuantity = Convert.ToInt32(flat.ElementAt(0).ItemQty);
                            flat.ElementAt(0).ItemQty = 1;
                            var regionGroups = from d in flat
                                               group d by d.ItemRegion into dd
                                               select new
                                               {
                                                   RegionName = dd.Key,
                                                   RegionPosition = dd.Select(f => f.RegionPosition).FirstOrDefault(),
                                                   RegionItems = dd.Select(f => f)
                                               };
                            string CurrentRegionName = string.Empty;
                            foreach (var region in regionGroups.OrderBy(f => f.RegionPosition))
                            {
                                var regionitemsCount = (region.RegionItems != null) ? region.RegionItems.Count() : 0;
                                var regionCounter = 0;
                                foreach (var items in region.RegionItems)
                                {
                                    bool ignoredRegion = false;
                                    ignoredRegion = section.SectionRows.Select(f => f.SectionColumns).Where(g => g.Count(ggg => ggg.ColumnText == "@ItemRegion") > 0).Count() == 0;
                                    if (CurrentRegionName != items.ItemRegion)
                                    {
                                        // new region item
                                        regionCounter = 1;
                                        // set current region name to items region
                                        CurrentRegionName = items.ItemRegion;
                                        List<string> regionStr = new List<string>();
                                        regionStr = ProcessSection(section.SectionRows, items, printer, (string.IsNullOrEmpty(CurrentRegionName)));
                                        tempResult.AddRange(regionStr);
                                    }
                                    else
                                    {
                                        // item is in current region
                                        regionCounter++;
                                        // add line but ignore region info
                                        tempResult.AddRange(ProcessSection(section.SectionRows, items, printer, true));
                                    }
                                    if (items.Extras.Count > 0)
                                    {
                                        var extrasDoc = repTemplate.Sections.Where(w => w.SectionType == (int)SectionTypeEnums.Extras).FirstOrDefault();
                                        if (extrasDoc != null)
                                        {
                                            foreach (var extra in items.Extras)
                                            {
                                                tempResult.AddRange(ProcessSection(extrasDoc.SectionRows, extra, printer));
                                            }
                                        }
                                    }
                                    if (!ignoredRegion && !string.IsNullOrEmpty(CurrentRegionName) && regionCounter == regionitemsCount && !string.IsNullOrEmpty(items.ItemRegion))
                                    {
                                        tempResult.Add("----------------------------------------");
                                    }
                                }
                            }
                        }
                        else if (section.SectionType == (int)SectionTypeEnums.Footer)
                        {
                            // get last line of printout
                            string strTmpLastLine = tempResult.Last();
                            // form the appropriate counter indication
                            string strTmpCounter = (counter + "/" + intNumberOfTotalItems);
                            // build last line backwards (starting from the last character, up to the first)
                            StringBuilder strTmpNewLastLine = new StringBuilder(strTmpLastLine);
                            for (int i = strTmpLastLine.Length - 2, j = strTmpCounter.Length - 1; i >= 0 && j >= 0; i--, j--)
                            {
                                strTmpNewLastLine[i] = strTmpCounter[j];
                            }
                            // remove previous last line from the result
                            tempResult.Remove(strTmpLastLine);
                            // add to the result the newly build last line (counter + any static text)
                            tempResult.Add(strTmpNewLastLine.ToString());
                            blnFooterSectionExists = true;
                        }
                        else
                        {
                            tempResult.AddRange(ProcessSection(section.SectionRows, currentReceipt, printer));
                        }
                    }
                }
                result.AddRange(tempResult);
                for (int qt = 0; qt < intItemQuantity; qt++)
                {
                    // synchronous printing to kitchen printer for delivery
                    SendTextToPrinter(tempResult, printer, printerSettings);
                    if (blnFooterSectionExists)
                    {
                        tempResult.Remove(counter + "/" + intNumberOfTotalItems);
                        counter++;
                        tempResult.Add(counter + "/" + intNumberOfTotalItems);
                    }
                }
                if (blnFooterSectionExists)
                {
                    tempResult.Remove(counter + "/" + intNumberOfTotalItems);
                    counter--;
                }
            }
        }

        /// <summary>
        /// kitchen - standard mode
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentReceipt"></param>
        /// <param name="spoolItemsG"></param>
        /// <param name="result"></param>
        /// <param name="printerSettings"></param>
        /// <param name="printer"></param>
        /// <param name="sections"></param>
        /// <param name="totalItems"></param>
        /// <param name="counter"></param>
        private void ProcessKitchenTemplate_KitchenMode(RollerTypeReportModel repTemplate, ReceiptModel currentReceipt, List<KpsSpooler> spoolItemsG, List<string> result, KitchenPrinterModel printerSettings, Printer printer, List<ReportSectionsModel> sections, int totalItems, int counter)
        {
            foreach (var spoolItemsl in spoolItemsG)
            {
                List<string> tempResult = new List<string>();
                counter++;
                List<KpsSpooler> spoolItems = new List<KpsSpooler>();
                spoolItems.Add(spoolItemsl);
                foreach (var section in sections.Where(f => f.SectionType != (int)SectionTypeEnums.Extras))
                {
                    if (section != null)
                    {
                        if (section.SectionType == (int)SectionTypeEnums.Details)
                        {
                            // group items by region group 
                            var spoolIds = spoolItems.Select(ff => ff.ItemDescription);
                            var flat = currentReceipt.Details.Where(f => spoolIds.Contains(f.ItemDescr));
                            var regionGroups = from d in flat
                                               group d by d.ItemRegion into dd
                                               select new
                                               {
                                                   RegionName = dd.Key,
                                                   RegionPosition = dd.Select(f => f.RegionPosition).FirstOrDefault(),
                                                   RegionItems = dd.Select(f => f)
                                               };
                            string CurrentRegionName = string.Empty;
                            foreach (var region in regionGroups.OrderBy(f => f.RegionPosition))
                            {
                                var regionitemsCount = (region.RegionItems != null) ? region.RegionItems.Count() : 0;
                                var regionCounter = 0;
                                foreach (var items in region.RegionItems)
                                {
                                    bool ignoredRegion = false;
                                    ignoredRegion = section.SectionRows.Select(f => f.SectionColumns).Where(g => g.Count(ggg => ggg.ColumnText == "@ItemRegion") > 0).Count() == 0;
                                    if (CurrentRegionName != items.ItemRegion)
                                    {
                                        // new region item
                                        regionCounter = 1;
                                        // set current region name to items region
                                        CurrentRegionName = items.ItemRegion;
                                        List<string> regionStr = new List<string>();
                                        if (string.IsNullOrEmpty(CurrentRegionName))
                                        {
                                            regionStr = ProcessSection(section.SectionRows, items, printer, true);
                                            tempResult.AddRange(regionStr);
                                        }
                                        else
                                        {
                                            regionStr = ProcessSection(section.SectionRows, items, printer, false);
                                            tempResult.AddRange(regionStr);
                                        }
                                    }
                                    else
                                    {
                                        // item is in current region
                                        regionCounter++;
                                        // add line but ignore region info
                                        tempResult.AddRange(ProcessSection(section.SectionRows, items, printer, true));
                                    }
                                    if (items.Extras.Count > 0)
                                    {
                                        var extrasDoc = repTemplate.Sections.Where(w => w.SectionType == (int)SectionTypeEnums.Extras).FirstOrDefault();
                                        if (extrasDoc != null)
                                        {
                                            foreach (var extra in items.Extras)
                                            {
                                                tempResult.AddRange(ProcessSection(extrasDoc.SectionRows, extra, printer));
                                            }
                                        }
                                    }
                                    if (!ignoredRegion && !string.IsNullOrEmpty(CurrentRegionName) && regionCounter == regionitemsCount && !string.IsNullOrEmpty(items.ItemRegion))
                                    {
                                        tempResult.Add("----------------------------------------");
                                    }
                                }
                            }
                        }
                        else
                        {
                            tempResult.AddRange(ProcessSection(section.SectionRows, currentReceipt, printer));
                        }
                    }
                }
                tempResult.Add(counter + "/" + totalItems);
                result.AddRange(tempResult);
                Task task = Task.Run(() => SendTextToPrinter(tempResult, printer, printerSettings));
            }
        }

        /// <summary>
        /// Process reservation
        /// </summary>
        /// <param name="template"></param>
        /// <param name="reservationModel"></param>
        /// <param name="printResult"></param>
        /// <returns></returns>
        private List<string> ProcessReservation(RollerTypeReportModel template, ExtecrTableReservetionModel reservationModel, PrintResultModel printResult)
        {
            List<string> result = new List<string>();
            printResult.ReceiptNo = reservationModel.Reservation.Id.ToString();
            try
            {
                var sections = template.Sections;
                var resPrinter = availablePrinters.Where(f => f.PrinterType == PrinterTypeEnum.Reservation).FirstOrDefault();
                KitchenPrinterModel printerToPrint = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Reservation);
                var receiptPrinter = printerToPrint;
                //get esc characters
                Printer printer = PrintersEscList.Where(f => f.Name == receiptPrinter.EscapeCharsTemplate).FirstOrDefault();
                foreach (var section in sections)
                {
                    if (section.SectionType == (int)SectionTypeEnums.Header)
                    {
                        ReservationsModel temp = reservationModel.Reservation;
                        string tmpdate = temp.ReservationDate.ToShortDateString();
                        string tmptime = string.Format("{0:00}:{1:00}", temp.ReservationTime.Hours, temp.ReservationTime.Minutes);
                        tmpdate = tmpdate + " " + tmptime;
                        temp.ReservationDate = DateTime.Parse(tmpdate);
                        result.AddRange(ProcessSection(section.SectionRows, reservationModel.Reservation, printer, false, false, null));
                        result.AddRange(ProcessSection(section.SectionRows, reservationModel, printer, false, false, null));
                        result[1] = result[6];
                        int index = result[2].LastIndexOf(":");
                        if (index > 0)
                            result[2] = result[2].Substring(0, index);
                        result.RemoveRange(5, 5);
                    }
                    else if (section.SectionType == (int)SectionTypeEnums.Details)
                    {
                        foreach (ReservationCustomersModel item in reservationModel.ReservationCustomers)
                        {
                            result.AddRange(ProcessSection(section.SectionRows, item, printer, false, false, null));
                        }
                    }
                    else if (section.SectionType == (int)SectionTypeEnums.Footer)
                    {
                        result.AddRange(ProcessSection(section.SectionRows, reservationModel.Reservation, printer, false, false, null));
                    }
                }
                Task task = Task.Run(() => SendTextToPrinter(result, printer, printerToPrint));
                printResult.Status = PrintStatusEnum.Printed;
            }
            catch (CustomException exception)
            {
                printResult.ErrorDescription = exception.Message;
                logger.LogError(ExtcerLogger.logErr("Error printing Reservation receipt  #: " + printResult.ReceiptNo + " Message: ", exception, FiscalName));
                printResult.Status = PrintStatusEnum.Failed;
            }
            catch (Exception exception)
            {
                printResult.ErrorDescription = exception.Message;
                logger.LogError(ExtcerLogger.logErr("Error printing Reservation receipt  #: " + printResult.ReceiptNo + " Message: ", exception, FiscalName));
                printResult.Status = PrintStatusEnum.Failed;
            }
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
            var zPrinter = availablePrinters.Where(f => f.Name == printerSettings.Name && f.PrinterType == PrinterTypeEnum.ZReport).FirstOrDefault();
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
                    else if (section.SectionType == (int)SectionTypeEnums.Lockers)
                    {
                        if (currentZData.Lockers != null && currentZData.Lockers.HasLockers)
                        {
                            var item = currentZData.Lockers;
                            result.AddRange(ProcessSection(section.SectionRows, item, printer));
                        }
                    }
                    else if (section.SectionType == (int)SectionTypeEnums.ProductsForEODStats)
                    {
                        if (currentZData.ProductsForEODStats != null && currentZData.ProductsForEODStats.Count() > 0)
                        {
                            foreach (var item in currentZData.ProductsForEODStats)
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
            Task task = Task.Run(() => SendTextToPrinter(result, printer, printerSettings));
            return result;
        }

        /// <summary>
        /// Process the given section and create the string to print
        /// </summary>
        /// <param name="section"></param>
        /// <param name="obj"></param>
        /// <param name="printer"></param>
        /// <param name="ignoreRegionInfo"></param>
        /// <param name="blnReceiptExtras"></param>
        /// <param name="objReprintData"></param>
        /// <param name="dblFieldMaxWidth"></param>
        /// <param name="blnIsVoid"></param>
        /// <returns></returns>
        private List<string> ProcessSection(List<ReportSectionsRowsModel> section, object obj, Printer printer, bool ignoreRegionInfo = false, bool blnReceiptExtras = false, object objReprintData = null, double dblFieldMaxWidth = 0.0, bool blnIsVoid = false)
        {
            if (objReprintData != null)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(((PrintResultModel)objReprintData).ReceiptCultureInfo);
            }
            // the list of receipt's lines
            List<string> result = new List<string>();
            // loop through lines of the section
            foreach (var line in section)
            {
                if (!ignoreRegionInfo || (ignoreRegionInfo && line.SectionColumns.Count(f => f.ColumnText == "@ItemRegion") == 0))
                {
                    bool blnZeroPriceExtra_SkipLine = false;
                    bool add = true;
                    var strLine = string.Empty;
                    // loop through columns of the current line
                    foreach (var col in line.SectionColumns)
                    {
                        var tempStr = string.Empty;
                        string colText = col.ColumnText ?? "";
                        int colWidth = Convert.ToInt32(col.Width);
                        string data = GenericCommon.ReplacePatterns(obj, colText, blnIsVoid);
                        switch (colText)
                        {
                            case "@ItemDescr":
                                {
                                    string returnitemtempstring = null;
                                    var strTmpClean = strLine.Replace(" ", "");
                                    if (strTmpClean == "-1x")
                                    {
                                        returnitemtempstring = "ΑΛΛΑΓΗ";
                                    }
                                    strTmpClean = strTmpClean.Replace("\t", " ");
                                    strTmpClean = new string(strTmpClean.Where(c => !char.IsPunctuation(c)).ToArray());
                                    if (returnitemtempstring != null)
                                    {
                                        strTmpClean = strTmpClean.Replace("x", "  x ");
                                        strLine = strTmpClean + " " + returnitemtempstring + " ";
                                    }
                                    if (strTmpClean == "0x")
                                    {
                                        strLine = new string(' ', strLine.Length);
                                    }
                                }
                                break;
                            case "@TotalNetVat1":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        data = Convert.ToString(model.TotalNetVat1);
                                    }
                                }
                                break;
                            case "@TotalNetVat2":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        data = Convert.ToString(model.TotalNetVat2);
                                    }
                                }
                                break;
                            case "@TotalNetVat3":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        data = Convert.ToString(model.TotalNetVat3);
                                    }
                                }
                                break;
                            case "@TotalNetVat4":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        data = Convert.ToString(model.TotalNetVat4);
                                    }
                                }
                                break;
                            case "@TotalNetVat5":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        data = Convert.ToString(model.TotalNetVat5);
                                    }
                                }
                                break;
                            case "@Vat1":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        data = Convert.ToString(model.TotalVat1);
                                    }
                                }
                                break;
                            case "@Vat2":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        data = Convert.ToString(model.TotalVat2);
                                    }
                                }
                                break;
                            case "@Vat3":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        data = Convert.ToString(model.TotalVat3);
                                    }
                                }
                                break;
                            case "@Vat4":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        data = Convert.ToString(model.TotalVat4);
                                    }
                                }
                                break;
                            case "@Vat5":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        data = Convert.ToString(model.TotalVat5);
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
                                    bool blnIsReprint = (objReprintData != null);
                                    var date = blnIsReprint ? ((PrintResultModel)objReprintData).ProcessDateTime : DateTime.Now;
                                    if (obj.GetType() == typeof(ZReportModel))
                                    {
                                        var d = obj as ZReportModel;
                                        if (d != null)
                                        {
                                            date = d.dtDateTime;
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
                                    bool blnIsReprint = (objReprintData != null);
                                    var date = blnIsReprint ? ((PrintResultModel)objReprintData).ProcessDateTime : DateTime.Now;
                                    if (obj.GetType() == typeof(ZReportModel))
                                    {
                                        var d = obj as ZReportModel;
                                        if (d != null)
                                        {
                                            date = d.dtDateTime;
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
                            case "@ItemDiscount":
                                {
                                    if (obj.GetType().Name == "ReceiptItemsModel")
                                    {
                                        if (((ReceiptItemsModel)obj).ItemDiscount == null || ((ReceiptItemsModel)obj).ItemDiscount == 0)
                                        {
                                            add = false;
                                        }
                                    }
                                    else if (string.IsNullOrEmpty(data) || Convert.ToDecimal(data) == 0.0m)
                                    {
                                        add = false;
                                    }
                                    else
                                    {
                                        data = "-" + data;
                                    }
                                }
                                break;
                            case "@ItemGross":
                                {
                                    if (blnReceiptExtras)
                                    {
                                        if (data == null || double.Parse(data) == 0.0)
                                        {
                                            blnZeroPriceExtra_SkipLine = col.SkipZeroPrice;
                                        }
                                    }
                                }
                                break;
                            case "@TableTotal":
                                {
                                    if (string.IsNullOrEmpty(data))
                                    {
                                        add = false;
                                    }
                                }
                                break;
                            case "@ItemTotal":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null && model.CreditTransactions.Count() > 0)
                                    {
                                        data = (model.CreditTransactions.FirstOrDefault().Amount ?? 0).ToString("0.00");
                                    }
                                }
                                break;
                            case "@VatSubTotals":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        StringBuilder sbTmp = new StringBuilder(strLine);
                                        foreach (var c in model.CalculateVatPrice)
                                        {
                                            var vatValue = "VAT " + c.VatRate + "%" + "                       " + c.VatPrice.Value.ToString("0.00").PadLeft(5) + " €";
                                            sbTmp.Append(vatValue);
                                            sbTmp.Append("\r\n");
                                            var net = c.Total.Value - c.VatPrice.Value;
                                            var netValue = "NET " + c.VatRate + "%" + "                       " + net.ToString("0.00").PadLeft(5) + " €";
                                            sbTmp.Append(netValue);
                                            sbTmp.Append("\r\n");
                                        }
                                        strLine = sbTmp.ToString();
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        if (data.Length > 0)
                        {
                            if (colWidth == 0)
                            {
                                tempStr = tempStr + data.Trim();
                            }
                            else if (colWidth > data.Length)
                            {
                                tempStr = tempStr + GenericCommon.AlignmentOfSectionField(col, colWidth, data);
                            }
                            else
                            {
                                tempStr += data.Substring(0, colWidth);
                            }
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
                        if (colText == "@RelatedReceipts")
                        {
                            var receiptNumLines = (obj as List<string>);
                            if (colWidth == 0)
                            {
                                // merge invoice numbers to a single line
                                int intFieldMaxWidth = (int)dblFieldMaxWidth;
                                StringBuilder sbTmp = new StringBuilder(strLine);
                                bool blnFirstTime = true;
                                foreach (var receiptNum in receiptNumLines)
                                {
                                    var receiptNumProcessed = receiptNum.Trim();
                                    if (!blnFirstTime)
                                    {
                                        if ((sbTmp.Length % intFieldMaxWidth) + receiptNumProcessed.Length >= intFieldMaxWidth)
                                        {
                                            sbTmp.Append("\r\n");
                                        }
                                        else if (0 < sbTmp.Length)
                                        {
                                            sbTmp.Append(", ");
                                        }
                                    }
                                    sbTmp.Append(receiptNumProcessed);
                                    blnFirstTime = false;
                                }
                                strLine = sbTmp.ToString();
                            }
                            else
                            {
                                // print invoice numbers one line each
                                foreach (var receiptNum in receiptNumLines)
                                {
                                    string strTempLine = GenericCommon.AlignmentOfSectionField(col, colWidth, receiptNum.Trim());
                                    tempStr = tempStr + GenericCommon.SetStringEscChars(printer, strTempLine, col.IsBold, col.IsItalic, col.IsUnderline, col.IsDoubleSize) + "\r\n";
                                }
                            }
                        }
                        strLine = strLine + tempStr;
                    }
                    if (SetFooter != null && strLine.StartsWith("[<]"))
                    {
                        List<string> footer = SetFooter(strLine);
                        result.AddRange(footer);
                    }
                    else
                    {
                        result.Add(strLine);
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
            lock (thisLock)
            {
                logger.LogInformation("In kitchen to printer. Selected printer escape sequence: " + printer.Name);
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
                        logger.LogInformation("In OEM PrintTimes: " + printTimes);
                        for (int i = 1; i <= printTimes; i++)
                        {
                            Encoding utf8 = new UTF8Encoding();
                            Encoding oem737 = Encoding.GetEncoding(737);
                            str = initChar + str + new string('\n', intKitchenHeaderGapLines) + '\r';
                            if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen && printerSettings.UseBuzzer == true)
                            {
                                logger.LogInformation("In OEM Buzzer: " + buzzerEscChars.ToString());
                                str = buzzerEscChars + str;
                            }
                            if (printerSettings.UseCutter == null || printerSettings.UseCutter == true)
                            {
                                str = str + cutterEscChars;
                            }
                            logger.LogInformation("In OEM SendBytesToPrinter");
                            SendBytesToprinter(str, printerSettings.Name, 737);
                        }
                        break;
                    case PrintCharsFormatEnum.ANSI:
                        logger.LogInformation("In ANSI PrintTimes: " + printTimes);
                        for (int i = 1; i <= printTimes; i++)
                        {
                            var toPrint = initChar + str + new string('\n', intKitchenHeaderGapLines) + '\r';
                            if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen && printerSettings.UseBuzzer == true)
                            {
                                logger.LogInformation("In ANSI buzzer: " + buzzerEscChars.ToString());
                                toPrint = buzzerEscChars + toPrint;
                            }
                            if (printerSettings.UseCutter == null || printerSettings.UseCutter == true)
                            {
                                toPrint = toPrint + cutterEscChars;
                            }
                            logger.LogInformation("In ANSI SendBytesToPrinter");
                            RawPrinterHelper.SendStringToPrinter(printerSettings.Name, toPrint);
                        }
                        break;
                    case PrintCharsFormatEnum.GRAPHIC:
                        logger.LogInformation("In Graphic PrintTimes: " + printTimes);
                        for (int i = 1; i <= printTimes; i++)
                        {
                            if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen && printerSettings.UseBuzzer == true)
                            {
                                logger.LogInformation("In OEM graphic: " + buzzerEscChars.ToString());
                                RawPrinterHelper.SendStringToPrinter(printerSettings.Name, buzzerEscChars);
                            }
                            var toPrint = initChar + str + new string('\n', intKitchenHeaderGapLines) + '\r';
                            PrintGraphic(printerSettings.Name, toPrint.ToString(), (7 == intKitchenHeaderGapLines));
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
        }

        /// <summary>
        /// Send given bytes to the raw printer.
        /// </summary>
        /// <param name="tosend"></param>
        /// <param name="printername"></param>
        /// <param name="codePage"></param>
        private void SendBytesToprinter(string tosend, string printername, int codePage)
        {
            Encoding utf8 = new UTF8Encoding();
            Encoding destcodepage = Encoding.GetEncoding(codePage);
            byte[] input_utf8 = utf8.GetBytes(tosend);
            byte[] output_dest = Encoding.Convert(utf8, destcodepage, input_utf8);
            int nLength = Convert.ToInt32(output_dest.Length);
            // Allocate some unmanaged memory for those bytes.
            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.
            Marshal.Copy(output_dest, 0, pUnmanagedBytes, nLength);
            logger.LogInformation("SendBytesToPrinter:" + printername);
            if (!RawPrinterHelper.SendBytesToPrinter(printername, pUnmanagedBytes, nLength))
            {
                logger.LogError("SendBytesToPrinter:" + printername + " -> Failed!");
            }
        }

        /// <summary>
        /// Gets the items per printer to print
        /// </summary>
        /// <param name="artcasObj">receipt model</param>
        /// <returns>a list of kpsSpooler objects</returns>
        private List<KpsSpooler> CalculateWebKps(ReceiptModel artcasObj)
        {
            List<KpsSpooler> kpsSpooler = new List<KpsSpooler>();
            // loop through ArtCas details
            foreach (var item in artcasObj.Details)
            {
                // loop through available kitchen printers
                foreach (var prn in availablePrinters)
                {
                    // if item group can be printed by current printer
                    if (item.KitchenId != null && prn.Groups.Contains((int)item.KitchenId))
                    {
                        // add item to the current printer
                        kpsSpooler.Add(new KpsSpooler()
                        {
                            PrinterName = prn.Name,
                            ItemCode = item.ItemCode,
                            ItemDescription = item.ItemDescr,
                            PrinterType = prn.PrinterType,
                            KitchenId = item.KitchenId
                        });
                    }
                }
            }
            return kpsSpooler;
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
            string line = null;
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