namespace ExtECRMainLogic.Models.DriverModels
{
    public class CommunicationSettingsModel
    {
        /// <summary>
        /// Server url
        /// </summary>
        public string connectionUrl { get; set; }
        /// <summary>
        /// Store id used as part of connection name
        /// </summary>
        public string connectionStoreId { get; set; }
        /// <summary>
        /// Username for authorization header
        /// </summary>
        public string authorizationUsername { get; set; }
        /// <summary>
        /// Password for authorization header
        /// </summary>
        public string authorizationPassword { get; set; }
    }
}