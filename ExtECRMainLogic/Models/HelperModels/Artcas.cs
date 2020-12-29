using ExtECRMainLogic.Enumerators.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtECRMainLogic.Models.HelperModels
{
    public class Artcas
    {
        /// <summary>
        /// 
        /// </summary>
        public ArtcasTypeEnum ArtcasType
        {
            get
            {
                if (_TableStatus == 2)
                    return ArtcasTypeEnum.CloseTable;
                if (Details.Select(s => s.ItemType).Contains(ItemTypeEnum.ZReport))
                    return ArtcasTypeEnum.ZReport;
                if (Details.Select(s => s.ItemType).Contains(ItemTypeEnum.XReport))
                    return ArtcasTypeEnum.XReport;
                return ArtcasTypeEnum.Receipt;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private Int32 _WaiterId;
        public Int32 WaiterId
        {
            get { return _WaiterId; }
            set { _WaiterId = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private String _WaiterName;
        public String WaiterName
        {
            get { return _WaiterName; }
            set { _WaiterName = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private String _Table;
        public String Table
        {
            get { return _Table; }
            set { _Table = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private String _STable;
        public String STable
        {
            get { return _STable; }
            set { _STable = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private int _TableStatus;
        public int TableStatus
        {
            get { return _TableStatus; }
            set { _TableStatus = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private int _Discount;
        public int Discount
        {
            get { return _Discount; }
            set { _Discount = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private String _Room;
        public String Room
        {
            get { return _Room; }
            set { _Room = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private String _OrderId;
        public String OrderId
        {
            get { return _OrderId; }
            set { _OrderId = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<ArtcassDetail> Details { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Artcas()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="str"></param>
        public Artcas(String str)
        {
            Details = new List<ArtcassDetail>();
            try
            {
                String[] header = str.Split(',');
                Int32.TryParse(header[0], out _WaiterId);
                _WaiterName = header[1];
                _Table = header[2].Split('-')[0];
                _STable = header[2].Split('-')[1];
                Int32.TryParse(header[3], out _TableStatus);
                Int32.TryParse(header[4], out _Discount);
                _Room = header[5];
                _OrderId = header[6];
            }
            catch
            {
            }
        }
    }
}