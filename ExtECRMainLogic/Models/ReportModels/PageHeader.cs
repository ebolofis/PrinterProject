namespace ExtECRMainLogic.Models.ReportModels
{
    /// <summary>
    /// The fields used within each page header.
    /// </summary>
    public class PageHeader
    {
        /// <summary>
        /// The waiter data to be used within the page header.
        /// </summary>
        public Waiter Waiter { get; set; }
        /// <summary>
        /// The data of the department.
        /// </summary>
        public Department Department { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PageHeader()
        {

        }
    }
}