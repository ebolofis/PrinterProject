namespace ExtECRMainLogic.Models.ReportModels
{
    /// <summary>
    /// The fields used within the header of the report.
    /// </summary>
    public class ReportHeader
    {
        /// <summary>
        /// The text to be used as the report title.
        /// </summary>
        public string ReportTitle { get; set; }
        /// <summary>
        /// The waiter name.
        /// </summary>
        public string WaiterName { get; set; }
        /// <summary>
        /// total of receipts
        /// </summary>
        public long ReceiptsTotal { get; set; }
        /// <summary>
        /// The description of the department.
        /// </summary>
        public string DepartmentDesc { get; set; }
        /// <summary>
        /// The total amount of the report.
        /// </summary>
        public decimal? ReportTotalAmount { get; set; }
        /// <summary>
        /// The total number of invoices in the report.
        /// </summary>
        public int? ReportTotalInvoices { get; set; }
        /// <summary>
        /// The description of the table used.
        /// </summary>
        public string Table { get; set; }
        /// <summary>
        /// The room number related to the report (pre-bill, ...).
        /// </summary>
        public string RoomNo { get; set; }
        /// <summary>
        /// The number of couvers used in the table order.
        /// </summary>
        public int Cover { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ReportHeader()
        {

        }
    }
}