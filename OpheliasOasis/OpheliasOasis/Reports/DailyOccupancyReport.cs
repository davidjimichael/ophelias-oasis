using System;
using System.Collections.Generic;
using System.Linq;

namespace Oasis.Reports
{
    public class DailyOccupancyReportRow : IReportRow
    {
        public DateTime Date { get; set; }
        public int Room { get; set; }
        /// <summary>
        ///     preceding * = Leaving that date
        /// </summary>
        public string Guest { get; set; }
        public DateTime Departure { get; set; }

    }

    public class DailyOccupancyReport : HotelReport<DailyOccupancyReportRow>
    {
        public override string Title => "Daily Occupancy " + base.Title;
        private IEnumerable<IReportRow> _Rows;

        public override IEnumerable<IReportRow> Rows
        {
            get
            {
                if (_Rows != null)
                {
                    return _Rows;
                }

                var DAL = new IO.DAL();
                End = Start;
                var reservations = DAL.Read<Models.Reservation>(filter: r => !(r.End <= Start || End <= r.Start));
                var days = DAL.Read<Models.Day>(filter: d => Start <= d.Date && d.Date <= End);

                var rows = new List<DailyOccupancyReportRow>();

                for (int i = 0; i < reservations.Count(); i++)
                {
                    var res = reservations.ElementAt(i);

                    var row = new DailyOccupancyReportRow()
                    {
                        Date = Start,
                        Departure = res.End,
                        Guest = res.Name,
                        Room = res.Room,
                    };

                    if (row.Departure == row.Date)
                    {
                        row.Guest = "*" + row.Guest;
                    }

                    rows.Add(row);
                }

                var ordered = rows.OrderBy(r => r.Room);

                this._Rows = ordered;

                return ordered;
            }
        }
    }
}
