using ExtECRMainLogic.Enumerators.ExtECR;
using System;
using System.Collections.Generic;

namespace ExtECRMainLogic.Models.PrinterModels
{
    public class PrintResultModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SenderName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ReceiptReceiveTypeEnum ReceiptReceiveType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime ProcessDateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String FileName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String ReceiptNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<String> ReceiptData { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PrintStatusEnum Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PrintModeEnum ReceiptType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ExtcerTypesEnum ExtcerType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String FiscalName { get; set; }
        /// <summary>
        /// The InvoiceIndex used for the last print.
        /// </summary>
        public string InvoiceIndex { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsInvoiceIndexError { get; set; }
        /// <summary>
        /// The ReportIndex used for the last print.
        /// </summary>
        public string ReportIndex { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string crn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long rseq { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsReportIndexError { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ResponseValue { get; set; }
        /// <summary>
        /// For OPOS error handling
        /// </summary>
        public string ErrorDescription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<PrinterResultModel> SecondaryPrintersList { get; set; }
        /// <summary>
        /// The culture info (name) used for the receipt/invoice printout. Will be used when we need a reprint.
        /// </summary>
        public String ReceiptCultureInfo { get; set; }
        /// <summary>
        /// What part of receipt is goint to print (print the whole receipt or a part of it)
        /// </summary>
        public PrintType PrintType { get; set; }

        public PrintResultModel()
        {
            SecondaryPrintersList = new List<PrinterResultModel>();
            this.Id = Guid.NewGuid();
            this.ProcessDateTime = DateTime.Now;
            this.IsInvoiceIndexError = false;
            this.PrintType = PrintType.PrintWhole;
        }

        public PrintResultModel(DateTime processDateTime)
        {
            SecondaryPrintersList = new List<PrinterResultModel>();
            this.ProcessDateTime = processDateTime;
            this.PrintType = PrintType.PrintWhole;
        }
    }
}