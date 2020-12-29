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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ExtECRMainLogic.Classes.ExtECR.Types
{
    /// <summary>
    /// Synergy PF500 Instance
    /// </summary>
    public class SynergyPF500Extcer : FiscalManager
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
        string[] Vats;
        /// <summary>
        /// 
        /// </summary>
        private PrintResultModel printResult;
        /// <summary>
        /// 
        /// </summary>
        int commandIndex = 32;
        /// <summary>
        /// 
        /// </summary>
        int minCommandVal = 32;
        /// <summary>
        /// 
        /// </summary>
        int maxCommandVal = 126;
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
        private readonly ILogger<SynergyPF500Extcer> logger;
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
        public SynergyPF500Extcer(Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>> printerTemplatesList, List<Printer> printerEscList, List<KitchenPrinterModel> availablePrinters, InstallationDataModel instData, string fiscName, string applicationPath, IConfiguration configuration, IApplicationBuilder applicationBuilder)
        {
            // instance identification
            this.InstanceID = ExtcerTypesEnum.SynergyPF500;
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
            this.logger = (ILogger<SynergyPF500Extcer>)applicationBuilder.ApplicationServices.GetService(typeof(ILogger<SynergyPF500Extcer>));
            this.localHubInvoker = (LocalHubInvoker)applicationBuilder.ApplicationServices.GetService(typeof(LocalHubInvoker));
            setupAccountIds();
            ReadVatRates();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~SynergyPF500Extcer()
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
                ReadVatRates();
                if (receiptModel.IsVoid)
                {
                    return PrintVoidReceipt(receiptModel, fiscalName, printResult);
                }
                else if (receiptModel.Details[0].ItemCode == "871" || receiptModel.Details[0].ItemCode == "872")
                {
                    return CashInOut(receiptModel, fiscalName, printResult);
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
                logger.LogError(ExtcerLogger.logErr("Error printing SynergyPF500 receipt (4): #" + printResult.ReceiptNo + "  ERRORDESCR: ", exception, FiscalName));
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
            string appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            try
            {
                string XCommand = GetCommandChar().ToString() + (char)69 + "3";
                File.WriteAllText("PF500.in", XCommand, Encoding.GetEncoding(1251));
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = appDirectory + @"\fiscal32.exe";
                startInfo.Arguments = appDirectory + @"\PF500.in";
                Process.Start(startInfo);
            }
            catch (Exception exception)
            {
                printResult.ErrorDescription = exception.Message;
                printResult.Status = PrintStatusEnum.Failed;
                logger.LogError(ExtcerLogger.logErr("Error printing SynergyPF500 X Report. ERRORDESCR: ", exception, FiscalName));
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
            string appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            try
            {
                string ZCommand = " " + (char)69 + Environment.NewLine;
                File.WriteAllText("PF500.in", ZCommand, Encoding.GetEncoding(1251));
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = appDirectory + @"\fiscal32.exe";
                startInfo.Arguments = appDirectory + @"\PF500.in";
                Process.Start(startInfo);
            }
            catch (Exception exception)
            {
                printResult.ErrorDescription = exception.Message;
                printResult.Status = PrintStatusEnum.Failed;
                logger.LogError(ExtcerLogger.logErr("Error printing SynergyPF500 Z report. ERRORDESCR: ", exception, FiscalName));
            }
            return printResult;
        }

        /// <summary>
        /// Open drawer interface
        /// </summary>
        public override void OpenDrawer()
        {
            printResult = initPrintResultModel();
            string appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            try
            {
                string command = "!j6" + Environment.NewLine;
                File.WriteAllText("PF500.in", command, Encoding.GetEncoding(1251));
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = appDirectory + @"\fiscal32.exe";
                startInfo.Arguments = appDirectory + @"\PF500.in";
                Process.Start(startInfo);
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
        /// Read VAT rates. Results are stored into array VATS.
        /// </summary>
        private void ReadVatRates()
        {
            logger.LogInformation(ExtcerLogger.Log("SynergyPF500: Reading VAT Rates [" + InstallationData.VAT + "]...", FiscalName));
            Vats = InstallationData.VAT.Split(',').ToArray();
        }

        /// <summary>
        /// Initialize a new PrintResultModel
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <returns></returns>
        private PrintResultModel initPrintResultModel(ReceiptModel receiptModel = null)
        {
            PrintResultModel printResult = new PrintResultModel();
            printResult.ExtcerType = ExtcerTypesEnum.SynergyPF500;
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
            //codePage = 855: code page used under DOS to write Cyrillic script. At one time it was widely used in Serbia, Macedonia and Bulgaria, but it never caught on in Russia.
            //codePage = 1251: Windows Cyrillic (Slavic)
            logger.LogInformation(ExtcerLogger.Log("Creating Receipt file's contents...", FiscalName));
            List<byte[]> btArr = new List<byte[]>();
            decimal total = 0;
            bool isCC = isCreditCard(receiptModel);
            //A. insert first line - header
            btArr.Add(Encoding.GetEncoding(866).GetBytes(" 01,0000,1" + Environment.NewLine));
            //B. get receipt items and add them to the PF500.IN file
            createItemsLines(receiptModel, ref btArr, fiscalName, ref total);
            //C. footer
            if (isCC)
                btArr.Add(Encoding.GetEncoding(855).GetBytes(Environment.NewLine + "$5" + (char)9 + "N+" + total.ToString("#.00", CultureInfo.InvariantCulture) + Environment.NewLine + "%8"));
            else
                btArr.Add(Encoding.GetEncoding(855).GetBytes(Environment.NewLine + "$5" + (char)9 + Environment.NewLine + "%8"));
            btArr.Add(Encoding.GetEncoding(855).GetBytes(Environment.NewLine + "!j6")); //>>>>---->>  Opens Drawer
            //D. write to file PF500.in and run the fiscal.exe to print the receipt
            writeEndExecute(btArr);
            printResult.Status = PrintStatusEnum.Printed;
            return printResult;
        }

        /// <summary>
        /// Print void receipt
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="fiscalName"></param>
        /// <param name="printResult"></param>
        /// <returns></returns>
        private PrintResultModel PrintVoidReceipt(ReceiptModel receiptModel, string fiscalName, PrintResultModel printResult)
        {
            //codePage = 855: code page used under DOS to write Cyrillic script. At one time it was widely used in Serbia, Macedonia and Bulgaria, but it never caught on in Russia.
            //codePage = 1251: Windows Cyrillic (Slavic)
            logger.LogInformation(ExtcerLogger.Log("Creating Void file's contents...", FiscalName));
            bool printSpecialChar = true;
            List<byte[]> btArr = new List<byte[]>();
            decimal total = 0;
            bool isCC = isCreditCard(receiptModel);
            //A. insert first line - header
            btArr.Add(Encoding.GetEncoding(866).GetBytes(" " + (char)85 + "1,0000,1" + Environment.NewLine));
            //B. get receipt items and add them to the PF500.IN file (DO NOT add the domestic char '@', set useDomestic=false)
            createItemsLines(receiptModel, ref btArr, fiscalName, ref total, false);
            //C. footer
            if (isCC)
                btArr.Add(Encoding.GetEncoding(855).GetBytes(Environment.NewLine + "$5" + (char)9 + "N+" + total.ToString("#.00", CultureInfo.InvariantCulture) + Environment.NewLine + "%8"));
            else
                btArr.Add(Encoding.GetEncoding(855).GetBytes(Environment.NewLine + "$5" + (char)9 + Environment.NewLine + "%8"));
            btArr.Add(Encoding.GetEncoding(855).GetBytes(Environment.NewLine + " " + (char)86));
            //D. write to file PF500.in and run the fiscal.exe to print the receipt
            writeEndExecute(btArr);
            printResult.Status = PrintStatusEnum.Printed;
            return printResult;
        }

        /// <summary>
        /// Print cash in/out
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="fiscalName"></param>
        /// <param name="printResult"></param>
        /// <returns></returns>
        private PrintResultModel CashInOut(ReceiptModel receiptModel, string fiscalName, PrintResultModel printResult)
        {
            string appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            try
            {
                string command = "";
                if (receiptModel.Details[0].ItemCode == "871")
                {
                    // cash out
                    logger.LogInformation(ExtcerLogger.Log("Creating Cachout file...", fiscalName));
                    command = "!F-" + receiptModel.Total.ToString("#.00", CultureInfo.InvariantCulture);
                }
                else
                {
                    // cash in
                    logger.LogInformation(ExtcerLogger.Log("Creating Cachin file...", fiscalName));
                    command = "\"F" + receiptModel.Total.ToString("#.00", CultureInfo.InvariantCulture);
                }
                File.WriteAllText("PF500.in", command, Encoding.GetEncoding(1251));
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = appDirectory + @"\fiscal32.exe";
                startInfo.Arguments = appDirectory + @"\PF500.in";
                Process.Start(startInfo);
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
        /// <param name="fiscalName"></param>
        /// <param name="total"></param>
        /// <param name="useDomestic"></param>
        private void createItemsLines(ReceiptModel receiptModel, ref List<byte[]> btArr, string fiscalName, ref decimal total, bool useDomestic = true)
        {
            bool printSpecialChar = true;
            string sc = " ";
            decimal dicoundPerCent = 0;
            foreach (var item in receiptModel.Details)
            {
                //1. calculate item discount
                if (item.ItemGross > 0)
                    dicoundPerCent = (item.ItemGross - item.ItemTotal) * 100 / item.ItemGross;
                else
                    dicoundPerCent = 0;
                //2. calculate total price taking care of discount
                total = total + item.ItemGross * (100 - dicoundPerCent) / 100;
                //3. set special character 
                sc = (printSpecialChar) ? "`" : " ";
                // change flag for the rest of the items
                printSpecialChar = !printSpecialChar;
                //4. create line data (special character + 1 + description + TAB)
                btArr.Add(Encoding.GetEncoding(855).GetBytes(sc + '1'));
                btArr.Add(Encoding.GetEncoding(1251).GetBytes(item.ItemDescr.Trim()));
                btArr.Add(new byte[] { 9 });
                //5. set domestic code
                if (item.SalesDescription.EndsWith("@") && (useDomestic))
                    btArr.Add(Encoding.GetEncoding(855).GetBytes("@"));
                //6. insert character related to VAT category
                btArr.Add(getVatChar(item.ItemVatDesc));
                //7. add item price and quantity
                btArr.Add(Encoding.GetEncoding(855).GetBytes(item.ItemPrice.ToString("#.00", CultureInfo.InvariantCulture) + '*' + item.ItemQty.ToString("#.000", CultureInfo.InvariantCulture)));
                //8. add discount data if available   
                if (dicoundPerCent > 0)
                {
                    btArr.Add(Encoding.GetEncoding(855).GetBytes(",-" + dicoundPerCent.ToString("#.00", CultureInfo.InvariantCulture)));
                }
                //9. insert CRLF to the end of item line
                btArr.Add(Encoding.GetEncoding(855).GetBytes(Environment.NewLine));
                foreach (ReceiptExtrasModel extra in item.Extras)
                {
                    AddExtra(extra, ref printSpecialChar, ref total, ref btArr, item.SalesDescription, useDomestic);
                }
            }
        }

        /// <summary>
        /// Add item's extras
        /// </summary>
        /// <param name="item"></param>
        /// <param name="printSpecialChar"></param>
        /// <param name="total"></param>
        /// <param name="btArr"></param>
        /// <param name="salesDescription"></param>
        /// <param name="useDomestic"></param>
        private void AddExtra(ReceiptExtrasModel item, ref bool printSpecialChar, ref decimal total, ref List<byte[]> btArr, string salesDescription, bool useDomestic)
        {
            string sc = " ";
            decimal dicoundPerCent = 0;
            decimal price = item.ItemQty * item.ItemPrice ?? 0;
            //1. calculate item discount
            if (price > 0)
                dicoundPerCent = (price - (item.ItemGross ?? 0)) * 100 / price;
            //2. calculate total price taking care of discount
            total = total + price * (100 - dicoundPerCent) / 100;
            //3. set special character 
            sc = (printSpecialChar) ? "`" : " ";
            // change flag for the rest of the items
            printSpecialChar = !printSpecialChar;
            //4. create line data (special character + 1 + description + TAB)
            btArr.Add(Encoding.GetEncoding(855).GetBytes(sc + '1'));
            btArr.Add(Encoding.GetEncoding(1251).GetBytes(item.ItemDescr.Trim()));
            btArr.Add(new byte[] { 9 });
            //5. set domestic code (based on item's salesDescription, not extras description)
            if (salesDescription.EndsWith("@") && (useDomestic))
                btArr.Add(Encoding.GetEncoding(855).GetBytes("@"));
            //6. insert character related to VAT category
            btArr.Add(getVatChar(item.ItemVatDesc));
            //7. add item price and quantity
            btArr.Add(Encoding.GetEncoding(855).GetBytes((item.ItemPrice ?? 0).ToString("#.00", CultureInfo.InvariantCulture) + '*' + item.ItemQty.ToString("#.000", CultureInfo.InvariantCulture)));
            //8. add discount data if available   
            if (dicoundPerCent > 0)
            {
                btArr.Add(Encoding.GetEncoding(855).GetBytes(",-" + dicoundPerCent.ToString("#.00", CultureInfo.InvariantCulture)));
            }
            //9. insert CRLF to the end of item line
            btArr.Add(Encoding.GetEncoding(855).GetBytes(Environment.NewLine));
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
        /// Return the Vat Character (А:192, Б:193,...) as byte
        /// </summary>
        /// <param name="vatDesc"></param>
        /// <returns></returns>
        private byte[] getVatChar(string vatDesc)
        {
            int index = Array.IndexOf(Vats, vatDesc);
            if (index == -1)
            {
                logger.LogWarning("NOT able to find vat " + vatDesc + " into VAT list set for PF500 printer. See Installation Data again");
                index = 0;
            }
            int ascii = 192 + index;
            if (index <= 0)
                return new byte[] { 192 };
            else if (index == 1)
                return new byte[] { 193 };
            else if (index == 2)
                return new byte[] { 194 };
            else if (index == 3)
                return new byte[] { 195 };
            else
                return new byte[] { 196 };
        }

        /// <summary>
        /// Get command character
        /// </summary>
        /// <returns></returns>
        private char GetCommandChar()
        {
            commandIndex++;
            if (commandIndex > maxCommandVal)
            {
                commandIndex = minCommandVal;
                return (char)commandIndex;
            }
            return (char)commandIndex;
        }

        /// <summary>
        /// Write to file PF500.in  and execute fiscal32.exe
        /// </summary>
        /// <param name="btArr"></param>
        private void writeEndExecute(List<byte[]> btArr)
        {
            int count = 0;
            foreach (var row in btArr)
            {
                count += row.Length;
            }
            byte[] byteArr = new byte[count];
            int i = 0;
            foreach (var row in btArr)
            {
                foreach (var data in row)
                {
                    byteArr[i] = data;
                    i++;
                }
            }
            logger.LogInformation(ExtcerLogger.Log("Writing contents to file PF500.in...", FiscalName));
            File.WriteAllBytes(@"PF500.in", byteArr);
            string appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = appDirectory + @"\fiscal32.exe";
            startInfo.Arguments = appDirectory + @"\PF500.in";
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            logger.LogInformation("Running: " + startInfo.FileName);
            logger.LogInformation(ExtcerLogger.Log("Executing " + startInfo.FileName + " " + startInfo.Arguments, FiscalName));
            Process.Start(startInfo);
        }

        #endregion
    }
}