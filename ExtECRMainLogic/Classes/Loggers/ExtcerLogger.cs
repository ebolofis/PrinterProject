using System;

namespace ExtECRMainLogic.Classes.Loggers
{
    public static class ExtcerLogger
    {
        public static string Log(string messageToLog, string fiscalName)
        {
            return fiscalName + " -- " + messageToLog;
        }

        public static string logErr(string messageToLog, string fiscalName)
        {
            return fiscalName + " -- " + messageToLog;
        }

        public static string logErr(string messageToLog, Exception ex, string fiscalName)
        {
            return fiscalName + " -- " + messageToLog + " -- " + ex.ToString();
        }
    }
}