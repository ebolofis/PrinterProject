using ExtECRMainLogic.Classes.Extenders;
using ExtECRMainLogic.Models.WrapperModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace ExtECRMainLogic.Models.PrinterModels
{
    /// <summary>
    /// represents a printer definition (esc chars) from PrintersXML.xml file
    /// </summary>
    public class Printer : BindableObject, INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 
        /// </summary>
        private int _code;
        [XmlAttribute("Code")]
        [Range(0, int.MaxValue, ErrorMessage = "Printer code must be a positive integer!")]
        public int Code
        {
            get { return this._code; }
            set { if (value != this._code) { this._code = value; base.RaiseDataErrorChanged("Code"); base.RaisePropertyChanged("Code"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private string _name;
        [XmlAttribute("Name")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Printer name must be at least 5 characters long!")]
        public string Name
        {
            get { return this._name; }
            set { if (value != this._name) { this._name = value; base.RaiseDataErrorChanged("Name"); base.RaisePropertyChanged("Name"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<CharWrapper> _initChar;
        [XmlArray("InitChar")]
        [XmlArrayItem("Char")]
        public ObservableCollection<CharWrapper> InitChar
        {
            get { return this._initChar; }
            set { if (_initChar != value) { _initChar = value; OnPropertyChanged("InitChar"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<CharWrapper> _boldOn;
        [XmlArray("BoldOn")]
        [XmlArrayItem("Char")]
        public ObservableCollection<CharWrapper> BoldOn
        {
            get { return this._boldOn; }
            set { if (_boldOn != value) { _boldOn = value; OnPropertyChanged("BoldOn"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<CharWrapper> _boldOff;
        [XmlArray("BoldOff")]
        [XmlArrayItem("Char")]
        public ObservableCollection<CharWrapper> BoldOff
        {
            get { return this._boldOff; }
            set { if (_boldOff != value) { _boldOff = value; OnPropertyChanged("BoldOff"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<CharWrapper> _italicOn;
        [XmlArray("ItalicOn")]
        [XmlArrayItem("Char")]
        public ObservableCollection<CharWrapper> ItalicOn
        {
            get { return this._italicOn; }
            set { if (_italicOn != value) { _italicOn = value; OnPropertyChanged("ItalicOn"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<CharWrapper> _italicOff;
        [XmlArray("ItalicOff")]
        [XmlArrayItem("Char")]
        public ObservableCollection<CharWrapper> ItalicOff
        {
            get { return this._italicOff; }
            set { if (_italicOff != value) { _italicOff = value; OnPropertyChanged("ItalicOff"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<CharWrapper> _underLineOn;
        [XmlArray("UnderLineOn")]
        [XmlArrayItem("Char")]
        public ObservableCollection<CharWrapper> UnderLineOn
        {
            get { return this._underLineOn; }
            set { if (_underLineOn != value) { _underLineOn = value; OnPropertyChanged("UnderLineOn"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<CharWrapper> _underLineOff;
        [XmlArray("UnderLineOff")]
        [XmlArrayItem("Char")]
        public ObservableCollection<CharWrapper> UnderLineOff
        {
            get { return this._underLineOff; }
            set { if (_underLineOff != value) { _underLineOff = value; OnPropertyChanged("UnderLineOff"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<CharWrapper> _doubleSizeOn;
        [XmlArray("DoubleSizeOn")]
        [XmlArrayItem("Char")]
        public ObservableCollection<CharWrapper> DoubleSizeOn
        {
            get { return this._doubleSizeOn; }
            set { if (_doubleSizeOn != value) { _doubleSizeOn = value; OnPropertyChanged("DoubleSizeOn"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<CharWrapper> _doubleSizeOff;
        [XmlArray("DoubleSizeOff")]
        [XmlArrayItem("Char")]
        public ObservableCollection<CharWrapper> DoubleSizeOff
        {
            get { return this._doubleSizeOff; }
            set { if (_doubleSizeOff != value) { _doubleSizeOff = value; OnPropertyChanged("DoubleSizeOff"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<CharWrapper> _cutter;
        [XmlArray("Cutter")]
        [XmlArrayItem("Char")]
        public ObservableCollection<CharWrapper> Cutter
        {
            get { return this._cutter; }
            set { if (_cutter != value) { _cutter = value; OnPropertyChanged("Cutter"); } }
        }
        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<CharWrapper> _buzzer;
        [XmlArray("Buzzer")]
        [XmlArrayItem("Char")]
        public ObservableCollection<CharWrapper> Buzzer
        {
            get { return this._buzzer; }
            set { if (_buzzer != value) { _buzzer = value; OnPropertyChanged("Buzzer"); } }
        }

        public Printer()
        {
            base.InitializeErrors();
        }

        /// <summary>
        /// Create the OnPropertyChanged method to raise the event
        /// </summary>
        /// <param name="name"></param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}