namespace ExtECRMainLogic.Models.LCDModels
{
    public class LCDModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ThankYouMessage { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public LCDModel()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description"></param>
        /// <param name="price"></param>
        public LCDModel(string description, decimal price)
        {
            this.Description = description;
            this.Price = price;
        }
    }
}