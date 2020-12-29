using ExtECRMainLogic.Models.ReceiptModels;
using ExtECRMainLogic.Models.TemplateModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtECRMainLogic.Classes.Helpers
{
    public static class HTMLReceiptHelper
    {
        /// <summary>
        /// Create html from receipt according to selected template
        /// </summary>
        /// <param name="receipt"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static string CreateHtmlReceipt(ReceiptModel receipt, RollerTypeReportModel template)
        {
            List<string> receiptLines = GenericCommon.GetReceiptLines(receipt, template);
            string html = CreateHtmlString(receiptLines, receipt.DigitalSignature);
            return html;
        }

        /// <summary>
        /// Create html string
        /// </summary>
        /// <param name="receiptLines"></param>
        /// <param name="digitalSignature"></param>
        /// <returns></returns>
        private static string CreateHtmlString(List<string> receiptLines, byte[] digitalSignature)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<!DOCTYPE html>\n");
            html.Append("<html>\n");
            html.Append("<head>\n");
            html.Append("<meta charset=\"utf-8\" />\n");
            html.Append("</head>\n");
            html.Append("<body>\n");
            CreateHtmlBody(html, receiptLines, digitalSignature);
            html.Append("</body>\n");
            html.Append("</html>\n");
            return html.ToString();
        }

        /// <summary>
        /// Create html body
        /// </summary>
        /// <param name="html"></param>
        /// <param name="receiptLines"></param>
        /// <param name="digitalSignature"></param>
        private static void CreateHtmlBody(StringBuilder html, List<string> receiptLines, byte[] digitalSignature)
        {
            // Body container start tag
            html.Append("<div style=\"display: flex; flex-direction: column;\">\n");
            foreach (string line in receiptLines)
            {
                CreateHtmlItem(html, line);
            }
            if (digitalSignature != null)
            {
                CreateHtmlSignature(html, digitalSignature);
            }
            // Body container end tag
            html.Append("</div>\n");
        }

        /// <summary>
        /// Create html receipt item
        /// </summary>
        /// <param name="html"></param>
        /// <param name="line"></param>
        private static void CreateHtmlItem(StringBuilder html, string line)
        {
            // Row container start tag
            html.Append("<div style=\"display: flex; flex-direction: row; padding: 0px; margin: 5px 0px;\">\n");
            // Item container start tag
            html.Append("<div style=\"display: flex; flex-basis: 100%; justify-content: flex-start; align-items: center;\">\n");
            // Item start tag
            html.Append("<span>");
            // Item
            html.Append(line);
            // Item end tag
            html.Append("</span>\n");
            // Item container end tag
            html.Append("</div>\n");
            // Row container end tag
            html.Append("</div>\n");
            //html.Append("<br>\n");
        }

        /// <summary>
        /// Create html receipt signature
        /// </summary>
        /// <param name="html"></param>
        /// <param name="digitalSignature"></param>
        private static void CreateHtmlSignature(StringBuilder html, byte[] digitalSignature)
        {
            string signature = Convert.ToBase64String(digitalSignature);
            // Row container start tag
            html.Append("<div style=\"height: 210px; display: flex; flex-direction: row;\">\n");
            // Signature container start tag
            html.Append("<div style=\"display: flex; flex-basis: 100%; justify-content: flex-start; align-items: center;\">\n");
            // Signature tag
            html.Append("<img style=\"width: auto; height: 95%; object-fit: contain; border: 1px solid black;\" src=\"data:image/png;base64,");
            html.Append(signature);
            html.Append("\" />\n");
            // Signature container end tag
            html.Append("</div>\n");
            // Row container end tag
            html.Append("</div>\n");
            //html.Append("<br>\n");
        }
    }
}