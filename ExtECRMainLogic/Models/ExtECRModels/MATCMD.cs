using ExtECRMainLogic.Classes.Loggers;
using ExtECRMainLogic.Enumerators.OPOS3;
using NLog;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ExtECRMainLogic.Models.ExtECRModels
{
    /// <summary>
    /// Format and Send Commands to an OPOS3 fiscal devise through COM Port
    /// </summary>
    public class MATCMD
    {
        #region Properties
        /// <summary>
        /// 
        /// </summary>
        private static int intLastErrorRecorded;
        /// <summary>
        /// true if the receipt is printed ok
        /// </summary>
        public bool blnEndFiscalReceiptOk;
        /// <summary>
        /// 
        /// </summary>
        short StatusCode;
        /// <summary>
        /// 
        /// </summary>
        public decimal[] VATS;
        /// <summary>
        /// 
        /// </summary>
        public Serial SerialCM;
        /// <summary>
        /// The device status if available, else null
        /// </summary>
        public byte? deviceStatus;
        /// <summary>
        /// The fiscal status if available, else null
        /// </summary>
        public byte? fiscalStatus;
        /// <summary>
        /// Greek Character Conversion Look-Up-Table
        /// </summary>
        private char[] LUT_UnicodeTo1253 =
            {
            (char)0xB4,        // 0x384 -> '΄'
            (char)0xA1,        // 0x385 -> '΅'
            (char)0xA2,        // 0x386 -> 'Ά'
            (char)0xB7,        // 0x387 -> '.'
            (char)0xB8,        // 0x388 -> 'Έ'
            (char)0xB9,        // 0x389 -> 'Ή'
            (char)0xBA,        // 0x38A -> 'Ί'
            (char)0xBB,        // 0x38B -> '>>'
            (char)0xBC,        // 0x38C -> 'Ό'
            (char)0xBD,        // 0x38D -> '1/2'
            (char)0xBE,        // 0x38E -> 'Ύ'
            (char)0xBF,        // 0x38F -> 'Ώ'
            (char)0xC0,        // 0x390 -> 'ΐ'
            (char)0xC1,        // 0x391 -> 'Α'
            (char)0xC2,        // 0x392 -> 'Β'
            (char)0xC3,        // 0x393 -> 'Γ'
            (char)0xC4,        // 0x394 -> 'Δ'
            (char)0xC5,        // 0x395 -> 'Ε'
            (char)0xC6,        // 0x396 -> 'Ζ'
            (char)0xC7,        // 0x397 -> 'Η'
            (char)0xC8,        // 0x398 -> 'Θ'
            (char)0xC9,        // 0x399 -> 'Ι'
            (char)0xCA,        // 0x39A -> 'Κ'
            (char)0xCB,        // 0x39B -> 'Λ'
            (char)0xCC,        // 0x39C -> 'Μ'
            (char)0xCD,        // 0x39D -> 'Ν'
            (char)0xCE,        // 0x39E -> 'Ξ'
            (char)0xCF,        // 0x39F -> 'Ο'
            (char)0xD0,        // 0x3A0 -> 'Π'
            (char)0xD1,        // 0x3A1 -> 'Ρ'
            (char)0xD2,        // 0x3A2 -> '???'
            (char)0xD3,        // 0x3A3 -> 'Σ'
            (char)0xD4,        // 0x3A4 -> 'Τ'
            (char)0xD5,        // 0x3A5 -> 'Υ'
            (char)0xD6,        // 0x3A6 -> 'Φ'
            (char)0xD7,        // 0x3A7 -> 'Χ'
            (char)0xD8,        // 0x3A8 -> 'Ψ'
            (char)0xD9,        // 0x3A9 -> 'Ω'
            (char)0xDA,        // 0x3AA -> 'Ϊ'
            (char)0xDB,        // 0x3AB -> 'Ϋ'
            (char)0xDC,        // 0x3AC -> 'ά'
            (char)0xDD,        // 0x3AD -> 'έ'
            (char)0xDE,        // 0x3AE -> 'ή'
            (char)0xDF,        // 0x3AF -> 'ί'
            (char)0xE0,        // 0x3B0 -> 'ΰ'
            (char)0xE1,        // 0x3B1 -> 'α'
            (char)0xE2,        // 0x3B2 -> 'β'
            (char)0xE3,        // 0x3B3 -> 'γ'
            (char)0xE4,        // 0x3B4 -> 'δ'
            (char)0xE5,        // 0x3B5 -> 'ε'
            (char)0xE6,        // 0x3B6 -> 'ζ'
            (char)0xE7,        // 0x3B7 -> 'η'
            (char)0xE8,        // 0x3B8 -> 'θ'
            (char)0xE9,        // 0x3B9 -> 'ι'
            (char)0xEA,        // 0x3BA -> 'κ'
            (char)0xEB,        // 0x3BB -> 'λ'
            (char)0xEC,        // 0x3BC -> 'μ'
            (char)0xED,        // 0x3BD -> 'ν'
            (char)0xEE,        // 0x3BE -> 'ξ'
            (char)0xEF,        // 0x3BF -> 'ο'
            (char)0xF0,        // 0x3C0 -> 'π'
            (char)0xF1,        // 0x3C1 -> 'ρ'
            (char)0xF2,        // 0x3C2 -> 'ς'
            (char)0xF3,        // 0x3C3 -> 'σ'
            (char)0xF4,        // 0x3C4 -> 'τ'
            (char)0xF5,        // 0x3C5 -> 'υ'
            (char)0xF6,        // 0x3C6 -> 'φ'
            (char)0xF7,        // 0x3C7 -> 'χ'
            (char)0xF8,        // 0x3C8 -> 'ψ'
            (char)0xF9,        // 0x3C9 -> 'ω'
            (char)0xFA,        // 0x3CA -> 'ϊ'
            (char)0xFB,        // 0x3CB -> 'ϋ'
            (char)0xFC,        // 0x3CC -> 'ό'
            (char)0xFD,        // 0x3CD -> 'ύ'
            (char)0xFE         // 0x3CE -> 'ώ'
        };
        /// <summary>
        /// error code   --   message   --   suggested action
        /// </summary>
        private string[,] strErrorMessage =
            {
            /* 0x00 */   { "No errors - success",                         "None" }
            /* 0x01 */ , { "Wrong number of fields",                      "Check the command's field count" }
            /* 0x02 */ , { "Field too long",                              "A field is long: check it & retry" }
            /* 0x03 */ , { "Field too small",                             "A field is small: check it & retry" }
            /* 0x04 */ , { "Field fixed size mismatch",                   "A field size is wrong: check it & retry" }
            /* 0x05 */ , { "Field range or type check failed",            "Check ranges or types in command" }
            /* 0x06 */ , { "Bad request code",                            "Correct the request code (unknown)" }
            /* 0x07 */ , { "Fiscal Record Number error",                  "The requested fiscal record number is wrong" }
            /* 0x08 */ , { "Fiscal Record Type error",                    "The requested fiscal record type is wrong" }
            /* 0x09 */ , { "Printing type bad",                           "Correct the specified printing style" }
            /* 0x0A */ , { "Cannot execute with day open",                "Issue a Z report to close the day" }
            /* 0x0B */ , { "RTC programming requires jumper",             "Short the 'clock' jumper and retry" }
            /* 0x0C */ , { "RTC date or time invalid",                    "Check the date/time range. Also check if date is prior to a date of a fiscal record" }
            /* 0x0D */ , { "No records in fiscal period",                 "No suggested action; the operation cannot be executed in the specified period" }
            /* 0x0E */ , { "Device is busy in another task",              "Wait for the device to get ready" }
            /* 0x0F */ , { "No more header records allowed",              "No suggested action; the header programming cannot be executed because the Fiscal memory cannot hold more records" }
            /* 0x10 */ , { "Cannot execute with block open",              "The specified command requires no open signature block for proceeding. Close the block and retry" }
            /* 0x11 */ , { "Transaction not opened",                      "Open a transaction first" }
            /* 0x12 */ , { "Sign Data Error",                             "Error in signing the electronic data" }
            /* 0x13 */ , { "Sign Error",                                  "Error in signing" }
            /* 0x14 */ , { "Z closure time limit",                        "Means that 24 hours passed from the last Z closure. Issue a Z and retry" }
            /* 0x15 */ , { "Z closure not found",                         "The specified Z closure number does not exist. Pass an existing Z number" }
            /* 0x16 */ , { "Z closure record bad",                        "The requested Z record is unreadable (damaged). Device requires service" }
            /* 0x17 */ , { "User browsing in progress",                   "The user is accessing the device by manual operation. The protocol usage is suspended until the user terminates the keyboard browsing. Just wait or inform application user." }
            /* 0x18 */ , { "No more Invoice",                             "Take a Z Report in order to continue issuing an invoice" }
            /* 0x19 */ , { "Printer paper end detected",                  "Replace the paper roll and retry" }
            /* 0x1A */ , { "Printer is offline",                          "Printer disconnection. Service required" }
            /* 0x1B */ , { "Fiscal unit is offline",                      "Fiscal disconnection. Service required" }
            /* 0x1C */ , { "Fatal hardware fiscal error",                 "Mostly fiscal errors. Service required" }
            /* 0x1D */ , { "Fiscal unit is full",                         "Need fiscal replacement. Service" }
            /* 0x1E */ , { "No Data for Signature",                       "There are no data to be signed" }
            /* 0x1F */ , { "Signature not in range",                      "The signature number is not in range" }
            /* 0x20 */ , { "Battery fault detected",                      "If problem persists, service required" }
            /* 0x21 */ , { "Open day for signature reprint",              "Close the day to reprint signature" }
            /* 0x22 */ , { "Reprint Signature CMOS error",                "Signature cannot be reprinted due to CMO error. Call service" }
            /* 0x23 */ , { "Real-Time Clock needs programming (This means that the RTC has invalid Data and needs to be reprogrammed. As a consequence, service is needed).", "This means that the RTC has invalid Data and needs to be reprogrammed. As a consequence, service is needed" }
            /* 0x24 */ , { "JUMPERON",                                    "The Jumper are on, They must be removed for the operation to continue." }
            /* 0x25 */ , { "INVSALEOP",                                   "Error Sale type It must be S/V/R" }
            /* 0x26 */ , { "DPTINDEXERR",                                 "Department’s code number out of range (1-5)" }
            /* 0x27 */ , { "VATRATE",                                     "The VAT rate sent by the PC isn’t equal to the Printer’s one" }
            /* 0x28 */ , { "PAYMENTINDEXERR",                             "Payment’s code is out of range (1-3) 1=CASH, 2=CARD, 3=CREDIT" }
            /* 0x29 */ , { "Printer Time Out",                            "Connection with Printer Head cannot be established" }
            /* 0x2A */ , { "COVEROPEN",                                   "The printer tray is opened" }
            /* 0x2B */ , { "SLIP Printer Error",                          "The slip printer is not ready" }
            /* 0x2C */ , { "Printer Head Error",                          "The printer's Head is damaged" }
            /* 0x2D */ , { "Sensor Error",                                "Sensor is damaged" }
            /* 0x2E */ , { "Sensor Reading Error",                        "The Sensor cannot read" }
            /* 0x2F */ , { "NOTENDREADLEGAL",                             "There are illegal receipts in the journal that must be read" }
            /* 0x30 */ , { "NOTENDREADILEGAL",                            "There are legal receipts in the journal that must be read" }
            /* 0x31 */ , { "WRONGILEGALNUMBER",                           "The requested illegal receipt doesn’t exist in the electronic journal" }
            /* 0x32 */ , { "FLASHERROR",                                  "CARD reading problem" }
            /* 0x33 */ , { "NOTFOUNDRECEIPT",                             "The requested legal receipt doesn’t exist in the electronic journal" }
            /* 0x34 */ , { "NOMOREILEGALRECEIP",                          "There are no more receipts to be read in the CARD" }
            /* 0x35 */ , { "NOTSTARTREAD",                                "Printer must first be told about the reading of the CARD before the CARD’s reading begins" }
            /* 0x36 */ , { "NOTFINISHREADRECEIPTDATA",                    "The CARD’s reading isn’t finished" }
            /* 0x37 */ , { "NOTREADFORFOUNDRECEIPT",                      "A record hasn’t been read" }
            /* 0x38 */ , { "ENDREADFLAS",                                 "The CARD’s reading was successful" }
            /* 0x39 */ , { "HWTRAYAGAN",                                  "Error reading the CARD, please try again" }
            /* 0x3A */ , { "NOTSTARTREADFLASH",                           "Printer must first be told about the reading of the CARD before the CARD’s reading begins" }
            /* 0x3B */ , { "NOTFOUNDOPENDAY",                             "DAY isn’t opened and no transactions are present" }
            /* 0x3C */ , { "NOMOREINRECEIPTLINES",                        "No more than 6 comment lines can be printed on the receipt" }
            /* 0x3D */ , { "NOTTRANSFERFLASH",                            "The CARD’s data transfer to the PC isn’t over yet" }
            /* 0x3E */ , { "PRINTERDISCONECT",                            "Printer is disconnected" }
            /* 0x3F */ , { "TRANSACTIONINPROGRES",                        "Another Printer’s function is in progress" }
            /* 0x40 */ , { "TRANSACTIONNOTOPEN",                          "There is no opened receipt" }
            /* 0x41 */ , { "TRANSACTIONISOPEN",                           "There is an opened receipt" }
            /* 0x42 */ , { "NOMOREVAT",                                   "No more VTA codes can be programmed in the fiscal memory" }
            /* 0x43 */ , { "CASHINOPEN",                                  "Cash in is in progress" }
            /* 0x44 */ , { "CASHOUTOPEN",                                 "Cash out is in progress" }
            /* 0x45 */ , { "INPAYMENT",                                   "Payment is in progress" }
            /* 0x46 */ , { "NOZERODM",                                    "No zero Discount/Markup is allowed" }
            /* 0x47 */ , { "MAXDISCOUNTINVAT",                            "Greater Discount than the Printer’s VAT amount" }
            /* 0x48 */ , { "MAXDMINTRANSTOTAL",                           "The discount exceeds the minimum transaction amount" }
            /* 0x49 */ , { "NOTEQUALDMGETSUM",                            "VAT’s allocation’s totals do not match" }
            /* 0x4A */ , { "NEGATIVEVATSALES",                            "No negative sales-transactions are allowed" }
            /* 0x4B */ , { "MUSTCLOSETRANSACTION",                        "The receipt must be closed in order for the function to continue" }
            /* 0x4C */ , { "FLASHFULL",                                   "CARD is full, it must be read" }
            /* 0x4D */ , { "NOZEROVAT",                                   "The VAT rate cannot be 0" }
            /* 0x4E */ , { "NOSANEVATRATE",                               "No equal VAT rates in different categories" }
            /* 0x4F */ , { "NOSALESZEROPRICE",                            "Zero sale’s price cannot occur" }
            /* 0x50 */ , { "NODATAFORPRNX",                               "There are no transactions-A X Report cannot be issued" }
            /* 0x51 */ , { "WORNIGDATE",                                  "DATE/TIME Error. Call service" }
            /* 0x52 */ , { "FLASSTOPWORK",                                "CARD error. The Printer cannot perform sales" }
            /* 0x53 */ , { "NOTVALIDPLU",                                 "PLU Internal Code Error (1-200)" }
            /* 0x54 */ , { "INVALIDCATEGORI",                             "Category Code Error (1-20)" }
            /* 0x55 */ , { "INVALID DPT",                                 "Department Code Error (1-5)" }
            /* 0x56 */ , { "BMP Index Error",                             "The BMP Index Number is not correct" }
            /* 0x57 */ , { "Cutter Error",                                "Turn off the Printer and try again" }
            /* 0x58 */ , { "Recover data from FLASH",                     "The Flash CARD must be read. The machine is in an after-CMOS status" }
            /* 0x59 */ , { "PAYMENT cannot be cancelled",                 "There is no payment amount to be cancelled" }
            /* 0x5A */ , { "ZERO PAYMENT cannot be cancelled",            "A zero payment cannot be cancelled" }
            /* 0x5B */ , { "NOT in Payment Mode",                         "The Printer is not in payment mode" }
            /* 0x5C */ , { "Barcode Data Error",                          "The Barcode Data are not valid" }
            /* 0x5D */ , { "BMP Data Error",                              "The BMP Data are damaged" }
            /* 0x5E */ , { "Clerk index error",                           "Wrong clerk index" }
            /* 0x5F */ , { "Clerk password error",                        "Wrong clerk password" }
            /* 0x60 */ , { "Price Error",                                 "Wrong Price" }
            /* 0x61 */ , { "Invalid DM Type",                             "Invalid Discount/Markup Type" }
            /* 0x62 */ , { "DM Index",                                    "Wrong Discount/Markup Index" }
            /* 0x63 */ , { "NO MORE SALES",                               "Maximum Number of Sales in Receipt" }
            /* 0x64 */ , { "Battery Error",                               "Battery Li error" }
            /* 0x65 */ , { "Clerk access problem",                        "Access Denied for current clerk" }
            /* 0x66 */ , { "Baud Rate",                                   "Wrong Baud Rate" }
            /* 0x67 */ , { "Qty Error",                                   "Quantity error" }
            /* 0x68 */ , { "In Ticket",                                   "After Ticket Discount" }
            /* 0x69 */ , { "Inactive Ticket",                             "The ticket is inactive" }
            /* 0x6A */ , { "DM Limit",                                    "Discount/Markup limit error" }
            /* 0x6B */ , { "Blank Description",                           "Blank Description is not allowed" }
            /* 0x6C */ , { "Barcode Error",                               "Error in barcode" }
            /* 0x6D */ , { "Negative Receipt Total",                      "The receipt cannot close, negative total" }
            /* 0x6E */ , { "Client Index Error",                          "Wrong Client index" }
            /* 0x6F */ , { "Client mot found",                            "Wrong Client code" }
            /* 0x70 */ , { "Payment no change" ,                          "This Payment type cannot give change" }
            /* 0x71 */ , { "Insert Payment amount",                       "Must insert amount for payment" }
            /* 0x72 */ , { "Same Header",                                 "The header is same with previous" }
            /* 0x73 */ , { "In Error",                                    "There is an error and must use printer keyboard" }
            /* 0x74 */ , { "Receipt Limit",                               "Total of receipt exceed the limit" }
            /* 0x75 */ , { "Day Limit",                                   "Daily total sales exceed the limit" }
            /* 0x76 */ , { "Fiscal Communication Error",                  "There is a problem with fiscal communication" }
            /* 0x77 */ , { "NAND FULL",                                   "NAND memory is full" }
            /* 0x78 */ , { "AFM Error",                                   "Wrong AFM" }
            /* 0x79 */ , { "Empty EJ",                                    "The Electronic Journal is empty" }
            /* 0x7A */ , { "Invalid IP",                                  "Invalid IP Address" }
            /* 0x7B */ , { "Invalid Refund",                              "Refund is not allowed" }
            /* 0x7C */ , { "Invalid Void",                                "Void is not allowed" }
            /* 0x7D */ , { "Amount limit",                                "Out of range amount" }
            /* 0x7E */ , { "Empty Header",                                "The header must have at least 1 line" }
            /* 0x7F */ , { "Inactive Clerk",                              "Clerk is inactive" }
            /* 0x80 */ , { "No transactions",                             "There are not daily transactions" }
            /* 0x81 */ , { "Program AFM",                                 "You must programming AFM" }
            /* 0x82 */ , { "Unformatted SD",                              "Format SD fail, SD is unformatted" }
            /* 0x83 */ , { "Time Error",                                  "Wrong Time" }
            /* 0x84 */ , { "Call Technician",                             "You must call Technician" }
            /* 0x85 */ , { "Open EJ file",                                "Cannot open EJ file" }
            /* 0x86 */ , { "Write EJ file",                               "Cannot write EJ file" }
            /* 0x87 */ , { "Read EJ file",                                "Cannot read EJ file" }
            /* 0x88 */ , { "AES Code",                                    "Wrong AES Code" }
            /* 0x89 */ , { "Wrong Coupon",                                "Wrong Coupon Index/Barcode" }
            /* 0x8A */ , { "Ethernet Communication",                      "Error in Ethernet communication" }
            /* 0x8B */ , { "Upload GGPS",                                 "Error while upload files in GGPS" }
            /* 0x8C */ , { "File System Error: Cannot open bmp file",     "Retry/Reset" }
            /* 0x8D */ , { "Severe Fiscal Device Error",                  "You have to perform a Reset to the Fiscal Device!" }
        };
        #endregion
        /// <summary>
        /// Logger
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public MATCMD()
        {
            intLastErrorRecorded = -1;
            this.blnEndFiscalReceiptOk = false;
            this.StatusCode = 0;
            this.VATS = new decimal[5];
            var logPath = GetConfigurationPath();
            logger = NLogBuilder.ConfigureNLog(logPath).GetCurrentClassLogger();
        }

        #region Public Methods

        /// <summary>
        /// Reads the VAT rates that have been set in the fiscal device memory and updates the VATS array.
        /// On success returns zero (0), else returns the error code number.
        /// </summary>
        /// <returns></returns>
        public int GetVatRates()
        {
            int ret;
            string reply = string.Empty;
            int trs = 0;
            // wait for available com port
            while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
            if (logger.IsInfoEnabled)
                logger.Info("Read VAT Rates. COMMAND : e/");
            if ((ret = SerialCM.Command_Request("e/", ref reply, out deviceStatus, out fiscalStatus)) != 0)
            {
                MAT_ErrorLogger("Error in GetVatRates", ret);
            }
            string[] ParFields = reply.Split('/');
            for (int i = 0; i < ParFields.Length - 1; i++)
            {
                // parse returned fields
                VATS[i] = decimal.Parse(ParFields[i].Replace(".", ","));
            }
            return ret;
        }

        /// <summary>
        /// Read fiscal device flash memory files and save them at currently defined Journal path.
        /// Returns true on success, else false.
        /// </summary>
        /// <param name="strFiscalName"></param>
        /// <param name="strPathToJournal"></param>
        /// <returns></returns>
        internal bool ReadDeviceFlashMemoryFiles(string strFiscalName, string strPathToJournal)
        {
            StringBuilder strFileContents = new StringBuilder();
            string strReplyFromCommand_Q = string.Empty;
            string strFileName = string.Empty;
            bool blnReadRepeat = true;
            bool ret = false;
            int resultForCommand_Q;
            do
            {
                int trs = 0;
                // wait for available com port
                while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
                // start read each line from fiscal memory
                if (logger.IsDebugEnabled)
                    logger.Debug("Reading flash Memory to create journal files...");
                int intRetryCounter = 0;
                do
                {
                    if (logger.IsDebugEnabled)
                        logger.Debug("Read Flash Memory. COMMAND : Q/");
                    resultForCommand_Q = SerialCM.Command_Request("Q/", ref strReplyFromCommand_Q, out deviceStatus, out fiscalStatus);
                    if (logger.IsDebugEnabled)
                        logger.Debug("Return: " + resultForCommand_Q.ToString() + "\r\n   strReplyFromCommand_Q : " + strReplyFromCommand_Q + "   deviceStatus:" + deviceStatus.ToString() + "\r\n   fiscalStatus:" + fiscalStatus.ToString());
                    intRetryCounter++;
                }
                while (resultForCommand_Q == 14 && intRetryCounter < 10);
                if (resultForCommand_Q != 0)
                {
                    // handle error condition: break loop and exit procedure
                    if (resultForCommand_Q != 256)
                        logger.Error(ExtcerLogger.logErr("Error, cannot execute command 'Q' at fiscal device. Error Code = " + resultForCommand_Q, strFiscalName));
                    else
                        logger.Warn(ExtcerLogger.Log("Info, cannot execute command 'Q'. Code = " + resultForCommand_Q, strFiscalName));
                    break;
                }
                string[] strSplitted = strReplyFromCommand_Q.Split('/');
                switch (strSplitted[0][0])
                {
                    case '0':
                        // it is a file name
                        strFileName = strSplitted[2];
                        break;
                    case '1':
                        // valid file contents
                        strFileContents.Append(strSplitted[1]);
                        break;
                    case '2':
                        // end of file, store file contents to predefined directory
                        if (!SaveFiscalDeviceFile(strFileName, strPathToJournal, strFileContents))
                        {
                            // in case of error while saving files the Z report is failed!
                            logger.Error(ExtcerLogger.logErr("Error, cannot store file to PC HDD [003] ", strFiscalName));
                            return false;
                        }
                        // clean up StringBuilder
                        strFileContents.Clear();
                        break;
                    case '3':
                        // end of fiscal data, if we are here with file contents not empty there is a file to save too!
                        if (strFileContents.Length > 0)
                        {
                            if (!SaveFiscalDeviceFile(strFileName, strPathToJournal, strFileContents))
                            {
                                // in case of error while saving files the Z report is failed!
                                logger.Error(ExtcerLogger.logErr("Error, cannot store file to PC HDD [004] ", strFiscalName));
                                return false;
                            }
                        }
                        blnReadRepeat = false;
                        ret = true;
                        break;
                    default:
                        break;
                }
            }
            while (blnReadRepeat);
            logger.Info("Read Flash Memory end. Return :" + ret.ToString());
            return ret;
        }

        /// <summary>
        /// Sends a command to the OPOS3 fiscal device to start a new receipt.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <returns></returns>
        internal int OpenTransaction()
        {
            SerialCM.ResetBuffers();
            return Transaction(TransactionRequestType.Open);
        }

        /// <summary>
        /// Sends a command to the OPOS3 fiscal device to close an open transaction/receipt.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <returns></returns>
        internal int CloseTransaction()
        {
            if (blnEndFiscalReceiptOk)
            {
                return 0;
            }
            SerialCM.ResetBuffers();
            return Transaction(TransactionRequestType.Close);
        }

        /// <summary>
        /// Sends a command to the OPOS3 fiscal device to force close an open transaction/receipt.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <returns></returns>
        internal int ForceCloseTransaction()
        {
            SerialCM.ResetBuffers();
            return Transaction(TransactionRequestType.Close);
        }

        /// <summary>
        /// Sends a command to the OPOS3 fiscal device to cancel an open transaction/receipt.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <returns></returns>
        internal int CancelTransaction()
        {
            SerialCM.ResetBuffers();
            return Transaction(TransactionRequestType.Cancel);
        }

        /// <summary>
        /// Ask fiscal device about the current status. Returns true if all error condition flags are off, else false.
        /// </summary>
        /// <param name="tries"></param>
        /// <param name="retCode"></param>
        /// <param name="maxTries"></param>
        /// <returns></returns>
        public bool IsDeviceStatusOK(int tries, out short retCode, int maxTries = 40)
        {
            retCode = 0;
            string reply = string.Empty;
            // get device status
            string strCmd1 = string.Format("?/");
            if (logger.IsDebugEnabled)
                logger.Debug("Read Device Status : " + strCmd1);
            if ((retCode = SerialCM.Command_Request(strCmd1, ref reply, out deviceStatus, out fiscalStatus)) != 0)
            {
                // OPOS3 communication problem special handling
                if (retCode == 256 || retCode == 999)
                {
                    if (tries > maxTries)
                    {
                        logger.Error("OPOS3 device communication problem. Check cables/COM port settings/driver settings and retry.");
                        logger.Warn("reply:" + (reply ?? "<NULL>") + ", deviceStatus:" + ((deviceStatus != null) ? deviceStatus.ToString() : "<NULL>") + ", fiscalStatus:" + ((fiscalStatus != null) ? fiscalStatus.ToString() : "<NULL>"));
                        return true;
                    }
                }
                Thread.Sleep(50);
                return false;
            }
            if (deviceStatus == null || deviceStatus == 0)
            {
                return true;
            }
            Thread.Sleep(30);
            return false;
        }

        /// <summary>
        /// Send a command to the OPOS3 fiscal device to start a new receipt.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <param name="blnPrintHeader"></param>
        /// <returns></returns>
        internal int BeginFiscalReceipt(bool blnPrintHeader)
        {
            SerialCM.ResetBuffers();
            return Transaction(TransactionRequestType.Open);
        }

        /// <summary>
        /// Sends a command to the OPOS3 fiscal device to close an open transaction/receipt.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <returns></returns>
        internal int EndFiscalReceipt()
        {
            SerialCM.ResetBuffers();
            return Transaction(TransactionRequestType.Close);
        }

        /// <summary>
        /// Cancel the currently open transaction/receipt.
        /// Sends a command to the OPOS3 fiscal device to cancel an open transaction/receipt.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <param name="strMessage"></param>
        /// <returns></returns>
        internal int PrintRecVoid(string strMessage)
        {
            SerialCM.ResetBuffers();
            return Transaction(TransactionRequestType.Cancel);
        }

        /// <summary>
        /// Adds a single item in an open transaction/receipt.
        /// On success returns zero (0), else the error code number.
        /// [see PROLINE ERGOSPEED III.pdf, page 43]
        /// </summary>
        /// <param name="description"></param>
        /// <param name="AdditionalInfo"></param>
        /// <param name="quantity"></param>
        /// <param name="vatDescription"></param>
        /// <param name="unitPrice"></param>
        /// <returns></returns>
        public int PrintRecItem(string description, string AdditionalInfo, decimal quantity, string vatDescription, decimal unitPrice)
        {
            Int16 ret = -1;
            try
            {
                vatDescription = ReplaceDecimals(vatDescription);
                int i = VatPos(Convert.ToDecimal(vatDescription));
                // for ERGOSPEED 3 firmware version V51
                string prep = string.Format("3/S//{0}/{1}//{2}/{3:0.###}/{4}/{5:0.##}/{6}/", description, AdditionalInfo, quantity, unitPrice, i + 1, VATS[i], i + 1).Replace(',', '.');
                string reply = string.Empty;
                string strPrep1253 = string.Empty;
                if (UnicodeTo1253_Special(prep, ref strPrep1253))
                {
                    int tries = 0;
                    // wait for available com port
                    while (!IsDeviceStatusOK(++tries, out StatusCode)) ;
                    if (logger.IsInfoEnabled)
                        logger.Info("Item sale. COMMAND : " + prep);
                    if ((ret = SerialCM.Command_Request(strPrep1253, ref reply, out deviceStatus, out fiscalStatus)) != 0)
                    {
                        MAT_ErrorLogger("Error in PrintReceiptItem", ret, strPrep1253);
                    }
                }
            }
            catch (Exception exception)
            {
                logger.Error("Error in PrintReceiptItem (catch) : " + exception.ToString());
            }
            return ret;
        }

        /// <summary>
        /// Called to apply and print a discount or a surcharge to the last receipt item sold or the receipt (sub-)total.
        /// [see PROLINE ERGOSPEED III.pdf, page 45]
        /// </summary>
        /// <param name="adjType"></param>
        /// <param name="amount"></param>
        /// <param name="description"></param>
        /// <param name="strVatDescription"></param>
        /// <returns></returns>
        public int PrintRecItemAdjustment(AdjustmentType adjType, decimal amount, string description, string strVatDescription)
        {
            string pout = string.Empty;
            strVatDescription = ReplaceDecimals(strVatDescription);
            int i = VatPos(Convert.ToDecimal(strVatDescription));
            string logstr = "";
            switch (adjType)
            {
                case AdjustmentType.SalesDiscount:
                    pout = string.Format("4/1/{0}/{1}//{2:0.##}//////", i + 1, description, amount).Replace(',', '.');
                    logstr = "Discount in sales";
                    break;
                case AdjustmentType.SubtotalDiscount:
                    pout = string.Format("4/2/{0}/{1}//{2:0.##}//////", 1, description, amount).Replace(',', '.');
                    logstr = "Discount in subtotal.";
                    break;
                case AdjustmentType.SalesMarkup:
                    pout = string.Format("4/3/{0}/{1}//{2:0.##}//////", i + 1, description, amount).Replace(',', '.');
                    logstr = "Markup in sales.";
                    break;
                case AdjustmentType.SubtotalMarkup:
                    pout = string.Format("4/4/{0}/{1}//{2:0.##}//////", 1, description, amount).Replace(',', '.');
                    logstr = "Markup in subtotal.";
                    break;
                default:
                    break;
            }
            int ret = -1;
            string reply = string.Empty;
            string strPrep869 = string.Empty;
            if (UnicodeTo1253_Special(pout, ref strPrep869))
            {
                SerialCM.ResetBuffers();
                int tries = 0;
                // wait for available com port
                while (!IsDeviceStatusOK(++tries, out StatusCode)) ;
                if (logger.IsInfoEnabled)
                    logger.Info(logstr + " COMMAND : " + pout);
                if ((ret = SerialCM.Command_Request(strPrep869, ref reply, out deviceStatus, out fiscalStatus)) != 0)
                {
                    MAT_ErrorLogger("Error in PrintReceiptItemAdjustment.", ret, strPrep869);
                }
            }
            return ret;
        }

        /// <summary>
        /// Refund an item within an open receipt.
        /// [see PROLINE ERGOSPEED III.pdf, page 43]
        /// </summary>
        /// <param name="description"></param>
        /// <param name="AdditionalInfo"></param>
        /// <param name="quantity"></param>
        /// <param name="vatDescription"></param>
        /// <param name="unitPrice"></param>
        /// <returns></returns>
        public int PrintRecRefund(string description, string AdditionalInfo, decimal quantity, string vatDescription, decimal unitPrice)
        {
            int ret = -1;
            vatDescription = ReplaceDecimals(vatDescription);
            int i = VatPos(Convert.ToDecimal(vatDescription));
            string prep = string.Format("3/R//{0}/{1}//{2:0.###}/{3:0.##}/{4}/{5:0.##}/{6}/", description, AdditionalInfo, quantity, unitPrice, i + 1, VATS[i], i + 1).Replace(',', '.');
            string strPrep869 = string.Empty;
            if (UnicodeTo1253_Special(prep, ref strPrep869))
            {
                string reply = string.Empty;
                if (logger.IsInfoEnabled)
                    logger.Info("Item refund. COMMAND : " + prep);
                int trs = 0;
                // wait for available com port
                while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
                if ((ret = SerialCM.Command_Request(strPrep869, ref reply, out deviceStatus, out fiscalStatus)) != 0)
                {
                    MAT_ErrorLogger("Error in PrintReceiptRefund", ret, strPrep869);
                }
            }
            return ret;
        }

        /// <summary>
        /// Cancel an item that has been added to the receipt and print a void description.
        /// On success returns zero (0), else the error code number.
        /// [see PROLINE ERGOSPEED III.pdf, page 43]
        /// </summary>
        /// <param name="description"></param>
        /// <param name="AdditionalInfo"></param>
        /// <param name="quantity"></param>
        /// <param name="vatDescription"></param>
        /// <param name="unitPrice"></param>
        /// <returns></returns>
        public int PrintRecItemVoid(string description, string AdditionalInfo, decimal quantity, string vatDescription, decimal unitPrice)
        {
            int ret = -1;
            vatDescription = ReplaceDecimals(vatDescription);
            int i = VatPos(Convert.ToDecimal(vatDescription));
            string prep = string.Format("3/V//{0}/{1}//{2:0.###}/{3:0.##}/{4}/{5:0.##}/{6}/", description, AdditionalInfo, quantity, unitPrice, i + 1, VATS[i], i + 1).Replace(',', '.');
            string strPrep869 = string.Empty;
            if (UnicodeTo1253_Special(prep, ref strPrep869))
            {
                string reply = string.Empty;
                int tries = 0;
                // wait for available com port
                while (!IsDeviceStatusOK(++tries, out StatusCode)) ;
                if (logger.IsInfoEnabled)
                    logger.Info("Item void. COMMAND : " + strPrep869);
                if ((ret = SerialCM.Command_Request(strPrep869, ref reply, out deviceStatus, out fiscalStatus)) != 0)
                {
                    MAT_ErrorLogger("Error in PrintReceiptItemVoid", ret, strPrep869);
                }
            }
            return ret;
        }

        /// <summary>
        /// Print a line to the printer
        /// </summary>
        /// <param name="line"></param>
        /// <param name="printType"></param>
        /// <returns></returns>
        internal int PrintLine(string line, int printType = 1)
        {
            int ret;
            line = "P/" + line + "/" + printType.ToString() + "/";
            string reply = string.Empty;
            string nLine = string.Empty;
            UnicodeTo1253_Special(line, ref nLine);
            if (logger.IsInfoEnabled)
                logger.Info("Print Line. COMMAND : " + line);
            if ((ret = SerialCM.Command_Request(nLine, ref reply, out deviceStatus, out fiscalStatus)) != 0)
            {
                MAT_ErrorLogger("Error in PrintComments", ret);
            }
            return ret;
        }

        /// <summary>
        /// Print subtotal
        /// </summary>
        /// <returns></returns>
        internal int PrintSubtotal()
        {
            int ret;
            string reply = string.Empty;
            int trs = 0;
            // wait for available com port
            while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
            if (logger.IsInfoEnabled)
                logger.Info("Print Subtotal. COMMAND : o/");
            if ((ret = SerialCM.Command_Request("o/", ref reply, out deviceStatus, out fiscalStatus)) != 0)
            {
                MAT_ErrorLogger("Error in PrintSubtotal", ret);
            }
            return ret;
        }

        /// <summary>
        /// Instruct fiscal device to calculate and print the currently open receipt/transaction total.
        /// Returns zero (0) on success, else the error code returned from the device.
        /// [see PROLINE ERGOSPEED III.pdf, page 48]
        /// </summary>
        /// <param name="paymentType"></param>
        /// <param name="payment"></param>
        /// <param name="strDescription">IMPORTANT: this optional parameter cannot be empty!!!</param>
        /// <param name="strExtraDescription"></param>
        /// <returns></returns>
        public int PrintRecTotal(PaymentType paymentType, decimal payment = 0, string strDescription = "Total", string strExtraDescription = "")
        {
            Int16 ret = -1;
            string reply = string.Empty;
            string strPrep869 = string.Empty;
            string prep = string.Format("5/{0}/{1}/{2}/{3:0.##}/", (int)paymentType, strDescription, strExtraDescription, payment).Replace(',', '.');
            if (UnicodeTo1253_Special(prep, ref strPrep869))
            {
                int trs = 0;
                // wait for available com port
                while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
                if (logger.IsInfoEnabled)
                    logger.Info("Enter Payment Code. COMMAND : " + prep);
                if ((ret = SerialCM.Command_Request(strPrep869, ref reply, out deviceStatus, out fiscalStatus)) != 0)
                {
                    MAT_ErrorLogger("Error in PrintReceiptTotal", ret, strPrep869);
                }
            }
            return ret;
        }

        /// <summary>
        /// Mainly used to implement Voucher entry within receipt, but can be used as Credit Cards also, given the appropriate description.
        /// </summary>
        /// <param name="paidAmound"></param>
        /// <param name="strDescription"></param>
        /// <returns></returns>
        internal int PrintRecNotPaid(decimal paidAmound, string strDescription = "Voucher")
        {
            return PrintRecTotal(PaymentType.Credit, paidAmound, strDescription);
        }

        /// <summary>
        /// Send a command to the OPOS3 fiscal device to print Invoice.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <param name="BillingVatNo"></param>
        /// <param name="CustomerName"></param>
        /// <param name="BillingJob"></param>
        /// <param name="BillingName"></param>
        /// <param name="CustomerDeliveryAddress"></param>
        /// <param name="BillingDOY"></param>
        /// <param name="CustomerPhone"></param>
        /// <returns></returns>
        internal int PrintInvoice(string BillingVatNo, string CustomerName, string BillingJob, string BillingName, string CustomerDeliveryAddress, string BillingDOY, string CustomerPhone)
        {
            CustomerName = CustomerName.Replace('/', '-').Replace("\"", "").Replace("'", "");
            BillingJob = BillingJob.Replace('/', '-').Replace("\"", "").Replace("'", "");
            BillingName = BillingName.Replace('/', '-').Replace("\"", "").Replace("'", "");
            CustomerDeliveryAddress = CustomerDeliveryAddress.Replace('/', '-').Replace("\"", "").Replace("'", "");
            BillingDOY = BillingDOY.Replace('/', '-').Replace("\"", "").Replace("'", "");
            CustomerPhone = CustomerPhone.Replace('/', '-').Replace("\"", "").Replace("'", "");
            if (CustomerName.Length > 35)
                CustomerName = CustomerName.Substring(0, 35);
            if (BillingVatNo.Length > 9)
                BillingVatNo = BillingVatNo.Substring(2, 9);
            if (BillingJob.Length > 35)
                BillingJob = BillingJob.Substring(0, 35);
            if (BillingName.Length > 35)
                BillingName = BillingName.Substring(0, 35);
            if (CustomerDeliveryAddress.Length > 35)
                CustomerDeliveryAddress = CustomerDeliveryAddress.Substring(0, 35);
            if (BillingDOY.Length > 35)
                BillingDOY = BillingDOY.Substring(0, 35);
            if (CustomerPhone.Length > 12)
                CustomerPhone = CustomerPhone.Substring(0, 12);
            int ret = -1;
            string reply = string.Empty;
            string strPrep869 = string.Empty;
            string prep = string.Format("I/{0}/{1}/{2}/{3}/{4}/{5}/{6}", BillingVatNo, CustomerName, BillingJob, BillingName, CustomerDeliveryAddress, BillingDOY, CustomerPhone);
            if (UnicodeTo1253_Special(prep, ref strPrep869))
            {
                SerialCM.ResetBuffers();
                int tries = 0;
                // wait for available com port
                while (!IsDeviceStatusOK(++tries, out StatusCode)) ;
                if (logger.IsInfoEnabled)
                    logger.Info("Print Invoice COMMAND : " + prep);
                if ((ret = SerialCM.Command_Request(strPrep869, ref reply, out deviceStatus, out fiscalStatus)) != 0)
                {
                    MAT_ErrorLogger("Error in PrintReceiptItemAdjustment.", ret, strPrep869);
                }
            }
            return ret;
        }

        /// <summary>
        /// Print footer (Y command)
        /// </summary>
        /// <param name="footerCommand">printer's command without Y/ ex: 1/FOOTERLINE1/2/FOOTERLINE2/1/FOOTERLINE3</param>
        /// <returns></returns>
        internal int PrintFooter(string footerCommand)
        {
            Int16 ret = -1;
            string prep = "Y/" + footerCommand;
            string reply = string.Empty;
            string strPrep1253 = string.Empty;
            if (UnicodeTo1253_Special(prep, ref strPrep1253))
            {
                int trs = 0;
                // wait for available com port
                while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
                if (logger.IsInfoEnabled)
                    logger.Info("Write Footer . COMMAND : " + prep);
                if ((ret = SerialCM.Command_Request(strPrep1253, ref reply, out deviceStatus, out fiscalStatus)) != 0)
                {
                    MAT_ErrorLogger("Error in PrintFooterComments", ret, prep);
                }
            }
            return ret;
        }

        /// <summary>
        /// Adds three (3) lines of comments that will be printed at the end of the receipt. (m command)
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <param name="strCommentLine1">First line of comments.</param>
        /// <param name="strCommentLine2">Second line of comments.</param>
        /// <param name="strCommentLine3">Third line of comments.</param>
        /// <param name="intMaxPrintableChars">The maximum number of characters to be used per line.</param>
        /// <returns></returns>
        internal int PrintFooterComments(string strCommentLine1, string strCommentLine2, string strCommentLine3, int? intMaxPrintableChars)
        {
            if (intMaxPrintableChars == null)
            {
                intMaxPrintableChars = 48;
            }
            Int16 ret = -1;
            if (intMaxPrintableChars < strCommentLine1.Length)
            {
                strCommentLine1 = strCommentLine1.Substring(0, (int)intMaxPrintableChars);
            }
            if (intMaxPrintableChars < strCommentLine2.Length)
            {
                strCommentLine2 = strCommentLine2.Substring(0, (int)intMaxPrintableChars);
            }
            if (intMaxPrintableChars < strCommentLine3.Length)
            {
                strCommentLine3 = strCommentLine3.Substring(0, (int)intMaxPrintableChars);
            }
            string prep = string.Format("m/{0}/{1}/{2}/", strCommentLine1, strCommentLine2, strCommentLine3);
            string reply = string.Empty;
            string strPrep1253 = string.Empty;
            if (UnicodeTo1253_Special(prep, ref strPrep1253))
            {
                int trs = 0;
                // wait for available com port
                while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
                if (logger.IsInfoEnabled)
                    logger.Info("Write Comments. COMMAND : " + prep);
                if ((ret = SerialCM.Command_Request(strPrep1253, ref reply, out deviceStatus, out fiscalStatus)) != 0)
                {
                    MAT_ErrorLogger("Error in PrintFooterComments", ret, prep);
                }
            }
            return ret;
        }

        /// <summary>
        /// Cancel the currently open transaction/receipt.
        /// Sends a command to the OPOS3 fiscal device to cancel an open transaction/receipt.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <returns></returns>
        public int PrintRecCancel()
        {
            int trs = 0;
            // wait for available com port
            while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
            if (logger.IsInfoEnabled)
                logger.Info("Cancel the current receipt.");
            SerialCM.ResetBuffers();
            return Transaction(TransactionRequestType.Cancel);
        }

        /// <summary>
        /// Issue the X report
        /// </summary>
        /// <returns></returns>
        public int PrintXreport()
        {
            int ret = 0;
            string reply = string.Empty;
            int trs = 0;
            while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
            if (logger.IsInfoEnabled)
                logger.Info("Issue an X report. COMMAND : x/1/");
            if ((ret = SerialCM.Command_Request("x/1/", ref reply, out deviceStatus, out fiscalStatus)) != 0)
            {
                MAT_ErrorLogger("Error in Print X Report", ret);
                if (ret == 0x50)
                {
                }
            }
            Thread.Sleep(800);
            int trs3 = 0;
            while (!IsDeviceStatusOK(++trs3, out StatusCode)) ;
            return ret;
        }

        /// <summary>
        /// Read the daily totals accumulated in one day.
        /// On success return a string with totals, else the string ERROR.
        /// </summary>
        /// <returns></returns>
        internal string ReadDailyTotals()
        {
            int ret;
            string reply = string.Empty;
            int trs = 0;
            while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
            if (logger.IsInfoEnabled)
                logger.Info("Read the daily totals accumulated in one day. COMMAND : 0/");
            if ((ret = SerialCM.Command_Request("0/", ref reply, out deviceStatus, out fiscalStatus)) != 0)
            {
                MAT_ErrorLogger("Error in reading the daily totals", ret);
                reply = "ERROR";
            }
            return reply;
        }

        /// <summary>
        /// Issue the Z report
        /// </summary>
        /// <param name="strFiscalName"></param>
        /// <param name="strPathToJournal"></param>
        /// <returns></returns>
        public int PrintZreport(string strFiscalName, string strPathToJournal)
        {
            string strReplyFromCommand_A = "";
            short resultForCommand_A;
            SerialCM.ResetBuffers();
            int trs = 0;
            // wait for available com port
            while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
            if (logger.IsInfoEnabled)
                logger.Info("Issue Dayly Closure. COMMAND : A/");
            if ((resultForCommand_A = SerialCM.Command_Request("A/", ref strReplyFromCommand_A, out deviceStatus, out fiscalStatus)) == 0x51)
            {
                Thread.Sleep(800);
                if (logger.IsInfoEnabled)
                    logger.Info("Issue Dayly Closure. COMMAND : A/");
                int trs0 = 0;
                // wait for available com port
                while (!IsDeviceStatusOK(++trs0, out StatusCode)) ;
                if ((resultForCommand_A = SerialCM.Command_Request("A/", ref strReplyFromCommand_A, out deviceStatus, out fiscalStatus)) == 0x51)
                {
                    // we receive twice the error 0x51, device RTC problem -> call service!!!
                    MAT_ErrorLogger("IMPORTANT!!! - Device problem ", 0x51);
                }
            }
            int trs3 = 0;
            // wait for available com port
            while (!IsDeviceStatusOK(++trs3, out StatusCode)) ;
            // start read flash memory
            StringBuilder strFileContents = new StringBuilder();
            bool blnRepeatRead = true;
            string strFileName = string.Empty;
            string strReplyFromCommand_Q = string.Empty;
            short resultForCommand_Q = 0;
            string strCurrentReportNumber;
            bool isError = false;
            int tries = 0;
            do
            {
                // read a line from flash memory
                tries = 0;
            TryAgain:
                try
                {
                    int trs44 = 0;
                    // wait for available com port
                    while (!IsDeviceStatusOK(++trs44, out StatusCode)) ;
                    SerialCM.ResetBuffers();
                    int trs4 = 0;
                    // wait for available com port
                    while (!IsDeviceStatusOK(++trs4, out StatusCode)) ;
                    if (logger.IsInfoEnabled)
                        logger.Info("Read a line from Flash Memory. COMMAND : Q/");
                    // start read each line from fiscal memory
                    resultForCommand_Q = SerialCM.Command_Request("Q/", ref strReplyFromCommand_Q, out deviceStatus, out fiscalStatus);
                    isError = false;
                }
                catch (Exception exception)
                {
                    logger.Error("Error reading a line from Flash Memory: " + exception.ToString());
                    resultForCommand_Q = 999;
                    Thread.Sleep(70);
                    tries++;
                    isError = true;
                }
                // on error try to continue printing z report
                if (isError && tries < 20)
                    goto TryAgain;
                if (resultForCommand_Q != 0)
                {
                    // handle error condition: break loop and exit procedure
                    MAT_ErrorLogger("Error, cannot execute command 'Q' at fiscal device. ", resultForCommand_Q);
                    break;
                }
                string[] strSplitted = strReplyFromCommand_Q.Split('/');
                switch (strSplitted[0][0])
                {
                    case '0':
                        // it is a file name
                        strCurrentReportNumber = strSplitted[1];
                        strFileName = strSplitted[2];
                        break;
                    case '1':
                        // valid file contents
                        strFileContents.Append(strSplitted[1]);
                        break;
                    case '2':
                        // end of file, store file contents to predefined directory
                        if (!SaveFiscalDeviceFile(strFileName, strPathToJournal, strFileContents))
                        {
                            // in case of error while saving files the Z report is failed!
                            logger.Error(ExtcerLogger.logErr("Error, cannot store file to PC HDD [001] ", strFiscalName));
                            return -1;
                        }
                        // clean up StringBuilder
                        strFileContents.Clear();
                        break;
                    case '3':
                        // end of fiscal data, if we are here with file contents not empty there is a file to save too!
                        if (strFileContents.Length > 0)
                        {
                            if (!SaveFiscalDeviceFile(strFileName, strPathToJournal, strFileContents))
                            {
                                // in case of error while saving files the Z report is failed!
                                logger.Error(ExtcerLogger.logErr("Error, cannot store file to PC HDD [002] ", strFiscalName));
                                return -1;
                            }
                        }
                        // if we are here without errors, issue command "x/2/0/" to tell fiscal device data transfer was successful
                        int intRetry = 30;
                        int intErrorCode;
                        int trs04 = 0;
                        // wait for available com port
                        while (!IsDeviceStatusOK(++trs04, out StatusCode)) ;
                        SerialCM.ResetBuffers();
                        do
                        {
                            tries = 0;
                        TryAgain2:
                            try
                            {
                                int trs5 = 0;
                                // wait for available com port
                                while (!IsDeviceStatusOK(++trs5, out StatusCode)) ;
                                if (logger.IsInfoEnabled)
                                    logger.Info("Validating that the Daily Flash data transfer (Z report) was successful.... COMMAND : x/2/");
                                if ((intErrorCode = SerialCM.Command_Request("x/2/", ref strReplyFromCommand_Q, out deviceStatus, out fiscalStatus)) != 0)
                                {
                                    MAT_ErrorLogger("Error, cannot execute command 'x/2/' at fiscal device. ", intErrorCode);
                                    if (0 == intRetry--)
                                    {
                                        return -1;
                                    }
                                }
                                else
                                {
                                    if (intErrorCode == 0)
                                        logger.Info(">> Daily Flash data transfer (Z report) was successful.");
                                    break;
                                }
                            }
                            catch (Exception exception)
                            {
                                logger.Error("Error Validating Flash data transfer(Z report): " + exception.ToString());
                                resultForCommand_Q = 999;
                                tries++;
                                Thread.Sleep(50);
                            }
                            if (isError && tries < 10)
                                goto TryAgain2;
                        }
                        while (true);
                        // store all files from journal root to a new folder to close fiscal day
                        CleanUpJournalFiles(strFiscalName, strPathToJournal);
                        blnRepeatRead = false;
                        break;
                    default:
                        break;
                }
                Thread.Sleep(5);
            }
            while (blnRepeatRead);
            return 0;
        }

        /// <summary>
        /// Send a command to the OPOS3 fiscal device to feed line.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        internal int LineFeed(int lines = 1)
        {
            int ret;
            string str = "F/1/";
            string reply = string.Empty;
            int trs = 0;
            // wait for available com port
            while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
            for (int i = 1; i <= lines; i++)
            {
                if (logger.IsInfoEnabled)
                    logger.Info("Line Feed. COMMAND : " + str);
                if ((ret = SerialCM.Command_Request(str, ref reply, out deviceStatus, out fiscalStatus)) != 0)
                {
                    MAT_ErrorLogger("Error in LineFeed", ret);
                }
                if (ret > 0)
                    return ret;
            }
            return 0;
        }

        /// <summary>
        /// Send a command to the OPOS3 fiscal device to use cutter.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <returns></returns>
        internal int Cutter()
        {
            int ret;
            string reply = string.Empty;
            if (logger.IsInfoEnabled)
                logger.Info("Cut paper. COMMAND : p/2/");
            int trs = 0;
            // wait for available com port
            while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
            if ((ret = SerialCM.Command_Request("p/2/", ref reply, out deviceStatus, out fiscalStatus)) != 0)
            {
                MAT_ErrorLogger("Error in Cutter", ret);
            }
            return ret;
        }

        /// <summary>
        /// Send a command to the OPOS3 fiscal device to open drawer.
        /// On success returns zero (0), else the error code number.
        /// </summary>
        /// <returns></returns>
        internal int OpenDrawer()
        {
            int ret;
            string reply = string.Empty;
            int trs = 0;
            // wait for available com port
            while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
            if (logger.IsInfoEnabled)
                logger.Info("Open Drawer. COMMAND : p/1/");
            if ((ret = SerialCM.Command_Request("p/1/", ref reply, out deviceStatus, out fiscalStatus)) != 0)
            {
                MAT_ErrorLogger("Error in OpenDrawer", ret);
            }
            return ret;
        }

        /// <summary>
        /// Used to get from fiscal device: the number and date/time of last Z report, and the number and date/time of the last receipt printed.
        /// </summary>
        /// <param name="strFiscalName"></param>
        /// <param name="intNumberOfLastZ"></param>
        /// <param name="strDateOfLastZ"></param>
        /// <param name="strTimeOfLastZ"></param>
        /// <param name="intNumberOfLastReceipt"></param>
        /// <param name="strDateOfLastReceipt"></param>
        /// <param name="strTimeOfLastReceipt"></param>
        /// <returns></returns>
        internal int GetFiscalDeviceLastPrintsInfo(string strFiscalName, out int intNumberOfLastZ, out string strDateOfLastZ, out string strTimeOfLastZ, out int intNumberOfLastReceipt, out string strDateOfLastReceipt, out string strTimeOfLastReceipt)
        {
            int returnCode;
            intNumberOfLastZ = 0;
            strDateOfLastZ = string.Empty;
            strTimeOfLastZ = string.Empty;
            intNumberOfLastReceipt = 0;
            strDateOfLastReceipt = string.Empty;
            strTimeOfLastReceipt = string.Empty;
            string strReply = string.Empty;
            int trs = 0;
            // wait for available com port
            while (!IsDeviceStatusOK(++trs, out StatusCode)) ;
            if (logger.IsInfoEnabled)
                logger.Info("Read last Z number and datetime. COMMAND : */");
            if ((returnCode = SerialCM.Command_Request("*/", ref strReply, out deviceStatus, out fiscalStatus)) != 0)
            {
                MAT_ErrorLogger("Cannot execute command 'ReadLastReceiptInfo'", returnCode, "*/");
                return returnCode;
            }
            string[] strSplitted = strReply.Split('/');
            try
            {
                intNumberOfLastZ = Convert.ToInt32(strSplitted[0]);
                strDateOfLastZ = strSplitted[1];
                strTimeOfLastZ = strSplitted[2];
                intNumberOfLastReceipt = Convert.ToInt32(strSplitted[3]);
                strDateOfLastReceipt = strSplitted[4];
                strTimeOfLastReceipt = strSplitted[5];
            }
            catch (Exception exception)
            {
                logger.Error(ExtcerLogger.logErr("Error getting fiscal device last prints info: " + exception.ToString(), strFiscalName));
                return -1;
            }
            return returnCode;
        }

        /// <summary>
        /// Returns the last recorded error message and the related suggested action, in case of a valid error ( 0x01 to 0x8D ).
        /// </summary>
        /// <param name="strSuggestedAction">Returns the suggested action to overcome the error.</param>
        /// <returns></returns>
        internal string GetLastError(out string strSuggestedAction)
        {
            if (intLastErrorRecorded == 256)
            {
                strSuggestedAction = "Check cables/COM port settings/driver settings and retry.";
                return "  OPOS3 device communication problem (Error Code = 256).                 ";
            }
            if (intLastErrorRecorded == 999)
            {
                strSuggestedAction = "Check printer's connectivity and cables, COM port settings, ExtECR settings, and retry.";
                return "  Communication Problem. Port is not open (Error Code = 999).                 ";
            }
            if (intLastErrorRecorded > 0)
            {
                strSuggestedAction = strErrorMessage[intLastErrorRecorded, 1];
                return strErrorMessage[intLastErrorRecorded, 0];
            }
            strSuggestedAction = string.Empty;
            return "";
        }

        /// <summary>
        /// Return error description based on int error code printer returns.
        /// </summary>
        /// <param name="intErrorCode"></param>
        /// <returns></returns>
        public string getErrorDescription(int intErrorCode)
        {
            string strSupport = string.Empty;
            if (intErrorCode <= 0)
            {
                strSupport = "";
            }
            else if (intErrorCode == 256)
            {
                strSupport = "Communication Problem. Port is not open (Error code: 256)";
            }
            else if (intErrorCode == 999)
            {
                strSupport = "Communication Problem. Port is not open (Error code: 999)";
            }
            else
            {
                strSupport = string.Format("ERROR \r\n \t      Error Code = {0}\r\n" + "\t Error Message :\t {1}\r\n" + "\t Suggested Action :\t {2}", intErrorCode, strErrorMessage[intErrorCode, 0], strErrorMessage[intErrorCode, 1]);
            }
            return strSupport;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the transaction/receipt actions: Open, Close, Cancel.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private int Transaction(TransactionRequestType cmd)
        {
            int ret;
            string reply = string.Empty;
            string strCommand = string.Format("O/{0}/", (int)cmd);
            if (logger.IsInfoEnabled)
                logger.Info(cmd.ToString().ToUpper() + " Transaction. COMMAND : " + strCommand);
            if ((ret = SerialCM.Command_Request(strCommand, ref reply, out deviceStatus, out fiscalStatus)) != 0)
            {
                MAT_ErrorLogger(string.Format("OPOS3 - {0} Receipt", cmd.ToString().ToUpper()), ret, strCommand);
            }
            return ret;
        }

        /// <summary>
        /// We save a new fiscal device - flash memory - data file to PCs HDD, at the predefined path.
        /// </summary>
        /// <param name="strFileName"></param>
        /// <param name="strPathToJournal"></param>
        /// <param name="strFileContents"></param>
        /// <returns></returns>
        private bool SaveFiscalDeviceFile(string strFileName, string strPathToJournal, StringBuilder strFileContents)
        {
            string fullpath = strPathToJournal + '\\' + strFileName;
            logger.Info("Saving file " + fullpath);
            try
            {
                // store file contents to predefined directory. if the predefined path does not exist, create it
                DirectoryInfo di_JournalRoot = new DirectoryInfo(strPathToJournal);
                if (!di_JournalRoot.Exists)
                {
                    di_JournalRoot.Create();
                }
                // store strFileContents to a file at predefined path for journal
                using (StreamWriter streamWriter = File.CreateText(fullpath))
                {
                    string strDest = "";
                    if (From1253ToUnicode_Special(strFileContents.ToString(), ref strDest))
                        streamWriter.Write(strDest);
                    else
                        streamWriter.Write(strFileContents.ToString() ?? "");
                }
                return true;
            }
            catch (Exception exception)
            {
                logger.Error("Error at journal files save : " + exception.ToString());
                return false;
            }
        }

        /// <summary>
        /// Create a new work-day-log directory within journal, copy all files from journal-root to our new directory, delete all files from journal-root.
        /// </summary>
        /// <param name="strFiscalName"></param>
        /// <param name="strPathToJournal"></param>
        private void CleanUpJournalFiles(string strFiscalName, string strPathToJournal)
        {
            logger.Info(strFiscalName + ": Cleaning up Journal files...");
            try
            {
                // create the directory only if it does not already exist
                DirectoryInfo di_JournalRoot = new DirectoryInfo(strPathToJournal);
                if (!di_JournalRoot.Exists)
                {
                    di_JournalRoot.Create();
                }
                int intTmp = 0;
                string strTmp = string.Empty;
                string strFromDate = string.Empty;
                string strUptoDate = string.Empty;
                if (GetFiscalDeviceLastPrintsInfo(strFiscalName, out intTmp, out strFromDate, out strTmp, out intTmp, out strUptoDate, out strTmp) != 0)
                {
                    logger.Error(ExtcerLogger.logErr("Error, cannot read fiscal device date ", strFiscalName));
                    return;
                }
                string strSubDirectoryName = string.Format("{0}_{1}", DateTime.Now.ToString("yyyy-MM-dd"), DateTime.ParseExact(strFromDate, "ddMMyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"));
                // create a subdirectory in the directory just created
                DirectoryInfo di_WorkDay = di_JournalRoot.CreateSubdirectory(strSubDirectoryName);
                // copy all files from journal-root to new subdirectory
                string strSourcePath = strPathToJournal;
                string strDestPath = strPathToJournal + '\\' + strSubDirectoryName;
                string[] strTextFilesList = Directory.GetFiles(strSourcePath, "*.txt");
                foreach (string f in strTextFilesList)
                {
                    // remove path from the file name
                    string strFileName = f.Substring(strSourcePath.Length + 1);
                    File.Copy(Path.Combine(strSourcePath, strFileName), Path.Combine(strDestPath, strFileName), true);
                }
                // delete all files from journal-root
                foreach (string f in strTextFilesList)
                {
                    File.Delete(f);
                }
            }
            catch (Exception exception)
            {
                logger.Error("Error at journal cleanup: " + exception.ToString());
            }
        }

        /// <summary>
        ///  For the given VAT value, returns the position of it at the VATS table.
        ///  In case the value cannot be found, returns the position four (4) which corresponds to the value of zero (0).
        /// </summary>
        /// <param name="decVatValue"></param>
        /// <returns></returns>
        private int VatPos(decimal decVatValue)
        {
            int res = -1;
            for (int i = 0; i < 5; i++)
            {
                if (VATS[i] == decVatValue)
                {
                    res = i;
                    break;
                }
            }
            if (res >= 5)
                res = 4;
            if (res == -1)
            {
                logger.Error(">>>>>>>>  Vat Value " + decVatValue.ToString() + " did not find into printer's VATS list. Set VatRate=-10 to prevent printer to print !!!!!");
                res = -11;
            }
            return res;
        }

        /// <summary>
        /// Convert unicode to 1253
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strDest"></param>
        /// <returns></returns>
        private bool UnicodeTo1253_Special(string strSource, ref string strDest)
        {
            if (string.IsNullOrEmpty(strSource))
            {
                return false;
            }
            StringBuilder sb = new StringBuilder();
            int intLen = strSource.Length;
            for (int j = 0; j < intLen; j++)
            {
                sb.Append((0x384 <= strSource[j] && 0x3CE >= strSource[j]) ? LUT_UnicodeTo1253[strSource[j] - 0x384] : strSource[j]);
            }
            strDest = sb.ToString();
            return true;
        }

        /// <summary>
        /// Convert 1253 to unicode
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strDest"></param>
        /// <returns></returns>
        private bool From1253ToUnicode_Special(string strSource, ref string strDest)
        {
            if (string.IsNullOrEmpty(strSource))
                return false;
            StringBuilder sb = new StringBuilder();
            int intLen = strSource.Length;
            for (int j = 0; j < intLen; j++)
            {
                sb.Append((strSource[j] >= 182 && strSource[j] <= 254) ? (char)(strSource[j] + 720) : (char)strSource[j]);
            }
            strDest = sb.ToString();
            return true;
        }

        /// <summary>
        /// In a numeric string replace . or , to the right decimal separator
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ReplaceDecimals(string value)
        {
            string a = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (!value.Contains(a) && a != ".")
                value = value.Replace(".", a);
            if (!value.Contains(a) && a != ",")
                value = value.Replace(",", a);
            return value;
        }

        /// <summary>
        /// Log error.
        /// </summary>
        /// <param name="strMessage">The primary message to give to the logger.</param>
        /// <param name="intErrorCode">The fiscal device error code to use as index in the predefined messages array.</param>
        /// <param name="strCommand"></param>
        private void MAT_ErrorLogger(string strMessage, int intErrorCode, string strCommand = "")
        {
            // keep record of last error occurred
            intLastErrorRecorded = intErrorCode;
            string strSupport = getErrorDescription(intErrorCode);
            logger.Error(ExtcerLogger.Log(string.Format("{0} -- OPOS3 On: {1}  --  {2}", strMessage, SerialCM.m_Port.PortName, strSupport) + " -  Offending command: " + strCommand + "\r\n", "OPOS3 (MAT)"));
        }

        #endregion

        /// <summary>
        /// Get log configuration from application path
        /// </summary>
        /// <returns></returns>
        private string GetConfigurationPath()
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