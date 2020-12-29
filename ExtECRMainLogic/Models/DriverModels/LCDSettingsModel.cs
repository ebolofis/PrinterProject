using ExtECRMainLogic.Enumerators.ExtECR;
using System.IO.Ports;

namespace ExtECRMainLogic.Models.DriverModels
{
    public class LCDSettingsModel
    {
        /// <summary>
        /// Enables LCD
        /// </summary>
        public bool enableLCD { get; set; }
        /// <summary>
        /// LCD instance name
        /// </summary>
        public string LCDInstance { get; set; }
        /// <summary>
        /// LCD device type
        /// </summary>
        public LcdTypeEnum LCDType { get; set; } = LcdTypeEnum.NCR;
        /// <summary>
        /// LCD device maximum message length
        /// </summary>
        public int LCDLength { get; set; } = 20;
        /// <summary>
        /// LCD COM port
        /// </summary>
        public string LCDCOMPort { get; set; } = "COM2";
        /// <summary>
        /// LCD serial port
        /// </summary>
        public SerialPort LCDSerialPort { get; set; }
    }
}