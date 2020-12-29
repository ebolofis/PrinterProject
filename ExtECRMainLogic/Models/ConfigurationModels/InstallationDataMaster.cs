using ExtECRMainLogic.Enumerators.ExtECR;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ExtECRMainLogic.Models.ConfigurationModels
{
    /// <summary>
    /// Model class. Contains data from InstallationData.xml See UI user's settings, tab 'Installation Data'.
    /// </summary>
    public class InstallationDataMaster
    {
        //============================================================
        // Temporary Flag to upgrade process with Compression Support
        //============================================================
        [XmlElement("EnableCompression")]
        public bool EnableCompression { get; set; }
        //=============================
        // Communication Server Region
        //=============================
        private string _StoreId;
        [XmlElement("StoreId")]
        public string StoreId
        {
            get { return _StoreId; }
            set { _StoreId = value.Trim(); }
        }
        [XmlElement("CommunicationsUrl")]
        public string CommunicationsUrl { get; set; }
        [XmlElement("ServerIPAddress")]
        public string ServerIPAddress { get; set; }
        [XmlElement("ServerPort")]
        public string ServerPort { get; set; }
        [XmlElement("SignalR_UserName")]
        public string SignalR_UserName { get; set; }
        [XmlElement("SignalR_Password")]
        public string SignalR_Password { get; set; }
        [XmlElement("UseSignalR_Server")]
        public Boolean UseSignalR_Server { get; set; }
        [XmlElement("UseCommunicationServer")]
        public Boolean UseCommunicationServer { get; set; }
        //========================
        // Crystal Reports Region
        //========================
        [XmlElement("CrystalDataSource")]
        public string CrystalDataSource { get; set; }
        [XmlElement("CrystalUserName")]
        public string CrystalUserName { get; set; }
        [XmlElement("CrystalPassword")]
        public string CrystalPassword { get; set; }
        //=========================
        // Customer Display Region
        //=========================
        [XmlElement("EnableDisplay")]
        public bool EnableDisplay { get; set; }
        [XmlElement("LcdDisplayInstance")]
        public string LcdDisplayInstance { get; set; }
        [XmlElement("DisplayCharLength")]
        public int DisplayCharLength { get; set; }
        [XmlElement("DisplayType")]
        public LcdTypeEnum DisplayType { get; set; }
        [XmlElement("DisplayComPortName")]
        public string DisplayComPortName { get; set; }
        //=========================
        // Electronic Scale Region
        //=========================
        [XmlElement("EnableScale")]
        public bool EnableScale { get; set; }
        [XmlElement("ScaleInstance")]
        public string ScaleInstance { get; set; }
        [XmlElement("ScaleType")]
        public ScaleTypeEnum ScaleType { get; set; }
        [XmlElement("ScaleComPortName")]
        public string ScaleComPortName { get; set; }
        //=========================
        // Payment Terminal Region
        //=========================
        [XmlElement("EnableEFTPOS")]
        public bool EnableEFTPOS { get; set; }
        [XmlElement("EFTPOSInstance")]
        public string EFTPOSInstance { get; set; }
        [XmlElement("EFTPOSType")]
        public EFTPOSTypeEnum EFTPOSType { get; set; }
        [XmlElement("EFTPOS_IP_Address")]
        public string EFTPOS_IP_Address { get; set; }
        [XmlElement("EFTPOS_TCPIP_Port")]
        public string EFTPOS_TCPIP_Port { get; set; }
        //=========================
        // HTML Receipt Region
        //=========================
        [XmlElement("EnableHtmlReceipt")]
        public bool EnableHtmlReceipt { get; set; }
        [XmlElement("HtmlReceiptTemplate")]
        public string HtmlReceiptTemplate { get; set; }
        //=======================
        // EXTECR Details Region
        //=======================
        [XmlArray("ExtcerDetailsList")]
        public List<InstallationDataModel> ExtcerDetailsList { get; set; }

        public InstallationDataMaster()
        {
            ExtcerDetailsList = new List<InstallationDataModel>();
        }
    }
}