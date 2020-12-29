using System.Xml.Serialization;

namespace ExtECRMainLogic.Models.TemplateModels
{
    /// <summary>
    /// Represents a column into a row
    /// </summary>
    public class ReportSectionsColumnsModel
    {
        [XmlAttribute("Text")]
        public string ColumnText { get; set; }
        [XmlAttribute("Width")]
        public double Width { get; set; }
        /// <summary>
        /// When true, we use the associated bold escape sequence for the current line.
        /// </summary>
        [XmlAttribute("IsBold")]
        public bool IsBold { get; set; }
        /// <summary>
        /// When true, we use the associated underline escape sequence for the current line.
        /// </summary>
        [XmlAttribute("IsUnderline")]
        public bool IsUnderline { get; set; }
        /// <summary>
        /// When true, we use the associated italic escape sequence for the current line.
        /// </summary>
        [XmlAttribute("IsItalic")]
        public bool IsItalic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("AlignOption")]
        public string AlignOption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("FormatOption")]
        public string FormatOption { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("IsDoubleSize")]
        public bool IsDoubleSize { get; set; }
        /// <summary>
        /// When true, in association with the 'ItemGross' on Generic driver mode, when the price is
        /// zero, we get the current line ignored.
        /// </summary>
        [XmlAttribute("SkipZeroPrice")]
        public bool SkipZeroPrice { get; set; }

        public ReportSectionsColumnsModel()
        {

        }
    }
}