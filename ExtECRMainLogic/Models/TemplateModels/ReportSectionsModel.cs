using System.Collections.Generic;
using System.Xml.Serialization;

namespace ExtECRMainLogic.Models.TemplateModels
{
    /// <summary>
    /// Represents a section into a report/receipt template
    /// </summary>
    public class ReportSectionsModel
    {
        [XmlAttribute("SectionName")]
        public string SectionName { get; set; }
        [XmlAttribute("SectionType")]
        public int SectionType { get; set; }
        [XmlElement("Row")]
        public List<ReportSectionsRowsModel> SectionRows { get; set; }

        public ReportSectionsModel()
        {
            SectionRows = new List<ReportSectionsRowsModel>();
        }
    }
}