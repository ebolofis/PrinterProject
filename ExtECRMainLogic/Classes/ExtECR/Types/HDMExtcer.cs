using ExtECRMainLogic.Classes.Extenders;
using ExtECRMainLogic.Classes.Loggers;
using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Hubs;
using ExtECRMainLogic.Models.ConfigurationModels;
using ExtECRMainLogic.Models.ExtECRModels;
using ExtECRMainLogic.Models.PrinterModels;
using ExtECRMainLogic.Models.ReceiptModels;
using ExtECRMainLogic.Models.TemplateModels;
using ExtECRMainLogic.Models.ZReportModels;
using GSoft_ComObject;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ExtECRMainLogic.Classes.ExtECR.Types
{
    /// <summary>
    /// HDM Instance
    /// </summary>
    public class HDMExtcer : FiscalManager
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
        private PrintResultModel printResult;
        /// <summary>
        /// 
        /// </summary>
        private HDMErrorCodes HDMErrorCodes;
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
        private readonly ILogger<HDMExtcer> logger;
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
        public HDMExtcer(Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>> printerTemplatesList, List<Printer> printerEscList, List<KitchenPrinterModel> availablePrinters, InstallationDataModel instData, string fiscName, string applicationPath, IConfiguration configuration, IApplicationBuilder applicationBuilder)
        {
            // instance identification
            this.InstanceID = ExtcerTypesEnum.HDM;
            // initialization of variable to be used with lock on printouts
            this.thisLock = new object();
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
            this.HDMErrorCodes = new HDMErrorCodes();
            this.logger = (ILogger<HDMExtcer>)applicationBuilder.ApplicationServices.GetService(typeof(ILogger<HDMExtcer>));
            this.localHubInvoker = (LocalHubInvoker)applicationBuilder.ApplicationServices.GetService(typeof(LocalHubInvoker));
            setupAccountIds();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~HDMExtcer()
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
            printResult = new PrintResultModel();
            printResult.OrderNo = receiptModel.OrderNo;
            printResult.ReceiptNo = receiptModel.ReceiptNo;
            printResult.ExtcerType = ExtcerTypesEnum.HDM;
            printResult.ReceiptType = PrintModeEnum.Receipt;
            ECR_Integratio ComObject = new ECR_Integration();
            ComObject.CreateClient(InstallationData.HDMIP, InstallationData.HDMPort, InstallationData.HDMECR_Password);
            Response1C Connection = ComObject.ConnectClient();
            if (Connection.FatalError != null)
            {
                printResult.Status = PrintStatusEnum.Failed;
                printResult.ErrorDescription = Connection.FatalError;
                logger.LogError(ExtcerLogger.Log(Connection.FatalError, FiscalName));
                return printResult;
            }
            else if (Connection.ErrorCode != 200)
            {
                printResult.Status = PrintStatusEnum.Failed;
                printResult.ErrorDescription = Connection.ErrorCode + "-" + HDMErrorCodes.GetError(Connection.ErrorCode);
                logger.LogError(ExtcerLogger.Log(Connection.ErrorCode + "-" + HDMErrorCodes.GetError(Connection.ErrorCode), FiscalName));
                return printResult;
            }
            OperatorLoginResponse Login = ComObject.OperatorLogin(InstallationData.HDMCaisherLogin, InstallationData.HDMCaisherPassword);
            if (Login.FatalError != null)
            {
                ComObject.DisconnectClient();
                printResult.Status = PrintStatusEnum.Failed;
                printResult.ErrorDescription = Login.FatalError;
                logger.LogError(ExtcerLogger.Log(Login.FatalError, FiscalName));
                return printResult;
            }
            else if (Login.ErrorCode != 200)
            {
                ComObject.DisconnectClient();
                printResult.Status = PrintStatusEnum.Failed;
                printResult.ErrorDescription = Login.ErrorCode + "-" + HDMErrorCodes.GetError(Login.ErrorCode);
                logger.LogError(ExtcerLogger.Log(Login.ErrorCode + "-" + HDMErrorCodes.GetError(Login.ErrorCode), FiscalName));
                return printResult;
            }
            if (receiptModel.IsVoid)
            {
                string[] parts = receiptModel.ExtECRCode.Split('|');
                double cash = 0, card = 0;
                foreach (PaymentTypeModel payment in receiptModel.PaymentsList)
                {
                    if (payment.AccountType == 1)
                        cash = Convert.ToDouble(-payment.Amount);
                    else if (payment.AccountType == 4)
                        card = Convert.ToDouble(-payment.Amount);
                }
                PrintReturnReceiptResponse PrintReturn = ComObject.PrintReturn(parts[0], parts[1], cash, card);
                string json = JsonSerializer.Serialize(PrintReturn);
                if (PrintReturn.FatalError != null)
                {
                    ComObject.OperatorLogout();
                    ComObject.DisconnectClient();
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ErrorDescription = PrintReturn.FatalError;
                    logger.LogError(ExtcerLogger.Log(PrintReturn.FatalError, FiscalName));
                    logger.LogError(ExtcerLogger.Log("PrintReturnReceiptResponse: " + json, FiscalName));
                    return printResult;
                }
                else if (PrintReturn.ErrorCode != 200)
                {
                    ComObject.OperatorLogout();
                    ComObject.DisconnectClient();
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ErrorDescription = PrintReturn.ErrorCode + "-" + HDMErrorCodes.GetError(PrintReturn.ErrorCode);
                    logger.LogError(ExtcerLogger.Log(PrintReturn.ErrorCode + "-" + HDMErrorCodes.GetError(PrintReturn.ErrorCode), FiscalName));
                    logger.LogError(ExtcerLogger.Log("PrintReturnReceiptResponse: " + json, FiscalName));
                    return printResult;
                }
            }
            else
            {
                foreach (ReceiptItemsModel item in receiptModel.Details)
                {
                    if (item.ItemPrice >= 0)
                    {
                        int discountType = 1;
                        if (item.ItemDiscount > 0)
                            discountType = 4;
                        ComObject.AddItem_Unicode(item.ItemDescr, item.ItemCode, 1, item.ItemQty.ToString(), item.ItemPrice.ToString(), item.ItemDiscount.ToString(), "KG", "0101", discountType, 0, "0");
                        foreach (ReceiptExtrasModel extra in item.Extras)
                        {
                            if (extra.ItemPrice > 0)
                            {
                                discountType = 1;
                                if (extra.ItemDiscount > 0)
                                    discountType = 4;
                                ComObject.AddItem_Unicode(extra.ItemDescr, extra.ItemCode, 1, extra.ItemQty.ToString(), extra.ItemPrice.ToString(), extra.ItemDiscount.ToString(), "KG", "0101", discountType, 0, "0");
                            }
                        }
                    }
                }
                string paidAmount = "0", paidAmountCard = "0", partialAmount = "0", prePaymentAmount = "0";
                foreach (PaymentTypeModel payment in receiptModel.PaymentsList)
                {
                    if (payment.AccountType == 1)
                        paidAmount = payment.Amount.ToString();
                    else if (payment.AccountType == 4)
                        paidAmountCard = payment.Amount.ToString();
                }
                PrintReceiptResponse PrtResponse = ComObject.Print(2, paidAmount, paidAmountCard, 1, partialAmount, prePaymentAmount, null);
                string json = JsonSerializer.Serialize(PrtResponse);
                if (PrtResponse.FatalError != null)
                {
                    ComObject.OperatorLogout();
                    ComObject.DisconnectClient();
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ErrorDescription = PrtResponse.FatalError;
                    logger.LogError(ExtcerLogger.Log(PrtResponse.FatalError, FiscalName));
                    logger.LogError(ExtcerLogger.Log("PrintReceiptResponse: " + json, FiscalName));
                    return printResult;
                }
                else if (PrtResponse.ErrorCode != 200)
                {
                    ComObject.OperatorLogout();
                    ComObject.DisconnectClient();
                    printResult.Status = PrintStatusEnum.Failed;
                    printResult.ErrorDescription = PrtResponse.ErrorCode + "-" + HDMErrorCodes.GetError(PrtResponse.ErrorCode);
                    logger.LogError(ExtcerLogger.Log(PrtResponse.ErrorCode + "-" + HDMErrorCodes.GetError(PrtResponse.ErrorCode), FiscalName));
                    logger.LogError(ExtcerLogger.Log("PrintReceiptResponse: " + json, FiscalName));
                    return printResult;
                }
                HDMPrintResponseModel printResponse = JsonSerializer.Deserialize<HDMPrintResponseModel>(json);
                printResult.crn = printResponse.crn;
                printResult.rseq = printResponse.rseq;
            }
            ComObject.OperatorLogout();
            ComObject.DisconnectClient();
            printResult.Status = PrintStatusEnum.Printed;
            return printResult;
        }

        /// <summary>
        /// Print X report interface
        /// </summary>
        /// <param name="zData"></param>
        /// <param name="printResultModel"></param>
        public override void PrintX(ZReportModel zData, out PrintResultModel printResultModel)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long unixTimeStampInTicks = (zData.Day.ToUniversalTime() - unixStart).Ticks;
            double startDate = unixTimeStampInTicks / TimeSpan.TicksPerSecond;
            unixTimeStampInTicks = (zData.dtDateTime.ToUniversalTime() - unixStart).Ticks;
            double endDate = unixTimeStampInTicks / TimeSpan.TicksPerSecond;
            printResultModel = printXZ(1, startDate, endDate);
            printResultModel.ReceiptNo = zData.ReportNo.ToString();
        }

        /// <summary>
        /// Print Z report interface
        /// </summary>
        /// <param name="zData"></param>
        /// <returns></returns>
        public override PrintResultModel PrintZ(ZReportModel zData)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long unixTimeStampInTicks = (zData.Day.ToUniversalTime() - unixStart).Ticks;
            double startDate = unixTimeStampInTicks / TimeSpan.TicksPerSecond;
            unixTimeStampInTicks = (zData.dtDateTime.ToUniversalTime() - unixStart).Ticks;
            double endDate = unixTimeStampInTicks / TimeSpan.TicksPerSecond;
            PrintResultModel result = printXZ(2, startDate, endDate);
            result.ReceiptNo = zData.ReportNo.ToString();
            return result;
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
        /// Print X and Z reports
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private PrintResultModel printXZ(int mode, double startDate, double endDate)
        {
            printResult = new PrintResultModel();
            printResult.ExtcerType = ExtcerTypesEnum.HDM;
            if (mode == 1)
                printResult.ReceiptType = PrintModeEnum.XReport;
            else
                printResult.ReceiptType = PrintModeEnum.ZReport;
            ECR_Integratio ComObject = new ECR_Integration();
            ComObject.CreateClient(InstallationData.HDMIP, InstallationData.HDMPort, InstallationData.HDMECR_Password);
            Response1C Connection = ComObject.ConnectClient();
            if (Connection.FatalError != null)
            {
                printResult.Status = PrintStatusEnum.Failed;
                printResult.ErrorDescription = Connection.FatalError;
                logger.LogError(ExtcerLogger.Log(Connection.FatalError, FiscalName));
                return printResult;
            }
            else if (Connection.ErrorCode != 200)
            {
                printResult.Status = PrintStatusEnum.Failed;
                printResult.ErrorDescription = Connection.ErrorCode + "-" + HDMErrorCodes.GetError(Connection.ErrorCode);
                logger.LogError(ExtcerLogger.Log(Connection.ErrorCode + "-" + HDMErrorCodes.GetError(Connection.ErrorCode), FiscalName));
                return printResult;
            }
            OperatorLoginResponse Login = ComObject.OperatorLogin(InstallationData.HDMCaisherLogin, InstallationData.HDMCaisherPassword);
            if (Login.FatalError != null)
            {
                ComObject.DisconnectClient();
                printResult.Status = PrintStatusEnum.Failed;
                printResult.ErrorDescription = Login.FatalError;
                logger.LogError(ExtcerLogger.Log(Login.FatalError, FiscalName));
                return printResult;
            }
            else if (Login.ErrorCode != 200)
            {
                ComObject.DisconnectClient();
                printResult.Status = PrintStatusEnum.Failed;
                printResult.ErrorDescription = Login.ErrorCode + "-" + HDMErrorCodes.GetError(Login.ErrorCode);
                logger.LogError(ExtcerLogger.Log(Login.ErrorCode + "-" + HDMErrorCodes.GetError(Login.ErrorCode), FiscalName));
                return printResult;
            }
            Response1C receiptReport = ComObject.receiptReport(mode, startDate, endDate, "", "", "");
            string json = JsonSerializer.Serialize(receiptReport);
            if (receiptReport.FatalError != null)
            {
                ComObject.OperatorLogout();
                ComObject.DisconnectClient();
                printResult.Status = PrintStatusEnum.Failed;
                printResult.ErrorDescription = receiptReport.FatalError;
                logger.LogError(ExtcerLogger.Log(receiptReport.ErrorCode + "-" + HDMErrorCodes.GetError(receiptReport.ErrorCode), FiscalName));
                logger.LogError(ExtcerLogger.Log("receiptReport: " + json, FiscalName));
                return printResult;
            }
            else if (receiptReport.ErrorCode != 200)
            {
                ComObject.OperatorLogout();
                ComObject.DisconnectClient();
                printResult.Status = PrintStatusEnum.Failed;
                printResult.ErrorDescription = receiptReport.ErrorCode + "-" + HDMErrorCodes.GetError(receiptReport.ErrorCode);
                printResult.ErrorDescription = receiptReport.FatalError;
                logger.LogError(ExtcerLogger.Log(receiptReport.ErrorCode + "-" + HDMErrorCodes.GetError(receiptReport.ErrorCode), FiscalName));
                logger.LogError(ExtcerLogger.Log("receiptReport: " + json, FiscalName));
                return printResult;
            }
            ComObject.OperatorLogout();
            ComObject.DisconnectClient();
            printResult.Status = PrintStatusEnum.Printed;
            return printResult;
        }

        #endregion
    }
}