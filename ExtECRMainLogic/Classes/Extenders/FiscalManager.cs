using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Interfaces.ExtECR;
using ExtECRMainLogic.Models.KitchenModels;
using ExtECRMainLogic.Models.PrinterModels;
using ExtECRMainLogic.Models.ReceiptModels;
using ExtECRMainLogic.Models.ReportModels;
using ExtECRMainLogic.Models.ReservationModels;
using ExtECRMainLogic.Models.ZReportModels;
using System;

namespace ExtECRMainLogic.Classes.Extenders
{
    public class FiscalManager : IPrinterMethodsInterface
    {
        /// <summary>
        /// Instance identification. To be used when we want to have in a switch command, many different instances.
        /// </summary>
        public ExtcerTypesEnum InstanceID;

        public virtual PrintResultModel PrintReceipt()
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintReceipt(ReceiptModel receiptModel)
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintReceipt(ReceiptModel receiptModel, string fiscalName)
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintReceipt(ReceiptModel receiptModel, string fiscalName, string strInvoiceNumber = "")
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintReceipt(ReceiptModel receiptModel, string fiscalName, string strInvoiceNumber = "", object objReprintData = null)
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintReceiptSum()
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintReceiptSum(ReceiptModel receiptModel)
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintKitchen()
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintKitchen(ReceiptModel receiptModel, bool blnIsVoid = false)
        {
            throw new NotImplementedException();
        }

        public virtual void PrintKitchenInstruction()
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintKitchenInstruction(KitchenInstructionModel ktchInstruction)
        {
            throw new NotImplementedException();
        }

        public virtual void PrintReservation(string msgtoprint, PrinterTypeEnum printerType)
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintReservations(ExtecrTableReservetionModel reservationModel, string fiscalName, object objReprintData = null)
        {
            throw new NotImplementedException();
        }

        public virtual void PrintReport(string msgtoprint, PrinterTypeEnum printerType)
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintReports()
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintReports(ReportsModel reportModel, string fiscalName, object objReprintData = null)
        {
            throw new NotImplementedException();
        }

        public virtual void PrintX(ZReportModel xDataModel)
        {
            throw new NotImplementedException();
        }

        public virtual void PrintX(ZReportModel zDataModel, out PrintResultModel result)
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintX()
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintZ()
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel PrintZ(ZReportModel zDataModel)
        {
            throw new NotImplementedException();
        }

        public virtual PrintResultModel GetZTotal()
        {
            throw new NotImplementedException();
        }

        public virtual void PrintGraphic(string printerName, string strToPrint, bool blnUseDefaultMargins = true)
        {
            throw new NotImplementedException();
        }

        public virtual void PrintImage()
        {
            throw new NotImplementedException();
        }

        public virtual void OpenDrawer()
        {
            throw new NotImplementedException();
        }

        public virtual void PrinterConnectivity()
        {
            throw new NotImplementedException();
        }

        public virtual void Claim()
        {
            throw new NotImplementedException();
        }

        public virtual void Close()
        {
            throw new NotImplementedException();
        }
    }
}