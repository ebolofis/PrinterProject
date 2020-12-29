namespace ExtECRMainLogic.Classes.Helpers
{
    public static class OposErrorHelper
    {
        #region Extended Error Messages For Fiscal Printer
        private static string[] strExtendedErrors_FiscalPrinter =
            { "No errors - success              | None                                   "
                , "Wrong number of fields /Check the command's field count"
                , "Field too long /A field is long: check it & retry"
                , "Field too small / A field is small: check it & retry "
                , "Field fixed size mismatch /| A field size is wrong: check it & retry"
                , "Field range or type check failed | Check ranges or types in command, "
                , "Bad request code                 | Correct the request code (unknown)"
                , "PLU index range error/| Correct command's PLU index number "
                , "DPT index range error /Correct command's DPT index number     "
                , "Wrong VAT Code. Correct the Code"
                , "CLERK index range error/| Correct command's CLERK index number "
                , "Wrong CLERK's Password"
                , "PAYMENT index range error | Correct command's PAYMENT index number "
                , "The requested Fiscal record doesn't exist"
                , "The requested Fiscal record type doesn't exist"
                , "Printing type range error| Correct command's printing type "
                , "Cannot execute with day open | Issue a Z report to close the day "
                , "RTC programming requires jumper  | Short the 'clock' jumper and retry "
                , "Wrong TIME, call SERVICE"
                , "NOT USED"
                , "Cannot execute with receipt open | Close the receipt and retry "
                , "Invalid Payment"
                , "Cash in / Cash out open | Close the cash in/out and retry"
                , "Wrong VAT rate"
                , "Price Error"
                , "Cannot execute with online active| Close the online and retry "
                , "The ECR is busy, try again later"
                , "Invalid sale operation | Correct the sale operation and retry "
                , "Invalid Discount/Markup/.type | Correct the discount/markup type  "
                , "No more headers can be programmed"
                , "A user's report is open"
                , "A user's report is open"
                , "The Fiscal Memory has no transactions"
                , "Discount/Markup index number error"
                , "You can't program any more PLUs"
                , "Error in BMP Data"
                , "The BMP index number doesn't exist"
                , "The category index number doesn't exist"
                , "NOT USED"
                , "Error printing type"
                , "NOT USED"
                , "No more sales can be performed"
                , "Keyboard error-or keyboard disconnected"
                , "Battery error-or battery low"
                , "Larger Quantity Value than the one allowed "
                , "Larger Sales Value than the one allowed."
                , "PLU does not exist."
                , "DPT does not exist."
                , "Receipt in PAYMENT state | Send the command when the transaction is not in payment state."
                , "Cannot execute: CASH IN is open  | Do not send the command when in CASHIN, "
                , "Cannot execute: CASH OUT is open | Do not send the command when in CASHOUT"
                , "Negative VAT amount."
                , "Transaction not open yet  | A transaction must be opened first "
                , "No positive sale records yet | A positive sale must be done first "
                , "Invalid operation in this state  | Check the state of the ECR"
                , "Zero amount invalid for payment  | Change the payment amount to non zero "
                , "Negative total is not allowed"
                , "Subtotal must be inserted first"
                , "Change not allowed (this payment)| Correct the payment amount and retry "
                , "A transaction is open | Close the transaction and retry"
                , "A cash in/out receipt is open | Close the cash in/out receipt and retry"
                , "A cash in receipt is not open | Open a cash in receipt and retry"
                , "A cash out receipt is not open | Open a cash out receipt and retry "
                , "Payment code range error | Correct the payment code and retry, "
                , "Department index range error | Correct the department index and retry "
                , "Ticket quantity non integer | Make the ticket quantity integer "
                , "Discount/Markup amount is zero | Correct the amount and retry "
                , "Discount/Markup amount limit | Make the amount less or equal to the setup's allowed maximum"
                , "No zero PLU/DPT sale is allowed."
                , "Item not available (not 'set') | Try another item or modify it's 'set' flag by ECR's setup"
                , "Transaction record limit reached | No more sale records allowed for this receipt. Close the receipt and open a new one."
                , "No negative drawer allowed | Correct the payment amount and retry"
                , "Coupon discount in subtotal is not allowed."
                , "No more discount/markup is allowed, a sale must occur first."
                , "Wrong DATE-TIME Call Service."
                , "No negative VAT amount is allowed"
                , "Subtotal must be pressed first before Discount/Markup."
                , "No Discount/Markup is allowed in Subtotal ."
                , "There are no fiscal data for the requested period ."
                , "The Discount/Markup is not active "
                , "Fiscal Memory is full"
                , "There are no transactions"
                , "Clerk is not allowed to perform this action"
                , "No daily transactions"
                , "Wrong DATE, call SERVICE"
                , "Wrong TIME, call SERVICE"
                , "Fiscal MEMORY disconnected"
                , "There daily transactions, issue a Z report first"
                , "Access only to SERVICE"
                , "Paper End in Journal Station"
                , "Paper End in Receipt Station"
                , "Printer Head open"
                , "Printer Disconnected"
                , "Fiscal Memory Error"
                , "You cannot program any more PLUs cause the PLU index number is exceeded"
                , "You cannot program this Discount/Markup"
                , "The Client Code does not exist"
                , "You cannot program 2 different VAT rates to have the same value"
                , "No sales are allowed after a ticket discount"
                , "No more Headers can be programmed"
                , "The PLUS must be zeroed first"
                , "A Z READ must be issued first"
                , "Inactive Payment"
                , "TimeOut"
        };
        #endregion
        #region Extended Error Messages For ADHME
        private static string[] strExtendedErrors_ADHME =
            { "No errors - success"
                , "Wrong number of fields\r\n(Check the command's field count)"
                , "Field too long\r\n(A field is long: check it & retry)"
                , "Field too small\r\n(A field is small: check it & retry)"
                , "Field fixed size mismatch\r\n(A field size is wrong: check it & retry)"
                , "Field range or type check failed\r\n(Check ranges or types in command)"
                , "Bad request code\r\n(Correct the request code (unknown)"
                , "Not use in this version"
                , "Not use in this version"
                , "Printing type bad\r\n(Correct the specified printing style)"
                , "cannot execute with day open\r\n(Issue a Z report to close the day)"
                , "RTC programming requires jumper\r\n(Short the 'clock' jumper and retry)"
                , "RTC date or time invalid\r\n(Check the date/time range. Also check if date is prior to a date of a fiscal record)"
                , "No records in fiscal period\r\n(No suggested action; the operation can not be executed in the specified period)"
                , "Device is busy in another task\r\n(Wait for the device to get ready)"
                , "No more header records allowed\r\n(No suggested action; the header programming cannot be executed because the Fiscal memory cannot hold more records)"
                , "cannot execute with block open\r\n(the specified command requires no open signature block for proceeding. Close the block and retry)"
                , "Not use in this version"
                , "Not use in this version"
                , "Not use in this version"
                , "Z closure time limit\r\n(Means that 24 hours passed from the last Z closure. Issue a Z and retry)"
                , "Z closure not found\r\n(the specified Z closure number does not exist. Pass an existing Z number)"
                , "Z closure record bad\r\n(The requested Z record is unreadable  (damaged). Device requires service)"
                , "User browsing in progress\r\n(The user is accessing the device by manual operation. The protocol usage is suspended until the user terminates the keyboard browsing. Just wait or inform application user)"
                , "Not use in this version"
                , "Printer paper end detected\r\n(Replace the paper roll and retry)"
                , "Printer is offline\r\n(Printer disconnection. Service required)"
                , "Fiscal unit is offline\r\n(Fiscal disconnection. Service required)"
                , "Fatal hardware error\r\n(Mostly fiscal errors. Service required)"
                , "Fiscal unit is full\r\n(Need fiscal replacement. Service)"
                , "Not use in this version"
                , "Not use in this version"
                , "Battery fault detected\r\n(If problem persists, service required)"
                , "Not use in this version"
                , "Not use in this version"
                , "Real-Time Clock needs programming\r\n(This means that the RTC has invalid Data and needs to be reprogrammed. As a consequence, service is needed)"
                , "JUPERON\r\n(Jumpers are on, remove them for the operation to continue"
                , "INVSALEOP\r\n(Wrong sales type It must be S/V/R)"
                , "DPTINDEXERR\r\n(Department's code out of range (1-5)"
                , "VATRATE\r\n(The VAT rate received from PC doesn't agree to the Fiscal Printer with  Electronic Journal's one)"
                , "PAYMENTINDEXERR\r\n(Payment Code out of range  (1-3) 1=CASH, 2=CARD, 3=CREDIT)"
                , "NOT USE IN THIS VERSION"
                , "COVEROPEN\r\n(Printer's tray is open)"
                , "NOT USE IN THIS VERSION"
                , "NOT USE IN THIS VERSION"
                , "NOT USE IN THIS VERSION"
                , "NOT USE IN THIS VERSION"
                , "NOTENDREADLEGAL\r\n(There are illegal records in the Journal that must be read)"
                , "NOTENDREADILEGAL\r\n(There are illegal records in the Journal that must be read)"
                , "WRONGILEGALNUMBER\r\n(The requested illegal receipt doesn’t exist in the electronic journal)"
                , "FLASHERROR\r\n(CARD reading problem)"
                , "NOTFOUNDRECEIPT\r\n(The requested legal receipt doesn’t exist in the electronic journal)"
                , "NOMOREILEGALRECEIP\r\n(There are no more receipts to be read in the CARD)"
                , "NOTSTARTREAD\r\n(The Fiscal Printer with electronic Journal must first be told about the reading of the CARD before the CARD’s reading begins)"
                , "NOTFINISHREADRECEIPTDATA\r\n(The CARD’s reading isn’t finished)"
                , "NOTREADFORFOUNDRECEIPT\r\n(A record hasn’t been read)"
                , "ENDREADFLAS\r\n(The CARD’s reading was successful)"
                , "HWTRAYAGAN\r\n(Error reading the CARD, please try again)"
                , "NOTSTARTREADFLASH\r\n(The Fiscal Printer with electronic Journal must first be told about the reading of the CARD before the CARD’s reading begins)"
                , "NOTFOUNDOPENDAY\r\n(DAY isn’t opened and no transactions are present)"
                , "NOMOREINRECEIPTLINES\r\n(No more than 6 comment lines can be printed on the receipt)"
                , "NOTTRANSFERFLASH\r\n(The CARD’s data transfer to the PC isn’t over yet)"
                , "PRINTERDISCONECT\r\n(Printer is disconnected)"
                , "TRANSACTIONINPROGRES\r\n(Another Fiscal Printer with electronic Journal’s function is in progress)"
                , "TRANSACTIONNOTOPEN\r\n(There is no opened receipt)"
                , "TRANSACTIONISOPEN\r\n(There is an opened receipt)"
                , "NOMOREVAT\r\n(No more VAT codes can be programmed in the fiscal memory)"
                , "CASHINOPEN\r\n(Cash in is in progress)"
                , "CASHOUTOPEN\r\n(Cash out is in progress)"
                , "INPAYMENT\r\n(Payment is in progress)"
                , "NOZERODM\r\n(No zero Discount/Markup is allowed)"
                , "MAXDISCOUNTINVAT\r\n(Greater Discount than the Fiscal Printer with electronic Journal’s VAT amount)"
                , "MAXDMINTRANSTOTAL"
                , "NOTEQUALDMGETSUM\r\n(VAT’s allocation’s totals do not match)"
                , "NEGATIVEVATSALES\r\n(No negative sales-transactions are allowed)"
                , "MUSTCLOSETRANSACTION\r\n(The receipt must be closed in order for the function to continue)"
                , "FLASHFULL\r\n(CARD is full, it must be read)"
                , "NOZEROVAT\r\n(The VAT rate can not be 0)"
                , "NOSANEVATRATE\r\n( No equal VAT rates in different categories)"
                , "NOSALESZEROPRICE\r\n(Zero sale’s price can not occur)"
                , "NODATAFORPRNX\r\n(There are no transactions-A X Report can not be issued)"
                , "WORNIGDATE\r\n(DATE/TIME Error. Call service)"
                , "FLASSTOPWORK\r\n(CARD error. The Fiscal Printer with electronic Journal can not perform sales)"
                , "NOTVALIDPLU\r\n(PLU Internal Code Error  (1-200))"
                , "INVALIDCATEGORI\r\n(Category Code Error (1-20)"
                , "INVALID DPT\r\n(Department Code Error    (1-5)"
                , "Cutter Error\r\n(Turn off The Fiscal Printer with electronic Journal and try again)"
                , "Recover data from FLASH\r\n(The Flash CARD must be read. The machine is in an after-CMOS status"
                , "PAYMENT can not be cancelled\r\n(There is no payment amount to be cancelled"
                , "ZERO PAYMENT can not be cancelled\r\n(A zero payment can not be cancelled"
                , "NOT in Payment Mode\r\n(The Fiscal Printer with electronic Journal is not in payment mode)"
                , "TimeOut"
        };
        #endregion
        #region Fiscal Printer Station Constants
        public const int
            FPTR_S_JOURNAL = 1,
            FPTR_S_RECEIPT = 2,
            FPTR_S_SLIP = 4,
            FPTR_S_JOURNAL_RECEIPT = 3,
            // "ActualCurrency" Property Constants
            FPTR_AC_BRC = 1,
            FPTR_AC_BGL = 2,
            FPTR_AC_EUR = 3,
            FPTR_AC_GRD = 4,
            FPTR_AC_HUF = 5,
            FPTR_AC_ITL = 6,
            FPTR_AC_PLZ = 7,
            FPTR_AC_ROL = 8,
            FPTR_AC_RUR = 9,
            FPTR_AC_TRL = 10,
            // "ContractorId" Property Constants
            FPTR_CID_FIRST = 1,
            FPTR_CID_SECOND = 2,
            FPTR_CID_SINGLE = 3,
            // "CountryCode" Property Constants
            FPTR_CC_BRAZIL = 1,
            FPTR_CC_GREECE = 2,
            FPTR_CC_HUNGARY = 4,
            FPTR_CC_ITALY = 8,
            FPTR_CC_POLAND = 16,
            FPTR_CC_TURKEY = 32,
            FPTR_CC_RUSSIA = 64,
            FPTR_CC_BULGARIA = 128,
            FPTR_CC_ROMANIA = 256,
            // "DateType" Property Constants
            FPTR_DT_CONF = 1,
            FPTR_DT_EOD = 2,
            FPTR_DT_RESET = 3,
            FPTR_DT_RTC = 4,
            FPTR_DT_VAT = 5,
            // "ErrorLevel" Property Constants
            FPTR_EL_NONE = 1,
            FPTR_EL_RECOVERABLE = 2,
            FPTR_EL_FATAL = 3,
            FPTR_EL_BLOCKED = 4,
            // "ErrorState", "PrinterState" Property Constants
            FPTR_PS_MONITOR = 1,
            FPTR_PS_FISCAL_RECEIPT = 2,
            FPTR_PS_FISCAL_RECEIPT_TOTAL = 3,
            FPTR_PS_FISCAL_RECEIPT_ENDING = 4,
            FPTR_PS_FISCAL_DOCUMENT = 5,
            FPTR_PS_FIXED_OUTPUT = 6,
            FPTR_PS_ITEM_LIST = 7,
            FPTR_PS_LOCKED = 8,
            FPTR_PS_NONFISCAL = 9,
            FPTR_PS_REPORT = 10,
            // "FiscalReceiptStation" Property Constants
            FPTR_RS_RECEIPT = 1,
            FPTR_RS_SLIP = 2,
            // "FiscalReceiptType" Property Constants
            FPTR_RT_CASH_IN = 1,
            FPTR_RT_CASH_OUT = 2,
            FPTR_RT_GENERIC = 3,
            FPTR_RT_SALES = 4,
            FPTR_RT_SERVICE = 5,
            FPTR_RT_SIMPLE_INVOICE = 6,
            // "MessageType" Property Constants
            FPTR_MT_ADVANCE = 1,
            FPTR_MT_ADVANCE_PAID = 2,
            FPTR_MT_AMOUNT_TO_BE_PAID = 3,
            FPTR_MT_AMOUNT_TO_BE_PAID_BACK = 4,
            FPTR_MT_CARD = 5,
            FPTR_MT_CARD_NUMBER = 6,
            FPTR_MT_CARD_TYPE = 7,
            FPTR_MT_CASH = 8,
            FPTR_MT_CASHIER = 9,
            FPTR_MT_CASH_REGISTER_NUMBER = 10,
            FPTR_MT_CHANGE = 11,
            FPTR_MT_CHEQUE = 12,
            FPTR_MT_CLIENT_NUMBER = 13,
            FPTR_MT_CLIENT_SIGNATURE = 14,
            FPTR_MT_COUNTER_STATE = 15,
            FPTR_MT_CREDIT_CARD = 16,
            FPTR_MT_CURRENCY = 17,
            FPTR_MT_CURRENCY_VALUE = 18,
            FPTR_MT_DEPOSIT = 19,
            FPTR_MT_DEPOSIT_RETURNED = 20,
            FPTR_MT_DOT_LINE = 21,
            FPTR_MT_DRIVER_NUMB = 22,
            FPTR_MT_EMPTY_LINE = 23,
            FPTR_MT_FREE_TEXT = 24,
            FPTR_MT_FREE_TEXT_WITH_DAY_LIMIT = 25,
            FPTR_MT_GIVEN_DISCOUNT = 26,
            FPTR_MT_LOCAL_CREDIT = 27,
            FPTR_MT_MILEAGE_KM = 28,
            FPTR_MT_NOTE = 29,
            FPTR_MT_PAID = 30,
            FPTR_MT_PAY_IN = 31,
            FPTR_MT_POINT_GRANTED = 32,
            FPTR_MT_POINTS_BONUS = 33,
            FPTR_MT_POINTS_RECEIPT = 34,
            FPTR_MT_POINTS_TOTAL = 35,
            FPTR_MT_PROFITED = 36,
            FPTR_MT_RATE = 37,
            FPTR_MT_REGISTER_NUMB = 38,
            FPTR_MT_SHIFT_NUMBER = 39,
            FPTR_MT_STATE_OF_AN_ACCOUNT = 40,
            FPTR_MT_SUBSCRIPTION = 41,
            FPTR_MT_TABLE = 42,
            FPTR_MT_THANK_YOU_FOR_LOYALTY = 43,
            FPTR_MT_TRANSACTION_NUMB = 44,
            FPTR_MT_VALID_TO = 45,
            FPTR_MT_VOUCHER = 46,
            FPTR_MT_VOUCHER_PAID = 47,
            FPTR_MT_VOUCHER_VALUE = 48,
            FPTR_MT_WITH_DISCOUNT = 49,
            FPTR_MT_WITHOUT_UPLIFT = 50,
            // "SlipSelection" Property Constants
            FPTR_SS_FULL_LENGTH = 1,
            FPTR_SS_VALIDATION = 2,
            // "TotalizerType" Property Constants
            FPTR_TT_DOCUMENT = 1,
            FPTR_TT_DAY = 2,
            FPTR_TT_RECEIPT = 3,
            FPTR_TT_GRAND = 4,
            // "GetData" Method Constants
            FPTR_GD_CURRENT_TOTAL = 1,
            FPTR_GD_DAILY_TOTAL = 2,
            FPTR_GD_RECEIPT_NUMBER = 3,
            FPTR_GD_REFUND = 4,
            FPTR_GD_NOT_PAID = 5,
            FPTR_GD_MID_VOID = 6,
            FPTR_GD_Z_REPORT = 7,
            FPTR_GD_GRAND_TOTAL = 8,
            FPTR_GD_PRINTER_ID = 9,
            FPTR_GD_FIRMWARE = 10,
            FPTR_GD_RESTART = 11,
            FPTR_GD_REFUND_VOID = 12,
            FPTR_GD_NUMB_CONFIG_BLOCK = 13,
            FPTR_GD_NUMB_CURRENCY_BLOCK = 14,
            FPTR_GD_NUMB_HDR_BLOCK = 15,
            FPTR_GD_NUMB_RESET_BLOCK = 16,
            FPTR_GD_NUMB_VAT_BLOCK = 17,
            FPTR_GD_FISCAL_DOC = 18,
            FPTR_GD_FISCAL_DOC_VOID = 19,
            FPTR_GD_FISCAL_REC = 20,
            FPTR_GD_FISCAL_REC_VOID = 21,
            FPTR_GD_NONFISCAL_DOC = 22,
            FPTR_GD_NONFISCAL_DOC_VOID = 23,
            FPTR_GD_NONFISCAL_REC = 24,
            FPTR_GD_SIMP_INVOICE = 25,
            FPTR_GD_TENDER = 26,
            FPTR_GD_LINECOUNT = 27,
            FPTR_GD_DESCRIPTION_LENGTH = 28,
            //
            FPTR_PDL_CASH = 1,
            FPTR_PDL_CHEQUE = 2,
            FPTR_PDL_CHITTY = 3,
            FPTR_PDL_COUPON = 4,
            FPTR_PDL_CURRENCY = 5,
            FPTR_PDL_DRIVEN_OFF = 6,
            FPTR_PDL_EFT_IMPRINTER = 7,
            FPTR_PDL_EFT_TERMINAL = 8,
            FPTR_PDL_TERMINAL_IMPRINTER = 9,
            FPTR_PDL_FREE_GIFT = 10,
            FPTR_PDL_GIRO = 11,
            FPTR_PDL_HOME = 12,
            FPTR_PDL_IMPRINTER_WITH_ISSUER = 13,
            FPTR_PDL_LOCAL_ACCOUNT = 14,
            FPTR_PDL_LOCAL_ACCOUNT_CARD = 15,
            FPTR_PDL_PAY_CARD = 16,
            FPTR_PDL_PAY_CARD_MANUAL = 17,
            FPTR_PDL_PREPAY = 18,
            FPTR_PDL_PUMP_TEST = 19,
            FPTR_PDL_SHORT_CREDIT = 20,
            FPTR_PDL_STAFF = 21,
            FPTR_PDL_VOUCHER = 22,
            //
            FPTR_LC_ITEM = 1,
            FPTR_LC_ITEM_VOID = 2,
            FPTR_LC_DISCOUNT = 3,
            FPTR_LC_DISCOUNT_VOID = 4,
            FPTR_LC_SURCHARGE = 5,
            FPTR_LC_SURCHARGE_VOID = 6,
            FPTR_LC_REFUND = 7,
            FPTR_LC_REFUND_VOID = 8,
            FPTR_LC_SUBTOTAL_DISCOUNT = 9,
            FPTR_LC_SUBTOTAL_DISCOUNT_VOID = 10,
            FPTR_LC_SUBTOTAL_SURCHARGE = 11,
            FPTR_LC_SUBTOTAL_SURCHARGE_VOID = 12,
            FPTR_LC_COMMENT = 13,
            FPTR_LC_SUBTOTAL = 14,
            FPTR_LC_TOTAL = 15,
            //
            FPTR_DL_ITEM = 1,
            FPTR_DL_ITEM_ADJUSTMENT = 2,
            FPTR_DL_ITEM_FUEL = 3,
            FPTR_DL_ITEM_FUEL_VOID = 4,
            FPTR_DL_NOT_PAID = 5,
            FPTR_DL_PACKAGE_ADJUSTMENT = 6,
            FPTR_DL_REFUND = 7,
            FPTR_DL_REFUND_VOID = 8,
            FPTR_DL_SUBTOTAL_ADJUSTMENT = 9,
            FPTR_DL_TOTAL = 10,
            FPTR_DL_VOID = 11,
            FPTR_DL_VOID_ITEM = 12,
            // "GetTotalizer" Method Constants
            FPTR_GT_GROSS = 1,
            FPTR_GT_NET = 2,
            FPTR_GT_DISCOUNT = 3,
            FPTR_GT_DISCOUNT_VOID = 4,
            FPTR_GT_ITEM = 5,
            FPTR_GT_ITEM_VOID = 6,
            FPTR_GT_NOT_PAID = 7,
            FPTR_GT_REFUND = 8,
            FPTR_GT_REFUND_VOID = 9,
            FPTR_GT_SUBTOTAL_DISCOUNT = 10,
            FPTR_GT_SUBTOTAL_DISCOUNT_VOID = 11,
            FPTR_GT_SUBTOTAL_SURCHARGES = 12,
            FPTR_GT_SUBTOTAL_SURCHARGES_VOID = 13,
            FPTR_GT_SURCHARGE = 14,
            FPTR_GT_SURCHARGE_VOID = 15,
            FPTR_GT_VAT = 16,
            FPTR_GT_VAT_CATEGORY = 17,
            // "AdjustmentType" arguments in diverse methods
            FPTR_AT_AMOUNT_DISCOUNT = 1,
            FPTR_AT_AMOUNT_SURCHARGE = 2,
            FPTR_AT_PERCENTAGE_DISCOUNT = 3,
            FPTR_AT_PERCENTAGE_SURCHARGE = 4,
            // "ReportType" argument in "PrintReport" method
            FPTR_RT_ORDINAL = 1,
            FPTR_RT_DATE = 2,
            // "NewCurrency" argument in "SetCurrency" method
            FPTR_SC_EURO = 1,
            // "StatusUpdateEvent" Event: "Data" Parameter Constants
            FPTR_SUE_COVER_OPEN = 11,
            FPTR_SUE_COVER_OK = 12,
            //
            FPTR_SUE_JRN_EMPTY = 21,
            FPTR_SUE_JRN_NEAREMPTY = 22,
            FPTR_SUE_JRN_PAPEROK = 23,
            //
            FPTR_SUE_REC_EMPTY = 24,
            FPTR_SUE_REC_NEAREMPTY = 25,
            FPTR_SUE_REC_PAPEROK = 26,
            //
            FPTR_SUE_SLP_EMPTY = 27,
            FPTR_SUE_SLP_NEAREMPTY = 28,
            FPTR_SUE_SLP_PAPEROK = 29,
            //
            FPTR_SUE_IDLE = 1001,
            // "ResultCodeExtended" Property Constants
            OPOS_EFPTR_COVER_OPEN = 201,
            OPOS_EFPTR_JRN_EMPTY = 202,
            OPOS_EFPTR_REC_EMPTY = 203,
            OPOS_EFPTR_SLP_EMPTY = 204,
            OPOS_EFPTR_SLP_FORM = 205,
            OPOS_EFPTR_MISSING_DEVICES = 206,
            OPOS_EFPTR_WRONG_STATE = 207,
            OPOS_EFPTR_TECHNICAL_ASSISTANCE = 208,
            OPOS_EFPTR_CLOCK_ERROR = 209,
            OPOS_EFPTR_FISCAL_MEMORY_FULL = 210,
            OPOS_EFPTR_FISCAL_MEMORY_DISCONNECTED = 211,
            OPOS_EFPTR_FISCAL_TOTALS_ERROR = 212,
            OPOS_EFPTR_BAD_ITEM_QUANTITY = 213,
            OPOS_EFPTR_BAD_ITEM_AMOUNT = 214,
            OPOS_EFPTR_BAD_ITEM_DESCRIPTION = 215,
            OPOS_EFPTR_RECEIPT_TOTAL_OVERFLOW = 216,
            OPOS_EFPTR_BAD_VAT = 217,
            OPOS_EFPTR_BAD_PRICE = 218,
            OPOS_EFPTR_BAD_DATE = 219,
            OPOS_EFPTR_NEGATIVE_TOTAL = 220,
            OPOS_EFPTR_WORD_NOT_ALLOWED = 221,
            OPOS_EFPTR_BAD_LENGTH = 222,
            OPOS_EFPTR_MISSING_SET_CURRENCY = 223;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public static string GetStandardErrorMessage(int errorCode)
        {
            switch (errorCode)
            {
                case 101: /* OPOS_E_CLOSED     */ return "The device is closed.\r\nOpen and claim the device.";
                case 102: /* OPOS_E_CLAIMED    */ return "The device is already claimed.\r\nThe process claiming the device must release it before another process can use it.";
                case 103: /* OPOS_E_NOTCLAIMED */ return "The device has not been claimed for use.\r\nThe process must claim a device before using it.";
                case 104: /* OPOS_E_NOSERVICE  */ return "The control cannot communicate with the service object.\r\nThis is most likely a setup or configuration error.";
                case 105: /* OPOS_E_DISABLED   */ return "The device must always be enabled to access it.";
                case 106: /* OPOS_E_ILLEGAL    */ return "Illegal command.\r\nThis is usually caused by an invalid parameter in a command such as writing beyond the end of a row on a display.";
                case 107: /* OPOS_E_NOHARDWARE */ return "No valid hardware.";
                case 108: /* OPOS_E_OFFLINE    */ return "The physical device is not connected or not communicating with the terminal.";
                case 109: /* OPOS_E_NOEXIST    */ return "Fiscal device does not exist.\r\nUse the configuration utilities to setup the device.";
                case 110: /* OPOS_E_EXISTS     */ return "Fiscal device found.";
                case 111: /* OPOS_E_FAILURE    */ return "Fiscal device failure!\r\nPlease check the device and retry after you fix the problem.";
                case 112: /* OPOS_E_TIMEOUT    */ return "Fiscal device communication timeout.\r\nBe sure you have no software configuration conflicts.";
                case 113: /* OPOS_E_BUSY       */ return "The device is busy.\r\nPlease try again later.";
                case 114: /* OPOS_E_EXTENDED   */ return "Extended error code:";
            }
            return "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="intExtendedErrorCode"></param>
        /// <returns></returns>
        public static string GetExtendedErrorMessage(int intExtendedErrorCode)
        {
            if (10500 <= intExtendedErrorCode)
            {
                return strExtendedErrors_ADHME[intExtendedErrorCode - 10500];
            }
            else if (10000 <= intExtendedErrorCode)
            {
                return strExtendedErrors_FiscalPrinter[intExtendedErrorCode - 10000];
            }
            return "";
        }
    }
}

#region Comments
/*

/////////////////////////////////////////////////////////////////////
// OPOS "State" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG OPOS_S_CLOSED = 1;
const LONG OPOS_S_IDLE = 2;
const LONG OPOS_S_BUSY = 3;
const LONG OPOS_S_ERROR = 4;

/////////////////////////////////////////////////////////////////////
// OPOS "ResultCode" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG OPOS_SUCCESS = 0;
const LONG OPOS_E_CLOSED = 101;
const LONG OPOS_E_CLAIMED = 102;
const LONG OPOS_E_NOTCLAIMED = 103;
const LONG OPOS_E_NOSERVICE = 104;
const LONG OPOS_E_DISABLED = 105;
const LONG OPOS_E_ILLEGAL = 106;
const LONG OPOS_E_NOHARDWARE = 107;
const LONG OPOS_E_OFFLINE = 108;
const LONG OPOS_E_NOEXIST = 109;
const LONG OPOS_E_EXISTS = 110;
const LONG OPOS_E_FAILURE = 111;
const LONG OPOS_E_TIMEOUT = 112;
const LONG OPOS_E_BUSY = 113;
const LONG OPOS_E_EXTENDED = 114;

const LONG OPOSERR = 100; // Base for ResultCode errors.
const LONG OPOSERREXT = 200; // Base for ResultCodeExtendedErrors.

/////////////////////////////////////////////////////////////////////
// OPOS "OpenResult" Property Constants
/////////////////////////////////////////////////////////////////////

// The following can be set by the control object.
const LONG OPOS_OR_ALREADYOPEN = 301;
// Control Object already open.
const LONG OPOS_OR_REGBADNAME = 302;
// The registry does not contain a key for the specified
// device name.
const LONG OPOS_OR_REGPROGID = 303;
// Could not read the device name key's default value, or
// could not convert this Program ID to a valid Class ID.
const LONG OPOS_OR_CREATE = 304;
// Could not create a service object instance, or
// could not get its IDispatch interface.
const LONG OPOS_OR_BADIF = 305;
// The service object does not support one or more of the
// method required by its release.
const LONG OPOS_OR_FAILEDOPEN = 306;
// The service object returned a failure status from its
// open call, but doesn't have a more specific failure code.
const LONG OPOS_OR_BADVERSION = 307;
// The service object major version number is not 1.

// The following can be returned by the service object if it
// returns a failure status from its open call.
const LONG OPOS_ORS_NOPORT = 401;
// Port access required at open, but configured port
// is invalid or inaccessible.
const LONG OPOS_ORS_NOTSUPPORTED = 402;
// Service Object does not support the specified device.
const LONG OPOS_ORS_CONFIG = 403;
// Configuration information error.
const LONG OPOS_ORS_SPECIFIC = 450;
// Errors greater than this value are SO-specific.

/////////////////////////////////////////////////////////////////////
// OPOS "BinaryConversion" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG OPOS_BC_NONE = 0;
const LONG OPOS_BC_NIBBLE = 1;
const LONG OPOS_BC_DECIMAL = 2;

/////////////////////////////////////////////////////////////////////
// "CheckHealth" Method: "Level" Parameter Constants
/////////////////////////////////////////////////////////////////////

const LONG OPOS_CH_INTERNAL = 1;
const LONG OPOS_CH_EXTERNAL = 2;
const LONG OPOS_CH_INTERACTIVE = 3;

/////////////////////////////////////////////////////////////////////
// OPOS "CapPowerReporting", "PowerState", "PowerNotify" Property
//   Constants
/////////////////////////////////////////////////////////////////////

const LONG OPOS_PR_NONE = 0;
const LONG OPOS_PR_STANDARD = 1;
const LONG OPOS_PR_ADVANCED = 2;

const LONG OPOS_PN_DISABLED = 0;
const LONG OPOS_PN_ENABLED = 1;

const LONG OPOS_PS_UNKNOWN = 2000;
const LONG OPOS_PS_ONLINE = 2001;
const LONG OPOS_PS_OFF = 2002;
const LONG OPOS_PS_OFFLINE = 2003;
const LONG OPOS_PS_OFF_OFFLINE = 2004;

/////////////////////////////////////////////////////////////////////
// "ErrorEvent" Event: "ErrorLocus" Parameter Constants
/////////////////////////////////////////////////////////////////////

const LONG OPOS_EL_OUTPUT = 1;
const LONG OPOS_EL_INPUT = 2;
const LONG OPOS_EL_INPUT_DATA = 3;

/////////////////////////////////////////////////////////////////////
// "ErrorEvent" Event: "ErrorResponse" Constants
/////////////////////////////////////////////////////////////////////

const LONG OPOS_ER_RETRY = 11;
const LONG OPOS_ER_CLEAR = 12;
const LONG OPOS_ER_CONTINUEINPUT = 13;

/////////////////////////////////////////////////////////////////////
// "StatusUpdateEvent" Event: Common "Status" Constants
/////////////////////////////////////////////////////////////////////

const LONG OPOS_SUE_POWER_ONLINE = 2001;
const LONG OPOS_SUE_POWER_OFF = 2002;
const LONG OPOS_SUE_POWER_OFFLINE = 2003;
const LONG OPOS_SUE_POWER_OFF_OFFLINE = 2004;

/////////////////////////////////////////////////////////////////////
// General Constants
/////////////////////////////////////////////////////////////////////

const LONG OPOS_FOREVER = -1;

/////////////////////////////////////////////////////////////////////
// Fiscal Printer Station Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_S_JOURNAL = 1;
const LONG FPTR_S_RECEIPT = 2;
const LONG FPTR_S_SLIP = 4;

const LONG FPTR_S_JOURNAL_RECEIPT = 3;

/////////////////////////////////////////////////////////////////////
// "ActualCurrency" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_AC_BRC = 1;
const LONG FPTR_AC_BGL = 2;
const LONG FPTR_AC_EUR = 3;
const LONG FPTR_AC_GRD = 4;
const LONG FPTR_AC_HUF = 5;
const LONG FPTR_AC_ITL = 6;
const LONG FPTR_AC_PLZ = 7;
const LONG FPTR_AC_ROL = 8;
const LONG FPTR_AC_RUR = 9;
const LONG FPTR_AC_TRL = 10;

/////////////////////////////////////////////////////////////////////
// "ContractorId" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_CID_FIRST = 1;
const LONG FPTR_CID_SECOND = 2;
const LONG FPTR_CID_SINGLE = 3;

/////////////////////////////////////////////////////////////////////
// "CountryCode" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_CC_BRAZIL = 1;
const LONG FPTR_CC_GREECE = 2;
const LONG FPTR_CC_HUNGARY = 4;
const LONG FPTR_CC_ITALY = 8;
const LONG FPTR_CC_POLAND = 16;
const LONG FPTR_CC_TURKEY = 32;
const LONG FPTR_CC_RUSSIA = 64;
const LONG FPTR_CC_BULGARIA = 128;
const LONG FPTR_CC_ROMANIA = 256;

/////////////////////////////////////////////////////////////////////
// "DateType" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_DT_CONF = 1;
const LONG FPTR_DT_EOD = 2;
const LONG FPTR_DT_RESET = 3;
const LONG FPTR_DT_RTC = 4;
const LONG FPTR_DT_VAT = 5;

/////////////////////////////////////////////////////////////////////
// "ErrorLevel" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_EL_NONE = 1;
const LONG FPTR_EL_RECOVERABLE = 2;
const LONG FPTR_EL_FATAL = 3;
const LONG FPTR_EL_BLOCKED = 4;

/////////////////////////////////////////////////////////////////////
// "ErrorState", "PrinterState" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_PS_MONITOR = 1;
const LONG FPTR_PS_FISCAL_RECEIPT = 2;
const LONG FPTR_PS_FISCAL_RECEIPT_TOTAL = 3;
const LONG FPTR_PS_FISCAL_RECEIPT_ENDING = 4;
const LONG FPTR_PS_FISCAL_DOCUMENT = 5;
const LONG FPTR_PS_FIXED_OUTPUT = 6;
const LONG FPTR_PS_ITEM_LIST = 7;
const LONG FPTR_PS_LOCKED = 8;
const LONG FPTR_PS_NONFISCAL = 9;
const LONG FPTR_PS_REPORT = 10;

/////////////////////////////////////////////////////////////////////
// "FiscalReceiptStation" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_RS_RECEIPT = 1;
const LONG FPTR_RS_SLIP = 2;

/////////////////////////////////////////////////////////////////////
// "FiscalReceiptType" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_RT_CASH_IN = 1;
const LONG FPTR_RT_CASH_OUT = 2;
const LONG FPTR_RT_GENERIC = 3;
const LONG FPTR_RT_SALES = 4;
const LONG FPTR_RT_SERVICE = 5;
const LONG FPTR_RT_SIMPLE_INVOICE = 6;

/////////////////////////////////////////////////////////////////////
// "MessageType" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_MT_ADVANCE = 1;
const LONG FPTR_MT_ADVANCE_PAID = 2;
const LONG FPTR_MT_AMOUNT_TO_BE_PAID = 3;
const LONG FPTR_MT_AMOUNT_TO_BE_PAID_BACK = 4;
const LONG FPTR_MT_CARD = 5;
const LONG FPTR_MT_CARD_NUMBER = 6;
const LONG FPTR_MT_CARD_TYPE = 7;
const LONG FPTR_MT_CASH = 8;
const LONG FPTR_MT_CASHIER = 9;
const LONG FPTR_MT_CASH_REGISTER_NUMBER = 10;
const LONG FPTR_MT_CHANGE = 11;
const LONG FPTR_MT_CHEQUE = 12;
const LONG FPTR_MT_CLIENT_NUMBER = 13;
const LONG FPTR_MT_CLIENT_SIGNATURE = 14;
const LONG FPTR_MT_COUNTER_STATE = 15;
const LONG FPTR_MT_CREDIT_CARD = 16;
const LONG FPTR_MT_CURRENCY = 17;
const LONG FPTR_MT_CURRENCY_VALUE = 18;
const LONG FPTR_MT_DEPOSIT = 19;
const LONG FPTR_MT_DEPOSIT_RETURNED = 20;
const LONG FPTR_MT_DOT_LINE = 21;
const LONG FPTR_MT_DRIVER_NUMB = 22;
const LONG FPTR_MT_EMPTY_LINE = 23;
const LONG FPTR_MT_FREE_TEXT = 24;
const LONG FPTR_MT_FREE_TEXT_WITH_DAY_LIMIT = 25;
const LONG FPTR_MT_GIVEN_DISCOUNT = 26;
const LONG FPTR_MT_LOCAL_CREDIT = 27;
const LONG FPTR_MT_MILEAGE_KM = 28;
const LONG FPTR_MT_NOTE = 29;
const LONG FPTR_MT_PAID = 30;
const LONG FPTR_MT_PAY_IN = 31;
const LONG FPTR_MT_POINT_GRANTED = 32;
const LONG FPTR_MT_POINTS_BONUS = 33;
const LONG FPTR_MT_POINTS_RECEIPT = 34;
const LONG FPTR_MT_POINTS_TOTAL = 35;
const LONG FPTR_MT_PROFITED = 36;
const LONG FPTR_MT_RATE = 37;
const LONG FPTR_MT_REGISTER_NUMB = 38;
const LONG FPTR_MT_SHIFT_NUMBER = 39;
const LONG FPTR_MT_STATE_OF_AN_ACCOUNT = 40;
const LONG FPTR_MT_SUBSCRIPTION = 41;
const LONG FPTR_MT_TABLE = 42;
const LONG FPTR_MT_THANK_YOU_FOR_LOYALTY = 43;
const LONG FPTR_MT_TRANSACTION_NUMB = 44;
const LONG FPTR_MT_VALID_TO = 45;
const LONG FPTR_MT_VOUCHER = 46;
const LONG FPTR_MT_VOUCHER_PAID = 47;
const LONG FPTR_MT_VOUCHER_VALUE = 48;
const LONG FPTR_MT_WITH_DISCOUNT = 49;
const LONG FPTR_MT_WITHOUT_UPLIFT = 50;

/////////////////////////////////////////////////////////////////////
// "SlipSelection" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_SS_FULL_LENGTH = 1;
const LONG FPTR_SS_VALIDATION = 2;

/////////////////////////////////////////////////////////////////////
// "TotalizerType" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_TT_DOCUMENT = 1;
const LONG FPTR_TT_DAY = 2;
const LONG FPTR_TT_RECEIPT = 3;
const LONG FPTR_TT_GRAND = 4;

/////////////////////////////////////////////////////////////////////
// "GetData" Method Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_GD_CURRENT_TOTAL = 1;
const LONG FPTR_GD_DAILY_TOTAL = 2;
const LONG FPTR_GD_RECEIPT_NUMBER = 3;
const LONG FPTR_GD_REFUND = 4;
const LONG FPTR_GD_NOT_PAID = 5;
const LONG FPTR_GD_MID_VOID = 6;
const LONG FPTR_GD_Z_REPORT = 7;
const LONG FPTR_GD_GRAND_TOTAL = 8;
const LONG FPTR_GD_PRINTER_ID = 9;
const LONG FPTR_GD_FIRMWARE = 10;
const LONG FPTR_GD_RESTART = 11;
const LONG FPTR_GD_REFUND_VOID = 12;
const LONG FPTR_GD_NUMB_CONFIG_BLOCK = 13;
const LONG FPTR_GD_NUMB_CURRENCY_BLOCK = 14;
const LONG FPTR_GD_NUMB_HDR_BLOCK = 15;
const LONG FPTR_GD_NUMB_RESET_BLOCK = 16;
const LONG FPTR_GD_NUMB_VAT_BLOCK = 17;
const LONG FPTR_GD_FISCAL_DOC = 18;
const LONG FPTR_GD_FISCAL_DOC_VOID = 19;
const LONG FPTR_GD_FISCAL_REC = 20;
const LONG FPTR_GD_FISCAL_REC_VOID = 21;
const LONG FPTR_GD_NONFISCAL_DOC = 22;
const LONG FPTR_GD_NONFISCAL_DOC_VOID = 23;
const LONG FPTR_GD_NONFISCAL_REC = 24;
const LONG FPTR_GD_SIMP_INVOICE = 25;
const LONG FPTR_GD_TENDER = 26;
const LONG FPTR_GD_LINECOUNT = 27;
const LONG FPTR_GD_DESCRIPTION_LENGTH = 28;

const LONG FPTR_PDL_CASH = 1;
const LONG FPTR_PDL_CHEQUE = 2;
const LONG FPTR_PDL_CHITTY = 3;
const LONG FPTR_PDL_COUPON = 4;
const LONG FPTR_PDL_CURRENCY = 5;
const LONG FPTR_PDL_DRIVEN_OFF = 6;
const LONG FPTR_PDL_EFT_IMPRINTER = 7;
const LONG FPTR_PDL_EFT_TERMINAL = 8;
const LONG FPTR_PDL_TERMINAL_IMPRINTER = 9;
const LONG FPTR_PDL_FREE_GIFT = 10;
const LONG FPTR_PDL_GIRO = 11;
const LONG FPTR_PDL_HOME = 12;
const LONG FPTR_PDL_IMPRINTER_WITH_ISSUER = 13;
const LONG FPTR_PDL_LOCAL_ACCOUNT = 14;
const LONG FPTR_PDL_LOCAL_ACCOUNT_CARD = 15;
const LONG FPTR_PDL_PAY_CARD = 16;
const LONG FPTR_PDL_PAY_CARD_MANUAL = 17;
const LONG FPTR_PDL_PREPAY = 18;
const LONG FPTR_PDL_PUMP_TEST = 19;
const LONG FPTR_PDL_SHORT_CREDIT = 20;
const LONG FPTR_PDL_STAFF = 21;
const LONG FPTR_PDL_VOUCHER = 22;

const LONG FPTR_LC_ITEM = 1;
const LONG FPTR_LC_ITEM_VOID = 2;
const LONG FPTR_LC_DISCOUNT = 3;
const LONG FPTR_LC_DISCOUNT_VOID = 4;
const LONG FPTR_LC_SURCHARGE = 5;
const LONG FPTR_LC_SURCHARGE_VOID = 6;
const LONG FPTR_LC_REFUND = 7;
const LONG FPTR_LC_REFUND_VOID = 8;
const LONG FPTR_LC_SUBTOTAL_DISCOUNT = 9;
const LONG FPTR_LC_SUBTOTAL_DISCOUNT_VOID = 10;
const LONG FPTR_LC_SUBTOTAL_SURCHARGE = 11;
const LONG FPTR_LC_SUBTOTAL_SURCHARGE_VOID = 12;
const LONG FPTR_LC_COMMENT = 13;
const LONG FPTR_LC_SUBTOTAL = 14;
const LONG FPTR_LC_TOTAL = 15;

const LONG FPTR_DL_ITEM = 1;
const LONG FPTR_DL_ITEM_ADJUSTMENT = 2;
const LONG FPTR_DL_ITEM_FUEL = 3;
const LONG FPTR_DL_ITEM_FUEL_VOID = 4;
const LONG FPTR_DL_NOT_PAID = 5;
const LONG FPTR_DL_PACKAGE_ADJUSTMENT = 6;
const LONG FPTR_DL_REFUND = 7;
const LONG FPTR_DL_REFUND_VOID = 8;
const LONG FPTR_DL_SUBTOTAL_ADJUSTMENT = 9;
const LONG FPTR_DL_TOTAL = 10;
const LONG FPTR_DL_VOID = 11;
const LONG FPTR_DL_VOID_ITEM = 12;

/////////////////////////////////////////////////////////////////////
// "GetTotalizer" Method Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_GT_GROSS = 1;
const LONG FPTR_GT_NET = 2;
const LONG FPTR_GT_DISCOUNT = 3;
const LONG FPTR_GT_DISCOUNT_VOID = 4;
const LONG FPTR_GT_ITEM = 5;
const LONG FPTR_GT_ITEM_VOID = 6;
const LONG FPTR_GT_NOT_PAID = 7;
const LONG FPTR_GT_REFUND = 8;
const LONG FPTR_GT_REFUND_VOID = 9;
const LONG FPTR_GT_SUBTOTAL_DISCOUNT = 10;
const LONG FPTR_GT_SUBTOTAL_DISCOUNT_VOID = 11;
const LONG FPTR_GT_SUBTOTAL_SURCHARGES = 12;
const LONG FPTR_GT_SUBTOTAL_SURCHARGES_VOID = 13;
const LONG FPTR_GT_SURCHARGE = 14;
const LONG FPTR_GT_SURCHARGE_VOID = 15;
const LONG FPTR_GT_VAT = 16;
const LONG FPTR_GT_VAT_CATEGORY = 17;

/////////////////////////////////////////////////////////////////////
// "AdjustmentType" arguments in diverse methods
/////////////////////////////////////////////////////////////////////

const LONG FPTR_AT_AMOUNT_DISCOUNT = 1;
const LONG FPTR_AT_AMOUNT_SURCHARGE = 2;
const LONG FPTR_AT_PERCENTAGE_DISCOUNT = 3;
const LONG FPTR_AT_PERCENTAGE_SURCHARGE = 4;

/////////////////////////////////////////////////////////////////////
// "ReportType" argument in "PrintReport" method
/////////////////////////////////////////////////////////////////////

const LONG FPTR_RT_ORDINAL = 1;
const LONG FPTR_RT_DATE = 2;

/////////////////////////////////////////////////////////////////////
// "NewCurrency" argument in "SetCurrency" method
/////////////////////////////////////////////////////////////////////

const LONG FPTR_SC_EURO = 1;

/////////////////////////////////////////////////////////////////////
// "StatusUpdateEvent" Event: "Data" Parameter Constants
/////////////////////////////////////////////////////////////////////

const LONG FPTR_SUE_COVER_OPEN = 11;
const LONG FPTR_SUE_COVER_OK = 12;

const LONG FPTR_SUE_JRN_EMPTY = 21;
const LONG FPTR_SUE_JRN_NEAREMPTY = 22;
const LONG FPTR_SUE_JRN_PAPEROK = 23;

const LONG FPTR_SUE_REC_EMPTY = 24;
const LONG FPTR_SUE_REC_NEAREMPTY = 25;
const LONG FPTR_SUE_REC_PAPEROK = 26;

const LONG FPTR_SUE_SLP_EMPTY = 27;
const LONG FPTR_SUE_SLP_NEAREMPTY = 28;
const LONG FPTR_SUE_SLP_PAPEROK = 29;

const LONG FPTR_SUE_IDLE = 1001;

/////////////////////////////////////////////////////////////////////
// "ResultCodeExtended" Property Constants
/////////////////////////////////////////////////////////////////////

const LONG OPOS_EFPTR_COVER_OPEN = 201; // (Several)
const LONG OPOS_EFPTR_JRN_EMPTY = 202; // (Several)
const LONG OPOS_EFPTR_REC_EMPTY = 203; // (Several)
const LONG OPOS_EFPTR_SLP_EMPTY = 204; // (Several)
const LONG OPOS_EFPTR_SLP_FORM = 205; // EndRemoval
const LONG OPOS_EFPTR_MISSING_DEVICES = 206; // (Several)
const LONG OPOS_EFPTR_WRONG_STATE = 207; // (Several)
const LONG OPOS_EFPTR_TECHNICAL_ASSISTANCE = 208; // (Several)
const LONG OPOS_EFPTR_CLOCK_ERROR = 209; // (Several)
const LONG OPOS_EFPTR_FISCAL_MEMORY_FULL = 210; // (Several)
const LONG OPOS_EFPTR_FISCAL_MEMORY_DISCONNECTED = 211; // (Several)
const LONG OPOS_EFPTR_FISCAL_TOTALS_ERROR = 212; // (Several)
const LONG OPOS_EFPTR_BAD_ITEM_QUANTITY = 213; // (Several)
const LONG OPOS_EFPTR_BAD_ITEM_AMOUNT = 214; // (Several)
const LONG OPOS_EFPTR_BAD_ITEM_DESCRIPTION = 215; // (Several)
const LONG OPOS_EFPTR_RECEIPT_TOTAL_OVERFLOW = 216; // (Several)
const LONG OPOS_EFPTR_BAD_VAT = 217; // (Several)
const LONG OPOS_EFPTR_BAD_PRICE = 218; // (Several)
const LONG OPOS_EFPTR_BAD_DATE = 219; // (Several)
const LONG OPOS_EFPTR_NEGATIVE_TOTAL = 220; // (Several)
const LONG OPOS_EFPTR_WORD_NOT_ALLOWED = 221; // (Several)
const LONG OPOS_EFPTR_BAD_LENGTH = 222; // (Several)
const LONG OPOS_EFPTR_MISSING_SET_CURRENCY = 223; // (Several)

/////////////////////////////////////////////////////////////////////
// "ResultCodeExtended" Property Constants
/////////////////////////////////////////////////////////////////////
#define FSCPRT_ERRORS  10000
#define ADHME_ERRORS   10500

const LONG OPOS_EFSCPRT_PROTOCOL_COMMAND_EXPECTS_MORE_FIELDS_1 = FSCPRT_ERRORS + 1;
const LONG OPOS_EFSCPRT_PROTOCOL_COMMAND_FIELD_IS_LONGER_THAN_EXPECTED = FSCPRT_ERRORS + 2;
const LONG OPOS_EFSCPRT_PROTOCOL_COMMAND_FIELD_IS_SMALLER_THAN_EXPECTED = FSCPRT_ERRORS + 3;
const LONG OPOS_EFSCPRT_PROTOCOL_COMMAND_EXPECTS_MORE_FIELDS_2 = FSCPRT_ERRORS + 4;
const LONG OPOS_EFSCPRT_CHECK_PROTOCOL_COMMAND_FIELDS = FSCPRT_ERRORS + 5;
const LONG OPOS_EFSCPRT_PROTOCOL_COMMAND_NOT_SUPPORTED = FSCPRT_ERRORS + 6;
const LONG OPOS_EFSCPRT_PLU_CODE_NOT_EXIST = FSCPRT_ERRORS + 7;
const LONG OPOS_EFSCPRT_DPT_CODE_NOT_EXIST = FSCPRT_ERRORS + 8;
const LONG OPOS_EFSCPRT_WRONG_VAT_CODE = FSCPRT_ERRORS + 9;
const LONG OPOS_EFSCPRT_CLERK_INDEX_NUMBER_NOT_EXIST = FSCPRT_ERRORS + 10;
const LONG OPOS_EFSCPRT_WRONG_CLERK_PASSWORD = FSCPRT_ERRORS + 11;
const LONG OPOS_EFSCPRT_PAYMENTCODE_NOT_EXIST = FSCPRT_ERRORS + 12;
const LONG OPOS_EFSCPRT_REQUESTED_FISCAL_RECORD_NOT_EXIST = FSCPRT_ERRORS + 13;
const LONG OPOS_EFSCPRT_REQUESTED_FISCAL_RECORD_TYPE_NOT_EXIST = FSCPRT_ERRORS + 14;
const LONG OPOS_EFSCPRT_PRINTING_TYPE_ERROR = FSCPRT_ERRORS + 15;
const LONG OPOS_EFSCPRT_DAY_IS_OPEN_ISSUE_Z_REPORT_FIRST = FSCPRT_ERRORS + 16;
const LONG OPOS_EFSCPRT_DISCONNECT_JUMPER_FIRST = FSCPRT_ERRORS + 17;
const LONG OPOS_EFSCPRT_WRONG_TIME_CALL_SERVICE_1 = FSCPRT_ERRORS + 18;
const LONG OPOS_EFSCPRT_NOT_USED_1 = FSCPRT_ERRORS + 19;
const LONG OPOS_EFSCPRT_TRANSACTION_IS_OPEN_CLOSE_THE_TRNSACTIO_FIRST = FSCPRT_ERRORS + 20;
const LONG OPOS_EFSCPRT_INVALID_PAYMENT = FSCPRT_ERRORS + 21;
const LONG OPOS_EFSCPRT_CASH_IN_OUTTRANSACTION_IN_PROGRESS = FSCPRT_ERRORS + 22;
const LONG OPOS_EFSCPRT_WRONG_VAT_RATE = FSCPRT_ERRORS + 23;
const LONG OPOS_EFSCPRT_PRICE_ERROR = FSCPRT_ERRORS + 24;
const LONG OPOS_EFSCPRT_ONLINE_COMMUNICATION_OF_ECR_IS_ON = FSCPRT_ERRORS + 25;
const LONG OPOS_EFSCPRT_ECR_IS_BUSY = FSCPRT_ERRORS + 26;
const LONG OPOS_EFSCPRT_INVALID_SALES_OPERATION = FSCPRT_ERRORS + 27;
const LONG OPOS_EFSCPRT_INVALID_DISCOUNT_MARKUP_TYPE = FSCPRT_ERRORS + 28;
const LONG OPOS_EFSCPRT_NO_MORE_HEADERS_CAN_BE_PROGRAMMED_1 = FSCPRT_ERRORS + 29;
const LONG OPOS_EFSCPRT_USER_REPORT_IS_OPEN_1 = FSCPRT_ERRORS + 30;
const LONG OPOS_EFSCPRT_USER_REPORT_IS_OPEN_2 = FSCPRT_ERRORS + 31;
const LONG OPOS_EFSCPRT_FISCAL_MEMORY_HAS_NO_TRANSACTIONS = FSCPRT_ERRORS + 32;
const LONG OPOS_EFSCPRT_DISCOUNT_MARKUP_INDEX_NUMBER_ERROR = FSCPRT_ERRORS + 33;
const LONG OPOS_EFSCPRT_CANT_PROGRAM_MORE_PLUS = FSCPRT_ERRORS + 34;
const LONG OPOS_EFSCPRT_ERROR_IN_BMP_DATA = FSCPRT_ERRORS + 35;
const LONG OPOS_EFSCPRT_BMP_INDEX_NUMBER_NOT_EXIST = FSCPRT_ERRORS + 36;
const LONG OPOS_EFSCPRT_CATEGORY_INDEX_NUMBER_NOT_EXIST = FSCPRT_ERRORS + 37;
const LONG OPOS_EFSCPRT_NOT_USED_2 = FSCPRT_ERRORS + 38;
const LONG OPOS_EFSCPRT_ERROR_PRINTING_TYPE = FSCPRT_ERRORS + 39;
const LONG OPOS_EFSCPRT_NOT_USED = FSCPRT_ERRORS + 40;
const LONG OPOS_EFSCPRT_NO_MORE_SALES_CAN_BE_PERFORMED = FSCPRT_ERRORS + 41;
const LONG OPOS_EFSCPRT_KEYBOARD_ERROR_OR_DISCONNECTED = FSCPRT_ERRORS + 42;
const LONG OPOS_EFSCPRT_BATTERY_ERROR_LOW = FSCPRT_ERRORS + 43;
const LONG OPOS_EFSCPRT_LARGE_QUANTITY_VALUE = FSCPRT_ERRORS + 100;
const LONG OPOS_EFSCPRT_LARGE_SALE_VALUE = FSCPRT_ERRORS + 101;
const LONG OPOS_EFSCPRT_PLU_NOT_EXIST = FSCPRT_ERRORS + 102;
const LONG OPOS_EFSCPRT_DPT_NOT_EXIST = FSCPRT_ERRORS + 103;
const LONG OPOS_EFSCPRT_OPEN_RECEIPT_1 = FSCPRT_ERRORS + 104;
const LONG OPOS_EFSCPRT_OPEN_CASH_IN_1 = FSCPRT_ERRORS + 105;
const LONG OPOS_EFSCPRT_OPEN_CASH_OUT_1 = FSCPRT_ERRORS + 106;
const LONG OPOS_EFSCPRT_NEGATIVE_VAT_AMOUNT = FSCPRT_ERRORS + 107;
const LONG OPOS_EFSCPRT_NO_OPEN_RECEIPT = FSCPRT_ERRORS + 108;
const LONG OPOS_EFSCPRT_NO_TRANSACTIONS_IN_RECEIPT = FSCPRT_ERRORS + 109;
const LONG OPOS_EFSCPRT_ACTION_IS_NOT_ALLOWED = FSCPRT_ERRORS + 110;
const LONG OPOS_EFSCPRT_TOTAL_PAYMENT_AMOUNT_MUST_BE_INSERTED_FIRST = FSCPRT_ERRORS + 111;
const LONG OPOS_EFSCPRT_NEGATIVE_TOTAL_IS_NOT_ALLOWED = FSCPRT_ERRORS + 112;
const LONG OPOS_EFSCPRT_SUBTOTAL_MUST_BE_INSERTED_FIRST = FSCPRT_ERRORS + 113;
const LONG OPOS_EFSCPRT_CHANGE_ARE_NOT_ALLOWED = FSCPRT_ERRORS + 114;
const LONG OPOS_EFSCPRT_OPEN_RECEIPT_2 = FSCPRT_ERRORS + 115;
const LONG OPOS_EFSCPRT_OPEN_CASH_IN_2 = FSCPRT_ERRORS + 116;
const LONG OPOS_EFSCPRT_OPEN_CASH_OUT_2 = FSCPRT_ERRORS + 117;
const LONG OPOS_EFSCPRT_OPEN_CASH_OUT_3 = FSCPRT_ERRORS + 118;
const LONG OPOS_EFSCPRT_WRONG_PAYMENT_CODE = FSCPRT_ERRORS + 119;
const LONG OPOS_EFSCPRT_WRONG_DPT_CODE = FSCPRT_ERRORS + 120;
const LONG OPOS_EFSCPRT_TICKET_DECIMAL_QUANTITY_IS_NOT_ALLOWED = FSCPRT_ERRORS + 121;
const LONG OPOS_EFSCPRT_NO_ZERO_DISCOUNT_MARKUP_IS_ALLOWED = FSCPRT_ERRORS + 122;
const LONG OPOS_EFSCPRT_DISCOUNT_MARKUP_LIMIT_IS_EXCEEDED = FSCPRT_ERRORS + 123;
const LONG OPOS_EFSCPRT_NO_ZERO_PLU_DPT_SALE_IS_ALLOWED = FSCPRT_ERRORS + 124;
const LONG OPOS_EFSCPRT_DPT_PLU_IS_NOT_ACTIVE = FSCPRT_ERRORS + 125;
const LONG OPOS_EFSCPRT_MAXIMUM_ALLOWED_RECEIPT_TOTAL_IS_EXCEEDED = FSCPRT_ERRORS + 126;
const LONG OPOS_EFSCPRT_MAXIMUM_SALES_TOTAL_IS_EXCEEDED = FSCPRT_ERRORS + 127;
const LONG OPOS_EFSCPRT_COUPON_DISCOUNT_IN_SUBTOTAL_IS_NOT_ALLOWED = FSCPRT_ERRORS + 128;
const LONG OPOS_EFSCPRT_NO_MORE_DISCOUNT_MARKUP_IS_ALLOWED_SALE_MUST_OCCUR_FISRT = FSCPRT_ERRORS + 129;
const LONG OPOS_EFSCPRT_WRONG_DATE_TIME_CALL_SERVICE = FSCPRT_ERRORS + 130;
const LONG OPOS_EFSCPRT_NO_NEGATIVE_VAT_AMOUNT_IS_ALLOWED = FSCPRT_ERRORS + 131;
const LONG OPOS_EFSCPRT_SUBTOTAL_MUST_BE_PRESSED_FIRST_BEFORE_DISCOUNT_MARKUP = FSCPRT_ERRORS + 132;
const LONG OPOS_EFSCPRT_NO_DISCOUNT_MARKUP_IS_ALLOWED_IN_SUBTOTAL = FSCPRT_ERRORS + 133;
const LONG OPOS_EFSCPRT_THERE_NO_FISCAL_DATA_FOR_REQUESTED_PERIOD = FSCPRT_ERRORS + 134;
const LONG OPOS_EFSCPRT_DISCOUNT_MARKUP_IS_NOT_ACTIVE = FSCPRT_ERRORS + 135;
const LONG OPOS_EFSCPRT_FISCAL_MEMORY_IS_FULL = FSCPRT_ERRORS + 136;
const LONG OPOS_EFSCPRT_THERE_ARE_NO_TRANSACTIONS = FSCPRT_ERRORS + 137;
const LONG OPOS_EFSCPRT_CLERK_IS_NOT_ALLOWED_TO_PERFORM_THIS_ACTION = FSCPRT_ERRORS + 138;
const LONG OPOS_EFSCPRT_NO_DAILY_TRANSACTIONS = FSCPRT_ERRORS + 139;
const LONG OPOS_EFSCPRT_WRONG_DATE_CALL_SERVICE = FSCPRT_ERRORS + 140;
const LONG OPOS_EFSCPRT_WRONG_TIME_CALL_SERVICE_2 = FSCPRT_ERRORS + 141;
const LONG OPOS_EFSCPRT_FISCAL_MEMORY_DISCONNECTED = FSCPRT_ERRORS + 142;
const LONG OPOS_EFSCPRT_THERE_DAILY_TRANSACTIONS_ISSUE_Z_REPORT_FIRST = FSCPRT_ERRORS + 143;
const LONG OPOS_EFSCPRT_ACCESS_ONLY_TO_SERVICE = FSCPRT_ERRORS + 144;
const LONG OPOS_EFSCPRT_JOURNAL_PAPER_END = FSCPRT_ERRORS + 145;
const LONG OPOS_EFSCPRT_RECEIPT_PAPER_END = FSCPRT_ERRORS + 146;
const LONG OPOS_EFSCPRT_PRINTER_HEAD_OPEN = FSCPRT_ERRORS + 147;
const LONG OPOS_EFSCPRT_PINTER_DISCONNECTED = FSCPRT_ERRORS + 148;
const LONG OPOS_EFSCPRT_FISCAL_MEMORY_ERROR = FSCPRT_ERRORS + 149;
const LONG OPOS_EFSCPRT_CANNOT_PROGRAM_MORE_PLUS_CAUSE_PLU_INDEX_NUMBER_IS_EXCEEDED = FSCPRT_ERRORS + 150;
const LONG OPOS_EFSCPRT_CANNOT_PROGRAM_DISCOUNT_MARKUP = FSCPRT_ERRORS + 151;
const LONG OPOS_EFSCPRT_CLIENT_CODE_NOT_EXIST = FSCPRT_ERRORS + 152;
const LONG OPOS_EFSCPRT_CANT_PROGRAM_2_DIFFERENT_VAT_RATES_TO_HAVE_SAME_VALUE = FSCPRT_ERRORS + 153;
const LONG OPOS_EFSCPRT_NO_SALES_ALLOWED_AFTER_TICKET_DISCOUNT = FSCPRT_ERRORS + 154;
const LONG OPOS_EFSCPRT_NO_MORE_HEADERS_CAN_BE_PROGRAMMED_2 = FSCPRT_ERRORS + 155;
const LONG OPOS_EFSCPRT_PLUS_MUST_BE_ZEROED_FIRST = FSCPRT_ERRORS + 156;
const LONG OPOS_EFSCPRT_Z_READ_MUST_BE_ISSUED_FIRST = FSCPRT_ERRORS + 157;
const LONG OPOS_EFSCPRT_INACTIVE_PAYMENT = FSCPRT_ERRORS + 158;

char* g_ErrorMessagesFSCPRT[] =
{
"No errors - success              | None                                   ",
"Wrong number of fields /Check the command's field count",
"Field too long /A field is long: check it & retry",
"Field too small / A field is small: check it & retry ",
"Field fixed size mismatch /| A field size is wrong: check it & retry",
"Field range or type check failed | Check ranges or types in command ",
"Bad request code                 | Correct the request code (unknown)",
"PLU index range error/| Correct command's PLU index number ",
"DPT index range error /Correct command's DPT index number     ",
"Wrong VAT Code. Correct the Code",
"CLERK index range error/| Correct command's CLERK index number ",
"Wrong CLERK's Password",
"PAYMENT index range error | Correct command's PAYMENT index number ",
"The requested Fiscal record doesn't exist",
"The requested Fiscal record type doesn't exist",
"Printing type range error| Correct command's printing type ",
"Cannot execute with day open | Issue a Z report to close the day ",
"RTC programming requires jumper  | Short the 'clock' jumper and retry ",
"Wrong TIME, call SERVICE",
"NOT USED",
"Cannot execute with receipt open | Close the receipt and retry ",
"Invalid Payment",
"Cash in / Cash out open | Close the cash in/out and retry",
"Wrong VAT rate",
"Price Error",
"Cannot execute with online active| Close the online and retry ",
"The ECR is busy, try again later",
"Invalid sale operation | Correct the sale operation and retry ",
"Invalid Discount/Markup/.type | Correct the discount/markup type  ",
"No more headers can be programmed",
"A user's report is open",
"A user's report is open",
"The Fiscal Memory has no transactions",
"Discount/Markup index number error",
"You can't program any more PLUs",
"Error in BMP Data",
"The BMP index number doesn't exist",
"The category index number doesn't exist",
"NOT USED",
"Error printing type",
"NOT USED",
"No more sales can be performed",
"Keyboard error-or keyboard disconnected",
"Battery error-or battery low",
"Larger Quantity Value than the one allowed ",
"Larger Sales Value than the one allowed.",
"PLU does not exist.",
"DPT does not exist.",
"Receipt in PAYMENT state | Send the command when the transaction is not in payment state.",
"Cannot execute: CASH IN is open  | Do not send the command when in CASHIN ",
"Cannot execute: CASH OUT is open | Do not send the command when in CASHOUT",
"Negative VAT amount.",
"Transaction not open yet  | A transaction must be opened first ",
"No positive sale records yet | A positive sale must be done first ",
"Invalid operation in this state  | Check the state of the ECR",
"Zero amount invalid for payment  | Change the payment amount to non zero ",
"Negative total is not allowed",
"Subtotal must be inserted first",
"Change not allowed (this payment)| Correct the payment amount and retry ",
"A transaction is open | Close the transaction and retry",
"A cash in/out receipt is open | Close the cash in/out receipt and retry",
"A cash in receipt is not open | Open a cash in receipt and retry",
"A cash out receipt is not open | Open a cash out receipt and retry ",
"Payment code range error | Correct the payment code and retry ",
"Department index range error | Correct the department index and retry ",
"Ticket quantity non integer | Make the ticket quantity integer ",
"Discount/Markup amount is zero | Correct the amount and retry ",
"Discount/Markup amount limit | Make the amount less or equal to the setup's allowed maximum",
"No zero PLU/DPT sale is allowed.",
"Item not available (not 'set') | Try another item or modify it's 'set' flag by ECR's setup",
"Transaction record limit reached | No more sale records allowed for this receipt. Close the receipt and open a new one.",
"No negative drawer allowed | Correct the payment amount and retry",
"Coupon discount in subtotal is not allowed.",
"No more discount/markup is allowed, a sale must occur first.",
"Wrong DATE-TIME Call Service.",
"No negative VAT amount is allowed",
"Subtotal must be pressed first before Discount/Markup.",
"No Discount/Markup is allowed in Subtotal .",
"There are no fiscal data for the requested period .",
"The Discount/Markup is not active ",
"Fiscal Memory is full",
"There are no transactions",
"Clerk is not allowed to perform this action",
"No daily transactions",
"Wrong DATE, call SERVICE",
"Wrong TIME, call SERVICE",
"Fiscal MEMORY disconnected",
"There daily transactions, issue a Z report first",
"Access only to SERVICE",
"Paper End in Journal Station",
"Paper End in Receipt Station",
"Printer Head open",
"Printer Disconnected",
"Fiscal Memory Error",
"You cannot program any more PLUs cause the PLU index number is exceeded",
"You cannot program this Discount/Markup",
"The Client Code does not exist",
"You cannot program 2 different VAT rates to have the same value",
"No sales are allowed after a ticket discount",
"No more Headers can be programmed",
"The PLUS must be zeroed first",
"A Z READ must be issued first",
"Inactive Payment",
"TimeOut"
};

short g_ErrorCodesFSCPRT[] =
{
0x00,
1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,
21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,
38,39,40,41,42,43,100,101,102,103,104,105,106,107,
108,109,110,111,112,113,114,115,116,117,118,119,120,
121,122,123,124,125,126,127,128,129,130,131,132,133,
134,135,136,137,138,139,140,141,142,143,144,145,146,
147,148,149,150, 151,152,153,154,155,156,157,158,
0x100
};

const LONG OPOS_EADHME_WRONG_NUMBER_OF_FIELDS = ADHME_ERRORS + 0x01;
const LONG OPOS_EADHME_FIELD_TOO_LONG = ADHME_ERRORS + 0x02;
const LONG OPOS_EADHME_FIELD_TOO_SMALL = ADHME_ERRORS + 0x03;
const LONG OPOS_EADHME_FIELD_FIXED_SIZE_MISMATCH = ADHME_ERRORS + 0x04;
const LONG OPOS_EADHME_FIELD_RANGE_OR_TYPE_CHECK_FAILED = ADHME_ERRORS + 0x05;
const LONG OPOS_EADHME_BAD_REQUEST_CODE = ADHME_ERRORS + 0x06;
const LONG OPOS_EADHME_PRINTING_TYPE_BAD = ADHME_ERRORS + 0x09;
const LONG OPOS_EADHME_CANNOT_EXECUTE_WITH_DAY_OPEN = ADHME_ERRORS + 0x0A;
const LONG OPOS_EADHME_RTC_PROGRAMMING_REQUIRES_JUMPER = ADHME_ERRORS + 0x0B;
const LONG OPOS_EADHME_NO_RECORDS_IN_FISCAL_PERIOD = ADHME_ERRORS + 0x0D;
const LONG OPOS_EADHME_NO_MORE_HEADER_RECORDS_ALLOWED = ADHME_ERRORS + 0x0F;
const LONG OPOS_EADHME_CANNOT_EXECUTE_WITH_BLOCK_OPEN = ADHME_ERRORS + 0x10;
const LONG OPOS_EADHME_Z_CLOSURE_TIME_LIMIT = ADHME_ERRORS + 0x14;
const LONG OPOS_EADHME_Z_CLOSURE_NOT_FOUND = ADHME_ERRORS + 0x15;
const LONG OPOS_EADHME_Z_CLOSURE_RECORD_BAD = ADHME_ERRORS + 0x16;
const LONG OPOS_EADHME_USER_BROWSING_IN_PROGRESS = ADHME_ERRORS + 0x17;
const LONG OPOS_EADHME_FATAL_HARDWARE_ERROR = ADHME_ERRORS + 0x1C;
const LONG OPOS_EADHME_BATTERY_FAULT_DETECTED = ADHME_ERRORS + 0x20;
const LONG OPOS_EADHME_REALTIME_CLOCK_NEEDS_PROGRAMMING = ADHME_ERRORS + 0x23;
const LONG OPOS_EADHME_NO_JUPERON = ADHME_ERRORS + 0x24;
const LONG OPOS_EADHME_INVSALEOP = ADHME_ERRORS + 0x25;
const LONG OPOS_EADHME_DPTINDEXERR = ADHME_ERRORS + 0x26;
const LONG OPOS_EADHME_PAYMENTINDEXERR = ADHME_ERRORS + 0x28;
const LONG OPOS_EADHME_NOTENDREADLEGAL = ADHME_ERRORS + 0x2F;
const LONG OPOS_EADHME_NOTENDREADILEGAL = ADHME_ERRORS + 0x30;
const LONG OPOS_EADHME_WRONGILEGALNUMBER = ADHME_ERRORS + 0x31;
const LONG OPOS_EADHME_FLASHERROR = ADHME_ERRORS + 0x32;
const LONG OPOS_EADHME_NOTFOUNDRECEIPT = ADHME_ERRORS + 0x33;
const LONG OPOS_EADHME_NOMOREILEGALRECEIP = ADHME_ERRORS + 0x34;
const LONG OPOS_EADHME_NOTSTARTREAD = ADHME_ERRORS + 0x35;
const LONG OPOS_EADHME_NOTFINISHREADRECEIPTDATA = ADHME_ERRORS + 0x36;
const LONG OPOS_EADHME_NOTREADFORFOUNDRECEIPT = ADHME_ERRORS + 0x37;
const LONG OPOS_EADHME_ENDREADFLAS = ADHME_ERRORS + 0x38;
const LONG OPOS_EADHME_HWTRAYAGAN = ADHME_ERRORS + 0x39;
const LONG OPOS_EADHME_NOTSTARTREADFLASH = ADHME_ERRORS + 0x3A;
const LONG OPOS_EADHME_NOTFOUNDOPENDAY = ADHME_ERRORS + 0x3B;
const LONG OPOS_EADHME_NOMOREINRECEIPTLINES = ADHME_ERRORS + 0x3C;
const LONG OPOS_EADHME_NOTTRANSFERFLASH = ADHME_ERRORS + 0x3D;
const LONG OPOS_EADHME_PRINTERDISCONECT = ADHME_ERRORS + 0x3E;
const LONG OPOS_EADHME_TRANSACTIONINPROGRES = ADHME_ERRORS + 0x3F;
const LONG OPOS_EADHME_TRANSACTIONNOTOPEN = ADHME_ERRORS + 0x40;
const LONG OPOS_EADHME_TRANSACTIONISOPEN = ADHME_ERRORS + 0x41;
const LONG OPOS_EADHME_NOMOREVAT = ADHME_ERRORS + 0x42;
const LONG OPOS_EADHME_CASHINOPEN = ADHME_ERRORS + 0x43;
const LONG OPOS_EADHME_CASHOUTOPEN = ADHME_ERRORS + 0x44;
const LONG OPOS_EADHME_INPAYMENT = ADHME_ERRORS + 0x45;
const LONG OPOS_EADHME_NOZERODM = ADHME_ERRORS + 0x46;
const LONG OPOS_EADHME_MAXDISCOUNTINVAT = ADHME_ERRORS + 0x47;
const LONG OPOS_EADHME_MAXDMINTRANSTOTAL = ADHME_ERRORS + 0x48;
const LONG OPOS_EADHME_NOTEQUALDMGETSUM = ADHME_ERRORS + 0x49;
const LONG OPOS_EADHME_NEGATIVEVATSALES = ADHME_ERRORS + 0x4A;
const LONG OPOS_EADHME_MUSTCLOSETRANSACTION = ADHME_ERRORS + 0x4B;
const LONG OPOS_EADHME_NOZEROVAT = ADHME_ERRORS + 0x4D;
const LONG OPOS_EADHME_NOSANEVATRATE = ADHME_ERRORS + 0x4E;
const LONG OPOS_EADHME_NOSALESZEROPRICE = ADHME_ERRORS + 0x4F;
const LONG OPOS_EADHME_NODATAFORPRNX = ADHME_ERRORS + 0x50;
const LONG OPOS_EADHME_WORNIGDATE = ADHME_ERRORS + 0x51;
const LONG OPOS_EADHME_FLASSTOPWORK = ADHME_ERRORS + 0x52;
const LONG OPOS_EADHME_NOTVALIDPLU = ADHME_ERRORS + 0x53;
const LONG OPOS_EADHME_INVALIDCATEGORI = ADHME_ERRORS + 0x54;
const LONG OPOS_EADHME_INVALID_DPT = ADHME_ERRORS + 0x55;
const LONG OPOS_EADHME_CUTTER_ERROR = ADHME_ERRORS + 0x57;
const LONG OPOS_EADHME_RECOVER_DATA_FROM_FLASH = ADHME_ERRORS + 0x58;
const LONG OPOS_EADHME_ADYNATH_AKYRWSH_PLHRWMHS = ADHME_ERRORS + 0x59;
const LONG OPOS_EADHME_ADYNATH_AKYRWSH_MHDENIKHS_PLHRWMHS = ADHME_ERRORS + 0x5A;
const LONG OPOS_EADHME_NOT_IN_PAYMENT_STATE = ADHME_ERRORS + 0x5B;

char* g_ErrorMessagesADHME[] =
{
"No errors - success",
"Wrong number of fields\r\n(Check the command's field count)",
"Field too long\r\n(A field is long: check it & retry)",
"Field too small\r\n(A field is small: check it & retry)",
"Field fixed size mismatch\r\n(A field size is wrong: check it & retry)",
"Field range or type check failed\r\n(Check ranges or types in command)",
"Bad request code\r\n(Correct the request code (unknown)",
"Not use in this version",
"Not use in this version",
"Printing type bad\r\n(Correct the specified printing style)",
"cannot execute with day open\r\n(Issue a Z report to close the day)",
"RTC programming requires jumper\r\n(Short the 'clock' jumper and retry)",
"RTC date or time invalid\r\n(Check the date/time range. Also check if date is prior to a date of a fiscal record)",
"No records in fiscal period\r\n(No suggested action; the operation can not be executed in the specified period)",
"Device is busy in another task\r\n(Wait for the device to get ready)",
"No more header records allowed\r\n(No suggested action; the header programming cannot be executed because the Fiscal memory cannot hold more records)",
"cannot execute with block open\r\n(the specified command requires no open signature block for proceeding. Close the block and retry)",
"Not use in this version",
"Not use in this version",
"Not use in this version",
"Z closure time limit\r\n(Means that 24 hours passed from the last Z closure. Issue a Z and retry)",
"Z closure not found\r\n(the specified Z closure number does not exist. Pass an existing Z number)",
"Z closure record bad\r\n(The requested Z record is unreadable  (damaged). Device requires service)",
"User browsing in progress\r\n(The user is accessing the device by manual operation. The protocol usage is suspended until the user terminates the keyboard browsing. Just wait or inform application user)",
"Not use in this version",
"Printer paper end detected\r\n(Replace the paper roll and retry)",
"Printer is offline\r\n(Printer disconnection. Service required)",
"Fiscal unit is offline\r\n(Fiscal disconnection. Service required)",
"Fatal hardware error\r\n(Mostly fiscal errors. Service required)",
"Fiscal unit is full\r\n(Need fiscal replacement. Service)",
"Not use in this version",
"Not use in this version",
"Battery fault detected\r\n(If problem persists, service required)",
"Not use in this version",
"Not use in this version",
"Real-Time Clock needs programming\r\n(This means that the RTC has invalid Data and needs to be reprogrammed. As a consequence, service is needed)",
"JUPERON\r\n(Jumpers are on, remove them for the operation to continue",
"INVSALEOP\r\n(Wrong sales type It must be S/V/R)",
"DPTINDEXERR\r\n(Department's code out of range (1-5)",
"VATRATE\r\n(The VAT rate received from PC doesn't agree to the Fiscal Printer with  Electronic Journal's one)",
"PAYMENTINDEXERR\r\n(Payment Code out of range  (1-3) 1=CASH, 2=CARD, 3=CREDIT)",
"NOT USE IN THIS VERSION",
"COVEROPEN\r\n(Printer's tray is open)",
"NOT USE IN THIS VERSION",
"NOT USE IN THIS VERSION",
"NOT USE IN THIS VERSION",
"NOT USE IN THIS VERSION",
"NOTENDREADLEGAL\r\n(There are illegal records in the Journal that must be read)",
"NOTENDREADILEGAL\r\n(There are illegal records in the Journal that must be read)",
"WRONGILEGALNUMBER\r\n(The requested illegal receipt doesn’t exist in the electronic journal)",
"FLASHERROR\r\n(CARD reading problem)",
"NOTFOUNDRECEIPT\r\n(The requested legal receipt doesn’t exist in the electronic journal)",
"NOMOREILEGALRECEIP\r\n(There are no more receipts to be read in the CARD)",
"NOTSTARTREAD\r\n(The Fiscal Printer with electronic Journal must first be told about the reading of the CARD before the CARD’s reading begins)",
"NOTFINISHREADRECEIPTDATA\r\n(The CARD’s reading isn’t finished)",
"NOTREADFORFOUNDRECEIPT\r\n(A record hasn’t been read)",
"ENDREADFLAS\r\n(The CARD’s reading was successful)",
"HWTRAYAGAN\r\n(Error reading the CARD, please try again)",
"NOTSTARTREADFLASH\r\n(The Fiscal Printer with electronic Journal must first be told about the reading of the CARD before the CARD’s reading begins)",
"NOTFOUNDOPENDAY\r\n(DAY isn’t opened and no transactions are present)",
"NOMOREINRECEIPTLINES\r\n(No more than 6 comment lines can be printed on the receipt)",
"NOTTRANSFERFLASH\r\n(The CARD’s data transfer to the PC isn’t over yet)",
"PRINTERDISCONECT\r\n(Printer is disconnected)",
"TRANSACTIONINPROGRES\r\n(Another Fiscal Printer with electronic Journal’s function is in progress)",
"TRANSACTIONNOTOPEN\r\n(There is no opened receipt)",
"TRANSACTIONISOPEN\r\n(There is an opened receipt)",
"NOMOREVAT\r\n(No more VAT codes can be programmed in the fiscal memory)",
"CASHINOPEN\r\n(Cash in is in progress)",
"CASHOUTOPEN\r\n(Cash out is in progress)",
"INPAYMENT\r\n(Payment is in progress)",
"NOZERODM\r\n(No zero Discount/Markup is allowed)",
"MAXDISCOUNTINVAT\r\n(Greater Discount than the Fiscal Printer with electronic Journal’s VAT amount)",
"MAXDMINTRANSTOTAL",
"NOTEQUALDMGETSUM\r\n(VAT’s allocation’s totals do not match)",
"NEGATIVEVATSALES\r\n(No negative sales-transactions are allowed)",
"MUSTCLOSETRANSACTION\r\n(The receipt must be closed in order for the function to continue)",
"FLASHFULL\r\n(CARD is full, it must be read)",
"NOZEROVAT\r\n(The VAT rate can not be 0)",
"NOSANEVATRATE\r\n( No equal VAT rates in different categories)",
"NOSALESZEROPRICE\r\n(Zero sale’s price can not occur)",
"NODATAFORPRNX\r\n(There are no transactions-A X Report can not be issued)",
"WORNIGDATE\r\n(DATE/TIME Error. Call service)",
"FLASSTOPWORK\r\n(CARD error. The Fiscal Printer with electronic Journal can not perform sales)",
"NOTVALIDPLU\r\n(PLU Internal Code Error  (1-200))",
"INVALIDCATEGORI\r\n(Category Code Error (1-20)",
"INVALID DPT\r\n(Department Code Error    (1-5)",
"Cutter Error\r\n(Turn off The Fiscal Printer with electronic Journal and try again)",
"Recover data from FLASH\r\n(The Flash CARD must be read. The machine is in an after-CMOS status",
"PAYMENT can not be cancelled\r\n(There is no payment amount to be cancelled",
"ZERO PAYMENT can not be cancelled\r\n(A zero payment can not be cancelled",
"NOT in Payment Mode\r\n(The Fiscal Printer with electronic Journal is not in payment mode)",
"TimeOut",
};

short g_ErrorCodesADHME[] =
{
0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,
0x08,0x09,0x0A,0x0B,0x0C,0x0D,0x0E,0x0F,
0x10,0x11,0x12,0x13,0x14,0x15,0x16,0x17,
0x18,0x19,0x1A,0x1B,0x1C,0x1D,0x1E,0x1F,
0x20,0x21,0x22,0x23,0x24,0x25,0x26,0x27,
0x28,0x29,0x2A,0x2B,0x2C,0x2D,0x2E,0x2F,
0x30,0x31,0x32,0x33,0x34,0x35,0x36,0x37,
0x38,0x39,0x3A,0x3B,0x3C,0x3D,0x3E,0x3F,
0x40,0x41,0x42,0x43,0x44,0x45,0x46,0x47,
0x48,0x49,0x4A,0x4B,0x4C,0x4D,0x4E,0x4F,
0x50,0x51,0x52,0x53,0x54,0x55,0x57,0x58,
0x59,0x5A,0x5B,0x100
};

*/
#endregion