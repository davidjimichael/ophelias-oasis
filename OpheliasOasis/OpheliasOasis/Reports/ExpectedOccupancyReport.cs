using Oasis.IO;
using Oasis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oasis.Reports
{
    public class ExpectedOccupancyReportRow : IReportRow
    {
        public DateTime Date { get; set; }
        public int Prepaid { get; set; }
        public int SixtyDay { get; set; }
        public int Incentive { get; set; }
        public int Conventional { get; set; }

        public int NumberRoomsReserved
        {
            get
            {
                return Prepaid + SixtyDay + Incentive + Conventional;
            }
        }

        public ExpectedOccupancyReportRow(DateTime date)
        {
            this.Date = date;
            this.Prepaid = 0;
            this.SixtyDay = 0;
            this.Incentive = 0;
            this.Conventional = 0;
        }
    }

    public class ExpectedOcupancyReport : HotelReport<ExpectedOccupancyReportRow>
    {
        private IEnumerable<ExpectedOccupancyReportRow> _Rows { get; set; }

        public override string Title => "Expected Occupancy " + base.Title;

        public override IEnumerable<IReportRow> Rows
        {
            get
            {
                if (_Rows != null)
                {
                    return _Rows;
                }

                var dal = new DAL();

                Func<Reservation, bool> _withinDates = r => !(r.End < Start || End < r.Start);

                var days = dal.Read<Day>(filter: d => Start <= d.Date && d.Date <= End);
                var reservations = dal.Read<Reservation>(filter: r => _withinDates(r) && r.Status != ReservationStatus.Cancelled);

                var rows = new List<ExpectedOccupancyReportRow>();

                var numDays = (End - Start)?.Days + 1 ?? DEFAULT_REPORT_LENGTH;

                for (int i = 0; i < numDays; i++)
                {
                    DateTime dateTime = Start.AddDays(i);
                    Day day = days.FirstOrDefault(d => d.Date == dateTime);
                    var row = new ExpectedOccupancyReportRow(dateTime);

                    if (day == null)
                    {
                        // day not in "db", go ahead and add it 
                        dal.Create<Day>(new[] { new Day(dateTime, rooms: null, rate: Day.DEFAULT_RATE) });
                    }
                    else
                    {
                        // get all the reservation type counts for this day by getting the reservation ids for this day
                        IEnumerable<int> resIds = day.Rooms
                            .Where(r => !r.IsOpen() && r.ResId.HasValue) // paranoia check here on HasValue
                            .Select(r => r.ResId.Value);

                        // now that we have the reservation ids make sure to ++ the proper row attribute
                        foreach (int id in resIds)
                        {
                            // use First here because if the reservation is marked as booked but is cancelled
                            // means theres an error with the reservation cancelling functions
                            var res = reservations.Where(r => r.Id == id).First();

                            switch (res.Type)
                            {
                                case ReservationType.Prepaid:
                                    row.Prepaid++;
                                    break;
                                case ReservationType.SixtyDay:
                                    row.SixtyDay++;
                                    break;
                                case ReservationType.Conventional:
                                    row.Conventional++;
                                    break;
                                case ReservationType.Incentive:
                                    row.Incentive++;
                                    break;
                                default:
                                    throw new NotImplementedException("Unknown ReservationType " + res.Type);
                            }
                        }
                    }

                    rows.Add(row);
                }
                
                // store to prevent double calculation
                this._Rows = rows;

                return rows;
            }
        }

        public override IEnumerable<Summary<dynamic>> Summaries
        {
            get
            {
                var stats = new List<Summary<dynamic>>();

                var rows = Rows;

                var avgOccupancy = new Summary<dynamic>()
                {
                    Name = "Average Occupancy Rate",
                    Value = rows.Select(r => ((ExpectedOccupancyReportRow)r).NumberRoomsReserved).Sum() / (double)rows.Count(),
                    Format = avg => string.Format("{0:N2}%", (double)avg),
                };

                stats.Add(avgOccupancy);

                return stats;
            }
        }
    }

}
