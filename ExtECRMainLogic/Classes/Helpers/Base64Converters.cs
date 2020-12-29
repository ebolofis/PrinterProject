using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ExtECRMainLogic.Classes.Helpers
{
    public static class Base64Converters
    {
        /// <summary>
        /// Base64 string to Image
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static Image Base64ToImage(string base64String)
        {
            // Convert Base64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }

        /// <summary>
        /// Image to Base64 string
        /// </summary>
        /// <param name="image"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ImageToBase64(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();
                // Convert byte[] to Base64 string
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        /// <summary>
        /// Base64 string to Byte array
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static byte[] Base64ToByteArray(string base64String)
        {
            return Convert.FromBase64String(base64String);
        }

        /// <summary>
        /// Byte array to Base64 string
        /// </summary>
        /// <param name="byteArraySource"></param>
        /// <returns></returns>
        public static string ByteArrayToBase64(byte[] byteArraySource)
        {
            return Convert.ToBase64String(byteArraySource);
        }
    }
}