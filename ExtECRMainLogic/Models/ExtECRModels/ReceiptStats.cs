using System;
using System.ComponentModel;

namespace ExtECRMainLogic.Models.ExtECRModels
{
    public class ReceiptStats : INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 
        /// </summary>
        private int _printed;
        public int Printed
        {
            get { return _printed; }
            set { _printed = value; OnPropertyChanged("Printed"); }
        }
        /// <summary>
        /// 
        /// </summary>
        private int _failed;
        public int Failed
        {
            get { return _failed; }
            set { _failed = value; OnPropertyChanged("Failed"); }
        }
        /// <summary>
        /// 
        /// </summary>
        private int _total;
        public int Total
        {
            get { return _total; }
            set { _total = value; OnPropertyChanged("Total"); }
        }
        /// <summary>
        /// 
        /// </summary>
        private int _receipts;
        public int Receipts
        {
            get { return _receipts; }
            set { _receipts = value; OnPropertyChanged("Receipts"); }
        }
        /// <summary>
        /// 
        /// </summary>
        private int _voids;
        public int Voids
        {
            get { return _voids; }
            set { _voids = value; OnPropertyChanged("Voids"); }
        }
        /// <summary>
        /// 
        /// </summary>
        private int _zreports;
        public int Zreports
        {
            get { return _zreports; }
            set { _zreports = value; OnPropertyChanged("Zreports"); }
        }

        public ReceiptStats()
        {
            this.Printed = 0;
            this.Failed = 0;
        }

        public void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}