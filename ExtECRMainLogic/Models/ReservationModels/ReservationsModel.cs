using System;

namespace ExtECRMainLogic.Models.ReservationModels
{
    /// <summary>
    /// Class Model for 'Receipt Sums Reports', 'Waiters Reports', 'Cashier Totals Reports'.
    /// </summary>
    public class ReservationsModel
    {
        /// <summary>
        /// Id Record key
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// TR_Restaurants.Id
        /// </summary>
        public long RestId { get; set; }
        /// <summary>
        /// TR_Capacities.Id
        /// </summary>
        public long CapacityId { get; set; }
        /// <summary>
        /// Number of people
        /// </summary>
        public int Couver { get; set; }
        /// <summary>
        /// Reservation Date
        /// </summary>
        public DateTime ReservationDate { get; set; }
        /// <summary>
        /// Reservation Time
        /// </summary>
        public TimeSpan ReservationTime { get; set; }
        /// <summary>
        /// Date of Reservation
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 0: Active, 1: Cancel
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ReservationsModel()
        {

        }
    }
}