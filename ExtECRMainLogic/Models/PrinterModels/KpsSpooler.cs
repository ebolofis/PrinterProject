using ExtECRMainLogic.Enumerators.ExtECR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;

namespace ExtECRMainLogic.Models.PrinterModels
{
    public class KpsSpooler
    {
        [DisplayName("Printer Name")]
        public string PrinterName { get; set; }
        [DisplayName("Code")]
        public string ItemCode { get; set; }
        [DisplayName("Description")]
        public String ItemDescription { get; set; }
        [DisplayName("PrinterType")]
        public PrinterTypeEnum PrinterType { get; set; }
        [DisplayName("KitchenId")]
        public int? KitchenId { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public KpsSpooler()
        {

        }

        /// <summary>
        /// Return a list of printers as json
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ListToString(List<KpsSpooler> list)
        {
            if (list == null)
                return "<null>";
            return JsonSerializer.Serialize(list);
        }
    }
}