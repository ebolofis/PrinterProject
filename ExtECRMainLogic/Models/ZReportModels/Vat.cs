namespace ExtECRMainLogic.Models.ZReportModels
{
    /// <summary>
    /// 
    /// </summary>
    public class Vat
    {
        /// <summary>
        /// The VAT category (rate).
        /// </summary>
        public decimal? VatRate { get; set; }
        /// <summary>
        /// The VAT total gross value.
        /// </summary>
        public decimal? Gross { get; set; }
        /// <summary>
        /// The VAT total net value.
        /// </summary>
        public decimal? Net { get; set; }
        /// <summary>
        /// The total VAT value (amount).
        /// </summary>
        public decimal? VatAmount { get; set; }
        /// <summary>
        /// The description of VAT per category.
        /// </summary>
        public string VatDesc { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Vat()
        {

        }
    }
}