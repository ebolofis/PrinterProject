namespace ExtECRMainLogic.Models.ZReportModels
{
    /// <summary>
    /// 
    /// </summary>
    public class Void
    {
        /// <summary>
        /// The description of the void action.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The amount of the void action.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Void()
        {

        }
    }
}