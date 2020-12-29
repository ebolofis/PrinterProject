namespace ExtECRMainLogic.Enumerators.ExtECR
{
    /// <summary>
    /// Template sections.
    /// </summary>
    public enum SectionTypeEnums
    {
        /// <summary>
        /// 
        /// </summary>
        Header = 0,
        /// <summary>
        /// 
        /// </summary>
        Extras = 1,
        /// <summary>
        /// 
        /// </summary>
        Footer = 2,
        /// <summary>
        /// 
        /// </summary>
        ReportHeader = 4,
        /// <summary>
        /// 
        /// </summary>
        Details = 5,
        /// <summary>
        /// 
        /// </summary>
        PaymentAnalysis = 6,
        /// <summary>
        /// 
        /// </summary>
        VatAnalysis = 7,
        /// <summary>
        /// 
        /// </summary>
        VoidAnalysis = 8,
        /// <summary>
        /// 
        /// </summary>
        SectionHeader = 9,
        /// <summary>
        /// 
        /// </summary>
        SectionFooter = 10,
        /// <summary>
        /// 
        /// </summary>
        OposComments = 11,
        /// <summary>
        /// 
        /// </summary>
        Discount = 12,
        /// <summary>
        /// 
        /// </summary>
        DiscountDetails = 13,
        /// <summary>
        /// 
        /// </summary>
        Customer = 14,
        /// <summary>
        /// 
        /// </summary>
        FiscalSign = 15,
        /// <summary>
        /// 
        /// </summary>
        CardAnalysis = 16,
        /// <summary>
        /// 
        /// </summary>
        PaymentMethods = 17,
        /// <summary>
        /// 
        /// </summary>
        ReceiptSumHeader = 18,
        /// <summary>
        /// 
        /// </summary>
        ReceiptSumFooter = 19,
        /// <summary>
        /// 
        /// </summary>
        Lockers = 20,
        /// <summary>
        /// 
        /// </summary>
        ProductsForEODStats = 21,
        /// <summary>
        /// 
        /// </summary>
        CreditTransactions = 22,
        /// <summary>
        /// Used with receipt and kitchen printouts, related to the template section 'SalesType'.
        /// Associates the template section with the JSON part 'Details/SalesTypeExtDesc'.
        /// </summary>
        SalesTypeSection = 23,
        /// <summary>
        /// Used with reports, related to the template section 'Reports Page Header'.
        /// Associates the template section with the JSON part 'PageHeader'.
        /// </summary>
        ReportsPageHeader = 24,
        /// <summary>
        /// Used with reports, related to the template section 'Reports Page Footer'.
        /// Associates the template section with the JSON part 'PageFooter'.
        /// </summary>
        ReportsPageFooter = 25,
        /// <summary>
        /// Used with reports, related to the template section 'Reports Group Header'.
        /// Associates the template section with the JSON part 'GroupHeader'.
        /// </summary>
        ReportsGroupHeader = 26,
        /// <summary>
        /// Used with reports, related to the template section 'Reports Group Footer'.
        /// Associates the template section with the JSON part 'GroupFooter'.
        /// </summary>
        ReportsGroupFooter = 27,
        /// <summary>
        /// Used with receipt templates to support the printout with the related receipts/invoices.
        /// Associates the template section with the JSON part 'RelatedReceipts'.
        /// </summary>
        RelatedReceipts = 28
    }
}