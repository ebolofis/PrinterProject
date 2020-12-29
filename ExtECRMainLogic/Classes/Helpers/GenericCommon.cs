using ExtECRMainLogic.Classes.Extenders;
using ExtECRMainLogic.Classes.Loggers;
using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Exceptions;
using ExtECRMainLogic.Models.PrinterModels;
using ExtECRMainLogic.Models.ReceiptModels;
using ExtECRMainLogic.Models.ReportModels;
using ExtECRMainLogic.Models.TemplateModels;
using ExtECRMainLogic.Models.WrapperModels;
using ExtECRMainLogic.Models.ZReportModels;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExtECRMainLogic.Classes.Helpers
{
    public static class GenericCommon
    {
        /// <summary>
        /// Replace template patern (eg: @ReceiptNo), if there is one, with a value from the model object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="text"></param>
        /// <param name="blnIsVoid"></param>
        /// <returns></returns>
        public static string ReplacePatterns(object obj, string text, bool blnIsVoid = false)
        {
            if (text == null)
            {
                return string.Empty;
            }
            bool blnIsQuantity = false;
            foreach (Match match in Regex.Matches(text, @"(?<!\w)@\w+"))
            {
                //A template patern found: Replace template patern with a value from the model object.
                try
                {
                    switch (match.Value)
                    {
                        case "@ItemQty":
                            blnIsQuantity = true;
                            break;
                        default:
                            break;
                    }
                    var propToMatch = match.Value.Replace("@", "");
                    string propType = string.Empty;
                    if (propToMatch != "Date" && propToMatch != "Time")
                    {
                        propType = obj.GetType().GetProperty(propToMatch).PropertyType.Name;
                        if (propType.StartsWith("Nullable"))
                        {
                            propType = Nullable.GetUnderlyingType(obj.GetType().GetProperty(propToMatch).PropertyType).Name;
                        }
                        string a = obj.GetType().GetProperty(propToMatch).GetValue(obj, null).ToString();
                        if (propToMatch == "ItemVatDesc" && (a == "" || a == null))
                            a = "0";
                        if (string.IsNullOrEmpty(a))
                        {
                            return string.Empty;
                        }
                        text = text.Replace(match.Value, a);
                    }
                    // format the numbers
                    if (propType == "Decimal" || propType == "Double" || propType == "Float")
                    {
                        if (blnIsQuantity)
                        {
                            if (blnIsVoid && text[0] != '-')
                            {
                                text = '-' + text;
                            }
                            var f = decimal.Parse(text);
                            text = string.Format("{0:0.##}", f);
                            blnIsQuantity = false;
                        }
                        else
                        {
                            var f = decimal.Parse(text);
                            text = string.Format("{0:0.00}", f);
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    return string.Empty;
                }
            }
            return text;
        }

        /// <summary>
        /// Align the given string to the given width according to column property.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="colWidth"></param>
        /// <param name="strData"></param>
        /// <returns></returns>
        public static string AlignmentOfSectionField(ReportSectionsColumnsModel column, int colWidth, string strData)
        {
            string strResult = string.Empty;
            if (!string.IsNullOrEmpty(column.AlignOption))
            {
                TextAlignEnum alignment = (TextAlignEnum)Enum.Parse(typeof(TextAlignEnum), column.AlignOption, true);
                switch (alignment)
                {
                    case TextAlignEnum.Middle:
                        strResult = CenteredString(strData, colWidth);
                        break;
                    case TextAlignEnum.Right:
                        strResult = strData.PadLeft(colWidth, ' ');
                        break;
                    case TextAlignEnum.Left:
                    default:
                        strResult = strData.PadRight(colWidth, ' ');
                        break;
                }
            }
            else
            {
                strResult = strData.PadRight(colWidth, ' ');
            }
            return strResult;
        }

        /// <summary>
        /// Centers the given string to the given width.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static string CenteredString(string s, int width)
        {
            if (s.Length >= width)
            {
                return s;
            }
            int leftPadding = (width - s.Length) / 2;
            int rightPadding = width - s.Length - leftPadding;
            return new string(' ', leftPadding) + s + new string(' ', rightPadding);
        }

        /// <summary>
        /// Returns a string enriched with printer escape chars
        /// </summary>
        /// <param name="printer"></param>
        /// <param name="inputStr"></param>
        /// <param name="isBold"></param>
        /// <param name="isItalic"></param>
        /// <param name="isUnderline"></param>
        /// <param name="isDoubleSize"></param>
        /// <returns></returns>
        public static string SetStringEscChars(Printer printer, string inputStr, bool isBold, bool isItalic, bool isUnderline, bool isDoubleSize)
        {
            string startEsc = string.Empty;
            string endEsc = string.Empty;
            if (isBold)
            {
                startEsc += GetEscapeChars(printer.BoldOn);
                endEsc = GetEscapeChars(printer.BoldOff) + endEsc;
            }
            if (isItalic)
            {
                startEsc += GetEscapeChars(printer.ItalicOn);
                endEsc = GetEscapeChars(printer.ItalicOff) + endEsc;
            }
            if (isUnderline)
            {
                startEsc += GetEscapeChars(printer.UnderLineOn);
                endEsc = GetEscapeChars(printer.UnderLineOff) + endEsc;
            }
            if (isDoubleSize)
            {
                startEsc += GetEscapeChars(printer.DoubleSizeOn);
                endEsc = GetEscapeChars(printer.DoubleSizeOff) + endEsc;
            }
            return startEsc + inputStr + endEsc;
        }

        /// <summary>
        /// Get escape chars.
        /// </summary>
        /// <param name="escSequence"></param>
        /// <returns></returns>
        public static string GetEscapeChars(ObservableCollection<CharWrapper> escSequence)
        {
            string result = string.Empty;
            bool isEscChar = false;
            string tempstr = string.Empty;
            foreach (var item in escSequence)
            {
                if ((char)item.Char == ']' && isEscChar)
                {
                    isEscChar = false;
                    int? charInt = Convert.ToInt32(tempstr);
                    result += (charInt != null) ? ((char)charInt).ToString() : string.Empty;
                    tempstr = string.Empty;
                }
                if (isEscChar)
                {
                    tempstr += (char)item.Char;
                }
                if ((char)item.Char == '[' && !isEscChar)
                {
                    isEscChar = true;
                }
                if (!isEscChar && ((char)item.Char != ']'))
                {
                    result += (char)item.Char;
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if error string has value
        /// </summary>
        /// <param name="errStr"></param>
        /// <returns></returns>
        public static bool CheckHasErrors(string errStr)
        {
            if (errStr == null)
                errStr = "";
            if (errStr.Length > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Convert string to memory stream
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// In case the invoice index is > 1, on OPOS/OPOS3/EpsonFiscal modes.
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="fiscalName"></param>
        /// <param name="PrintersTemplates"></param>
        /// <param name="availablePrinters"></param>
        /// <param name="printersEscList"></param>
        /// <param name="parentInstance"></param>
        /// <param name="objLockToUse"></param>
        /// <returns></returns>
        public static PrintResultModel PrintGenericReceipt(ReceiptModel receiptModel, string fiscalName, Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>> PrintersTemplates, List<KitchenPrinterModel> availablePrinters, List<Printer> printersEscList, FiscalManager parentInstance, object objLockToUse)
        {
            var logPath = GetConfigurationPath();
            var logger = NLogBuilder.ConfigureNLog(logPath).GetCurrentClassLogger();
            PrintResultModel printResult = new PrintResultModel();
            ReceiptModel currentReceiptData = receiptModel;
            string receiptPrinterName = string.Empty;
            string error = string.Empty;
            KitchenPrinterModel printerSettings;
            RollerTypeReportModel template;
            try
            {
                printResult.OrderNo = receiptModel.OrderId.ToString();
                printResult.ReceiptNo = receiptModel.ReceiptNo;
                printResult.ExtcerType = ExtcerTypesEnum.Generic;
                receiptModel.FiscalType = FiscalTypeEnum.Generic;
                if (receiptModel.IsVoid)
                {
                    receiptModel.IsVoid = true;
                    // get printer for void
                    printerSettings = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Void);
                    if (printerSettings == null)
                    {
                        logger.Error(ExtcerLogger.Log("No Void Printer found with Type=Void.", fiscalName));
                    }
                    // get receipt printer template
                    if (PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.Void).Value == null)
                    {
                        logger.Error(ExtcerLogger.Log("Generic void template not found", fiscalName));
                    }
                    template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Key == PrinterTypeEnum.Void).Value.FirstOrDefault().Value;
                    if (template == null)
                    {
                        logger.Info(ExtcerLogger.Log("Generic void template not found", fiscalName));
                    }
                    printResult.ReceiptType = PrintModeEnum.Void;
                }
                else
                {
                    receiptModel.IsVoid = false;
                    // get printer for receipt
                    printerSettings = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Receipt && f.SlotIndex == receiptModel.InvoiceIndex);
                    if (printerSettings == null)
                    {
                        logger.Error(ExtcerLogger.Log("No Printer found with Type=Receipt and  SlotIndex=" + (receiptModel.InvoiceIndex ?? "<null>"), fiscalName));
                        throw new CustomException(ExtcerErrorHelper.INVOICE_NOT_FOUND);
                    }
                    // get receipt printer template
                    if (PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Value.ReportName == printerSettings.TemplateShortName).Value == null)
                    {
                        logger.Error(ExtcerLogger.Log("No PrinterTemplate found with ReportName=" + (printerSettings.TemplateShortName ?? "<null>"), fiscalName));
                    }
                    template = PrintersTemplates.FirstOrDefault(f => f.Value.FirstOrDefault().Value.ReportName == printerSettings.TemplateShortName).Value.FirstOrDefault().Value;
                    if (template == null)
                    {
                        logger.Warn(ExtcerLogger.Log("Generic template not found", fiscalName));
                    }
                    printResult.InvoiceIndex = receiptModel.InvoiceIndex;
                    printResult.ReceiptType = PrintModeEnum.Receipt;
                }
                var allowPrint = true;
                if (printerSettings != null && printerSettings.PrintKitchenOnly == true)
                {
                    allowPrint = false;
                }
                printResult.ReceiptData = ProcessReceiptTemplate(template, receiptModel, printerSettings, availablePrinters, printersEscList, parentInstance, objLockToUse, allowPrint);
                printResult.Status = PrintStatusEnum.Printed;
            }
            catch (CustomException exception)
            {
                printResult.ErrorDescription = exception.Message;
                logger.Error(ExtcerLogger.logErr("Error printing generic receipt  #: " + receiptModel.ReceiptNo + " Message: ", exception, fiscalName));
                printResult.Status = PrintStatusEnum.Failed;
                error = "Error printing generic receipt: " + exception.Message + " StackTRace: " + exception.StackTrace;
            }
            catch (Exception exception)
            {
                printResult.ErrorDescription = ExtcerErrorHelper.GetErrorDescription(ExtcerErrorHelper.GENERAL_ERROR);
                logger.Error(ExtcerLogger.logErr("Error printing generic receipt  #: " + receiptModel.ReceiptNo + " Message: ", exception, fiscalName));
                printResult.Status = PrintStatusEnum.Failed;
                error = "Error printing generic receipt: " + exception.Message + " StackTRace: " + exception.StackTrace;
            }
            ArtcasLogger.LogArtCasXML(receiptModel, DateTime.Now, error, printResult.ReceiptData, ReceiptReceiveTypeEnum.WEB, printResult.Status, fiscalName, printResult.Id);
            return printResult;
        }

        /// <summary>
        /// Print generic report
        /// </summary>
        /// <param name="reportModelData"></param>
        /// <param name="strFiscalName"></param>
        /// <param name="availablePrinters"></param>
        /// <param name="printersEscList"></param>
        /// <param name="printersTemplates"></param>
        /// <param name="parentInstance"></param>
        /// <param name="objLockToUse"></param>
        /// <param name="objReprintData"></param>
        /// <param name="SetFooter"></param>
        /// <returns></returns>
        public static PrintResultModel PrintReports(ReportsModel reportModelData, string strFiscalName, List<KitchenPrinterModel> availablePrinters, List<Printer> printersEscList, Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>> printersTemplates, FiscalManager parentInstance, object objLockToUse, object objReprintData = null, Func<string, List<string>> SetFooter = null)
        {
            var logPath = GetConfigurationPath();
            var logger = NLogBuilder.ConfigureNLog(logPath).GetCurrentClassLogger();
            logger.Info("Preparing report...");
            PrintResultModel printResult = new PrintResultModel();
            // set status to 'Failed' before error checking
            printResult.Status = PrintStatusEnum.Failed;
            Printer printerEscSet;
            RollerTypeReportModel template;
            KitchenPrinterModel printerToPrint;
            string error = string.Empty;
            ReportsModel currentReportData = reportModelData;
            try
            {
                // get report printer with the proper slot
                printerToPrint = availablePrinters.FirstOrDefault(f => f.PrinterType == PrinterTypeEnum.Report && f.SlotIndex == reportModelData.ReportIndex);
                if (printerToPrint == null)
                {
                    logger.Error(ExtcerLogger.Log("No Printer found with Type=Report and  SlotIndex=" + (reportModelData.ReportIndex ?? "<null>"), strFiscalName));
                    throw new CustomException(ExtcerErrorHelper.REPORT_NOT_FOUND);
                }
                // get the report printer with the proper escape set
                printerEscSet = printersEscList.Where(f => f.Name == printerToPrint.EscapeCharsTemplate).FirstOrDefault();
                if (printerToPrint.IsCrystalReportsPrintout == null || printerToPrint.IsCrystalReportsPrintout == false)
                {
                    // get receipt printer template
                    if (printersTemplates.FirstOrDefault(f => f.Key == printerToPrint.TemplateShortName && f.Value.FirstOrDefault().Value.ReportType == (int)PrinterTypeEnum.Report).Value == null)
                    {
                        logger.Error(ExtcerLogger.Log("No Report Template found with Type=Report and Name=" + (printerToPrint.TemplateShortName ?? "<null>"), strFiscalName));
                    }
                    template = printersTemplates.FirstOrDefault(f => f.Key == printerToPrint.TemplateShortName && f.Value.FirstOrDefault().Value.ReportType == (int)PrinterTypeEnum.Report).Value.FirstOrDefault().Value;
                    if (template == null)
                    {
                        logger.Info(ExtcerLogger.Log("Report template not found with Type=Report and Name=" + (printerToPrint.TemplateShortName ?? "<null>"), strFiscalName));
                    }
                    printResult.ReportIndex = reportModelData.ReportIndex;
                    printResult.ReceiptType = PrintModeEnum.Report;
                    // set status to 'Printed' before print
                    printResult.Status = PrintStatusEnum.Printed;
                    printResult.ExtcerType = ExtcerTypesEnum.Generic;
                    printResult.ReceiptData = ProcessReportsTemplate(template, reportModelData, printerToPrint, printerEscSet, parentInstance, objLockToUse);
                }
                else
                {
                    printResult.ReportIndex = reportModelData.ReportIndex;
                    printResult.ReceiptType = PrintModeEnum.Report;
                    // set status to 'Printed' before print
                    printResult.Status = PrintStatusEnum.Printed;
                    printResult.ExtcerType = ExtcerTypesEnum.Generic;
                    //TODO GEO
                    //PrintCrystalReport(printerToPrint.Template, printerToPrint, string.Empty, null, null, reportModelData.PosInfoId, SetFooter);
                }
                printResult.Status = PrintStatusEnum.Printed;
            }
            catch (CustomException exception)
            {
                printResult.ErrorDescription = exception.Message;
                logger.Error(ExtcerLogger.logErr("Error printing report (1): ", exception, strFiscalName));
                printResult.Status = PrintStatusEnum.Failed;
                error = "Error printing report.\r\n" + "Error:\r\n" + exception.Message + "\r\n" + "StackTrace:\r\n" + exception.StackTrace;
            }
            catch (Exception exception)
            {
                printResult.ErrorDescription = ExtcerErrorHelper.GetErrorDescription(ExtcerErrorHelper.GENERAL_ERROR);
                logger.Error(ExtcerLogger.logErr("Error printing report (2): ", exception, strFiscalName));
                printResult.Status = PrintStatusEnum.Failed;
                error = "Error printing report.\r\n" + "Error:\r\n" + exception.Message + "\r\n" + "StackTrace:\r\n" + exception.StackTrace;
            }
            return printResult;
        }

        /// <summary>
        /// Get receipt lines according to template
        /// </summary>
        /// <param name="receipt"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static List<string> GetReceiptLines(ReceiptModel receipt, RollerTypeReportModel template)
        {
            List<string> receiptLines = new List<string>();
            List<ReportSectionsModel> templateSections = template.Sections;
            // Create receipt lines from sections
            foreach (ReportSectionsModel section in templateSections.Where(s => s.SectionType != (int)SectionTypeEnums.Extras))
            {
                if (section != null)
                {
                    // Process receipt details
                    if (section.SectionType == (int)SectionTypeEnums.Details)
                    {
                        foreach (ReceiptItemsModel receiptItem in receipt.Details)
                        {
                            // Set negative price if receipt detail is returned item
                            if (receiptItem.IsChangeItem)
                            {
                                receiptItem.ItemPrice = Math.Abs(receiptItem.ItemPrice) * (-1);
                            }
                            // Create receipt detail
                            receiptLines.AddRange(CreateReceiptLine(section.SectionRows, receiptItem));
                            // Process receipt detail extras
                            if (receiptItem.Extras.Count > 0)
                            {
                                ReportSectionsModel extrasSection = template.Sections.Where(s => s.SectionType == (int)SectionTypeEnums.Extras).FirstOrDefault();
                                if (extrasSection != null)
                                {
                                    foreach (ReceiptExtrasModel extra in receiptItem.Extras)
                                    {
                                        // Set negative price if receipt detail extra is returned item
                                        if (extra.IsChangeItem && extra.ItemPrice != null)
                                        {
                                            extra.ItemPrice = Math.Abs(extra.ItemPrice ?? 0) * (-1);
                                        }
                                        // Create receipt detail extra
                                        receiptLines.AddRange(CreateReceiptLine(extrasSection.SectionRows, extra, 0.0, true));
                                        // Process receipt detail extra discount
                                        if (extra.ItemDiscount != null && extra.ItemDiscount > 0)
                                        {
                                            ReportSectionsModel itemDiscountSection = template.Sections.FirstOrDefault(s => s.SectionType == (int)SectionTypeEnums.DiscountDetails);
                                            if (itemDiscountSection != null)
                                            {
                                                // Set negative total discount
                                                extra.ItemDiscount = Math.Abs(extra.ItemDiscount ?? 0) * (-1);
                                                // Create receipt total discount
                                                receiptLines.AddRange(CreateReceiptLine(itemDiscountSection.SectionRows, extra));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // Process receipt total discount
                        if (receipt.TotalDiscount != null && receipt.TotalDiscount > 0)
                        {
                            ReportSectionsModel totalDiscountSection = templateSections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.Discount);
                            if (totalDiscountSection != null)
                            {
                                // Set negative total discount
                                receipt.TotalDiscount = Math.Abs(receipt.TotalDiscount ?? 0) * (-1);
                                // Create receipt total discount
                                receiptLines.AddRange(CreateReceiptLine(totalDiscountSection.SectionRows, receipt));
                            }
                        }
                    }
                    // Process receipt related orders and invoices
                    else if (section.SectionType == (int)SectionTypeEnums.RelatedReceipts)
                    {
                        if (receipt.RelatedReceipts != null && receipt.RelatedReceipts.Count > 0)
                        {
                            // Create receipt related order or invoice
                            receiptLines.AddRange(CreateReceiptLine(section.SectionRows, receipt.RelatedReceipts, template.MaxWidth));
                        }
                    }
                    // Process receipt sale types
                    else if (section.SectionType == (int)SectionTypeEnums.SalesTypeSection)
                    {
                        foreach (SalesTypeDescriptionsList saleType in receipt.SalesTypeDescriptions)
                        {
                            // Create receipt sale type
                            receiptLines.AddRange(CreateReceiptLine(section.SectionRows, saleType));
                        }
                    }
                    // Process receipt customer
                    else if (section.SectionType == (int)SectionTypeEnums.Customer)
                    {
                        if (!string.IsNullOrEmpty(receipt.RoomNo))
                        {
                            // Create receipt customer
                            receiptLines.AddRange(CreateReceiptLine(section.SectionRows, receipt));
                        }
                    }
                    // Process receipt payments
                    else if (section.SectionType == (int)SectionTypeEnums.PaymentMethods)
                    {
                        foreach (PaymentTypeModel payment in receipt.PaymentsList)
                        {
                            // Create receipt payment
                            receiptLines.AddRange(CreateReceiptLine(section.SectionRows, payment));
                        }
                    }
                    // Process receipt credit transactions
                    else if (section.SectionType == (int)SectionTypeEnums.CreditTransactions)
                    {
                        foreach (CreditTransaction creditTransaction in receipt.CreditTransactions)
                        {
                            // Create receipt credit transaction
                            receiptLines.AddRange(CreateReceiptLine(section.SectionRows, creditTransaction));
                        }
                    }
                    // Process receipt vat analysis
                    else if (section.SectionType == (int)SectionTypeEnums.VatAnalysis)
                    {
                        // Get vat analysis for receipt details
                        IEnumerable<Vat> receiptDetailsVat = receipt.Details.Select(d => new Vat
                        {
                            VatDesc = d.ItemVatDesc,
                            VatRate = d.ItemVatRate,
                            VatAmount = d.ItemVatValue,
                            Net = d.ItemNet,
                            Gross = (d.ItemNet + d.ItemVatValue)
                        }).AsEnumerable();
                        // Get vat analysis for receipt extras
                        IEnumerable<Vat> receiptExtrasVat = receipt.Details.SelectMany(d => d.Extras).Select(e => new Vat
                        {
                            VatDesc = e.ItemVatDesc,
                            VatRate = e.ItemVatRate,
                            VatAmount = e.ItemVatValue,
                            Net = e.ItemNet,
                            Gross = (e.ItemNet + e.ItemVatValue)
                        }).AsEnumerable();
                        // Get total vat analysis
                        IEnumerable<Vat> totalVat = receiptDetailsVat.Union(receiptExtrasVat);
                        // Group total vat analysis by vat rate
                        IEnumerable<Vat> totalVatGrouped = totalVat.GroupBy(v => v.VatRate).Select(v => new Vat
                        {
                            VatDesc = v.FirstOrDefault().VatDesc,
                            VatRate = v.Key.Value,
                            VatAmount = v.Sum(s => s.VatAmount),
                            Net = v.Sum(s => s.Net),
                            Gross = v.Sum(s => s.Gross)
                        });
                        foreach (Vat vat in totalVatGrouped)
                        {
                            // Create receipt vat
                            receiptLines.AddRange(CreateReceiptLine(section.SectionRows, vat));
                        }
                    }
                    // Process remaining sections except discounts
                    else if (section.SectionType != (int)SectionTypeEnums.Discount && section.SectionType != (int)SectionTypeEnums.DiscountDetails)
                    {
                        // Create receipt other section
                        receiptLines.AddRange(CreateReceiptLine(section.SectionRows, receipt));
                    }
                }
            }
            return receiptLines;
        }

        /// <summary>
        /// Create receipt line
        /// </summary>
        /// <param name="sectionRows"></param>
        /// <param name="objectItem"></param>
        /// <param name="fieldMaxWidth"></param>
        /// <param name="isExtra"></param>
        /// <param name="isVoid"></param>
        /// <param name="ignoreItemRegion"></param>
        /// <returns></returns>
        private static List<string> CreateReceiptLine(List<ReportSectionsRowsModel> sectionRows, object objectItem, double fieldMaxWidth = 0.0, bool isExtra = false, bool isVoid = false, bool ignoreItemRegion = false)
        {
            List<string> receiptLines = new List<string>();
            // Create receipt lines from section
            foreach (ReportSectionsRowsModel line in sectionRows)
            {
                if (!ignoreItemRegion || (ignoreItemRegion && line.SectionColumns.Count(f => f.ColumnText == "@ItemRegion") == 0))
                {
                    string lineString = string.Empty;
                    // Create receipt columns from line
                    foreach (var column in line.SectionColumns)
                    {
                        string temporaryString = string.Empty;
                        string columnVariable = column.ColumnText ?? "";
                        int columnWidth = Convert.ToInt32(column.Width);
                        // Replace column variable with respective object data
                        string data = ReplacePatterns(objectItem, columnVariable, isVoid);
                        switch (columnVariable)
                        {
                            // Process item description
                            case "@ItemDescr":
                                {
                                    string returnItemTemporaryString = null;
                                    string lineStringClean = lineString.Replace(" ", "");
                                    if (lineStringClean == "-1x")
                                    {
                                        returnItemTemporaryString = "ΑΛΛΑΓΗ";
                                    }
                                    lineStringClean = lineStringClean.Replace("\t", " ");
                                    lineStringClean = new string(lineStringClean.Where(c => !char.IsPunctuation(c)).ToArray());
                                    if (returnItemTemporaryString != null)
                                    {
                                        lineStringClean = lineStringClean.Replace("x", "  x ");
                                        lineString = lineStringClean + " " + returnItemTemporaryString + " ";
                                    }
                                    if (lineStringClean == "0x")
                                    {
                                        lineString = new string(' ', lineString.Length);
                                    }
                                }
                                break;
                            // Process total net vat 1
                            case "@TotalNetVat1":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null)
                                    {
                                        data = Convert.ToString(receipt.TotalNetVat1);
                                    }
                                }
                                break;
                            // Process total net vat 2
                            case "@TotalNetVat2":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null)
                                    {
                                        data = Convert.ToString(receipt.TotalNetVat2);
                                    }
                                }
                                break;
                            // Process total net vat 3
                            case "@TotalNetVat3":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null)
                                    {
                                        data = Convert.ToString(receipt.TotalNetVat3);
                                    }
                                }
                                break;
                            // Process total net vat 4
                            case "@TotalNetVat4":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null)
                                    {
                                        data = Convert.ToString(receipt.TotalNetVat4);
                                    }
                                }
                                break;
                            // Process total net vat 5
                            case "@TotalNetVat5":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null)
                                    {
                                        data = Convert.ToString(receipt.TotalNetVat5);
                                    }
                                }
                                break;
                            // Process total vat 1
                            case "@Vat1":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null)
                                    {
                                        data = Convert.ToString(receipt.TotalVat1);
                                    }
                                }
                                break;
                            // Process total vat 2
                            case "@Vat2":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null)
                                    {
                                        data = Convert.ToString(receipt.TotalVat2);
                                    }
                                }
                                break;
                            // Process total vat 3
                            case "@Vat3":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null)
                                    {
                                        data = Convert.ToString(receipt.TotalVat3);
                                    }
                                }
                                break;
                            // Process total vat 4
                            case "@Vat4":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null)
                                    {
                                        data = Convert.ToString(receipt.TotalVat4);
                                    }
                                }
                                break;
                            // Process total vat 5
                            case "@Vat5":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null)
                                    {
                                        data = Convert.ToString(receipt.TotalVat5);
                                    }
                                }
                                break;
                            // Process invoice customer name
                            case "@InvoiceCustomerName":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null)
                                    {
                                        data = receipt.CustomerName;
                                    }
                                }
                                break;
                            // Process customer name
                            case "@CustomerName":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null)
                                    {
                                        IEnumerable<string> customerData = receipt.PaymentsList.Where(p => p.Guest != null && p.Guest.LastName != null && p.Guest.LastName.Length > 0).Select(p => p.Guest.LastName);
                                        string customerNameTemporaryString = string.Empty;
                                        if (customerData != null && customerData.Count() > 0)
                                        {
                                            customerNameTemporaryString = customerData.Aggregate((current, next) => current + ", " + next);
                                        }
                                        data = customerNameTemporaryString;
                                    }
                                }
                                break;
                            // Process room number
                            case "@RoomNo":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null)
                                    {
                                        IEnumerable<string> roomData = receipt.PaymentsList.Where(p => p.Guest != null && p.Guest.Room != null && p.Guest.Room.Length > 0).Select(p => p.Guest.Room);
                                        string roomNumberTemporaryString = string.Empty;
                                        if (roomData != null && roomData.Count() > 0)
                                        {
                                            roomNumberTemporaryString = roomData.Aggregate((current, next) => current + ", " + next);
                                        }
                                        data = roomNumberTemporaryString;
                                    }
                                }
                                break;
                            // Process system date
                            case "@SystemDate":
                                {
                                    DateTime date = DateTime.Now;
                                    if (objectItem.GetType() == typeof(ZReportModel))
                                    {
                                        ZReportModel zReport = objectItem as ZReportModel;
                                        if (zReport != null)
                                        {
                                            date = zReport.dtDateTime;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(column.FormatOption))
                                    {
                                        data = string.Format(column.FormatOption, date).ToUpper();
                                    }
                                    else
                                    {
                                        data = string.Format("{0:dd/MM/yyyy}", date).ToUpper();
                                    }
                                }
                                break;
                            // Process system time
                            case "@SystemTime":
                                {
                                    DateTime date = DateTime.Now;
                                    if (objectItem.GetType() == typeof(ZReportModel))
                                    {
                                        ZReportModel zReport = objectItem as ZReportModel;
                                        if (zReport != null)
                                        {
                                            date = zReport.dtDateTime;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(column.FormatOption))
                                    {
                                        data = string.Format(column.FormatOption, date).ToUpper();
                                    }
                                    else
                                    {
                                        data = string.Format("{0:HH:mm}", date).ToUpper();
                                    }
                                }
                                break;
                            // Process day
                            case "@Day":
                                {
                                    if (!string.IsNullOrEmpty(column.FormatOption))
                                    {
                                        data = string.Format(column.FormatOption, data).ToUpper();
                                    }
                                    else
                                    {
                                        data = string.Format("{0:dd/MM/yyyy}", data).ToUpper();
                                    }
                                }
                                break;
                            // Process item discount
                            case "@ItemDiscount":
                                {
                                    if (objectItem.GetType() == typeof(ReceiptItemsModel))
                                    {
                                        ReceiptItemsModel receiptItem = objectItem as ReceiptItemsModel;
                                        if (receiptItem.ItemDiscount == null || receiptItem.ItemDiscount == 0)
                                        {
                                        }
                                    }
                                    else if (string.IsNullOrEmpty(data) || Convert.ToDecimal(data) == 0.0m)
                                    {
                                    }
                                    else
                                    {
                                        data = "-" + data;
                                    }
                                }
                                break;
                            // Process item gross
                            case "@ItemGross":
                                {
                                    if (isExtra)
                                    {
                                        if (data == null || double.Parse(data) == 0.0)
                                        {
                                        }
                                    }
                                }
                                break;
                            // Process table total
                            case "@TableTotal":
                                {
                                    if (string.IsNullOrEmpty(data))
                                    {
                                    }
                                }
                                break;
                            // Process item total
                            case "@ItemTotal":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null && receipt.CreditTransactions.Count() > 0)
                                    {
                                        data = (receipt.CreditTransactions.FirstOrDefault().Amount ?? 0).ToString("0.00");
                                    }
                                }
                                break;
                            // Process item total
                            case "@VatSubTotals":
                                {
                                    ReceiptModel receipt = objectItem as ReceiptModel;
                                    if (receipt != null)
                                    {
                                        StringBuilder stringBuilder = new StringBuilder(lineString);
                                        foreach (CalculateVatPrice vat in receipt.CalculateVatPrice)
                                        {
                                            var vatValue = "VAT " + vat.VatRate + "%" + "                       " + vat.VatPrice.Value.ToString("0.00").PadLeft(5) + " €";
                                            stringBuilder.Append(vatValue);
                                            stringBuilder.Append("\r\n");
                                            var net = vat.Total.Value - vat.VatPrice.Value;
                                            var netValue = "NET " + vat.VatRate + "%" + "                       " + net.ToString("0.00").PadLeft(5) + " €";
                                            stringBuilder.Append(netValue);
                                            stringBuilder.Append("\r\n");
                                        }
                                        lineString = stringBuilder.ToString();
                                    }
                                }
                                break;
                            case "@ItemCustomRemark":
                                {
                                    if (!string.IsNullOrEmpty(data))
                                    {
                                        temporaryString = " " + data.TrimEnd();
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        // Modify column according to width
                        if (data.Length > 0)
                        {
                            if (columnWidth == 0)
                            {
                                temporaryString = temporaryString + data.Trim();
                            }
                            else if (columnWidth > data.Length)
                            {
                                temporaryString = temporaryString + AlignmentOfSectionField(column, columnWidth, data);
                            }
                            else
                            {
                                temporaryString += data.Substring(0, columnWidth);
                            }
                        }
                        // Add line
                        if (line.SectionColumns.Select(c => c.ColumnText).Contains("@ItemRegion") && temporaryString.Length > 0)
                        {
                            temporaryString += "\n----------------------------------------";
                        }
                        // Process related receipts
                        if (columnVariable == "@RelatedReceipts")
                        {
                            List<string> relatedReceipts = objectItem as List<string>;
                            if (columnWidth == 0)
                            {
                                int fieldMaxWidthInteger = (int)fieldMaxWidth;
                                StringBuilder stringBuilder = new StringBuilder(lineString);
                                bool firstPass = true;
                                foreach (string relatedReceipt in relatedReceipts)
                                {
                                    string relatedReceiptClear = relatedReceipt.Trim();
                                    if (!firstPass)
                                    {
                                        if ((stringBuilder.Length % fieldMaxWidthInteger) + relatedReceiptClear.Length >= fieldMaxWidthInteger)
                                        {
                                            stringBuilder.Append("\r\n");
                                        }
                                        else if (stringBuilder.Length > 0)
                                        {
                                            stringBuilder.Append(", ");
                                        }
                                    }
                                    stringBuilder.Append(relatedReceiptClear);
                                    firstPass = false;
                                }
                                lineString = stringBuilder.ToString();
                            }
                            else
                            {
                                foreach (string relatedReceipt in relatedReceipts)
                                {
                                    string relatedReceiptClear = relatedReceipt.Trim();
                                    string strTempLine = AlignmentOfSectionField(column, columnWidth, relatedReceiptClear);
                                }
                            }
                        }
                        lineString = lineString + temporaryString;
                    }
                    receiptLines.Add(lineString);
                }
            }
            return receiptLines;
        }

        /// <summary>
        /// Process receipt template.
        /// </summary>
        /// <param name="repTemplate"></param>
        /// <param name="currentReceipt"></param>
        /// <param name="printerSettings"></param>
        /// <param name="availablePrinters"></param>
        /// <param name="PrintersEscList"></param>
        /// <param name="parentInstance"></param>
        /// <param name="objLockToUse"></param>
        /// <param name="allowPrint"></param>
        /// <returns></returns>
        private static List<string> ProcessReceiptTemplate(RollerTypeReportModel repTemplate, ReceiptModel currentReceipt, KitchenPrinterModel printerSettings, List<KitchenPrinterModel> availablePrinters, List<Printer> PrintersEscList, FiscalManager parentInstance, object objLockToUse, bool allowPrint = false)
        {
            var logPath = GetConfigurationPath();
            var logger = NLogBuilder.ConfigureNLog(logPath).GetCurrentClassLogger();
            logger.Info("Processing Receipt Template...");
            List<string> result = new List<string>();
            var receiptPrinter = availablePrinters.Where(f => f.Name == printerSettings.Name).FirstOrDefault();
            Printer printerEscSet = PrintersEscList.Where(f => f.Name == receiptPrinter.EscapeCharsTemplate).FirstOrDefault();
            var sections = repTemplate.Sections;
            foreach (var section in sections.Where(f => f.SectionType != (int)SectionTypeEnums.Extras))
            {
                if (section != null)
                {
                    if (section.SectionType == (int)SectionTypeEnums.Details)
                    {
                        foreach (var items in currentReceipt.Details)
                        {
                            if (items.IsChangeItem)
                            {
                                var price_original = items.ItemPrice;
                                items.ItemPrice = Math.Abs(items.ItemPrice);
                                result.AddRange(ProcessSection(section.SectionRows, items, printerEscSet));
                                // restore the original amount as positive
                                items.ItemPrice = price_original;
                            }
                            else
                            {
                                result.AddRange(ProcessSection(section.SectionRows, items, printerEscSet));
                            }
                            if (items.ItemDiscount != null && items.ItemDiscount > 0)
                            {
                                var discountDetailsSection = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.DiscountDetails);
                                if (discountDetailsSection != null)
                                {
                                    items.ItemDiscount = items.ItemDiscount * (-1);
                                    result.AddRange(ProcessSection(discountDetailsSection.SectionRows, items, printerEscSet));
                                    // restore the original amount as positive
                                    items.ItemDiscount = items.ItemDiscount * (-1);
                                }
                            }
                            if (items.Extras.Count > 0)
                            {
                                var extrasDoc = repTemplate.Sections.Where(w => w.SectionType == (int)SectionTypeEnums.Extras).FirstOrDefault();
                                if (extrasDoc != null)
                                {
                                    foreach (var extra in items.Extras)
                                    {
                                        extra.ItemPrice = extra.ItemPrice ?? 0;
                                        if (extra.IsChangeItem)
                                        {
                                            if (extra.ItemPrice != null)
                                            {
                                                extra.ItemPrice = extra.ItemPrice * -1;
                                                result.AddRange(ProcessSection(extrasDoc.SectionRows, extra, printerEscSet));
                                                // restore the original amount as positive
                                                extra.ItemPrice = extra.ItemPrice * -1;
                                            }
                                        }
                                        else
                                        {
                                            result.AddRange(ProcessSection(extrasDoc.SectionRows, extra, printerEscSet));
                                        }
                                        if (extra.ItemDiscount != null && extra.ItemDiscount > 0)
                                        {
                                            var discountDetailsSection = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.DiscountDetails);
                                            if (discountDetailsSection != null)
                                            {
                                                extra.ItemDiscount = extra.ItemDiscount * (-1);
                                                result.AddRange(ProcessSection(discountDetailsSection.SectionRows, extra, printerEscSet));
                                                // restore the original amount as positive
                                                extra.ItemDiscount = extra.ItemDiscount * (-1);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (currentReceipt.TotalDiscount != null && currentReceipt.TotalDiscount > 0)
                        {
                            var totalDiscountSection = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.Discount);
                            if (totalDiscountSection != null)
                            {
                                currentReceipt.TotalDiscount = currentReceipt.TotalDiscount * (-1);
                                result.AddRange(ProcessSection(totalDiscountSection.SectionRows, currentReceipt, printerEscSet));
                                // restore the original amount as positive
                                currentReceipt.TotalDiscount = currentReceipt.TotalDiscount * (-1);
                            }
                        }
                    }
                    else if (section.SectionType == (int)SectionTypeEnums.Customer)
                    {
                        if (!string.IsNullOrEmpty(currentReceipt.RoomNo))
                        {
                            result.AddRange(ProcessSection(section.SectionRows, currentReceipt, printerEscSet));
                        }
                    }
                    else if (section.SectionType == (int)SectionTypeEnums.Footer)
                    {
                        result.AddRange(ProcessSection(section.SectionRows, currentReceipt, printerEscSet));
                    }
                    else if (section.SectionType != (int)SectionTypeEnums.Discount && section.SectionType != (int)SectionTypeEnums.DiscountDetails)
                    {
                        result.AddRange(ProcessSection(section.SectionRows, currentReceipt, printerEscSet));
                    }
                }
            }
            if (allowPrint)
            {
                Task task = Task.Run(() => SendTextToPrinter(result, printerEscSet, printerSettings, objLockToUse, parentInstance));
            }
            return result;
        }

        /// <summary>
        /// Process report template.
        /// </summary>
        /// <param name="reportTemplate"></param>
        /// <param name="currentReportData"></param>
        /// <param name="printerSettings"></param>
        /// <param name="printerEscSet"></param>
        /// <param name="parentInstance"></param>
        /// <param name="objLockToUse"></param>
        /// <returns></returns>
        private static List<string> ProcessReportsTemplate(RollerTypeReportModel reportTemplate, ReportsModel currentReportData, KitchenPrinterModel printerSettings, Printer printerEscSet, FiscalManager parentInstance, object objLockToUse)
        {
            var logPath = GetConfigurationPath();
            var logger = NLogBuilder.ConfigureNLog(logPath).GetCurrentClassLogger();
            List<string> result = new List<string>();
            var sections = reportTemplate.Sections;
            var availableSections = sections.Where(f => f.SectionType != (int)SectionTypeEnums.ReportsPageHeader && f.SectionType != (int)SectionTypeEnums.ReportsPageFooter && f.SectionType != (int)SectionTypeEnums.ReportsGroupHeader && f.SectionType != (int)SectionTypeEnums.ReportsGroupFooter);
            foreach (var section in availableSections)
            {
                if (section == null)
                {
                    continue;
                }
                switch (section.SectionType)
                {
                    case (int)SectionTypeEnums.Details:
                        // Reports - process groups
                        foreach (var singleGroup in currentReportData.Groups)
                        {
                            // Reports - process group header
                            List<ReportSectionsRowsModel> headerRow = new List<ReportSectionsRowsModel>();
                            // this is the first item of the group -> take info from template.ReportsGroupHeader
                            var templateSection_ReportGroupHeader = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.ReportsGroupHeader);
                            if (templateSection_ReportGroupHeader != null)
                            {
                                headerRow = templateSection_ReportGroupHeader.SectionRows;
                                // add the report group header
                                result.AddRange(ProcessSection(headerRow, singleGroup.GroupHeader, printerEscSet));
                            }
                            // Reports - process details
                            foreach (var detailsLine in singleGroup.Details)
                            {
                                // process the details line - add the line
                                result.AddRange(ProcessSection(section.SectionRows, detailsLine, printerEscSet, false, false, detailsLine.ItemIsExtra));
                            }
                            // Reports - process group footer
                            List<ReportSectionsRowsModel> footerRow = new List<ReportSectionsRowsModel>();
                            // if this is the last item -> take info from template.ReportsGroupFooter
                            var templateSection_ReportGroupFooter = sections.FirstOrDefault(f => f.SectionType == (int)SectionTypeEnums.ReportsGroupFooter);
                            if (templateSection_ReportGroupFooter != null)
                            {
                                footerRow = templateSection_ReportGroupFooter.SectionRows;
                                // add the report group footer
                                result.AddRange(ProcessSection(footerRow, singleGroup.GroupFooter, printerEscSet));
                            }
                        }
                        break;
                    case (int)SectionTypeEnums.Header:
                        // Reports - process report header
                        result.AddRange(ProcessSection(section.SectionRows, currentReportData.ReportHeader, printerEscSet));
                        break;
                    case (int)SectionTypeEnums.Footer:
                        // Reports - process report footer
                        result.AddRange(ProcessSection(section.SectionRows, currentReportData.ReportFooter, printerEscSet));
                        break;
                    default:
                        result.AddRange(ProcessSection(section.SectionRows, currentReportData.ReportHeader, printerEscSet));
                        break;
                }
            }
            logger.Info("About to print report: " + currentReportData.ReportHeader.ReportTitle);
            Task task = Task.Run(() => SendTextToPrinter(result, printerEscSet, printerSettings, objLockToUse, parentInstance));
            return result;
        }

        /// <summary>
        /// Process the given section and create the string to print.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="obj"></param>
        /// <param name="printer"></param>
        /// <param name="ignoreRegionInfo"></param>
        /// <param name="isOposComments"></param>
        /// <param name="indentExtra"></param>
        /// <returns></returns>
        private static List<string> ProcessSection(List<ReportSectionsRowsModel> section, object obj, Printer printer, bool ignoreRegionInfo = false, bool isOposComments = false, bool? indentExtra = false)
        {
            List<string> result = new List<string>();
            // loop through lines of the section
            foreach (var line in section)
            {
                if (!ignoreRegionInfo || (ignoreRegionInfo && line.SectionColumns.Count(f => f.ColumnText == "@ItemRegion") == 0))
                {
                    string str = string.Empty;
                    bool add = true;
                    // loop through columns of the current line
                    foreach (var col in line.SectionColumns)
                    {
                        var tempStr = string.Empty;
                        string colText = col.ColumnText != null ? col.ColumnText : "";
                        int colWidth = Convert.ToInt32(col.Width);
                        string data = ReplacePatterns(obj, colText);
                        switch (colText)
                        {
                            case "@ItemDiscount":
                                {
                                    if (String.IsNullOrEmpty(data) || Convert.ToDecimal(data) == 0.0m)
                                    {
                                        add = false;
                                    }
                                }
                                break;
                            case "@InvoiceCustomerName":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        data = model.CustomerName;
                                    }
                                }
                                break;
                            case "@CustomerName":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        var guestdata = model.PaymentsList.Where(f => f.Guest != null && f.Guest.LastName != null && f.Guest.LastName.Length > 0).Select(ff => ff.Guest.LastName);
                                        var res = string.Empty;
                                        if (guestdata != null && guestdata.Count() > 0)
                                        {
                                            res = guestdata.Aggregate((current, next) => current + ", " + next);
                                        }
                                        data = res;
                                    }
                                }
                                break;
                            case "@RoomNo":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        var guestdata = model.PaymentsList.Where(f => f.Guest != null && f.Guest.Room != null && f.Guest.Room.Length > 0).Select(ff => ff.Guest.Room);
                                        var res = string.Empty;
                                        if (guestdata != null && guestdata.Count() > 0)
                                        {
                                            res = guestdata.Aggregate((current, next) => current + ", " + next);
                                        }
                                        data = res;
                                    }
                                }
                                break;
                            case "@SystemDate":
                                {
                                    var date = DateTime.Now;
                                    if (obj.GetType() == typeof(ZReportModel))
                                    {
                                        var d = obj as ZReportModel;
                                        if (d != null)
                                        {
                                            date = d.Day;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(col.FormatOption))
                                    {
                                        data = string.Format(col.FormatOption, date).ToUpper();
                                    }
                                    else
                                    {
                                        // set default format
                                        data = string.Format("{0:dd/MM/yyyy}", date).ToUpper();
                                    }
                                }
                                break;
                            case "@SystemTime":
                                {
                                    var date = DateTime.Now;
                                    if (obj.GetType() == typeof(ZReportModel))
                                    {
                                        var d = obj as ZReportModel;
                                        if (d != null)
                                        {
                                            date = d.Day;
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(col.FormatOption))
                                    {
                                        data = string.Format(col.FormatOption, date).ToUpper();
                                    }
                                    else
                                    {
                                        // set default format
                                        data = string.Format("{0:HH:mm}", date).ToUpper();
                                    }
                                }
                                break;
                            case "@Day":
                                {
                                    if (!string.IsNullOrEmpty(col.FormatOption))
                                    {
                                        data = string.Format(col.FormatOption, data).ToUpper();
                                    }
                                    else
                                    {
                                        // set default format
                                        data = string.Format("{0:dd/MM/yyyy}", data).ToUpper();
                                    }
                                }
                                break;
                            case "@TotalNetVat1":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        var tmp = model.Details.Where(f => f.ItemVatRate == 1).Select(ff => ff.ItemNet).ToList();
                                        if (tmp != null && tmp.Count > 0)
                                        {
                                            data = tmp.Aggregate((a, b) => a + b).ToString().Trim();
                                        }
                                        else
                                        {
                                            data = "0.00";
                                        }
                                    }
                                }
                                break;
                            case "@TotalNetVat3":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        var tmp = model.Details.Where(f => f.ItemVatRate == 3).Select(ff => ff.ItemNet).ToList();
                                        if (tmp != null && tmp.Count > 0)
                                        {
                                            data = tmp.Aggregate((a, b) => a + b).ToString().Trim();
                                        }
                                        else
                                        {
                                            data = "0.00";
                                        }
                                    }
                                }
                                break;
                            case "@TotalNetVat4":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        var tmp = model.Details.Where(f => f.ItemVatRate == 4).Select(ff => ff.ItemNet).ToList();
                                        if (tmp != null && tmp.Count > 0)
                                        {
                                            data = tmp.Aggregate((a, b) => a + b).ToString().Trim();
                                        }
                                        else
                                        {
                                            data = "0.00";
                                        }
                                    }
                                }
                                break;
                            case "@TotalNetVat5":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        var tmp = model.Details.Where(f => f.ItemVatRate == 5).Select(ff => ff.ItemNet).ToList();
                                        if (tmp != null && tmp.Count > 0)
                                        {
                                            data = tmp.Aggregate((a, b) => a + b).ToString().Trim();
                                        }
                                        else
                                        {
                                            data = "0.00";
                                        }
                                    }
                                }
                                break;
                            case "@Vat1":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        var tmp = model.Details.Where(f => f.ItemVatRate == 1).Select(ff => ff.ItemVatValue).ToList();
                                        if (tmp != null && tmp.Count > 0)
                                        {
                                            data = tmp.Aggregate((a, b) => a + b).ToString().Trim();
                                        }
                                        else
                                        {
                                            data = "0.00";
                                        }
                                    }
                                }
                                break;
                            case "@Vat3":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        var tmp = model.Details.Where(f => f.ItemVatRate == 3).Select(ff => ff.ItemVatValue).ToList();
                                        if (tmp != null && tmp.Count > 0)
                                        {
                                            data = tmp.Aggregate((a, b) => a + b).ToString().Trim();
                                        }
                                        else
                                        {
                                            data = "0.00";
                                        }
                                    }
                                }
                                break;
                            case "@Vat4":
                                {
                                    var model = obj as ReceiptModel;
                                    if (model != null)
                                    {
                                        var tmp = model.Details.Where(f => f.ItemVatRate == 4).Select(ff => ff.ItemVatValue).ToList();
                                        if (tmp != null && tmp.Count > 0)
                                        {
                                            data = tmp.Aggregate((a, b) => a + b).ToString().Trim();
                                        }
                                        else
                                        {
                                            data = "0.00";
                                        }
                                    }
                                }
                                break;
                            case "@TableTotal":
                                {
                                    if (string.IsNullOrEmpty(data))
                                    {
                                        add = false;
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        if (colWidth > data.Length)
                        {
                            if (!string.IsNullOrEmpty(col.AlignOption))
                            {
                                TextAlignEnum alignment = (TextAlignEnum)Enum.Parse(typeof(TextAlignEnum), col.AlignOption, true);
                                switch (alignment)
                                {
                                    case TextAlignEnum.Middle:
                                        tempStr += CenteredString(data, colWidth);
                                        break;
                                    case TextAlignEnum.Right:
                                        tempStr += data.PadLeft(colWidth, ' ');
                                        break;
                                    case TextAlignEnum.Left:
                                    default:
                                        tempStr += data.PadRight(colWidth, ' ');
                                        break;
                                }
                            }
                            else
                            {
                                tempStr += data.PadRight(colWidth, ' ');
                            }
                        }
                        else if (colWidth == 0)
                        {
                            tempStr = tempStr + data.Trim();
                        }
                        else
                        {
                            tempStr += data.Substring(0, colWidth);
                        }
                        if (colText == "@ItemCustomRemark")
                        {
                            if (!string.IsNullOrEmpty(data))
                            {
                                add = true;
                                tempStr = " " + data.TrimEnd();
                                tempStr = SetStringEscChars(printer, tempStr, col.IsBold, col.IsItalic, col.IsUnderline, col.IsDoubleSize);
                            }
                            else
                            {
                                add = false;
                            }
                        }
                        else
                        {
                            tempStr = SetStringEscChars(printer, tempStr, col.IsBold, col.IsItalic, col.IsUnderline, col.IsDoubleSize);
                        }
                        if (line.SectionColumns.Select(f => f.ColumnText).Contains("@ItemRegion") && tempStr.Length > 0)
                        {
                            tempStr += "\n----------------------------------------";
                        }
                        str += tempStr;
                    }
                    if (add)
                    {
                        if (indentExtra != null && indentExtra == true)
                        {
                            str = "  + " + str;
                        }
                        result.Add(str);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Send given text to the raw printer.
        /// </summary>
        /// <param name="stringToPrint"></param>
        /// <param name="printer"></param>
        /// <param name="printerSettings"></param>
        /// <param name="thisLock"></param>
        /// <param name="graphicPrintInstance"></param>
        private static void SendTextToPrinter(List<string> stringToPrint, Printer printer, KitchenPrinterModel printerSettings, object thisLock, FiscalManager graphicPrintInstance = null)
        {
            lock (thisLock)
            {
                var logPath = GetConfigurationPath();
                var logger = NLogBuilder.ConfigureNLog(logPath).GetCurrentClassLogger();
                logger.Info("In kitchen to printer -> Printer = " + printer.Name);
                // print to kitchen multiple times
                int printTimes = 1;
                if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen || printerSettings.PrinterType == PrinterTypeEnum.Receipt)
                {
                    if (printerSettings.KitchenPrintTimes == null || printerSettings.KitchenPrintTimes == 0)
                    {
                        printerSettings.KitchenPrintTimes = 1;
                    }
                    printTimes = (int)printerSettings.KitchenPrintTimes;
                }
                logger.Info("printTimes = " + printTimes);
                int intKitchenHeaderGapLines = 7;
                if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen)
                {
                    if (printerSettings.KitchenHeaderGapLines == null)
                    {
                        printerSettings.KitchenHeaderGapLines = 7;
                    }
                    intKitchenHeaderGapLines = (int)printerSettings.KitchenHeaderGapLines;
                }
                logger.Info("KitchenHeaderGapLines = " + intKitchenHeaderGapLines);
                // get cutter escape chars
                var cutterEscChars = GetEscapeChars(printer.Cutter);
                // get the init chars
                var initChar = GetEscapeChars(printer.InitChar);
                // create the string to print
                string str = stringToPrint.Aggregate(new StringBuilder(""), (current, next) => current.Append("\r\n").Append(next)).ToString();
                string buzzerEscChars = GetEscapeChars(printer.Buzzer);
                logger.Info("PrinterCharsFormat = " + printerSettings.PrinterCharsFormat);
                switch (printerSettings.PrinterCharsFormat)
                {
                    case PrintCharsFormatEnum.OEM:
                        logger.Info("In OEM PrintTimes: " + printTimes);
                        for (int i = 1; i <= printTimes; i++)
                        {
                            Encoding utf8 = new UTF8Encoding();
                            Encoding oem737 = Encoding.GetEncoding(737);
                            str = initChar + str + new string('\n', intKitchenHeaderGapLines) + '\r';
                            if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen && printerSettings.UseBuzzer == true)
                            {
                                logger.Info("In OEM Buzzer: " + buzzerEscChars.ToString());
                                str = buzzerEscChars + str;
                            }
                            if (printerSettings.UseCutter == null || printerSettings.UseCutter == true)
                            {
                                str = str + cutterEscChars;
                            }
                            logger.Info("In OEM SendBytesToPrinter");
                            SendBytesToPrinter(str, printerSettings.Name, 737);
                        }
                        break;
                    case PrintCharsFormatEnum.ANSI:
                        logger.Info(string.Format("Printing ANSI -> {0}, PrintTimes -> {1}.", printerSettings.FiscalName, printTimes));
                        for (int i = 1; i <= printTimes; i++)
                        {
                            var toPrint = initChar + str + new string('\n', intKitchenHeaderGapLines) + '\r';
                            if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen && printerSettings.UseBuzzer == true)
                            {
                                logger.Info("In ANSI buzzer: " + buzzerEscChars.ToString());
                                toPrint = buzzerEscChars + toPrint;
                            }
                            if (printerSettings.UseCutter == null || printerSettings.UseCutter == true)
                            {
                                toPrint = toPrint + cutterEscChars;
                            }
                            logger.Info("In ANSI SendBytesToPrinter");
                            RawPrinterHelper.SendStringToPrinter(printerSettings.Name, toPrint);
                        }
                        break;
                    case PrintCharsFormatEnum.GRAPHIC:
                        logger.Info("In Graphic PrintTimes: " + printTimes);
                        for (int i = 1; i <= printTimes; i++)
                        {
                            if (printerSettings.PrinterType == PrinterTypeEnum.Kitchen && printerSettings.UseBuzzer == true)
                            {
                                logger.Info("In OEM graphic: " + buzzerEscChars.ToString());
                                RawPrinterHelper.SendStringToPrinter(printerSettings.Name, buzzerEscChars);
                            }
                            var toPrint = initChar + str + new string('\n', intKitchenHeaderGapLines) + '\r';
                            logger.Info("About to call 'PrintGraphic' on -> " + graphicPrintInstance);
                            graphicPrintInstance.PrintGraphic(printerSettings.Name, toPrint.ToString(), (7 == intKitchenHeaderGapLines));
                            if (printerSettings.UseCutter == null || printerSettings.UseCutter == true)
                            {
                                RawPrinterHelper.SendStringToPrinter(printerSettings.Name, cutterEscChars);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Send given bytes to the raw printer.
        /// </summary>
        /// <param name="toSend"></param>
        /// <param name="printerName"></param>
        /// <param name="codePage"></param>
        private static void SendBytesToPrinter(string toSend, string printerName, int codePage)
        {
            var logPath = GetConfigurationPath();
            var logger = NLogBuilder.ConfigureNLog(logPath).GetCurrentClassLogger();
            Encoding utf8 = new UTF8Encoding();
            Encoding destcodepage = Encoding.GetEncoding(codePage);
            byte[] input_utf8 = utf8.GetBytes(toSend);
            byte[] output_dest = Encoding.Convert(utf8, destcodepage, input_utf8);
            int nLength = Convert.ToInt32(output_dest.Length);
            // Allocate some unmanaged memory for those bytes.
            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.
            Marshal.Copy(output_dest, 0, pUnmanagedBytes, nLength);
            logger.Info("SendBytesToPrinter:" + printerName);
            if (!RawPrinterHelper.SendBytesToPrinter(printerName, pUnmanagedBytes, nLength))
            {
                logger.Error("SendBytesToPrinter:" + printerName + " -> Failed!");
            }
        }

        /// <summary>
        /// Get log configuration from application path
        /// </summary>
        /// <returns></returns>
        private static string GetConfigurationPath()
        {
            string path;
            var currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var pathComponents = new List<string>() { currentPath, "..", "..", "..", "Config" };
            var logPath = Path.GetFullPath(Path.Combine(pathComponents.ToArray()));
            if (Directory.Exists(logPath))
                path = Path.Combine(logPath, "NLog.config");
            else
                path = Path.Combine(currentPath, "Config", "NLog.config");
            return path;
        }
    }
}