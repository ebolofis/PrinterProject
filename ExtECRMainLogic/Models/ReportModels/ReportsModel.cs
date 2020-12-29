using System;
using System.Collections.Generic;

namespace ExtECRMainLogic.Models.ReportModels
{
    /// <summary>
    /// The complete report class object.
    /// </summary>
    public class ReportsModel
    {
        /// <summary>
        /// The report slot selector.
        /// </summary>
        public string ReportIndex { get; set; }
        /// <summary>
        /// The POS (pos_info_id) that sends the report.
        /// </summary>
        public Int64 PosInfoId { get; set; }
        /// <summary>
        /// The header area of the report.
        /// </summary>
        public ReportHeader ReportHeader { get; set; }
        /// <summary>
        /// The header area that is used when the report has more than one pages.
        /// </summary>
        public PageHeader PageHeader { get; set; }
        /// <summary>
        /// The main data area of the report, that may have one or more similar objects.
        /// </summary>
        public List<Group> Groups { get; set; }
        /// <summary>
        /// The footer area that is used when the report has more than one pages.
        /// </summary>
        public PageFooter PageFooter { get; set; }
        /// <summary>
        /// The footer area of the report.
        /// </summary>
        public ReportFooter ReportFooter { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ReportsModel()
        {
            ReportHeader = new ReportHeader();
            PageHeader = new PageHeader();
            Groups = new List<Group>();
            ReportFooter = new ReportFooter();
            PageFooter = new PageFooter();
        }
    }
}