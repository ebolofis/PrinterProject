using System.Collections.Generic;

namespace ExtECRMainLogic.Models.ReservationModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ExtecrTableReservetionModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string RestaurantName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ReservationsModel Reservation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ReservationCustomersModel> ReservationCustomers { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ExtecrTableReservetionModel()
        {

        }
    }
}