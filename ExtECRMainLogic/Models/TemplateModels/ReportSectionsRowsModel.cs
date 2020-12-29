using System.Collections.Generic;
using System.Xml.Serialization;

namespace ExtECRMainLogic.Models.TemplateModels
{
    /// <summary>
    /// Represents a row into a section
    /// </summary>
    public class ReportSectionsRowsModel
    {
        [XmlElement("Column")]
        public List<ReportSectionsColumnsModel> SectionColumns { get; set; }

        public ReportSectionsRowsModel()
        {
            SectionColumns = new List<ReportSectionsColumnsModel>();
        }
    }
}