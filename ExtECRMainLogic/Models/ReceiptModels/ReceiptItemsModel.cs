using System;
using System.Collections.Generic;

namespace ExtECRMainLogic.Models.ReceiptModels
{
    /// <summary>
    /// Contains the info about an item into a Receipt (see class ReceiptModel)
    /// </summary>
    public class ReceiptItemsModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string InvoiceNo { get; set; }
        /// <summary>
        /// GIA EXTRA DESCRIPTION STO ITEM
        /// </summary>
        public string ItemCustomRemark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? KitchenId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ItemCode { get; set; }
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
        public decimal ItemPrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ItemVatRate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal ItemVatValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ItemVatDesc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? ItemDiscount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal ItemNet { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal ItemGross { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal ItemTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int32 ItemPosition { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int32 ItemSort { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ItemRegion { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? RegionPosition { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int32 ItemBarcode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SalesTypeExtDesc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ReceiptExtrasModel> Extras { get; set; }
        /// <summary>
        /// for OPOS void
        /// </summary>
        public bool IsVoid { get; set; }
        /// <summary>
        /// for OPOS
        /// </summary>
        public bool IsChangeItem { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String ExtraDescription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String SalesDescription { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ReceiptItemsModel()
        {
            Extras = new List<ReceiptExtrasModel>();
        }
    }
}