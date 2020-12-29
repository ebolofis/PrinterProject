using ExtECRMainLogic.Enumerators.ExtECR;
using System;
using System.Xml.Serialization;

namespace ExtECRMainLogic.Models.ConfigurationModels
{
    /// <summary>
    /// Model class. Describes an instanse. Part of InstallationData.xml See UI user's settings, tab 'Installation Data', section 'Extcer Settings'.
    /// </summary>
    public class InstallationDataModel
    {
        [XmlAttribute("FiscalId")]
        public Guid FiscalId { get; set; }
        [XmlElement("InstallationType")]
        public InstallationTypeEnums InstallationType { get; set; }
        [XmlElement("Pos")]
        public String Pos { get; set; }
        [XmlElement("PosDescr")]
        public String PosDescr { get; set; }
        [XmlElement("DepartmentDesc")]
        public String DepartmentDesc { get; set; }
        [XmlElement("Department")]
        public String Department { get; set; }
        [XmlElement("FiscalName")]
        public string FiscalName { get; set; }
        [XmlElement("ExtcerType")]
        public ExtcerTypesEnum? ExtcerType { get; set; }
        [XmlElement("OposDeviceName")]
        public OposDeviceNamesEnum? OposDeviceName { get; set; }
        [XmlElement("Opos3Com")]
        public string Opos3Com { get; set; }
        [XmlElement("VAT")]
        public string VAT { get; set; }
        [XmlElement("ErgoSpd3EmptyJournal")]
        public Boolean? ErgoSpd3EmptyJournal { get; set; }
        [XmlElement("JournalPath")]
        public String JournalPath { get; set; }
        [XmlElement("ZipDitronFilePath")]
        public String ZipDitronFilePath { get; set; }
        [XmlElement("ZipDitronErrorFilePath")]
        public String ZipDitronErrorFilePath { get; set; }
        [XmlElement("ZipDitronDpt")]
        public String ZipDitronDpt { get; set; }
        private Int32? _OposMaxString;
        [XmlElement("OposMaxString")]
        public Int32? OposMaxString
        {
            get { return _OposMaxString ?? 25; }
            set { _OposMaxString = value; }
        }
        [XmlElement("GenericExtraZeroPrice")]
        public Boolean? GenericExtraZeroPrice { get; set; }
        [XmlElement("OposSupportOldERGO")]
        public Boolean? OposSupportOldERGO { get; set; }
        [XmlElement("OposMethodOfPaymentCASH")]
        public String OposMethodOfPaymentCASH { get; set; }
        [XmlElement("GenericGraphicBold")]
        public bool GenericGraphicBold { get; set; }
        [XmlElement("GenericGraphicFontSize")]
        public int GenericGraphicFontSize { get; set; }
        [XmlElement("OposMethodOfPaymentCC")]
        public String OposMethodOfPaymentCC { get; set; }
        [XmlElement("OposMethodOfPaymentCREDIT")]
        public String OposMethodOfPaymentCREDIT { get; set; }
        [XmlElement("HDMIP")]
        public String HDMIP { get; set; }
        [XmlElement("HDMPort")]
        public int HDMPort { get; set; }
        [XmlElement("HDMCaisherLogin")]
        public int HDMCaisherLogin { get; set; }
        [XmlElement("HDMCaisherPassword")]
        public String HDMCaisherPassword { get; set; }
        [XmlElement("HDMECR_Password ")]
        public String HDMECR_Password { get; set; }
        [XmlElement("EpsonFiscalComPort")]
        public Int32? EpsonFiscalComPort { get; set; }
        [XmlElement("RBSSerialNumber")]
        public String RBSSerialNumber { get; set; }
        [XmlElement("RBSUnlockKey")]
        public String RBSUnlockKey { get; set; }
        [XmlElement("RBSIP")]
        public String RBSIP { get; set; }
        [XmlElement("RBSComPort")]
        public byte RBSComPort { get; set; }
        [XmlElement("RBSGGPSKey")]
        public String RBSGGPSKey { get; set; }
        [XmlElement("RBSGSISServAddr")]
        public String RBSGSISServAddr { get; set; }
        [XmlElement("RBSGSISServAddrPort")]
        public int RBSGSISServAddrPort { get; set; }
        [XmlElement("RBSSaveSignPath")]
        public String RBSSaveSignPath { get; set; }
        [XmlElement("RBSSaveSignPathAlt")]
        public String RBSSaveSignPathAlt { get; set; }
        [XmlElement("RBSIsEthernet")]
        public bool RBSIsEthernet { get; set; }
        [XmlElement("RBSIsProxy")]
        public bool RBSIsProxy { get; set; }
        [XmlElement("RBSDebug")]
        public bool RBSDebug { get; set; }

        public InstallationDataModel()
        {
            FiscalId = Guid.NewGuid();
        }

        public override string ToString()
        {
            return FiscalName;
        }
    }
}