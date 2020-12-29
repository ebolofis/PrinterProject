namespace ExtECRMainLogic.Enumerators.ExtECR
{
    public enum PrintType
    {
        /// <summary>
        /// When a receipt signaled from webapi, Print the whole receipt at once.
        /// </summary>
        PrintWhole = 0,
        /// <summary>
        /// When a receipt signaled from webapi, Print only the last item.
        /// </summary>
        PrintItem = 1,
        /// <summary>
        /// Print the receipt's footer only.
        /// </summary>
        PrintEnd = 2,
        /// <summary>
        /// Cancel the current receipt.
        /// </summary>
        CancelCurrentReceipt = 3,
        /// <summary>
        /// Print only the last extra of the last item.
        /// </summary>
        PrintExtra = 4,
        /// <summary>
        /// Print the discount of the last item.
        /// </summary>
        PrintItemDiscount = 5
    }
}