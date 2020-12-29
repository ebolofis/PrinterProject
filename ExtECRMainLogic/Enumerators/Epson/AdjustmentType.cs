namespace ExtECRMainLogic.Enumerators.Epson
{
    public enum AdjustmentType
    {
        /// <summary>
        /// Discount on previous item sale
        /// </summary>
        SalesDiscount = 1,
        /// <summary>
        /// Discount on transaction subtotal
        /// </summary>
        SubtotalDiscount = 2,
        /// <summary>
        /// Markup on previous item sale
        /// </summary>
        SalesMarkup = 3,
        /// <summary>
        /// Markup on transaction subtotal
        /// </summary>
        SubtotalMarkup = 4
    }
}