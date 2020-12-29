using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Models.ReceiptModels;
using System;
using System.Xml.Serialization;

namespace ExtECRMainLogic.Models.HelperModels
{
    public class ArtCasXmlModel
    {
        public Guid Id { get; set; }
        [XmlAttribute("LogDate")]
        public DateTime LogTime { get; set; }
        [XmlElement("ReceiptModel")]
        public ReceiptModel ReceiptModel { get; set; }
        [XmlElement("ArtCas")]
        public Artcas Artcas { get; set; }
        [XmlElement("PrintedLines")]
        public string PrintedLines { get; set; }
        [XmlElement("ErrorDescription")]
        public string ErrorDescription { get; set; }
        [XmlElement("ReceivedMode")]
        public string ReceivedMode { get; set; }
        [XmlElement("PrintStatus")]
        public PrintStatusEnum PrintStatus { get; set; }
        [XmlElement("FiscalName")]
        public string FiscalName { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ArtCasXmlModel()
        {

        }
    }
}