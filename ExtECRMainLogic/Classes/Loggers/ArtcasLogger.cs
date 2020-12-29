using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Models.HelperModels;
using ExtECRMainLogic.Models.ReceiptModels;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ExtECRMainLogic.Classes.Loggers
{
    public static class ArtcasLogger
    {
        /// <summary>
        /// 
        /// </summary>
        private static XmlSerializer serializer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private static StreamReader reader { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private static TextWriter textWriter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiptToLog"></param>
        /// <param name="date"></param>
        /// <param name="errorDescr"></param>
        /// <param name="artcasTextToLog"></param>
        /// <param name="receivedMode"></param>
        /// <param name="printStatus"></param>
        /// <param name="fiscalName"></param>
        /// <param name="recResultID"></param>
        public static void LogArtCasXML(ReceiptModel receiptToLog, DateTime date, string errorDescr, List<String> artcasTextToLog, ReceiptReceiveTypeEnum receivedMode, PrintStatusEnum printStatus, string fiscalName, Guid recResultID)
        {
            var logPath = GetConfigurationPath();
            var logger = NLogBuilder.ConfigureNLog(logPath).GetCurrentClassLogger();
            string appDir = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            if (!Directory.Exists(appDir + "\\Log\\ArtcasBackup\\"))
            {
                Directory.CreateDirectory(appDir + "\\Log\\ArtcasBackup\\");
            }
            appDir = appDir + "\\Log\\ArtcasBackup\\";
            ArtCasXmlModel artCasXml = new ArtCasXmlModel();
            string path = @"artcasxmllog_" + String.Format("{0:dd.MM.yyyy}", date) + ".xml";
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (string line in artcasTextToLog)
                {
                    sb.AppendLine(line);
                    sb.Append(Environment.NewLine);
                }
                artCasXml.LogTime = date;
                artCasXml.ReceiptModel = receiptToLog;
                artCasXml.ReceivedMode = receivedMode.ToString();
                artCasXml.PrintStatus = printStatus;
                artCasXml.FiscalName = fiscalName;
                artCasXml.Id = recResultID;
                if (!string.IsNullOrEmpty(errorDescr))
                {
                    artCasXml.ErrorDescription = errorDescr;
                }
                if (!File.Exists(appDir + path))
                {
                    ArtCasXmlWraperModel artCasWraper = new ArtCasXmlWraperModel();
                    artCasWraper.ArtCasList.Add(artCasXml);
                    serializer = new XmlSerializer(typeof(ArtCasXmlWraperModel));
                    textWriter = new StreamWriter(appDir + path);
                    serializer.Serialize(textWriter, artCasWraper);
                    textWriter.Close();
                }
                else
                {
                    serializer = new XmlSerializer(typeof(ArtCasXmlWraperModel));
                    reader = new StreamReader(appDir + path);
                    var artCasXmlModel = (ArtCasXmlWraperModel)serializer.Deserialize(reader);
                    artCasXmlModel.ArtCasList.Add(artCasXml);
                    reader.Close();
                    textWriter = new StreamWriter(appDir + path);
                    serializer.Serialize(textWriter, artCasXmlModel);
                    textWriter.Close();
                }
            }
            catch (XmlException)
            {
                try
                {
                    logger.Error("ERROR LOGGING RECEIPT xmlException");
                    logger.Info("Creating empty element..");
                    serializer = new XmlSerializer(typeof(ArtCasXmlWraperModel));
                    textWriter = new StreamWriter(appDir + path);
                    ArtCasXmlWraperModel artCasWraper = new ArtCasXmlWraperModel();
                    serializer.Serialize(textWriter, artCasWraper);
                    textWriter.Close();
                    logger.Error("Creating empty element ... DONE!");
                }
                catch (Exception)
                {
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (textWriter != null)
                {
                    textWriter.Close();
                }
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