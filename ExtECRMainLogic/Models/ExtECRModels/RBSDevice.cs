namespace ExtECRMainLogic.Models.ExtECRModels
{
    /// <summary>
    /// 
    /// </summary>
    public class RBSDevice
    {
        /// <summary>
        /// 
        /// </summary>
        public string SerialNO { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long TotalBytes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsEthernet { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsProxy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ActivationCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string GGPSKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string EthernetIP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ProxyIP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte ComNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LastConnectedClient { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool InProgress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BackupData { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TempSerno { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long TotalSigned { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool AfterSigndoc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool DebugWindow { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public RBSDevice()
        {

        }
    }
}