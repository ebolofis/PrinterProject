using System.Text;

namespace ExtECRMainLogic.Classes.Helpers
{
    public static class TextHelper
    {
        /// <summary>
        /// 
        /// </summary>
        static char[] replacementArray = { 'α', 'ε', 'ι', 'ο', 'υ', 'η', 'σ', 'ω', 'Α', 'Ε', 'Ι', 'Ο', 'Υ', 'Η', 'Ω' };
        /// <summary>
        /// 
        /// </summary>
        static char[] accentsArray = { 'ά', 'έ', 'ί', 'ό', 'ύ', 'ή', 'ς', 'ώ', 'Ά', 'Έ', 'Ί', 'Ό', 'Ύ', 'Ή', 'Ώ' };

        /// <summary>
        /// Remove punctuation and convert to upper case or lower case
        /// </summary>
        /// <param name="text"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static string RemovePunctuation(string text, bool upper)
        {
            if (upper)
            {
                return RemovePunctuation(text).ToUpper();
            }
            return RemovePunctuation(text);
        }

        /// <summary>
        /// Remove punctuation
        /// </summary>
        /// <param name="accentedStr"></param>
        /// <returns></returns>
        private static string RemovePunctuation(string accentedStr)
        {
            char[] replacement = replacementArray;
            char[] accents = accentsArray;
            string temp = new string(replacement).ToUpper();
            char[] upperReplacement = temp.ToCharArray();
            temp = new string(accents).ToUpper();
            char[] upperAccents = temp.ToCharArray();
            StringBuilder returnString = new StringBuilder();
            if (accents != null && replacement != null && accentedStr.IndexOfAny(accents) > -1)
            {
                returnString.Length = 0;
                returnString.Append(accentedStr);
                for (int i = 0; i < accents.Length; i++)
                {
                    returnString.Replace(accents[i], replacement[i]);
                }
                return returnString.ToString();
            }
            else if (accents != null && replacement != null && accentedStr.IndexOfAny(upperAccents) > -1)
            {
                returnString.Length = 0;
                returnString.Append(accentedStr);
                for (int i = 0; i < upperAccents.Length; i++)
                {
                    returnString.Replace(upperAccents[i], upperReplacement[i]);
                }
                return returnString.ToString();
            }
            else
                return accentedStr;
        }
    }
}