using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oasis.Reports
{
    //!(r.End < start || end < r.Start)
    public class DailyArrivalsReportRow : IReportRow
    {
        public DateTime Date { get; set; }

        /// <summary>
        ///     Leading * denotes guest leaving today.
        /// </summary>
        public int Room { get; set; }
        public string Guest { get; set; }
        public DateTime Departure { get; set; }
    }

    public class DailyArrivalsReport : HotelReport<DailyArrivalsReportRow>
    {
        public override string Title => "Daily Arrivals " + base.Title;

        private IEnumerable<IReportRow> _Rows { get; set; }

        public override IEnumerable<IReportRow> Rows
        {
            get
            {

            }
        }

        public override IEnumerable<Statistic<dynamic>> Statistics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

    }
}
