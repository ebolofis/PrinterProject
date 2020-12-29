using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Models.LCDModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ExtECRMainLogic.Classes.Helpers
{
    public static class LcdDisplayHelper
    {
        /// <summary>
        /// Returns the message in suitable format for display device input
        /// </summary>
        /// <param name="lcdType"></param>
        /// <param name="lcdObj"></param>
        /// <param name="lcdLength"></param>
        /// <param name="strComPortName"></param>
        /// <returns></returns>
        public static byte[] GetLcdMessage(LcdTypeEnum lcdType, LCDModel lcdObj, int lcdLength, string strComPortName = "")
        {
            byte[] result = new byte[1];
            switch (lcdType)
            {
                case LcdTypeEnum.Casio:
                    result = GetCassioLcdMessage(lcdObj, lcdLength);
                    break;
                case LcdTypeEnum.NCR:
                    result = GetNCRLcdMessage(lcdObj, lcdLength);
                    break;
                case LcdTypeEnum.IBM:
                    result = GetIBMLcdMessage(lcdObj, lcdLength);
                    break;
                case LcdTypeEnum.NCR_ENG:
                    result = GetNCRLcdMessage_Greeklish(lcdObj, lcdLength);
                    break;
                case LcdTypeEnum.TOSHIBA_ST_A20:
                    result = GetToshibaLcdMessage(lcdObj, lcdLength);
                    break;
                case LcdTypeEnum.Use_VF60Commander:
                    result = GetFujitsuLcdMessage_French(lcdObj, lcdLength, strComPortName);
                    break;
                case LcdTypeEnum.WINCOR_NIXDORF_BA63:
                    result = GetWincorNixdorfLcdMessage(lcdObj, lcdLength);
                    break;
                case LcdTypeEnum.VFD_PD220:
                    result = GetNCRLcdMessage_Greeklish(lcdObj, lcdLength);
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// CASIO customer display
        /// </summary>
        /// <param name="lcdObj"></param>
        /// <param name="lcdLength"></param>
        /// <returns></returns>
        private static byte[] GetCassioLcdMessage(LCDModel lcdObj, int lcdLength)
        {
            byte[] finalArr;
            if (!string.IsNullOrEmpty(lcdObj.ThankYouMessage))
            {
                // If Received LCD message is total
                string row1 = string.Empty;
                string row2 = string.Empty;
                lcdObj.Description = ConvertToGreeklish(lcdObj.Description);
                // Set item Description row
                // If item description is bigger than LCD first row (e.g. 20 chars)
                if (lcdObj.Description.Length > lcdLength)
                {
                    // Show the first LCDLength chars that fit in LCD first row
                    row1 = lcdObj.Description.Substring(0, lcdLength);
                }
                else
                {
                    // Show item description followed by spaces
                    row1 = lcdObj.Description.PadRight(lcdLength, ' ');
                }
                // Set item Price row
                var pr = string.Format("{0:0.00}", lcdObj.Price);
                pr = ConvertToGreeklish(pr);
                // If item price is too big
                if (pr.Length > 10)
                {
                    // Get the first LCDLength chars
                    var s = pr.Substring(0, 10);
                    // Append the price to the final string
                    row2 = s;
                }
                else
                {
                    // Set the alignment of price to right
                    row2 = pr.PadLeft(10, ' ');
                }
                string scrollRow = ConvertToGreeklish(lcdObj.ThankYouMessage);
                finalArr = getCassioStaticMsgByteArr(row1, row2, scrollRow);
            }
            else
            {
                // If LCD Message is item
                string row1 = string.Empty;
                string row2 = string.Empty;
                lcdObj.Description = ConvertToGreeklish(lcdObj.Description);
                // Set item Description row
                // If item description is bigger than LCD first row (e.g. 20 chars)
                if (lcdObj.Description.Length > lcdLength)
                {
                    // Show the first LCDLength chars that fit in LCD first row
                    row1 = lcdObj.Description.Substring(0, lcdLength);
                }
                else
                {
                    // Show item description followed by spaces
                    row1 = lcdObj.Description.PadRight(lcdLength, ' ');
                }
                // Set item Price row
                var pr = string.Format("{0:0.00}", lcdObj.Price);
                pr = ConvertToGreeklish(pr);
                // If item price is too big
                if (pr.Length > 10)
                {
                    // Get the first LCDLength chars
                    var s = pr.Substring(0, 10);
                    // Append the price to the final string
                    row2 = s;
                }
                else
                {
                    // Set the alignment of price to right
                    row2 = pr.PadLeft(10, ' ');
                }
                finalArr = getCassioStaticMsgByteArr(row1, row2);
            }
            return finalArr;
        }

        /// <summary>
        /// NCR customer display
        /// </summary>
        /// <param name="lcdObj"></param>
        /// <param name="lcdLength"></param>
        /// <returns></returns>
        private static byte[] GetNCRLcdMessage(LCDModel lcdObj, int lcdLength)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(lcdObj.ThankYouMessage))
            {
                // If Received LCD message is total
                string totalStr = lcdObj.Description;
                string thankStr = lcdObj.ThankYouMessage;
                // Get total message length
                var desclen = totalStr.Length;
                // Format the price with 2 floating points
                var pr = string.Format("{0:0.00}", lcdObj.Price);
                // If totalStr length is greater than price length + 1 space char
                if (desclen > (lcdLength - (pr.Length + 1)))
                {
                    // Get the total substring that fits the available space
                    totalStr = totalStr.Substring(0, lcdLength - (pr.Length + 1));
                    // Get the length of the total string
                    desclen = totalStr.Length;
                }
                // If thank message exceeds LCD row length
                if (thankStr.Length > lcdLength)
                {
                    // Get as many characters as LCD screen char length
                    thankStr = thankStr.Substring(0, lcdLength);
                }
                else
                {
                    // Get remaining space of 2nd row
                    var remSpaces = lcdLength - thankStr.Length;
                    // Align thank message to the center of LCD
                    if ((remSpaces % 2) == 0)
                    {
                        var spacesToAdd = remSpaces / 2;
                        // Add spaces to the left
                        thankStr = new string(' ', spacesToAdd) + thankStr;
                    }
                    else
                    {
                        var spacesToAdd = remSpaces / 2;
                        // Add spaces to the left
                        thankStr = new string(' ', spacesToAdd + 1) + thankStr;
                    }
                }
                // LCD row1
                string row1str = totalStr + pr.PadLeft(lcdLength - desclen, ' ');
                // LCD row2
                string row2str = thankStr.PadRight(lcdLength, ' ');
                // Final string
                result = row1str + row2str;
            }
            else
            {
                // If LCD Message is item
                // Set item Description row
                // If item description is bigger than LCD first row (e.g. 20 chars)
                if (lcdObj.Description.Length > lcdLength)
                {
                    // Show the first LCDLength chars that fit in LCD first row
                    result = lcdObj.Description.Substring(0, lcdLength);
                }
                else
                {
                    // Show item description followed by spaces
                    result = lcdObj.Description.PadRight(lcdLength, ' ');
                }
                // Set item Price row
                var pr = string.Format("{0:0.00}", lcdObj.Price);
                // If item price is too big
                if (pr.Length > lcdLength)
                {
                    // Get the first LCDLength chars
                    var s = pr.Substring(0, lcdLength);
                    // Append the price to the final string
                    result += s;
                }
                else
                {
                    // Set the alignment of price to right
                    result = result + pr.PadLeft(lcdLength, ' ');
                }
            }
            // Convert to upper case and remove punctuation from the text
            string resultUpper = TextHelper.RemovePunctuation(result, true);
            // Set encoding ASCII 737 code page
            var lcdEnc = Encoding.GetEncoding(737);
            // Get the byte array of the final message
            byte[] finaldata = lcdEnc.GetBytes(resultUpper);
            return finaldata;
        }

        /// <summary>
        /// IBM customer display
        /// </summary>
        /// <param name="lcdObj"></param>
        /// <param name="lcdLength"></param>
        /// <returns></returns>
        private static byte[] GetIBMLcdMessage(LCDModel lcdObj, int lcdLength)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(lcdObj.ThankYouMessage))
            {
                // If Received LCD message is total
                string totalStr = ConvertToGreeklish(lcdObj.Description);
                string thankStr = ConvertToGreeklish(lcdObj.ThankYouMessage);
                // Get total message length
                var desclen = totalStr.Length;
                // Format the price with 2 floating points
                var pr = string.Format("{0:0.00}", lcdObj.Price);
                // If totalStr length is greater than price length + 1 space char
                if (desclen > (lcdLength - (pr.Length + 1)))
                {
                    // Get the total substring that fits the available space
                    totalStr = totalStr.Substring(0, lcdLength - (pr.Length + 1));
                    // Get the length of the total string
                    desclen = totalStr.Length;
                }
                // If thank message exceeds LCD row length
                if (thankStr.Length > lcdLength)
                {
                    // Get as many characters as LCD screen char length
                    thankStr = thankStr.Substring(0, lcdLength);
                }
                else
                {
                    if (thankStr.Length > lcdLength - 1)
                    {
                        thankStr = thankStr.Substring(lcdLength - 1);
                    }
                }
                // LCD row1
                string row1str = totalStr + pr.PadLeft(lcdLength - desclen, ' ');
                // LCD row2
                string row2str = thankStr.PadRight(lcdLength - 1, ' ');
                // Final string
                result = row1str + row2str;
            }
            else
            {
                // If LCD Message is item
                lcdObj.Description = ConvertToGreeklish(lcdObj.Description);
                // Set item Description row
                // If item description is bigger than LCD first row (e.g. 20 chars)
                if (lcdObj.Description.Length > lcdLength)
                {
                    // Show the first LCDLength chars that fit in LCD first row
                    result = lcdObj.Description.Substring(0, lcdLength);
                }
                else
                {
                    // Show item description followed by spaces
                    result = lcdObj.Description.PadRight(lcdLength, ' ');
                }
                // Set item Price row
                var pr = string.Format("{0:0.00}", lcdObj.Price);
                // If item price is too big
                if (pr.Length > lcdLength - 1)
                {
                    // Get the first LCDLength chars
                    var s = pr.Substring(0, lcdLength - 1);
                    // Append the price to the final string
                    result += s;
                }
                else
                {
                    // Set the alignment of price to right
                    result = result + pr.PadLeft(lcdLength - 1, ' ');
                }
            }
            // Convert to upper case and remove punctuation from the text
            string resultUpper = TextHelper.RemovePunctuation(result, true);
            // Reset the cursor
            var resetCursor = ((char)31).ToString();
            // Set encoding ASCII 737 code page
            var lcdEnc = Encoding.GetEncoding(869);
            // Get the byte array of the final message
            byte[] finaldata = lcdEnc.GetBytes(resetCursor + resultUpper);
            return finaldata;
        }

        /// <summary>
        /// NCR display - GREEKLISH character set
        /// </summary>
        /// <param name="lcdObj"></param>
        /// <param name="lcdLength"></param>
        /// <returns></returns>
        private static byte[] GetNCRLcdMessage_Greeklish(LCDModel lcdObj, int lcdLength)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(lcdObj.ThankYouMessage))
            {
                if (lcdObj.ThankYouMessage == "ΕΥΧΑΡΙΣΤΟΥΜΕ !")
                {
                    lcdObj.ThankYouMessage = "THANK YOU!";
                }
                else
                {
                    lcdObj.ThankYouMessage = ConvertToGreeklish(lcdObj.ThankYouMessage);
                }
            }
            lcdObj.Description = ConvertToGreeklish(lcdObj.Description);
            if (!string.IsNullOrEmpty(lcdObj.ThankYouMessage))
            {
                // If Received LCD message is total
                string totalStr = lcdObj.Description;
                string thankStr = lcdObj.ThankYouMessage;
                // Get total message length
                var desclen = totalStr.Length;
                // Format the price with 2 floating points
                var pr = string.Format("{0:0.00}", lcdObj.Price);
                // If totalStr length is greater than price length + 1 space char
                if (desclen > (lcdLength - (pr.Length + 1)))
                {
                    // Get the total substring that fits the available space
                    totalStr = totalStr.Substring(0, lcdLength - (pr.Length + 1));
                    // Get the length of the total string
                    desclen = totalStr.Length;
                }
                // If thank message exceeds LCD row length
                if (thankStr.Length > lcdLength)
                {
                    // Get as many characters as LCD screen char length
                    thankStr = thankStr.Substring(0, lcdLength);
                }
                else
                {
                    // Get remaining space of 2nd row
                    var remSpaces = lcdLength - thankStr.Length;
                    // Align thank message to the center of LCD
                    if ((remSpaces % 2) == 0)
                    {
                        var spacesToAdd = remSpaces / 2;
                        // Add spaces to the left
                        thankStr = new string(' ', spacesToAdd) + thankStr;
                    }
                    else
                    {
                        var spacesToAdd = remSpaces / 2;
                        // Add spaces to the left
                        thankStr = new string(' ', spacesToAdd + 1) + thankStr;
                    }
                }
                // LCD row1
                string row1str = totalStr + pr.PadLeft(lcdLength - desclen, ' ');
                // LCD row2
                string row2str = thankStr.PadRight(lcdLength, ' ');
                // Final string
                result = row1str + row2str;
            }
            else
            {
                // If LCD Message is item
                // Set item Description row
                // If item description is bigger than LCD first row (e.g. 20 chars)
                if (lcdObj.Description.Length > lcdLength)
                {
                    // Show the first LCDLength chars that fit in LCD first row
                    result = lcdObj.Description.Substring(0, lcdLength);
                }
                else
                {
                    // Show item description followed by spaces
                    result = lcdObj.Description.PadRight(lcdLength, ' ');
                }
                // Set item Price row
                var pr = string.Format("{0:0.00}", lcdObj.Price);
                // If item price is too big
                if (pr.Length > lcdLength)
                {
                    // Get the first LCDLength chars
                    var s = pr.Substring(0, lcdLength);
                    // Append the price to the final string
                    result += s;
                }
                else
                {
                    // Set the alignment of price to right
                    result = result + pr.PadLeft(lcdLength, ' ');
                }
            }
            // Convert to upper case and remove punctuation from the text
            string resultUpper = TextHelper.RemovePunctuation(result, true);
            // Set encoding ASCII 737 code page
            var lcdEnc = Encoding.GetEncoding(737);
            // Get the byte array of the final message
            byte[] finaldata = lcdEnc.GetBytes(resultUpper);
            return finaldata;
        }

        /// <summary>
        /// Toshiba customer display - on 9600o81 - Greek cp869
        /// </summary>
        /// <param name="lcdObj"></param>
        /// <param name="lcdLength"></param>
        /// <returns></returns>
        private static byte[] GetToshibaLcdMessage(LCDModel lcdObj, int lcdLength)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(lcdObj.ThankYouMessage))
            {
                // If LCD Message is item
                // Set item Description row
                // If item descr bigger than LCD first row (e.g. 20 chars)
                if (lcdObj.Description.Length > lcdLength)
                {
                    // Show the first LCDLength chars that fit in LCD first row
                    result = lcdObj.Description.Substring(0, lcdLength);
                }
                else
                {
                    // Show item description followed by spaces
                    result = lcdObj.Description.PadRight(lcdLength, ' ');
                }
                result = result + "\r\n";
                // Set item Price row
                var pr = string.Format("{0:0.00}", lcdObj.Price);
                // If item price is too big
                if (pr.Length > lcdLength)
                {
                    // Get the first LCDLength chars
                    var s = pr.Substring(0, lcdLength);
                    // Append the price to the final string
                    result += s;
                }
                else
                {
                    // Set the alignment of price to right
                    result = result + pr.PadLeft(lcdLength, ' ');
                }
            }
            else
            {
                // If Received LCD message is total
                string totalStr;
                string thankStr;
                if (lcdObj.Description == "ΣΥΝΟΛΟ")
                {
                    totalStr = "TOTAL";
                }
                else
                {
                    totalStr = lcdObj.Description;
                }
                if (lcdObj.ThankYouMessage == "ΕΥΧΑΡΙΣΤΟΥΜΕ !")
                {
                    thankStr = "THANK YOU !";
                }
                else
                {
                    thankStr = lcdObj.ThankYouMessage;
                }
                // Get total message length
                var desclen = totalStr.Length;
                // Format the price with 2 floating points
                var pr = string.Format("{0:0.00}", lcdObj.Price);
                // If totalStr length is greater than price length + 1 space char
                if (desclen > (lcdLength - (pr.Length + 1)))
                {
                    // Get the total substring that fits the available space
                    totalStr = totalStr.Substring(0, lcdLength - (pr.Length + 1));
                    // Get the length of the total string
                    desclen = totalStr.Length;
                }
                // If thank message exceeds LCD row length
                if (thankStr.Length > lcdLength)
                {
                    // Get as many characters as LCD screen char length
                    thankStr = thankStr.Substring(0, lcdLength);
                }
                else
                {
                    // Get remaining space of 2nd row
                    var remSpaces = lcdLength - thankStr.Length;
                    // Align thank message to the center of LCD
                    if ((remSpaces % 2) == 0)
                    {
                        var spacesToAdd = remSpaces / 2;
                        // Add spaces to the left
                        thankStr = new string(' ', spacesToAdd) + thankStr;
                    }
                    else
                    {
                        var spacesToAdd = remSpaces / 2;
                        // Add spaces to the left
                        thankStr = new string(' ', spacesToAdd + 1) + thankStr;
                    }
                }
                // LCD row1
                string row1 = totalStr + pr.PadLeft(lcdLength - desclen, ' ');
                // LCD row2
                string row2 = thankStr.PadRight(lcdLength, ' ');
                // Final string
                result = row1 + "\r\n" + row2;
            }
            // Convert to upper case and remove punctuation from the text
            string resultUpper = TextHelper.RemovePunctuation(result, true);
            // Set encoding ASCII 869 codepage
            var lcdEnc = Encoding.GetEncoding(869);
            byte[] selectGreek = { 0x1B, 0x52, 0x10 };
            byte[] clearScreen = { 0x1B, 0x5B, 0x32, 0x4A };
            byte[] cursorAtTop = { 0x1B, 0x5B, 0x01, 0x3B, 0x01, 0x48 };
            // Get the byte array of the complete message
            byte[] fullMsg = lcdEnc.GetBytes(resultUpper);
            byte[] finaldata = new byte[selectGreek.Length + clearScreen.Length + cursorAtTop.Length + fullMsg.Length];
            selectGreek.CopyTo(finaldata, 0);
            clearScreen.CopyTo(finaldata, selectGreek.Length);
            cursorAtTop.CopyTo(finaldata, selectGreek.Length + clearScreen.Length);
            fullMsg.CopyTo(finaldata, selectGreek.Length + clearScreen.Length + cursorAtTop.Length);
            return finaldata;
        }

        /// <summary>
        /// Fujitsu TeamPOS series Customer Display on COMx - French
        /// </summary>
        /// <param name="lcdObj"></param>
        /// <param name="lcdLength"></param>
        /// <param name="strComPortName"></param>
        /// <returns></returns>
        private static byte[] GetFujitsuLcdMessage_French(LCDModel lcdObj, int lcdLength, string strComPortName)
        {
            string strLine01;
            string strLine02;
            if (!string.IsNullOrEmpty(lcdObj.ThankYouMessage))
            {
                // If Received LCD message is total
                if (lcdObj.ThankYouMessage == "ΕΥΧΑΡΙΣΤΟΥΜΕ !")
                {
                    lcdObj.ThankYouMessage = "MERCI !";
                }
                if (lcdObj.Description == "ΣΥΝΟΛΟ")
                {
                    lcdObj.Description = "TOTAL";
                }
                string totalStr = lcdObj.Description;
                string thankStr = lcdObj.ThankYouMessage;
                // Get total message length
                var desclen = totalStr.Length;
                // Format the price with 2 floating points
                var price = string.Format("{0:0.00}", lcdObj.Price);
                // If totalStr length is greater than price length + 1 space char
                if (desclen > (lcdLength - (price.Length + 1)))
                {
                    // Get the total substring that fits the available space
                    totalStr = totalStr.Substring(0, lcdLength - (price.Length + 1));
                    // Get the length of the total string
                    desclen = totalStr.Length;
                }
                // If thank message exceeds LCD row length
                if (thankStr.Length > lcdLength)
                {
                    // Get as many characters as LCD screen char length
                    thankStr = thankStr.Substring(0, lcdLength);
                }
                else
                {
                    // Get remaining space of 2nd row
                    var remSpaces = lcdLength - thankStr.Length;
                    // Align thank message to the center of LCD
                    if ((remSpaces % 2) == 0)
                    {
                        var spacesToAdd = remSpaces / 2;
                        // Add spaces to the left
                        thankStr = new string(' ', spacesToAdd) + thankStr;
                    }
                    else
                    {
                        var spacesToAdd = remSpaces / 2;
                        // Add spaces to the left
                        thankStr = new string(' ', spacesToAdd + 1) + thankStr;
                    }
                }
                // LCD row1
                strLine01 = totalStr + price.PadLeft(lcdLength - desclen, ' ');
                // LCD row2
                strLine02 = thankStr.PadRight(lcdLength, ' ');
            }
            else
            {
                // If LCD Message is item
                // Set item Description row
                // If item description bigger than LCD first row (e.g. 20 chars)
                if (lcdObj.Description.Length > lcdLength)
                {
                    // Show the first LCDLength chars that fit in LCD first row
                    strLine01 = lcdObj.Description.Substring(0, lcdLength);
                }
                else
                {
                    // Show item description followed by spaces
                    strLine01 = lcdObj.Description.PadRight(lcdLength, ' ');
                }
                // Set item Price row
                var pr = string.Format("{0:0.00}", lcdObj.Price);
                // if item price is too big get the first LCDLength chars else set the alignment of price to right
                strLine02 = ((pr.Length > lcdLength) ? pr.Substring(0, lcdLength) : pr.PadLeft(lcdLength, ' '));
            }
            // Build text file
            using (StreamWriter streamWriter = File.CreateText("VF60Commander.txt"))
            {
                streamWriter.WriteLine(strComPortName);
                streamWriter.WriteLine("DATA \\1b[1;1H" + strLine01);
                streamWriter.WriteLine("DATA \\1b[2;1H" + strLine02);
            }
            // Return empty result to avoid invoking the serial port
            return new byte[0];
        }

        /// <summary>
        /// Wincor-Nixdorf customer display (BA63) - on 9600o81 - Greek cp737
        /// </summary>
        /// <param name="lcdObj"></param>
        /// <param name="lcdLength"></param>
        /// <returns></returns>
        private static byte[] GetWincorNixdorfLcdMessage(LCDModel lcdObj, int lcdLength)
        {
            string row1 = string.Empty;
            string row2 = string.Empty;
            if (!string.IsNullOrEmpty(lcdObj.ThankYouMessage))
            {
                if (lcdObj.ThankYouMessage == "ΕΥΧΑΡΙΣΤΟΥΜΕ !")
                {
                    lcdObj.ThankYouMessage = "THANK YOU!";
                }
                else
                {
                    lcdObj.ThankYouMessage = ConvertToGreeklish(lcdObj.ThankYouMessage);
                }
            }
            lcdObj.Description = ConvertToGreeklish(lcdObj.Description);
            if (string.IsNullOrEmpty(lcdObj.ThankYouMessage))
            {
                // If LCD Message is item
                // Set item Description row
                // If item descr bigger than LCD first row (e.g. 20 chars)
                if (lcdObj.Description.Length > lcdLength)
                {
                    // Show the first LCDLength chars that fit in LCD first row
                    row1 = lcdObj.Description.Substring(0, lcdLength);
                }
                else
                {
                    // Show item description followed by spaces
                    row1 = lcdObj.Description.PadRight(lcdLength, ' ');
                }
                // Set item Price row
                var pr = string.Format("{0:0.00}", lcdObj.Price);
                // If item price is too big
                if (pr.Length > lcdLength)
                {
                    // Get the first LCDLength chars
                    row2 = pr.Substring(0, lcdLength);
                }
                else
                {
                    // Set the alignment of price to right
                    row2 = pr.PadLeft(lcdLength, ' ');
                }
            }
            else
            {
                // If Received LCD message is total
                string totalStr = lcdObj.Description;
                string thankStr = lcdObj.ThankYouMessage;
                // Get total message length
                var desclen = totalStr.Length;
                // Format the price with 2 floating points
                var pr = string.Format("{0:0.00}", lcdObj.Price);
                // If totalStr length is greater than price length + 1 space char
                if (desclen > (lcdLength - (pr.Length + 1)))
                {
                    // Get the total substring that fits the available space
                    totalStr = totalStr.Substring(0, lcdLength - (pr.Length + 1));
                    // Get the length of the total string
                    desclen = totalStr.Length;
                }
                // If thank message exceeds LCD row length
                if (thankStr.Length > lcdLength)
                {
                    // Get as many characters as LCD screen char length
                    thankStr = thankStr.Substring(0, lcdLength);
                }
                else
                {
                    // Get remaining space of 2nd row
                    var remSpaces = lcdLength - thankStr.Length;
                    // Align thank message to the center of LCD
                    if ((remSpaces % 2) == 0)
                    {
                        var spacesToAdd = remSpaces / 2;
                        // Add spaces to the left
                        thankStr = new string(' ', spacesToAdd) + thankStr;
                    }
                    else
                    {
                        var spacesToAdd = remSpaces / 2;
                        // Add spaces to the left
                        thankStr = new string(' ', spacesToAdd + 1) + thankStr;
                    }
                }
                // LCD row1
                row1 = totalStr + pr.PadLeft(lcdLength - desclen, ' ');
                // LCD row2
                row2 = thankStr.PadRight(lcdLength, ' ');
            }
            // Convert to upper case and remove punctuation from the text
            string result2UpperLine1 = TextHelper.RemovePunctuation(row1, true);
            string result2UpperLine2 = TextHelper.RemovePunctuation(row2, true);
            byte[] clearScreen = { 0x1B, 0x5B, 0x32, 0x4A };
            byte[] cursorAtStartOfLine1 = { 0x1B, 0x5B, 0x31, 0x3B, 0x31, 0x48 };
            byte[] cursorAtStartOfLine2 = { 0x1B, 0x5B, 0x32, 0x3B, 0x31, 0x48 };
            // Set dummy default encoding ASCII 437 codepage (no Greek) just to get byte array from string
            var lcdEnc = Encoding.GetEncoding(437);
            // Get the byte array of the row1 message
            byte[] fullMsgLine1 = lcdEnc.GetBytes(result2UpperLine1);
            // Get the byte array of the row2 message
            byte[] fullMsgLine2 = lcdEnc.GetBytes(result2UpperLine2);
            byte[] finaldata = new byte[clearScreen.Length + cursorAtStartOfLine1.Length + fullMsgLine2.Length + cursorAtStartOfLine2.Length + fullMsgLine2.Length];
            clearScreen.CopyTo(finaldata, 0);
            cursorAtStartOfLine1.CopyTo(finaldata, clearScreen.Length);
            fullMsgLine1.CopyTo(finaldata, clearScreen.Length + cursorAtStartOfLine1.Length);
            cursorAtStartOfLine2.CopyTo(finaldata, clearScreen.Length + cursorAtStartOfLine1.Length + fullMsgLine1.Length);
            fullMsgLine2.CopyTo(finaldata, clearScreen.Length + cursorAtStartOfLine1.Length + fullMsgLine1.Length + cursorAtStartOfLine2.Length);
            return finaldata;
        }

        /// <summary>
        /// Replace Greek string with the equivalent English
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ConvertToGreeklish(string str)
        {
            str = TextHelper.RemovePunctuation(str, true);
            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                {"Α", "A"},
                {"Β", "B"},
                {"Γ", "G"},
                {"Δ", "D"},
                {"Ε", "E"},
                {"Ζ", "Z"},
                {"Η", "I"},
                {"Θ", "TH"},
                {"Ι", "I"},
                {"Κ", "K"},
                {"Λ", "L"},
                {"Μ", "M"},
                {"Ν", "N"},
                {"Ξ", "KS"},
                {"Ο", "O"},
                {"Π", "P"},
                {"Ρ", "R"},
                {"Σ", "S"},
                {"Τ", "T"},
                {"Υ", "Y"},
                {"Φ", "F"},
                {"Χ", "X"},
                {"Ψ", "PS"},
                {"Ω", "O"}
            };
            var chArr = str.ToCharArray();
            StringBuilder stb = new StringBuilder();
            foreach (var ch in chArr)
            {
                var match = dict.Where(d => d.Key == ch.ToString());
                if (match != null && match.Count() == 1)
                {
                    stb.Append(match.FirstOrDefault().Value);
                }
                else
                {
                    stb.Append(ch);
                }
            }
            return stb.ToString();
        }

        /// <summary>
        /// Get Casio LCD byte array with the message and the correct esc chars
        /// </summary>
        /// <param name="row1"></param>
        /// <param name="row2"></param>
        /// <param name="scrollRow"></param>
        /// <returns></returns>
        private static byte[] getCassioStaticMsgByteArr(string row1, string row2, string scrollRow = " ")
        {
            // Handle scroll row
            byte[] scrollByteArr = Encoding.GetEncoding(869).GetBytes(scrollRow);
            byte[] scrollFinalArr = new byte[scrollByteArr.Length + 5];
            scrollFinalArr[0] = 12;
            scrollFinalArr[1] = 27;
            scrollFinalArr[2] = 81;
            scrollFinalArr[3] = 68;
            scrollByteArr.CopyTo(scrollFinalArr, 4);
            scrollFinalArr[scrollFinalArr.Length - 1] = 13;
            // Handle first row
            byte[] msgByteArr = Encoding.GetEncoding(869).GetBytes(row1);
            byte[] row1FinalArr = new byte[msgByteArr.Length + 4];
            row1FinalArr[0] = 27;
            row1FinalArr[1] = 81;
            row1FinalArr[2] = 66;
            msgByteArr.CopyTo(row1FinalArr, 3);
            row1FinalArr[row1FinalArr.Length - 1] = 13;
            // Handle second row
            byte[] msgByteArr2 = Encoding.GetEncoding(869).GetBytes(row2);
            byte[] row2FinalArr = new byte[msgByteArr2.Length + 4];
            row2FinalArr[0] = 27;
            row2FinalArr[1] = 81;
            row2FinalArr[2] = 67;
            msgByteArr2.CopyTo(row2FinalArr, 3);
            row2FinalArr[row2FinalArr.Length - 1] = 13;
            // Handle final row
            byte[] finalArr = new byte[scrollFinalArr.Length + row1FinalArr.Length + row2FinalArr.Length];
            scrollFinalArr.CopyTo(finalArr, 0);
            row1FinalArr.CopyTo(finalArr, scrollFinalArr.Length);
            row2FinalArr.CopyTo(finalArr, scrollFinalArr.Length + row1FinalArr.Length);
            return finalArr;
        }
    }
}