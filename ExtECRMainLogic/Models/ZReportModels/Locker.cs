namespace ExtECRMainLogic.Models.ZReportModels
{
    /// <summary>
    /// 
    /// </summary>
    public class Locker
    {
        /// <summary>
        /// 
        /// </summary>
        public bool HasLockers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalLockers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalLockersAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? Paidlockers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? PaidlockersAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? OccLockers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? OccLockersAmount { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Locker()
        {

        }
    }
}