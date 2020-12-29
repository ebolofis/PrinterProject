using System.Collections.Generic;
using System.Xml.Serialization;

namespace ExtECRMainLogic.Models.HelperModels
{
    [XmlRoot("ArtCasLogFile")]
    public class ArtCasXmlWraperModel
    {
        [XmlElement("ArtCasLog")]
        public List<ArtCasXmlModel> ArtCasList { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ArtCasXmlWraperModel()
        {
            ArtCasList = new List<ArtCasXmlModel>();
        }
    }
}