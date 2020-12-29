using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Models.PrinterModels;

namespace ExtECRMainLogic.Interfaces.ExtECR
{
    public interface IPrinterMethodsInterface
    {
        /// <summary>
        /// Print receipt
        /// </summary>
        /// <returns></returns>
        PrintResultModel PrintReceipt();
        /// <summary>
        /// Print report
        /// </summary>
        /// <returns></returns>
        PrintResultModel PrintReports();
        /// <summary>
        /// Print kitchen
        /// </summary>
        /// <returns></returns>
        PrintResultModel PrintKitchen();
        /// <summary>
        /// Print Z
        /// </summary>
        /// <returns></returns>
        PrintResultModel PrintZ();
        /// <summary>
        /// Print X
        /// </summary>
        /// <returns></returns>
        PrintResultModel PrintX();
        /// <summary>
        /// Calculate sums before Z
        /// </summary>
        /// <returns></returns>
        PrintResultModel GetZTotal();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        PrintResultModel PrintReceiptSum();
        /// <summary>
        /// A message for the kitchen
        /// </summary>
        void PrintKitchenInstruction();
        /// <summary>
        /// Release ECR device from exlcusive use
        /// </summary>
        void Close();
        /// <summary>
        /// Lock ECR device for exlcusive use
        /// </summary>
        void Claim();
        /// <summary>
        /// Print report
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="printerType"></param>
        void PrintReport(string msg, PrinterTypeEnum printerType);
        /// <summary>
        /// Print image (btm)
        /// </summary>
        void PrintImage();
        /// <summary>
        /// Open drawer
        /// </summary>
        void OpenDrawer();
        /// <summary>
        /// Check printer availability
        /// </summary>
        void PrinterConnectivity();
    }
}