using NLog.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ExtECRMainLogic.Classes.Helpers
{
    public static class PaymentTerminalHelper
    {
        /// <summary>
        /// Request fields
        /// </summary>
        private static StringBuilder sessionId, msgType, msgCode, amount, msgOpt, roomNumber, checkoutDate, agreementNum, agreementDate, uniqueTxnId;
        private static int amountLen;
        /// <summary>
        /// Response fields
        /// </summary>
        private static StringBuilder sessionIdResp, msgTypeResp, msgCodeResp, respCodeResp, respMessageResp, cardTypeResp, accNumResp, refNumResp, authCodeResp, batchNumResp, amountResp, msgOptResp, tipAmountResp, foreignAmountResp, foreignCurrencyCodeResp, exchangeRateInclMarkupResp, dccMarkupPercentageResp, dccExchangeDateOfRatetResp, merchantNameResp, addressResp, postalCodeResp, townResp, vatRegNoResp, mccResp, ecrRefNumResp, eftRefNumResp, batchNetAmountResp, batchOrigAmountResp, batchTotalCounterResp, batchTotalsResp, reconciliationModeResp, printResultResp, numOfInstallmentsResp, numOfPostdateInstallments, eftTidResp, bankId, aquirerName, earned, balance, discount, redemption, bonusPoolBlock, midResp, snResp, addtionalMerchantNameResp, additionalAddressResp, cityResp, phoneResp, applicationNameResp, cardExpiryDateResp, posTerminalVersionResp, paymentSpecsResp, AID, TC, transactionType, tokenResp;
        #region  DLL references
        [DllImport("EcrDll.dll", EntryPoint = "init10", CharSet = CharSet.Ansi)]
        public static extern void init10(ref StringBuilder host, int port, bool flag);
        [DllImport("EcrDll.dll", EntryPoint = "sale10", CharSet = CharSet.Ansi)]
        public static extern bool sale10(ref StringBuilder sessionId, ref StringBuilder msgType, ref StringBuilder msgCode, ref StringBuilder uniqueTxnId, ref StringBuilder amount, int amountLen, ref StringBuilder msgOpt, ref StringBuilder roomNumber, ref StringBuilder checkoutDate, ref StringBuilder agreementNum, ref StringBuilder agreementDate, ref StringBuilder sessionIdResp, ref StringBuilder msgTypeResp, ref StringBuilder msgCodeResp, ref StringBuilder respCodeResp, ref StringBuilder respMessageResp, ref StringBuilder cardTypeResp, ref StringBuilder accNumResp, ref StringBuilder refNumResp, ref StringBuilder authCodeResp, ref StringBuilder batchNumResp, ref StringBuilder amountResp, ref StringBuilder msgOptResp, ref StringBuilder tipAmountResp, ref StringBuilder foreignAmountResp, ref StringBuilder foreignCurrencyCodeResp, ref StringBuilder exchangeRateInclMarkupResp, ref StringBuilder dccMarkupPercentageResp, ref StringBuilder dccExchangeDateOfRatetResp, ref StringBuilder eftTidResp);
        #endregion

        /// <summary>
        /// Initialization of DLL. Should be called once before using any operation of DLL.
        /// </summary>
        /// <param name="strAmountToCharge"></param>
        /// <param name="strHostIP"></param>
        /// <param name="strPort"></param>
        public static void EFTPOS_Sale(string strAmountToCharge, string strHostIP, string strPort)
        {
            var logPath = GetConfigurationPath();
            var logger = NLogBuilder.ConfigureNLog(logPath).GetCurrentClassLogger();
            try
            {
                StringBuilder host = new StringBuilder(strHostIP);
                int port = Int32.Parse(strPort);
                init10(ref host, port, false);
                sessionId = new StringBuilder("000000");
                msgType = new StringBuilder("200");
                msgCode = new StringBuilder("00");
                amount = new StringBuilder(strAmountToCharge);
                amountLen = amount.Length;
                msgOpt = new StringBuilder("0000");
                roomNumber = new StringBuilder("1234");
                checkoutDate = new StringBuilder("210311");
                agreementNum = new StringBuilder("12345678");
                agreementDate = new StringBuilder("210311");
                generateUniquId();
                clearFields();
                bool result = sale10(ref sessionId, ref msgType, ref msgCode, ref uniqueTxnId, ref amount, amountLen, ref msgOpt, ref roomNumber, ref checkoutDate, ref agreementNum, ref agreementDate, ref sessionIdResp, ref msgTypeResp, ref msgCodeResp, ref respCodeResp, ref respMessageResp, ref cardTypeResp, ref accNumResp, ref refNumResp, ref authCodeResp, ref batchNumResp, ref amountResp, ref msgOptResp, ref tipAmountResp, ref foreignAmountResp, ref foreignCurrencyCodeResp, ref exchangeRateInclMarkupResp, ref dccMarkupPercentageResp, ref dccExchangeDateOfRatetResp, ref eftTidResp);
                if (result)
                {
                    logger.Info("Payment Terminal @" + strHostIP + ":" + strPort + " -> Sale operation was successfully completed [" + strAmountToCharge + "].");
                }
                else
                {
                    logger.Error("Payment Terminal @" + strHostIP + ":" + strPort + " -> Sale operation was completed with failure [" + strAmountToCharge + "].");
                }
            }
            catch (Exception exception)
            {
                logger.Error("Payment Terminal Error : ", exception);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void generateUniquId()
        {
            DateTime now = DateTime.Now;
            int seconds = now.Millisecond;
            string secondsStr = seconds.ToString();
            uniqueTxnId = new StringBuilder(new string(' ', 32 - secondsStr.Length));
            uniqueTxnId.Append(secondsStr);
        }

        /// <summary>
        /// 
        /// </summary>
        private static void clearFields()
        {
            sessionIdResp = new StringBuilder(new string(' ', 6));
            msgTypeResp = new StringBuilder(new string(' ', 3));
            msgCodeResp = new StringBuilder(new string(' ', 2));
            respCodeResp = new StringBuilder(new string(' ', 2));
            respMessageResp = new StringBuilder(new string(' ', 60));
            cardTypeResp = new StringBuilder(new string(' ', 16));
            accNumResp = new StringBuilder(new string(' ', 23));
            refNumResp = new StringBuilder(new string(' ', 6));
            authCodeResp = new StringBuilder(new string(' ', 6));
            batchNumResp = new StringBuilder(new string(' ', 6));
            amountResp = new StringBuilder(new string(' ', 13));
            msgOptResp = new StringBuilder(new string(' ', 4));
            tipAmountResp = new StringBuilder(new string(' ', 10));
            foreignAmountResp = new StringBuilder(new string(' ', 13));
            foreignCurrencyCodeResp = new StringBuilder(new string(' ', 3));
            exchangeRateInclMarkupResp = new StringBuilder(new string(' ', 9));
            dccMarkupPercentageResp = new StringBuilder(new string(' ', 8));
            dccExchangeDateOfRatetResp = new StringBuilder(new string(' ', 4));
            merchantNameResp = new StringBuilder(new string(' ', 24));
            addressResp = new StringBuilder(new string(' ', 30));
            postalCodeResp = new StringBuilder(new string(' ', 10));
            townResp = new StringBuilder(new string(' ', 24));
            vatRegNoResp = new StringBuilder(new string(' ', 9));
            mccResp = new StringBuilder(new string(' ', 4));
            ecrRefNumResp = new StringBuilder(new string(' ', 6));
            eftRefNumResp = new StringBuilder(new string(' ', 12));
            batchNetAmountResp = new StringBuilder(new string(' ', 13));
            batchOrigAmountResp = new StringBuilder(new string(' ', 13));
            batchTotalCounterResp = new StringBuilder(new string(' ', 4));
            batchTotalsResp = new StringBuilder(new string(' ', 1004));
            reconciliationModeResp = new StringBuilder(new string(' ', 1));
            printResultResp = new StringBuilder(new string(' ', 2));
            numOfInstallmentsResp = new StringBuilder(new string(' ', 2));
            numOfPostdateInstallments = new StringBuilder(new string(' ', 4));
            eftTidResp = new StringBuilder(new string(' ', 12));
            bankId = new StringBuilder(new string(' ', 3));
            aquirerName = new StringBuilder(new string(' ', 10));
            earned = new StringBuilder(new string(' ', 6));
            balance = new StringBuilder(new string(' ', 13));
            discount = new StringBuilder(new string(' ', 13));
            redemption = new StringBuilder(new string(' ', 13));
            bonusPoolBlock = new StringBuilder(new string(' ', 1004));
            midResp = new StringBuilder(new string(' ', 12));
            snResp = new StringBuilder(new string(' ', 12));
            addtionalMerchantNameResp = new StringBuilder(new string(' ', 40));
            additionalAddressResp = new StringBuilder(new string(' ', 40));
            cityResp = new StringBuilder(new string(' ', 40));
            phoneResp = new StringBuilder(new string(' ', 25));
            applicationNameResp = new StringBuilder(new string(' ', 16));
            cardExpiryDateResp = new StringBuilder(new string(' ', 4));
            posTerminalVersionResp = new StringBuilder(new string(' ', 9));
            paymentSpecsResp = new StringBuilder(new string(' ', 3));
            AID = new StringBuilder(new string(' ', 20));
            TC = new StringBuilder(new string(' ', 16));
            transactionType = new StringBuilder(new string(' ', 20));
            tokenResp = new StringBuilder(new string(' ', 60));
        }

        /// <summary>
        /// Get log configuration from application path
        /// </summary>
        /// <returns></returns>
        private static string GetConfigurationPath()
        {
            string path;
            var currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var pathComponents = new List<string>() { currentPath, "..", "..", "..", "Config" };
            var logPath = Path.GetFullPath(Path.Combine(pathComponents.ToArray()));
            if (Directory.Exists(logPath))
                path = Path.Combine(logPath, "NLog.config");
            else
                path = Path.Combine(currentPath, "Config", "NLog.config");
            return path;
        }
    }
}