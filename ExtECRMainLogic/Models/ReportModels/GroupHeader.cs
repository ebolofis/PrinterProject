namespace ExtECRMainLogic.Models.ReportModels
{
    /// <summary>
    /// The fields used within the group header.
    /// </summary>
    public class GroupHeader
    {
        /// <summary>
        /// The counter value of the invoices included within the current group.
        /// </summary>
        public int? InvoicesCount { get; set; }
        /// <summary>
        /// The description of the payment type used within the current group.
        /// </summary>
        public string PaymentDesc { get; set; }
        /// <summary>
        /// The counter value of the items included within the current group.
        /// </summary>
        public int? ItemsCount { get; set; }
        /// <summary>
        /// The total amount to be used with the current group.
        /// </summary>
        public decimal? GroupTotalAmount { get; set; }
        /// <summary>
        /// The total Cash amount to be used with the current group.
        /// </summary>
        public decimal? GroupTotalStaffCashAmount { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GroupHeader()
        {

        }
    }
}