namespace ExtECRMainLogic.Models.DriverModels
{
    public class EFTPOSSettingsModel
    {
        /// <summary>
        /// Enables EFTPOS
        /// </summary>
        public bool enableEFTPOS { get; set; }
        /// <summary>
        /// EFTPOS instance name
        /// </summary>
        public string EFTPOSInstance { get; set; }
        /// <summary>
        /// EFTPOS IP address
        /// </summary>
        public string EFTPOSIPAddress { get; set; }
        /// <summary>
        /// EFTPOS TCP/IP port
        /// </summary>
        public string EFTPOSTCPIPPort { get; set; }
    }
}