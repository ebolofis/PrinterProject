using System;
using System.Collections.Generic;

namespace ExtECRMainLogic.Models.ZReportModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ZReportModel
    {
        /// <summary>
        /// 
        /// </summary>
        private DateTime? _Day;
        public DateTime Day
        {
            get { if (_Day == null) { _Day = DateTime.Now; } return (DateTime)_Day; }
            set { if (value == null || value == DateTime.MinValue) { _Day = DateTime.Now; } else { _Day = value; } }
        }
        /// <summary>
        /// 
        /// </summary>
        public string PosCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PosDescription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? ReportNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Gross { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime dtDateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? VatAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? Net { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? Discount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? TicketCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? ItemsCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<Payment> PaymentAnalysis { get; set; }
        /// <summary>
        /// VAT analysis per Z report.
        /// </summary>
        public List<Vat> VatAnalysis { get; set; }
        /// <summary>
        /// Void analysis per Z report.
        /// </summary>
        public List<Void> VoidAnalysis { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<Payment> CardAnalysis { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Locker Lockers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<ProductsForEODStats> ProductsForEODStats { get; set; }
        /// <summary>
        /// The end_of_day_ID to get the right Z or X report when printing using Crystal Reports
        /// </summary>
        public Int64 EndOfDayId { get; set; }
        /// <summary>
        /// The POS_Info_ID to get the Z or X report of the right POS when printing using Crystal Reports
        /// </summary>
        public Int64 PosInfoId { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ZReportModel()
        {

        }
    }
}