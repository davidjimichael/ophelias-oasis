using System;
using System.Collections.Generic;
using System.Linq;

namespace Oasis.Reports
{
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
                if (_Rows != null)
                {
                    return _Rows;
                }

                var rows = new List<DailyArrivalsReportRow>();

                var DAL = new IO.DAL();

                var reservations = DAL.Read<Models.Reservation>(filter: r => r.Start == Start);

                for (int i = 0; i < reservations.Count(); i++)
                {
                    var res = reservations.ElementAt(i);

                    var report = new DailyArrivalsReportRow()
                    {
                        Date = Start,
                        Departure = res.End,
                        Guest = res.Name,
                        Room = res.Room,
                    };

                    rows.Add(report);
                }

                _Rows = rows.OrderBy(r => r.Guest);

                return _Rows;
            }
        }
    }
}
