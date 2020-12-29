using ExtECRMainLogic.Classes.Extenders;
using ExtECRMainLogic.Enumerators.ExtECR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;

namespace ExtECRMainLogic.Models.PrinterModels
{
    public class KitchenPrinterModel : BindableObject
    {
        /// <summary>
        /// 
        /// </summary>
        private Guid _fiscalId;
        public Guid FiscalId
        {
            get { return _fiscalId; }
            set { if (value != _fiscalId) { _fiscalId = value; base.RaiseDataErrorChanged("FiscalId"); base.RaisePropertyChanged("FiscalId"); } }
        }
        /// <summary>
        /// Kitchen printer number of copies.
        /// </summary>
        private int? _kitchenPrintTimes;
        public int? KitchenPrintTimes
        {
            get { return _kitchenPrintTimes; }
            set { if (value != _kitchenPrintTimes) { _kitchenPrintTimes = value; base.RaiseDataErrorChanged("KitchenPrintTimes"); base.RaisePropertyChanged("KitchenPrintTimes"); } }
        }
        /// <summary>
        /// If we want to use the cutter mechanism or not.
        /// </summary>
        private bool? _useCutter;
        public bool? UseCutter
        {
            get { return _useCutter; }
            set { if (value != _useCutter) { _useCutter = value; base.RaiseDataErrorChanged("UseCutter"); base.RaisePropertyChanged("UseCutter"); } }
        }
        /// <summary>
        /// How many lines to skip for header gap.
        /// These are the lines needed between printer head and printer cutter mechanism.
        /// </summary>
        private int? _kitchenHeaderGapLines;
        public int? KitchenHeaderGapLines
        {
            get { return _kitchenHeaderGapLines; }
            set { if (value != _kitchenHeaderGapLines) { _kitchenHeaderGapLines = value; base.RaiseDataErrorChanged("KitchenHeaderGapLines"); base.RaisePropertyChanged("KitchenHeaderGapLines"); } }
        }
        /// <summary>
        /// If we want to use the buzzer or not.
        /// </summary>
        private bool? _useBuzzer;
        public bool? UseBuzzer
        {
            get { return _useBuzzer; }
            set { if (value != _useBuzzer) { _useBuzzer = value; base.RaiseDataErrorChanged("UseBuzzer"); base.RaisePropertyChanged("UseBuzzer"); } }
        }
        /// <summary>
        /// If we want the kitchen printer, to print each item in a separated printout or not.
        /// </summary>
        private bool? _printSingleItem;
        public bool? PrintSingleItem
        {
            get { return _printSingleItem; }
            set { if (value != _printSingleItem) { _printSingleItem = value; base.RaiseDataErrorChanged("PrintSingleItem"); base.RaisePropertyChanged("PrintSingleItem"); } }
        }
        /// <summary>
        /// AN PREPEI NA BALEI MAZI KAI NA ATHROISEI TA EIDH POY EXOYN XTYPHTHEI XWRISTA,
        /// ALLA EXOYN TON IDIO KWDIKO
        /// </summary>
        private bool? _mergeSameCodes;
        public bool? MergeSameCodes
        {
            get { return _mergeSameCodes; }
            set { if (value != _mergeSameCodes) { _mergeSameCodes = value; base.RaiseDataErrorChanged("MergeSameCodes"); base.RaisePropertyChanged("MergeSameCodes"); } }
        }
        /// <summary>
        /// AN PREPEI NA TUPOSEI STO FISCAL I MONO STIN KOUZINA
        /// </summary>
        private bool? _printKitchenOnly;
        public bool? PrintKitchenOnly
        {
            get { return _printKitchenOnly; }
            set { if (value != _printKitchenOnly) { _printKitchenOnly = value; base.RaiseDataErrorChanged("PrintKitchenOnly"); base.RaisePropertyChanged("PrintKitchenOnly"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private string _SlotIndex;
        public string SlotIndex
        {
            get { return _SlotIndex; }
            set { if (value != _SlotIndex) { _SlotIndex = value; base.RaiseDataErrorChanged("SlotIndex"); base.RaisePropertyChanged("SlotIndex"); } }
        }
        /// <summary>
        /// The name of the selected instance.
        /// </summary>
        private string _fiscalName;
        public string FiscalName
        {
            get { return _fiscalName; }
            set { if (value != _fiscalName) { _fiscalName = value; base.RaiseDataErrorChanged("FiscalName"); base.RaisePropertyChanged("FiscalName"); } }
        }
        /// <summary>
        /// The name of the selected printer.
        /// </summary>
        private string _name;
        [Required(ErrorMessage = "Printer name is required", AllowEmptyStrings = false)]
        public string Name
        {
            get { return _name; }
            set { if (value != _name) { _name = value; base.RaiseDataErrorChanged("Name"); base.RaisePropertyChanged("Name"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private string _escapeCharsTemplate;
        [Required(ErrorMessage = "Escape characters template is required", AllowEmptyStrings = false)]
        public string EscapeCharsTemplate
        {
            get { return _escapeCharsTemplate; }
            set { if (value != _escapeCharsTemplate) { _escapeCharsTemplate = value; base.RaiseDataErrorChanged("EscapeCharsTemplate"); base.RaisePropertyChanged("EscapeCharsTemplate"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private PrintCharsFormatEnum _printerCharsFormat;
        [Required(ErrorMessage = "Printer characters format is required", AllowEmptyStrings = false)]
        public PrintCharsFormatEnum PrinterCharsFormat
        {
            get { return _printerCharsFormat; }
            set { if (value != _printerCharsFormat) { _printerCharsFormat = value; base.RaiseDataErrorChanged("PrinterCharsFormat"); base.RaisePropertyChanged("PrinterCharsFormat"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private string _template;
        [Required(ErrorMessage = "Template is required", AllowEmptyStrings = false)]
        public string Template
        {
            get { return _template; }
            set
            {
                if (value != _template)
                {
                    _template = value;
                    if (_template.EndsWith(".xml"))
                    {
                        _templateShortName = _template.Replace(".xml", "");
                        _isCrystalReportsPrintout = false;
                    }
                    if (_template.EndsWith(".rpt"))
                    {
                        _templateShortName = _template.Replace(".rpt", "");
                        _isCrystalReportsPrintout = true;
                    }
                    base.RaiseDataErrorChanged("Template");
                    base.RaisePropertyChanged("Template");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private string _templateShortName;
        public string TemplateShortName
        {
            get { return _templateShortName; }
        }
        /// <summary>
        /// 
        /// </summary>
        private bool? _isCrystalReportsPrintout;
        public bool? IsCrystalReportsPrintout
        {
            get { return _isCrystalReportsPrintout; }
            set { if (value != _isCrystalReportsPrintout) { _isCrystalReportsPrintout = value; } }
        }
        /// <summary>
        /// 
        /// </summary>
        private PrinterTypeEnum _printerType;
        public PrinterTypeEnum PrinterType
        {
            get { return _printerType; }
            set { if (value != _printerType) { _printerType = value; base.RaiseDataErrorChanged("PrinterType"); base.RaisePropertyChanged("PrinterType"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private List<Int32> _groups;
        public List<Int32> Groups
        {
            get { return _groups; }
            set { if (value != _groups) { _groups = value; base.RaiseDataErrorChanged("Groups"); base.RaisePropertyChanged("Groups"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private List<long> _regions;
        public List<long> Regions
        {
            get { return _regions; }
            set { if (value != _regions) { _regions = value; base.RaiseDataErrorChanged("Regions"); base.RaisePropertyChanged("Regions"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "Item groups are required", AllowEmptyStrings = false)]
        public string GroupAsString
        {
            get {
                if (Groups != null)
                {
                    string result = "";
                    List<string> numList = Groups.ToList().ConvertAll<string>(delegate (int i)
                    {
                        return i.ToString();
                    });
                    if (numList.Count > 0)
                    {
                        result = numList.Aggregate((a, b) => a + ',' + b).ToString();
                    }
                    return result;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (!string.IsNullOrEmpty(value.ToString()))
                {
                    var intList = value.Split(',').ToList().ConvertAll<int>(delegate (string i)
                    {
                        return Convert.ToInt32(i);
                    });
                    Groups.Clear();
                    Groups.AddRange(intList);
                    base.RaiseDataErrorChanged("GroupAsString");
                    base.RaisePropertyChanged("GroupAsString");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string RegionsAsString
        {
            get
            {
                if (Regions != null)
                {
                    string result = "";
                    List<string> numList = Regions.ToList().ConvertAll<string>(delegate (long i)
                    {
                        return i.ToString();
                    });
                    if (numList.Count > 0)
                    {
                        result = numList.Aggregate((a, b) => a + ',' + b).ToString();
                    }
                    return result;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (!string.IsNullOrEmpty(value.ToString()))
                {
                    var intList = value.Split(',').ToList().ConvertAll<long>(delegate (string i)
                    {
                        return Convert.ToInt64(i);
                    });
                    Regions.Clear();
                    Regions.AddRange(intList);
                }
                else
                    Regions.Clear();
                base.RaiseDataErrorChanged("RegionsAsString");
                base.RaisePropertyChanged("RegionsAsString");
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////////////
        // additional fields added to support orders printout at the same time as receipt issue
        // given as kitchen printout
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerDeliveryAddress { get; set; }
        public string CustomerPhone { get; set; }
        public string Floor { get; set; }
        public string City { get; set; }
        public string CustomerComments { get; set; }
        public string CustomerAfm { get; set; }
        public string CustomerDoy { get; set; }
        public string CustomerJob { get; set; }
        public string RegNo { get; set; }
        public string SumOfLunches { get; set; }
        public string SumofConsumedLunches { get; set; }
        public string GuestTerm { get; set; }
        public string Adults { get; set; }
        public string Kids { get; set; }
        public decimal? TotalDiscount { get; set; }
        public string ItemCustomRemark { get; set; }

        public KitchenPrinterModel()
        {
            base.InitializeErrors();
            Groups = new List<int>();
            Regions = new List<long>();
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this) + Environment.NewLine;
        }

        /// <summary>
        /// return a list of printers as json list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ListToString(List<KitchenPrinterModel> list)
        {
            if (list == null)
                return "<null>";
            return JsonSerializer.Serialize(list);
        }
    }
}