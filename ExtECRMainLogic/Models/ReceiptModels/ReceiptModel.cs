using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Enumerators.Receipt;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtECRMainLogic.Models.ReceiptModels
{
    /// <summary>
    /// Class representing a Receipt.
    /// Items are stored into a list of ReceiptItemsModel
    /// </summary>
    public class ReceiptModel
    {
        /// <summary>
        /// 
        /// </summary>
        public long? RegionId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RegionDescription { get; set; }
        /// <summary>
        /// what part of receipt is goint to print (print the whole receipt or a part of it)
        /// </summary>
        public PrintType PrintType { get; set; }
        /// <summary>
        /// true if print without ADHME 
        /// </summary>
        public bool TempPrint { get; set; }
        /// <summary>
        /// contains the method which is going to be print as item's second line (for OPOS/OPOS3)
        /// </summary>
        public string ItemAdditionalInfo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TableTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PaymentTypeId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<PaymentTypeModel> PaymentsList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool PrintKitchen { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float Longtitude { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float Latitude { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Nullable<DateTime> EstTakeoutDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DA_IsPaid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IsDelay { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ItemsChanged { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OrderNotes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string StoreNotes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerNotes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerSecretNotes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustomerLastOrderNotes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LogicErrors { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LoyaltyCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TelephoneNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Nullable<OrderOriginEnum> DA_Origin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string daorigin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Nullable<int> PointsGain { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Nullable<int> PointsRedeem { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CouponCodes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ReceiptTypeDescription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DepartmentTypeDescription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? PaidAmount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SalesTypeDescription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? ItemsCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? Couver { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ExtECRCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? PrintFiscalSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public FiscalTypeEnum FiscalType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DetailsId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string InvoiceIndex { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TableNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RoomNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Waiter { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string WaiterNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Pos { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PosDescr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DepartmentDesc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Department { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PaymentType { get; set; }
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
        /// 
        /// </summary>
        public string GuestTerm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long? PdaId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PdaDescription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] DigitalSignature { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BillingName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BillingJob { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BillingVatNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BillingDOY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BillingAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BillingCity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BillingZipCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private string _Adults;
        public string Adults
        {
            get { return _Adults; }
            set { _Adults = value ?? string.Empty; }
        }
        /// <summary>
        /// 
        /// </summary>
        private string _Kids;
        public string Kids
        {
            get { return _Kids; }
            set { _Kids = value ?? string.Empty; }
        }
        /// <summary>
        /// 
        /// </summary>
        public Int16 InvoiceType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalVat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalVat1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalVat2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalVat3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalVat4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalVat5 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalNetVat1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalNetVat2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalNetVat3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalNetVat4 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalNetVat5 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? TotalDiscount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int16 Bonus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int16 PriceList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ReceiptNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OrderComments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal TotalNet { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Total { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Change { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<CalculateVatPrice> CalculateVatPrice { get; set; }
        /// <summary>
        /// The cash amount the customer gave us, so we can calculate the change.
        /// </summary>
        public string CashAmount { get; set; }
        /// <summary>
        /// The number of the buzzer we gave the customer. Used for Goody's, to be able to print it in comments, and follow the customers order progress.
        /// </summary>
        public string BuzzerNumber { get; set; }
        /// <summary>
        /// list of Items
        /// </summary>
        public List<ReceiptItemsModel> Details { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<CreditTransaction> CreditTransactions { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Nullable<int> CouverAdults { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Nullable<int> CouverChildren { get; set; }
        /// <summary>
        /// In use with section type 23, we get the SalesTypeDescription within receipt printout
        /// </summary>
        public List<SalesTypeDescriptionsList> SalesTypeDescriptions
        {
            get { return this.Details.Select(s => s.SalesTypeExtDesc).Distinct().Select(f => new SalesTypeDescriptionsList { SalesTypeDescriptions = f }).Distinct().ToList(); }
        }
        /// <summary>
        /// for OPOS void
        /// </summary>
        public bool IsVoid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsForKitchen { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string InvoiceIdStr { get; set; }
        /// <summary>
        /// Related receipts/invoices that are associated to our current invoice issue.
        /// </summary>
        public List<string> RelatedReceipts { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ReceiptModel()
        {
            Details = new List<ReceiptItemsModel>();
            PaymentsList = new List<PaymentTypeModel>();
            CreditTransactions = new List<CreditTransaction>();
        }

        /// <summary>
        /// Convert some properties from null to 0 or ""
        /// </summary>
        public void Sanitize()
        {
            if (Phone == null)
                Phone = "";
            if (TableTotal == null)
                TableTotal = 0;
            if (PaidAmount == null)
                PaidAmount = 0;
            if (ItemsCount == null)
                ItemsCount = 0;
            if (PosDescr == null)
                PosDescr = "";
            if (DepartmentDesc == null)
                DepartmentDesc = "";
            if (OrderComments == null)
                OrderComments = "";
            if (CustomerComments == null)
                CustomerComments = "";
            if (CashAmount == null)
                CashAmount = "0";
            if (RegNo == null)
                RegNo = "";
            if (Change == null)
                Change = "";
            if (PaidAmount == null)
                PaidAmount = 0;
            if (CustomerName == null)
                CustomerName = "";
            if (DA_IsPaid == "true")
                DA_IsPaid = "Yes";
            if (DA_IsPaid == "false" || DA_IsPaid == "False")
                DA_IsPaid = "No";
            if (DA_IsPaid == null)
                DA_IsPaid = "";
            if (IsDelay == "true")
                IsDelay = "Yes";
            if (IsDelay == "false" || IsDelay == "False")
                IsDelay = "No";
            if (IsDelay == null)
                IsDelay = "";
            if (ItemsChanged == "true")
                ItemsChanged = "Yes";
            if (ItemsChanged == "false" || IsDelay == "False")
                ItemsChanged = "No";
            if (ItemsChanged == null)
                ItemsChanged = "";
        }
    }
}