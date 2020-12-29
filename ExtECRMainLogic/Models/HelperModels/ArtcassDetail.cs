using ExtECRMainLogic.Enumerators.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ExtECRMainLogic.Models.HelperModels
{
    public class ArtcassDetail
    {
        /// <summary>
        /// 
        /// </summary>
        private Int32 _ItemCode;
        [DisplayName("Code")]
        public Int32 ItemCode
        {
            get { return _ItemCode; }
            set { _ItemCode = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private String _ItemDescription;
        [DisplayName("Description")]
        public String ItemDescription
        {
            get { return _ItemDescription; }
            set { _ItemDescription = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private Int32 _ItemPrice;
        [DisplayName("Price")]
        public Int32 ItemPrice
        {
            get { return _ItemPrice; }
            set { _ItemPrice = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private Int32 _ItemsGroup;
        [DisplayName("Group")]
        public Int32 ItemsGroup
        {
            get { return _ItemsGroup; }
            set { _ItemsGroup = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private Int32 _PriceList;
        [DisplayName("Price List")]
        public Int32 PriceList
        {
            get { return _PriceList; }
            set { _PriceList = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private Int32 _ItemQty;
        [DisplayName("Qty")]
        public Int32 ItemQty
        {
            get { return _ItemQty; }
            set { _ItemQty = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private Int32 _ItemsVat;
        [DisplayName("Vat")]
        public Int32 ItemsVat
        {
            get { return _ItemsVat; }
            set { _ItemsVat = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private ItemTypeEnum _ItemType;
        [DisplayName("Type")]
        public ItemTypeEnum ItemType
        {
            get { return _ItemType; }
            set { _ItemType = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<ArtcasExtras> Extras { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ArtcassDetail()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="str"></param>
        public ArtcassDetail(String str)
        {
            try
            {
                String[] detail = str.Split(',');
                Int32.TryParse(detail[0], out _ItemCode);
                _ItemDescription = detail[1];
                Int32.TryParse(detail[2], out _ItemPrice);
                Int32.TryParse(detail[3], out _ItemsGroup);
                Int32.TryParse(detail[4], out _PriceList);
                Int32.TryParse(detail[5], out _ItemQty);
                Int32.TryParse(detail[6], out _ItemsVat);
            }
            catch
            {
            }
        }
    }
}