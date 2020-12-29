using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace ExtECRMainLogic.Models.PrinterModels
{
    public class Printers
    {
        /// <summary>
        /// The list of Printers from PrintersXML.xml file
        /// </summary>
        [XmlElement("Printer")]
        public ObservableCollection<Printer> PrinterList = new ObservableCollection<Printer>();
    }
}