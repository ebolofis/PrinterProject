using ExtECRMainLogic.Enumerators.ExtECR;
using ExtECRMainLogic.Hubs;
using ExtECRMainLogic.Models.ExtECRModels;
using ExtECRMainLogic.Models.PrinterModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ExtECRMainLogic.Classes
{
    public class ExtECRDisplayer
    {
        /// <summary>
        /// List of receipts printed during current date
        /// </summary>
        private List<PrintResultModel> ReceiptResultsList;
        /// <summary>
        /// Print stats
        /// </summary>
        private ReceiptStats stats;
        /// <summary>
        /// Application path
        /// </summary>
        private string applicationPath;
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<ExtECRDisplayer> logger;
        /// <summary>
        /// Local hub invoker
        /// </summary>
        private readonly LocalHubInvoker localHubInvoker;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="localHubInvoker"></param>
        public ExtECRDisplayer(ILogger<ExtECRDisplayer> logger, LocalHubInvoker localHubInvoker)
        {
            this.ReceiptResultsList = new List<PrintResultModel>();
            this.stats = new ReceiptStats();
            this.logger = logger;
            this.localHubInvoker = localHubInvoker;
        }

        /// <summary>
        /// Initialize printed receipts and print stats
        /// </summary>
        /// <param name="applicationPath"></param>
        public void InitializeExtECR(string applicationPath)
        {
            logger.LogInformation("Initializing ExtECRDisplayer...");
            try
            {
                this.applicationPath = applicationPath;
                GetReceiptResultsData();
                UpdateStatsData();
            }
            catch (Exception exception)
            {
                logger.LogError("Error. Fix the problem and restart the Service." + Environment.NewLine + exception.ToString());
            }
        }

        /// <summary>
        /// Add recently printed receipt to receipt list
        /// </summary>
        /// <param name="printResult"></param>
        public void AddPrintedReceipt(PrintResultModel printResult)
        {
            ReceiptResultsList.Add(printResult);
            UpdateReceiptResultsData();
            UpdateStatsData();
        }

        /// <summary>
        /// Get printed receipts during current date
        /// </summary>
        /// <returns></returns>
        public List<PrintResultModel> GetReceipts()
        {
            return ReceiptResultsList;
        }

        /// <summary>
        /// Get print stats
        /// </summary>
        /// <returns></returns>
        public ReceiptStats GetPrintStats()
        {
            return stats;
        }

        /// <summary>
        /// Read previously printed receipts for the current date
        /// </summary>
        private void GetReceiptResultsData()
        {
            string[] pathComponents = new string[3] { "Config", "ReceiptResults", "receiptResults_" + string.Format("{0:dd.MM.yyyy}", DateTime.Now) + ".xml" };
            string path = GetExecutingPath(pathComponents);
            if (File.Exists(path))
            {
                lock (ReceiptResultsList)
                {
                    try
                    {
                        using (TextReader textReader = new StreamReader(path))
                        {
                            XmlSerializer deserializer = new XmlSerializer(typeof(List<PrintResultModel>));
                            ReceiptResultsList = (List<PrintResultModel>)deserializer.Deserialize(textReader);
                        }
                    }
                    catch (Exception exception)
                    {
                        logger.LogError("Error getting receipt result data " + path + ": ", exception.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Update printed receipts for the current date
        /// </summary>
        private void UpdateReceiptResultsData()
        {
            string[] directoryComponents = new string[2] { "Config", "ReceiptResults" };
            string directoryPath = GetExecutingPath(directoryComponents);
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Error creating receipt results directory path: " + directoryPath + " Error: " + exception.ToString());
            }
            string[] fileComponents = new string[2] { directoryPath, "receiptResults_" + string.Format("{0:dd.MM.yyyy}", DateTime.Now) + ".xml" };
            string filePath = GetExecutingPath(fileComponents);
            string[] fileComponentsTemp = new string[2] { directoryPath, "receiptResults_" + string.Format("{0:dd.MM.yyyy}", DateTime.Now) + "_temp" + ".xml" };
            string filePathTemp = GetExecutingPath(fileComponentsTemp);
            XmlSerializer serializer;
            lock (ReceiptResultsList)
            {
                try
                {
                    // rename old file as temp
                    if (File.Exists(filePath))
                    {
                        File.Move(filePath, filePathTemp);
                    }
                    serializer = new XmlSerializer(typeof(List<PrintResultModel>));
                    // if there are data from multiple days group them
                    var groupPerDay = ReceiptResultsList.GroupBy(f => f.ProcessDateTime.Date);
                    foreach (var dayRec in groupPerDay)
                    {
                        string[] fileComponentsCurr = new string[2] { directoryPath, "receiptResults_" + string.Format("{0:dd.MM.yyyy}", dayRec.Key) + ".xml" };
                        string filePathCurr = GetExecutingPath(fileComponentsCurr);
                        try
                        {
                            using (TextWriter textWriter = new StreamWriter(filePathCurr))
                            {
                                serializer.Serialize(textWriter, dayRec.Select(f => f).ToList());
                            }
                        }
                        catch (IOException exception)
                        {
                            logger.LogError("Error creating receipt results file: " + filePathCurr + " Error: " + exception.ToString());
                        }
                    }
                    // if new file created delete temp file
                    if (File.Exists(filePathTemp))
                    {
                        File.Delete(filePathTemp);
                    }
                }
                catch (Exception exception)
                {
                    // if temp file exists
                    if (File.Exists(filePathTemp))
                    {
                        // delete corrupted file
                        File.Delete(filePath);
                        // set temp as file
                        File.Move(filePathTemp, filePath);
                    }
                    logger.LogError("Error creating " + filePath + " :", exception.Message);
                }
            }
        }

        /// <summary>
        /// Calculate stats
        /// </summary>
        private void UpdateStatsData()
        {
            var printed = ReceiptResultsList.Where(f => f.Status == PrintStatusEnum.Printed).Count();
            var failed = ReceiptResultsList.Where(f => f.Status == PrintStatusEnum.Failed).Count();
            stats.Printed = printed;
            stats.Failed = failed;
            stats.Total = printed + failed;
            stats.Zreports = ReceiptResultsList.Where(f => f.ReceiptType == PrintModeEnum.ZReport).Count();
            stats.Voids = ReceiptResultsList.Where(f => f.ReceiptType == PrintModeEnum.Void).Count();
            stats.Receipts = ReceiptResultsList.Where(f => f.ReceiptType == PrintModeEnum.Receipt).Count();
        }

        /// <summary>
        /// Get path from components
        /// </summary>
        /// <param name="pathDestinations"></param>
        /// <returns></returns>
        private string GetExecutingPath(string[] pathDestinations)
        {
            List<string> pathComponents = new List<string>();
            pathComponents.Add(applicationPath);
            foreach (string destination in pathDestinations)
            {
                pathComponents.Add(destination);
            }
            string path = Path.GetFullPath(Path.Combine(pathComponents.ToArray()));
            return path;
        }
    }
}