namespace ExtECRMainLogic.Enumerators.Epson
{
    public enum PaymentType
    {
        /// <summary>
        /// code '31' - '32' for EPSON Fiscal
        /// </summary>
        Cash = 1,
        /// <summary>
        /// code '11' - '20' for EPSON Fiscal
        /// </summary>
        CreditCard = 2,
        /// <summary>
        /// code '21' - '30' for EPSON Fiscal
        /// </summary>
        Check = 3
    }
}