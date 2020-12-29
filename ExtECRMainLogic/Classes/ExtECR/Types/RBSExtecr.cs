using ExtECRMainLogic.Classes.Loggers;
using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Hubs;
using ExtECRMainLogic.Models.ConfigurationModels;
using ExtECRMainLogic.Models.ExtECRModels;
using ExtECRMainLogic.Models.PrinterModels;
using ExtECRMainLogic.Models.TemplateModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ExtECRMainLogic.Classes.ExtECR.Types
{
    /// <summary>
    /// RBS Instance
    /// </summary>
    public class RBSExtecr : GenericExtcer
    {
        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public RBSDevice rbs;
        /// <summary>
        /// 
        /// </summary>
        public bool DebugActive;
        /// <summary>
        /// 
        /// </summary>
        public bool CheckingCfilesActive;
        /// <summary>
        /// 
        /// </summary>
        public bool ProgressWinActive;
        /// <summary>
        /// 
        /// </summary>
        public string RetErr;
        #endregion
        /// <summary>
        /// Application path
        /// </summary>
        private readonly string applicationPath;
        /// <summary>
        /// Configuration
        /// </summary>
        private readonly IConfiguration configuration;
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<RBSExtecr> logger;
        /// <summary>
        /// Local hub invoker
        /// </summary>
        private readonly LocalHubInvoker localHubInvoker;
        #region  DLL references
        [DllImport("DocMsign.dll")]
        public static extern Int16 VB_FSL_DisableCheckZFiles(int en);
        [DllImport("DocMsign.dll")]
        public static extern void VB_FSL_SetDebug(int fnDebug);
        [DllImport("DocMsign.dll")]
        public static extern Int16 FSL_SetLanguage(int st);
        [DllImport("DocMsign.dll")]
        public static extern void VB_FSL_SetProgress(int fEnable);
        [DllImport("DocMsign.dll")]
        public static extern void VB_FSL_ErrorsUI(int st);
        [DllImport("DocMsign.dll")]
        public static extern Int16 CVB_FSL_SelectDevice(string SerialNo, byte TType, string IPP, string VBstrBaseDir, byte port);
        [DllImport("DocMsign.dll")]
        public static extern void VB_FSL_ErrorToString(int iRet, [MarshalAs(UnmanagedType.AnsiBStr)] ref string strDescription, int ln);
        [DllImport("DocMsign.dll")]
        public static extern Int16 CVB_FSL_ReleaseDevice();
        [DllImport("DocMsign.dll")]
        public static extern Int16 CVB_FSL_SignDocument(string VBstrInfile, [MarshalAs(UnmanagedType.AnsiBStr)] ref string Sign);
        [DllImport("DocMsign.dll")]
        public static extern Int16 CVB_FSL_InvData(string AFM_publisher, string AFMRecipient, string CustomerID, string InvoiceType, string Seira, string InvoiceNo, string NET_A, string NET_B, string NET_C, string NET_D, string NET_E, string VAT_A, string VAT_B, string VAT_C, string VAT_D, string Total, string Currency, [In, Out, MarshalAs(UnmanagedType.AnsiBStr)] ref string Signature);
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="printerTemplatesList"></param>
        /// <param name="printerEscList"></param>
        /// <param name="theAvailablePrinters"></param>
        /// <param name="instData"></param>
        /// <param name="fiscName"></param>
        /// <param name="applicationPath"></param>
        /// <param name="configuration"></param>
        /// <param name="applicationBuilder"></param>
        public RBSExtecr(Dictionary<string, Dictionary<PrinterTypeEnum, RollerTypeReportModel>> printerTemplatesList, List<Printer> printerEscList, List<KitchenPrinterModel> theAvailablePrinters, InstallationDataModel instData, string fiscName, string applicationPath, IConfiguration configuration, IApplicationBuilder applicationBuilder) : base(printerTemplatesList, printerEscList, theAvailablePrinters, instData, fiscName, applicationPath, configuration, applicationBuilder)
        {
            // instance identification
            this.InstanceID = ExtcerTypesEnum.RBS;
            // initialization of variable to be used with lock on printouts
            //this.thisLock = new object();
            this.applicationPath = applicationPath;
            this.configuration = configuration;
            this.DebugActive = false;
            this.CheckingCfilesActive = false;
            this.ProgressWinActive = false;
            this.RetErr = "";
            this.logger = (ILogger<RBSExtecr>)applicationBuilder.ApplicationServices.GetService(typeof(ILogger<RBSExtecr>));
            this.localHubInvoker = (LocalHubInvoker)applicationBuilder.ApplicationServices.GetService(typeof(LocalHubInvoker));
            InitializeFiscal();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~RBSExtecr()
        {

        }

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        private void InitializeFiscal()
        {
            rbs = new RBSDevice();
            rbs.SerialNO = InstallationData.RBSSerialNumber;
            rbs.ActivationCode = InstallationData.RBSUnlockKey;
            rbs.EthernetIP = InstallationData.RBSIP;
            rbs.BackupData = InstallationData.RBSSaveSignPath;
            rbs.ProxyIP = InstallationData.RBSIP;
            rbs.GGPSKey = InstallationData.RBSGGPSKey;
            rbs.ComNo = InstallationData.RBSComPort;
            rbs.IsEthernet = InstallationData.RBSIsEthernet;
            rbs.IsProxy = InstallationData.RBSIsProxy;
            rbs.DebugWindow = InstallationData.RBSDebug;
            SetFooter = SignDocument;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataFile"></param>
        /// <returns></returns>
        private List<string> SignDocument(string dataFile)
        {
            List<string> split = new List<string>();
            string ReturnedErrorStr = "";
            string reply = "";
            int res = 0;
        resend:
            try
            {
                res = SelectESDDevice(rbs);
            }
            catch (Exception exception)
            {
                VB_FSL_ErrorToString(res, ref ReturnedErrorStr, 100);
                CVB_FSL_ReleaseDevice();
                ExtcerLogger.logErr(ReturnedErrorStr, "RBS");
                ExtcerLogger.logErr(exception.ToString(), "RBS");
                throw (new Exception(ReturnedErrorStr));
            }
            if (res == 0)
            {
                dataFile = dataFile.Substring(dataFile.IndexOf("[<]") + 3, dataFile.IndexOf("[>]") - 3).Replace(",", ".");
                string[] dar = dataFile.Split(';');
                FileInfo file = new FileInfo(rbs.BackupData + "\\" + rbs.SerialNO + "\\" + "testFile1.txt");
                file.Directory.Create();
                File.WriteAllText(file.FullName, dataFile);
                string ReturnedSignature = "";
                try
                {
                    res = CVB_FSL_SignDocument(file.FullName, ref ReturnedSignature);
                    res = CVB_FSL_InvData(dar[0], dar[1], dar[2], dar[3], dar[4], dar[5], dar[6], dar[7], dar[8], dar[9], dar[10], dar[11], dar[12], dar[13], dar[14], dar[15], dar[16], ref reply);
                }
                catch (Exception exception)
                {
                    CVB_FSL_ReleaseDevice();
                    throw (exception);
                }
                if (res > 0)
                {
                    reply = "";
                    VB_FSL_ErrorToString(res, ref ReturnedErrorStr, 100);
                    CVB_FSL_ReleaseDevice();
                    ExtcerLogger.logErr(ReturnedErrorStr, "RBS");
                    throw (new Exception(ReturnedErrorStr));
                }
                else
                {
                    reply = reply.Substring(0, reply.IndexOf(" - "));
                    int len = 0;
                    while (len < reply.Length)
                    {
                        if (len + 40 <= reply.Length)
                            split.Add(reply.Substring(len, 40));
                        else
                            split.Add(reply.Substring(len, reply.Length - len));
                        len += 40;
                    }
                    CVB_FSL_ReleaseDevice();
                    return split;
                }
            }
            else
            {
                if (res == 0x0e)
                    goto resend;
                else
                {
                    VB_FSL_ErrorToString(res, ref ReturnedErrorStr, 100);
                    CVB_FSL_ReleaseDevice();
                    ExtcerLogger.logErr(ReturnedErrorStr, "RBS");
                    throw (new Exception(ReturnedErrorStr));
                }
            }
            CVB_FSL_ReleaseDevice();
            return split;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private int SelectESDDevice(RBSDevice CDEv)
        {
            DebugActive = InstallationData.RBSDebug;
            int res = -1;
            string ReturnedErrorStr = "";
            string serno = CDEv.SerialNO;
            if (CheckingCfilesActive)
                VB_FSL_DisableCheckZFiles(0);
            else
                VB_FSL_DisableCheckZFiles(1);
            if (DebugActive)
                VB_FSL_SetDebug(1);
            else
                VB_FSL_SetDebug(0);
            // 0 = Greek Language , 1= English
            FSL_SetLanguage(0);
            // 0 = Disable Signing Progress Window, 1= Enable 
            if (ProgressWinActive)
                VB_FSL_SetProgress(1);
            else
                VB_FSL_SetProgress(0);
            // 0= Disables The Warning - Error -Status Messages, 1= Enables these messages.
            VB_FSL_ErrorsUI(1);
            if (CDEv.IsEthernet)
            {
                // "***********"  = ESD Serial Number if all paces are '*' works with every serial.
                // 2 = Connection type for Ethernet Communication
                // "192.168.0.10" = Ethernet IP Field In case of these type of connection
                // "C\out\ = Path where the _a, _b files are stored after signing procedure
                // 0 = Ignored
                res = CVB_FSL_SelectDevice(serno, 2, CDEv.EthernetIP, this.rbs.BackupData, 0);
            }
            else if (CDEv.IsProxy)
            {
                // "***********"  = ESD Serial Number if all paces are '*' works with every serial.
                // 2 = Connection type for Proxy Communication
                // "192.168.0.10" = Proxy IP Field In case of these type of connection
                // "C\out\ = Path where the _a, _b files are stored after signing procedure
                res = CVB_FSL_SelectDevice(serno, 3, CDEv.ProxyIP, this.rbs.BackupData, 0);
            }
            else
            {
                // "***********"  = ESD Serial Number if all paces are '*' works with every serial.
                // 1 = Connection type for Serial Communication
                // "" = Ethernet or Proxy IP Field In case of these type of connection
                // "C\out\ = Path where the _a, _b files are stored after signing procedure
                // 1 = COM1
                res = CVB_FSL_SelectDevice(serno, 1, "", this.rbs.BackupData, CDEv.ComNo);
            }
            if (res > 0)
            {
                VB_FSL_ErrorToString(res, ref ReturnedErrorStr, 100);
                RetErr = ReturnedErrorStr;
                CVB_FSL_ReleaseDevice();
                return (res);
            }
            return (0);
        }

        #endregion
    }
}