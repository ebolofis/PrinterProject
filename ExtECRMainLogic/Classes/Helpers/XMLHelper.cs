using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ExtECRMainLogic.Classes.Helpers
{
    public static class XMLHelper
    {
        /// <summary>
        /// Remove illegal escape chars to serialize to xml
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<string> RemoveIllegalXmlchars(List<string> source)
        {
            if (source == null)
            {
                return new List<string>();
            }
            List<string> result = new List<string>();
            foreach (var line in source)
            {
                var validXmlCharsArr = line.Where(ch => XmlConvert.IsXmlChar(ch)).ToArray();
                string clearStr = new string(validXmlCharsArr);
                result.Add(clearStr);
            }
            return result;
        }
    }
}