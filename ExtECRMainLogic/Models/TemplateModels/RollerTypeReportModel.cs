using System.Collections.Generic;
using System.Xml.Serialization;

namespace ExtECRMainLogic.Models.TemplateModels
{
    /// <summary>
    /// Represents a report/receipt template (from a template xml file)
    /// </summary>
    [XmlRoot("Report")]
    public class RollerTypeReportModel : PrinterTypeModel
    {
        [XmlAttribute("PrintType")]
        public int PrintType { get; set; }
        [XmlAttribute("Name")]
        public string ReportName { get; set; }
        [XmlAttribute("ReportType")]
        public int ReportType { get; set; }
        [XmlAttribute("MaxWidth")]
        public double MaxWidth { get; set; }
        [XmlElement("Section")]
        public List<ReportSectionsModel> Sections { get; set; }

        public RollerTypeReportModel()
        {
            Sections = new List<ReportSectionsModel>();
        }
    }
}