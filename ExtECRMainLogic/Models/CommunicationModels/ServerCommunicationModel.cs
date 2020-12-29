namespace ExtECRMainLogic.Models.CommunicationModels
{
    public class ServerCommunicationModel
    {
        /// <summary>
        /// Server url
        /// </summary>
        public string connectionUrl { get; set; }
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