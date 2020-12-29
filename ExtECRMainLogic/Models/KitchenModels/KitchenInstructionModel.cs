using System;

namespace ExtECRMainLogic.Models.KitchenModels
{
    /// <summary>
    /// 
    /// </summary>
    public class KitchenInstructionModel
    {
        /// <summary>
        /// 
        /// </summary>
        public String ExtcerInstance { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int KitchenId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// The waiter name to use.
        /// </summary>
        public string Waiter { get; set; }
        /// <summary>
        /// The table number to use.
        /// </summary>
        public string Table { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ReceivedDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ReceivedTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime SendTS { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public KitchenInstructionModel()
        {

        }
    }
}