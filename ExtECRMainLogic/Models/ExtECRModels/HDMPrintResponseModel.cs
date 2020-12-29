namespace ExtECRMainLogic.Models.ExtECRModels
{
    public class HDMPrintResponseModel
    {
        /// <summary>
        /// Regular number of coupon
        /// </summary>
        public long rseq { get; set; }
        /// <summary>
        /// Registration for CCM
        /// </summary>
        public string crn { get; set; }
        /// <summary>
        /// CCM Factory Output
        /// </summary>
        public string sn { get; set; }
        /// <summary>
        /// Organization TIN
        /// </summary>
        public string tin { get; set; }
        /// <summary>
        /// Company Name
        /// </summary>
        public string taxpayer { get; set; }
        /// <summary>
        /// Company address
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// Date and time of registration / receipt of coupon
        /// </summary>
        public double time { get; set; }
        /// <summary>
        /// Fiscal number
        /// </summary>
        public string fiscal { get; set; }
        /// <summary>
        /// Draw number
        /// </summary>
        public string lottery { get; set; }
        /// <summary>
        /// 0-no profit
        /// 1-There is profit
        /// </summary>
        public int prize { get; set; }
        /// <summary>
        /// Total amount
        /// </summary>
        public double total { get; set; }
        /// <summary>
        /// Coin
        /// </summary>
        public double change { get; set; }
        /// <summary>
        /// Error code
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>
        /// Non-standard error message
        /// </summary>
        public string FatalError { get; set; }
    }
}