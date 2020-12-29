using System;

namespace ExtECRMainLogic.Models.ZReportModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductsForEODStats
    {
        /// <summary>
        /// 
        /// </summary>
        public Int64? ProductId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? Qty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? Total { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProductsForEODStats()
        {

        }
    }
}