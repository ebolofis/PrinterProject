using ExtECRMainLogic.Enumerators.ExtECR;
using System.Collections.Generic;

namespace ExtECRMainLogic.Models.PrinterModels
{
    public class PrinterResultModel
    {
        /// <summary>
        /// The printer name
        /// </summary>
        public string PrinterName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PrinterTypeEnum PrinterType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PrintStatusEnum PrintStatus { get; set; }
        /// <summary>
        /// The receipt in string format (list of strings)
        /// </summary>
        public List<string> ReceiptData { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<int> KitchenIdList { get; set; }

        public PrinterResultModel()
        {
            ReceiptData = new List<string>();
            KitchenIdList = new List<int>();
        }
    }
}