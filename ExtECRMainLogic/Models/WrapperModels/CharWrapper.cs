using System;
using System.Xml.Serialization;

namespace ExtECRMainLogic.Models.WrapperModels
{
    public class CharWrapper
    {
        [XmlText]
        public Char Char { get; set; }
    }
}