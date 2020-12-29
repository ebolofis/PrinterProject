namespace ExtECRMainLogic.Models.ReceiptModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ReceiptExtrasModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? ItemDiscount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ItemDescr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal ItemQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? ItemPrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? ItemGross { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ItemVatRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ItemVatDesc { get; set; }
        /// <summary>
        /// for OPOS
        /// </summary>
        public bool IsChangeItem { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal ItemVatValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal ItemNet { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ReceiptExtrasModel()
        {

        }
    }
}