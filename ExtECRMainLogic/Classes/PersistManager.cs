using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Reflection;

namespace ExtECRMainLogic.Classes
{
    public class PersistManager
    {
        /// <summary>
        /// Application version
        /// </summary>
        public readonly string version;
        /// <summary>
        /// Application path
        /// </summary>
        private string applicationPath;
        /// <summary>
        /// Application builder
        /// </summary>
        private IApplicationBuilder applicationBuilder;
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<PersistManager> logger;
        /// <summary>
        /// ExtECR initializer
        /// </summary>
        private readonly ExtECRInitializer extecrInitializer;
        /// <summary>
        /// ExtECR driver
        /// </summary>
        private readonly ExtECRDriver extecrDriver;
        /// <summary>
        /// ExtECR displayer
        /// </summary>
        private readonly ExtECRDisplayer extecrDisplayer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="extecrInitializer"></param>
        /// <param name="extecrDriver"></param>
        public PersistManager(ILogger<PersistManager> logger, ExtECRInitializer extecrInitializer, ExtECRDriver extecrDriver, ExtECRDisplayer extecrDisplayer)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            this.version = versionInfo.FileVersion;
            this.logger = logger;
            this.extecrInitializer = extecrInitializer;
            this.extecrDriver = extecrDriver;
            this.extecrDisplayer = extecrDisplayer;
        }

        /// <summary>
        /// Set application data
        /// </summary>
        /// <param name="applicationPath"></param>
        /// <param name="applicationBuilder"></param>
        public void SetApplicationData(string applicationPath, IApplicationBuilder applicationBuilder)
        {
            this.applicationPath = applicationPath;
            this.applicationBuilder = applicationBuilder;
            InitializePersistManager();
        }

        /// <summary>
        /// Initialize everything at start of application
        /// </summary>
        private void InitializePersistManager()
        {
            logger.LogInformation("Initializing PersistManager...");
            try
            {
                //1. Initialize extecr initializer
                extecrInitializer.InitializeExtECR(applicationPath);
                //2. Initialize extecr driver
                extecrDriver.InitializeExtECR(version, applicationPath, applicationBuilder);
                //3. Initialize extecr displayer
                extecrDisplayer.InitializeExtECR(applicationPath);
            }
            catch (Exception exception)
            {
                logger.LogError("Error. Fix the problem and restart the Service." + Environment.NewLine + exception.ToString());
            }
        }
    }
}