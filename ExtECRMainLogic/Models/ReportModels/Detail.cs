namespace ExtECRMainLogic.Models.ReportModels
{
    /// <summary>
    /// The fields used within each repeated line of the report.
    /// </summary>
    public class Detail
    {
        #region Waiter Data
        /// <summary>
        /// The number of couvers used in the table order.
        /// </summary>
        public int Cover { get; set; }
        /// <summary>
        /// The waiter name.
        /// </summary>
        public string WaiterName { get; set; }
        /// <summary>
        /// The waiter Id or number.
        /// </summary>
        public string WaiterNo { get; set; }
        #endregion
        #region Invoice Data
        /// <summary>
        /// The sales type (cash, not paid, etc).
        /// </summary>
        public string SalesType { get; set; }
        /// <summary>
        /// The description of the invoice type (receipt, order, invoice, etc).
        /// </summary>
        public string InvoiceDesc { get; set; }
        /// <summary>
        /// The invoice number.
        /// </summary>
        public string InvoiceNo { get; set; }
        /// <summary>
        /// The order ID related to the invoice.
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// The order number related to the invoice.
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OrderComments { get; set; }
        /// <summary>
        /// The room number related to the invoice.
        /// </summary>
        public string RoomNo { get; set; }
        /// <summary>
        /// The amount of the invoice.
        /// </summary>
        public decimal? InvoiceAmount { get; set; }
        /// <summary>
        /// The paid status of the invoice
        /// </summary>
        public bool IsPaid { get; set; }
        /// <summary>
        /// The description of the table used.
        /// </summary>
        public string TableNo { get; set; }
        #endregion
        #region Customer Data
        /// <summary>
        /// 
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerDeliveryAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerPhone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Floor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerComments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerAfm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerDoy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerJob { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RegNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SumOfLunches { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SumofConsumedLunches { get; set; }
        /// <summary>
        /// Board Id
        /// </summary>
        public string GuestTerm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Adults { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Kids { get; set; }
        #endregion
        #region Item Data
        /// <summary>
        /// Flag to identify if the item is an extra.
        /// </summary>
        public bool? ItemIsExtra { get; set; }
        /// <summary>
        /// The quantity of the item.
        /// </summary>
        public decimal? ItemQty { get; set; }
        /// <summary>
        /// The description of the item.
        /// </summary>
        public string ItemDescription { get; set; }
        /// <summary>
        /// The total value of the item line ( = qty * price - discount ).
        /// </summary>
        public decimal? ItemTotal { get; set; }
        /// <summary>
        /// The VAT% in string form.
        /// </summary>
        public string ItemVatDesc { get; set; }
        /// <summary>
        /// The sale discount of the item.
        /// </summary>
        public decimal? ItemDiscount { get; set; }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Detail()
        {

        }
    }
}