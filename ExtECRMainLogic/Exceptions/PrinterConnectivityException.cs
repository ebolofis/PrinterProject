using System;

namespace ExtECRMainLogic.Exceptions
{
    public class PrinterConnectivityException : Exception
    {
        /// <summary>
        /// If true then POS must reset the current order.
        /// </summary>
        public bool ResetOrder { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PrinterConnectivityException() : base()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public PrinterConnectivityException(string message) : base(message)
        {
            ResetOrder = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="reserOrder"></param>
        public PrinterConnectivityException(string message, bool reserOrder) : base(message)
        {
            ResetOrder = reserOrder;
        }
    }
}