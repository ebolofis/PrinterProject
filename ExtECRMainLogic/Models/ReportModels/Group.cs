using System.Collections.Generic;

namespace ExtECRMainLogic.Models.ReportModels
{
    /// <summary>
    /// The area of the report that represents a complete data report object.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// The header area of the group.
        /// </summary>
        public GroupHeader GroupHeader { get; set; }
        /// <summary>
        /// The main data area of the group, that may have one or more similar objects.
        /// </summary>
        public List<Detail> Details { get; set; }
        /// <summary>
        /// The footer area of the group.
        /// </summary>
        public GroupFooter GroupFooter { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Group()
        {
            GroupHeader = new GroupHeader();
            Details = new List<Detail>();
            GroupFooter = new GroupFooter();
        }
    }
}