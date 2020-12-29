namespace ExtECRMainLogic.Models.ReportModels
{
    /// <summary>
    /// The fields used within the group footer.
    /// </summary>
    public class GroupFooter
    {
        /// <summary>
        /// The total amount to be used with the current group.
        /// </summary>
        public decimal? GroupTotalAmount { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GroupFooter()
        {

        }
    }
}