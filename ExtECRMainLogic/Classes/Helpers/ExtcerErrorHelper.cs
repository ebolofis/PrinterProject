using ExtECRMainLogic.Enumerators;

namespace ExtECRMainLogic.Classes.Helpers
{
    public static class ExtcerErrorHelper
    {
        private static string[,] strErrorMessage =
            {
            { "Ελέγξτε την κατάσταση του εκτυπωτή!"
                    , "Check the printer status!"
                    , "Vérifiez l'état de l'imprimante!"
            },
            { "Το είδος παραστατικού δεν έχει δηλωθεί!"
                    , "The invoice type is not registered!"
                    , "Le type de facture est pas inscrit!"
            },
            { "Ο εκτυπωτής χρειάζεται να τυπώσει Ζ!"
                    , "The printer needs to print Z report!"
                    , "L'imprimante doit imprimer un rapport Z!"
            },
            { "Το είδος της αναφοράς δεν έχει δηλωθεί!"
                    , "The report type is not registered!"
                    , "Le type de rapport est pas inscrit!"
            }
        };
        public const int
            GENERAL_ERROR = 1,
            INVOICE_NOT_FOUND = 2,
            EXCEEDED_Z_TIME = 3,
            REPORT_NOT_FOUND = 4;
        public static LangEnum Lang = LangEnum.English;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public static string GetErrorDescription(int errorCode)
        {
            string res = "";
            switch (Lang)
            {
                case LangEnum.French:
                    res = strErrorMessage[errorCode, 2];
                    break;
                case LangEnum.Greek:
                    res = strErrorMessage[errorCode, 0];
                    break;
                case LangEnum.English:
                default:
                    res = strErrorMessage[errorCode, 1];
                    break;
            }
            return res;
        }
    }
}