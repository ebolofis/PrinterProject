using ExtECRMainLogic.Enumerators.ExtECR;
using System.IO.Ports;

namespace ExtECRMainLogic.Models.DriverModels
{
    public class ScaleSettingsModel
    {
        /// <summary>
        /// Enables scale
        /// </summary>
        public bool enableScale { get; set; }
        /// <summary>
        /// Scale instance name
        /// </summary>
        public string scaleInstance { get; set; }
        /// <summary>
        /// Type of electronic scale
        /// </summary>
        public ScaleTypeEnum scaleType { get; set; } = ScaleTypeEnum.ICS_G310_TISA;
        /// <summary>
        /// COM port of electronic scale
        /// </summary>
        public string scaleCOMPort { get; set; } = "COM3";
        /// <summary>
        /// Serial port of electronic scale
        /// </summary>
        public SerialPort scaleSerialPort { get; set; }
    }
}