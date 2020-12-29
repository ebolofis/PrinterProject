namespace ExtECRMainLogic.Models.ReceiptModels
{
    /// <summary>
    /// 
    /// </summary>
    public class CalculateVatPrice
    {
        /// <summary>
        /// 
        /// </summary>
        public decimal? Total { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string VatRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? VatPrice { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CalculateVatPrice()
        {

        }
    }
}