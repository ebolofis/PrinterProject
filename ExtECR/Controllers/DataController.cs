using ExtECRMainLogic.Classes;
using ExtECRMainLogic.Models.ConfigurationModels;
using ExtECRMainLogic.Models.ExtECRModels;
using ExtECRMainLogic.Models.PrinterModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ExtECR.Controllers
{
    public class DataController : Controller
    {
        private readonly ILogger<DataController> logger;
        private readonly ExtECRInitializer extecrInitializer;
        private readonly ExtECRDisplayer extecrDisplayer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="extecrInitializer"></param>
        public DataController(ILogger<DataController> logger, ExtECRInitializer extecrInitializer, ExtECRDisplayer extecrDisplayer)
        {
            this.logger = logger;
            this.extecrInitializer = extecrInitializer;
            this.extecrDisplayer = extecrDisplayer;
        }

        #region Home Data

        /// <summary>
        /// Get printed receipts
        /// </summary>
        /// <returns></returns>
        public List<PrintResultModel> GetReceipts()
        {
            List<PrintResultModel> printedReceipts;
            try
            {
                printedReceipts = extecrDisplayer.GetReceipts();
            }
            catch (Exception exception)
            {
                printedReceipts = null;
                logger.LogError("Error getting printed receipts: " + exception.ToString());
            }
            return printedReceipts;
        }

        /// <summary>
        /// Get print stats
        /// </summary>
        /// <returns></returns>
        public ReceiptStats GetPrintStats()
        {
            ReceiptStats printStats;
            try
            {
                printStats = extecrDisplayer.GetPrintStats();
            }
            catch (Exception exception)
            {
                printStats = null;
                logger.LogError("Error getting print stats: " + exception.ToString());
            }
            return printStats;
        }

        #endregion

        #region Settings Data

        /// <summary>
        /// Get installation master data
        /// </summary>
        /// <returns></returns>
        public InstallationDataMaster GetInstallationMasterData()
        {
            InstallationDataMaster installationMasterData;
            try
            {
                installationMasterData = extecrInitializer.GetInstallationMasterData();
            }
            catch (Exception exception)
            {
                installationMasterData = null;
                logger.LogError("Error getting installation master data: " + exception.ToString());
            }
            return installationMasterData;
        }

        #endregion
    }
}