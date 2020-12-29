using System;

namespace ExtECRMainLogic.Models.ReceiptModels
{
    /// <summary>
    /// 
    /// </summary>
    public class CreditTransaction
    {
        /// <summary>
        /// 
        /// </summary>
        public decimal? Amount { get; set; }
        /// <summary>
        /// Timestamp of creation
        /// </summary>
        public DateTime? CreationTS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int32? Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int64? StaffId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int64? PosInfoId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int64? CreditAccountId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int64? CreditCodeId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? Balance { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CreditTransaction()
        {

        }
    }
}