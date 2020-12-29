namespace ExtECRMainLogic.Enumerators.ExtECR
{
    /// <summary>
    /// The message descriptor tokens.
    /// Used with the received messages from the communication class.
    /// </summary>
    public enum PrintModeEnum
    {
        Receipt,
        Lcd,
        FiscalReport,
        ZReport,
        Photo,
        XReport,
        Image,
        Kitchen,
        Drawer,
        Void,
        General,
        KitchenInstruction,
        ZTotals,
        InvoiceSum,
        Reservation,
        Report,
        /// <summary>
        /// Incoming message from the POS, to start reading electronic scale, for valid weight measures.
        /// </summary>
        StartWeighting,
        /// <summary>
        /// Incoming message from the POS, to stop reading electronic scale.
        /// </summary>
        StopWeighting,
        /// <summary>
        /// Incoming message from the POS, to start communication with EFTPOS Payment Terminal.
        /// Example of command from POS to extECR:
        /// "self.ws.send('SendMessage:' + fiscalName + '|CreditCardAmount:20.99');"
        /// </summary>
        CreditCardAmount,
        /// <summary>
        /// Check for printer availability
        /// </summary>
        PrintConnectivity
    }
}