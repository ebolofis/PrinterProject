namespace ExtECRMainLogic.Models.DriverModels
{
    public class HTMLReceiptSettingsModel
    {
        /// <summary>
        /// Enable transformation of receipt to html
        /// </summary>
        public bool enableHTMLReceipt { get; set; }
        /// <summary>
        /// Template for receipt transformation to html
        /// </summary>
        public string htmlReceiptTemplate { get; set; }
    }
}