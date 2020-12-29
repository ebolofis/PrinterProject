namespace ExtECRMainLogic.Models.ReservationModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ReservationCustomersModel
    {
        /// <summary>
        /// Id Record Key
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Protel Profile Id
        /// </summary>
        public long ProtelId { get; set; }
        /// <summary>
        /// ProtelName (encrypted)
        /// </summary>
        public string ProtelName { get; set; }
        /// <summary>
        /// Name given by the customer (encrypted)
        /// </summary>
        public string ReservationName { get; set; }
        /// <summary>
        /// Room number
        /// </summary>
        public string RoomNum { get; set; }
        /// <summary>
        /// email (encrypted)
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// TR_Reservations.Id
        /// </summary>
        public long ReservationId { get; set; }
        /// <summary>
        /// Hotel info index
        /// </summary>
        public long HotelId { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ReservationCustomersModel()
        {

        }
    }
}