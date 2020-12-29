using ExtECRMainLogic.Classes.Extenders;
using ExtECRMainLogic.Classes.Loggers;
using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Hubs;
using ExtECRMainLogic.Models.ConfigurationModels;
using ExtECRMainLogic.Models.PrinterModels;
using ExtECRMainLogic.Models.ReceiptModels;
using ExtECRMainLogic.Models.TemplateModels;
using ExtECRMainLogic.Models.ZReportModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ExtECRMainLogic.Classes.ExtECR.Types
{
    /// <summary>
    /// ZipDitron Instance
    /// </summary>
    public class ZipDitronExtcer : FiscalManager
    {
        #region Properties
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
        private PrintResultModel printResult;
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
        private readonly ILogger<ZipDitronExtcer> logger;
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
        public ZipDitronExtcer(Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>> printerTemplatesList, List<Printer> printerEscList, List<KitchenPrinterModel> availablePrinters, InstallationDataModel instData, string fiscName, string applicationPath, IConfiguration configuration, IApplicationBuilder applicationBuilder)
        {
            // instance identification
            this.InstanceID = ExtcerTypesEnum.ZipDitron;
            // initialization of variable to be used with lock on printouts
            //this.thisLock = new object();
            this.PrintersTemplates = printerTemplatesList;
            this.PrintersEscList = printerEscList;
            this.availablePrinters = availablePrinters;
            this.InstallationData = instData;
            this.FiscalName = fiscName;
            this.applicationPath = applicationPath;
            this.configuration = configuration;
            this.CashAccountIds = new List<int>();
            this.CCAccountIds = new List<int>();
            this.CreditAccountIds = new List<int>();
            this.logger = (ILogger<ZipDitronExtcer>)applicationBuilder.ApplicationServices.GetService(typeof(ILogger<ZipDitronExtcer>));
            this.localHubInvoker = (LocalHubInvoker)applicationBuilder.ApplicationServices.GetService(typeof(LocalHubInvoker));
            setupAccountIds();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~ZipDitronExtcer()
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
        public override PrintResultModel PrintReceipt(ReceiptModel receiptModel, string fiscalName, string strInvoiceNumber)
        {
            PrintResultModel printResult = initPrintResultModel(receiptModel);
            try
            {
                if (receiptModel.IsVoid)
                {
                    printResult.ErrorDescription = "Not Allowed!";
                    printResult.Status = PrintStatusEnum.Failed;
                    return printResult;
                }
                else
                {
                    return PrintReceipt(receiptModel, fiscalName, printResult);
                }
            }
            catch (Exception exception)
            {
                printResult.ErrorDescription = exception.Message;
                printResult.Status = PrintStatusEnum.Failed;
                logger.LogError(ExtcerLogger.logErr("Error printing ZipDitron receipt (4): #" + printResult.ReceiptNo + "  ERRORDESCR: ", exception, FiscalName));
            }
            return printResult;
        }

        /// <summary>
        /// Print X report interface
        /// </summary>
        /// <param name="zDataModel"></param>
        /// <param name="printResult"></param>
        public override void PrintX(ZReportModel zDataModel, out PrintResultModel printResult)
        {
            logger.LogInformation(ExtcerLogger.Log("Printing X...", FiscalName));
            printResult = initPrintResultModel();
            try
            {
                List<string> strArr = new List<string>();
                string errorMsg = "";
                // insert line data
                printXAndZ(0, ref strArr);
                writeAndReadFiles(ref strArr, out errorMsg);
                if (errorMsg.Length > 0)
                {
                    printResult.ErrorDescription = "Invalid file structure!";
                    printResult.Status = PrintStatusEnum.Failed;
                    logger.LogError("Error printing ZipDitron X Report. ERRORDESCR: " + errorMsg);
                }
                else
                {
                    printResult.Status = PrintStatusEnum.Printed;
                }
            }
            catch (Exception exception)
            {
                printResult.ErrorDescription = exception.Message;
                printResult.Status = PrintStatusEnum.Failed;
                logger.LogError(ExtcerLogger.logErr("Error printing ZipDitron X Report. ERRORDESCR: ", exception, FiscalName));
            }
        }

        /// <summary>
        /// Print Z report interface
        /// </summary>
        /// <param name="zData"></param>
        /// <returns></returns>
        public override PrintResultModel PrintZ(ZReportModel zData)
        {
            logger.LogInformation(ExtcerLogger.Log("Printing Z...", FiscalName));
            printResult = initPrintResultModel();
            try
            {
                List<string> strArr = new List<string>();
                string errorMsg = "";
                printXAndZ(1, ref strArr);
                writeAndReadFiles(ref strArr, out errorMsg);
                if (errorMsg.Length > 0)
                {
                    printResult.ErrorDescription = "Invalid file structure!";
                    printResult.Status = PrintStatusEnum.Failed;
                    logger.LogError("Error printing ZipDitron Z report. ERRORDESCR: " + errorMsg);
                }
                else
                {
                    printResult.Status = PrintStatusEnum.Printed;
                }
            }
            catch (Exception exception)
            {
                printResult.ErrorDescription = exception.Message;
                printResult.Status = PrintStatusEnum.Failed;
                logger.LogError(ExtcerLogger.logErr("Error printing ZipDitron Z report. ERRORDESCR: ", exception, FiscalName));
            }
            return printResult;
        }

        /// <summary>
        /// Open drawer interface
        /// </summary>
        public override void OpenDrawer()
        {
            printResult = initPrintResultModel();
            try
            {
                printResult.Status = PrintStatusEnum.Printed;
            }
            catch (Exception exception)
            {
                printResult.ErrorDescription = exception.Message;
                printResult.Status = PrintStatusEnum.Failed;
                logger.LogError(ExtcerLogger.logErr("Error opening Drawer. ERRORDESCR: ", exception, FiscalName));
            }
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
        /// Initialize a new PrintResultModel
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <returns></returns>
        private PrintResultModel initPrintResultModel(ReceiptModel receiptModel = null)
        {
            PrintResultModel printResult = new PrintResultModel();
            printResult.ExtcerType = ExtcerTypesEnum.ZipDitron;
            if (receiptModel != null)
            {
                receiptModel.FiscalType = FiscalTypeEnum.Opos;
                printResult.InvoiceIndex = receiptModel.InvoiceIndex;
                printResult.ReceiptNo = receiptModel.ReceiptNo;
                printResult.PrintType = receiptModel.PrintType;
            }
            return printResult;
        }

        /// <summary>
        /// Print receipt
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="fiscalName"></param>
        /// <param name="printResult"></param>
        /// <returns></returns>
        private PrintResultModel PrintReceipt(ReceiptModel receiptModel, string fiscalName, PrintResultModel printResult)
        {
            try
            {
                logger.LogInformation(ExtcerLogger.Log("Creating Receipt file's contents...", FiscalName));
                List<string> strArr = new List<string>();
                string errorMsg = "";
                //A. insert first line - header
                strArr.Add("CLEAR");
                strArr.Add("KEY REG");
                //B. get receipt items and add them to the file
                createItemsLines(receiptModel, ref strArr);
                if (receiptModel.TotalDiscount > 0)
                    strArr.Add("DISC VALUE=" + string.Format("{0:0.00}", receiptModel.TotalDiscount).Replace(",", ".") + ", SUBTOT");
                //C. footer
                strArr.Add("FOOTER L1='Waiter: " + receiptModel.Waiter + "', L2='POS: " + receiptModel.Pos + " - No: " + receiptModel.ReceiptNo.ToString() + " Type: " + receiptModel.SalesTypeDescription + "'");
                if (receiptModel.PaymentsList.Count > 1)
                {
                    int count = 0;
                    foreach (var item in receiptModel.PaymentsList)
                    {
                        if (CCAccountIds.FirstOrDefault(x => x == (receiptModel.PaymentsList[count].AccountType ?? 0)) == 0)
                            strArr.Add("CLOSE T=1, AMOUNT=" + string.Format("{0:0.00}", item.Amount).Replace(",", "."));
                        else
                            strArr.Add("CLOSE T=5, AMOUNT=" + string.Format("{0:0.00}", item.Amount).Replace(",", "."));
                        count++;
                    }
                }
                else
                {
                    bool isCC = isCreditCard(receiptModel);
                    if (isCC)
                        strArr.Add("CLOSE T=5, AMOUNT=" + string.Format("{0:0.00}", receiptModel.PaymentsList[0].Amount).Replace(",", "."));
                    else
                        strArr.Add("CLOSE T=1, AMOUNT=" + string.Format("{0:0.00}", receiptModel.PaymentsList[0].Amount).Replace(",", "."));
                }
                writeAndReadFiles(ref strArr, out errorMsg);
                if (errorMsg.Length > 0)
                {
                    printResult.ErrorDescription = "Invalid file structure!";
                    printResult.Status = PrintStatusEnum.Failed;
                    logger.LogError("Error Cashing in/out. ERRORDESCR: " + errorMsg);
                }
            }
            catch (Exception exception)
            {
                printResult.ErrorDescription = exception.Message;
                printResult.Status = PrintStatusEnum.Failed;
                logger.LogError(ExtcerLogger.logErr("Error Cashing in/out. ERRORDESCR: ", exception, fiscalName));
            }
            return printResult;
        }

        /// <summary>
        /// Create the lines of items for file
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="btArr"></param>
        private void createItemsLines(ReceiptModel receiptModel, ref List<string> btArr)
        {
            foreach (var item in receiptModel.Details)
            {
                if (item.ItemPrice < 0)
                {
                    if (item.ItemQty > 1)
                        btArr.Add("SALE DPT=" + InstallationData.ZipDitronDpt + ", PRICE=" + string.Format("{0:0.00}", -1 * item.ItemPrice).Replace(",", ".") + ", DES='" + item.ItemDescr.Trim().Replace("'", "") + "', QTY=" + item.ItemQty.ToString("0") + ", REFUND");
                    else
                        btArr.Add("SALE DPT=" + InstallationData.ZipDitronDpt + ", PRICE=" + string.Format("{0:0.00}", -1 * item.ItemPrice).Replace(",", ".") + ", DES='" + item.ItemDescr.Trim().Replace("'", "") + "', REFUND");
                }
                else
                {
                    if (item.ItemQty > 1)
                        btArr.Add("SALE DPT=" + InstallationData.ZipDitronDpt + ", PRICE=" + string.Format("{0:0.00}", item.ItemPrice).Replace(",", ".") + ", DES='" + item.ItemDescr.Trim().Replace("'", "") + "', QTY=" + item.ItemQty.ToString("0"));
                    else
                        btArr.Add("SALE DPT=" + InstallationData.ZipDitronDpt + ", PRICE=" + string.Format("{0:0.00}", item.ItemPrice).Replace(",", ".") + ", DES='" + item.ItemDescr.Trim().Replace("'", "") + "'");
                }
                if (item.ItemDiscount > 0)
                    btArr.Add("DISC VALUE=" + string.Format("{0:0.00}", item.ItemDiscount).Replace(",", "."));
                if (item.Extras.Count > 0)
                {
                    foreach (var extra in item.Extras)
                    {
                        if (extra.ItemPrice < 0)
                        {
                            if (extra.ItemQty > 1)
                                btArr.Add("SALE DPT=" + InstallationData.ZipDitronDpt + ", PRICE=" + string.Format("{0:0.00}", -1 * extra.ItemPrice).Replace(",", ".") + ", DES='" + extra.ItemDescr.Trim().Replace("'", "") + "', QTY=" + extra.ItemQty.ToString("0") + ", REFUND");
                            else
                                btArr.Add("SALE DPT=" + InstallationData.ZipDitronDpt + ", PRICE=" + string.Format("{0:0.00}", -1 * extra.ItemPrice).Replace(",", ".") + ", DES='" + extra.ItemDescr.Trim().Replace("'", "") + "', REFUND");
                        }
                        else
                        {
                            if (extra.ItemQty > 1)
                                btArr.Add("SALE DPT=" + InstallationData.ZipDitronDpt + ", PRICE=" + string.Format("{0:0.00}", extra.ItemPrice).Replace(",", ".") + ", DES='" + extra.ItemDescr.Trim().Replace("'", "") + "', QTY=" + extra.ItemQty.ToString("0"));
                            else
                                btArr.Add("SALE DPT=" + InstallationData.ZipDitronDpt + ", PRICE=" + string.Format("{0:0.00}", extra.ItemPrice).Replace(",", ".") + ", DES='" + extra.ItemDescr.Trim().Replace("'", "") + "'");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Return true if receipt is paid with credid card
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <returns></returns>
        private bool isCreditCard(ReceiptModel receiptModel)
        {
            if (CCAccountIds.FirstOrDefault(x => x == (receiptModel.PaymentsList[0].AccountType ?? 0)) == 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Commands for printX and printZ
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="strArr"></param>
        private void printXAndZ(int mode, ref List<string> strArr)
        {
            strArr.Add("CLEAR");
            strArr.Add("KEY REG");
            strArr.Add("REPORT NUM=1, MODE=" + mode);
        }

        /// <summary>
        /// Write and read files
        /// </summary>
        /// <param name="strArr"></param>
        /// <param name="errorMsg"></param>
        private void writeAndReadFiles(ref List<string> strArr, out string errorMsg)
        {
            if (File.Exists(InstallationData.ZipDitronErrorFilePath))
                deleteErroFileContents();
            else
                File.Create(InstallationData.ZipDitronErrorFilePath).Dispose();
            write(strArr);
            readErrorFile(out errorMsg);
        }

        /// <summary>
        /// Empty error file
        /// </summary>
        private void deleteErroFileContents()
        {
            File.WriteAllText(InstallationData.ZipDitronErrorFilePath, String.Empty);
        }

        /// <summary>
        /// Write to file
        /// </summary>
        /// <param name="btArr"></param>
        private void write(List<string> btArr)
        {
            logger.LogInformation(ExtcerLogger.Log("Writing contents to file...", FiscalName));
            logger.LogDebug(ExtcerLogger.Log(Environment.NewLine + "  " + string.Join(Environment.NewLine + "  ", btArr.ToArray()), FiscalName));
            logger.LogDebug(ExtcerLogger.Log(" ", FiscalName));
            File.WriteAllLines(InstallationData.ZipDitronFilePath, btArr);
        }

        /// <summary>
        /// Read error file
        /// </summary>
        /// <param name="errorMsg"></param>
        private void readErrorFile(out string errorMsg)
        {
            errorMsg = "";
            int i = 0;
            int tries = 5;
            while (i <= tries)
            {
                try
                {
                    Thread.Sleep(1500);
                    string line, error = "";
                    using (var stream = File.Open(InstallationData.ZipDitronErrorFilePath, FileMode.Open))
                    {
                        StreamReader reader = new StreamReader(stream);
                        while ((line = reader.ReadLine()) != null)
                        {
                            error += line.Trim() + Environment.NewLine;
                        }
                        reader.Close();
                    }
                    errorMsg = error;
                    i = tries + 1;
                    if (errorMsg != "")
                        logger.LogError(ExtcerLogger.Log("Error File: " + Environment.NewLine + errorMsg, FiscalName));
                }
                catch (Exception exception)
                {
                    i++;
                    if (i >= tries)
                        throw;
                }
            }
        }

        #endregion
    }
}