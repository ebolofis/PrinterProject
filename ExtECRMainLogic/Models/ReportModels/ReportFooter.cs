namespace ExtECRMainLogic.Models.ReportModels
{
    /// <summary>
    /// The fields used within the footer of the report.
    /// </summary>
    public class ReportFooter
    {
        /// <summary>
        /// The total amount of the report.
        /// </summary>
        public decimal? ReportTotalAmount { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ReportFooter()
        {

        }
    }
}