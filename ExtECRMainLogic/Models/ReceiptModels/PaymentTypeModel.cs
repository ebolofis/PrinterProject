namespace ExtECRMainLogic.Models.ReceiptModels
{
    /// <summary>
    /// 
    /// </summary>
    public class PaymentTypeModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? AccountType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? Amount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guest Guest { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PaymentTypeModel()
        {

        }
    }
}