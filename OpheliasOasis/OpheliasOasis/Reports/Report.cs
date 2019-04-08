using Oasis;
using Oasis.IO;
using Oasis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpheliasOasis.Reports
{
    #region interfaces and abstracts
    public interface IReportRow
    {
        DateTime Date { get; set; }
    }

    public class Statistic<T>
    {
        public string Name;
        public T Value;
        public Func<T, string> Format = null;
    }

    public abstract class HotelReport<TRow>
    {
        public DateTime Start;
        public DateTime? End;
        public static readonly short DEFAULT_REPORT_LENGTH = 30;
        
        public IEnumerable<PropertyInfo> RowProperties
        {
            get
            {
                return typeof(TRow).GetProperties();
            }
        }

        public IEnumerable<string> ColumnNames
        {
            get
            {
                return this.RowProperties.Select(p => p.Name);
            }
        }

        public virtual string Title
        {
            get
            {
                string format = "Report for {0}";

                // allows for single or multi day report titles
                if (End.HasValue)
                {
                    format += " to " + End.Value.ToShortDateString();
                }
                return string.Format(format, Start.ToShortDateString());
            }
        }
        
        public abstract IEnumerable<TRow> Rows { get; }

        public virtual IEnumerable<Statistic<dynamic>> Statistics { get; }
    }

    #endregion
    #region reportrows

    public class OccupancyReportRow : IReportRow
    {
        public DateTime Date { get; set; }
        public int Prepaid;
        public int SixtyDay;
        public int Incentive;
        public int Conventional;

        // todo possibly add lazy loading instead of calculating this each time
        public int NumberRoomsReserved
        {
            get
            {
                return Prepaid + SixtyDay + Incentive + Conventional;
            }
        }

        public OccupancyReportRow(DateTime date)
        {
            this.Date = date;
            this.Prepaid = 0;
            this.SixtyDay = 0;
            this.Incentive = 0;
            this.Conventional = 0;
        }
    }

    #endregion
    #region reports

    public class ExpectedOcupancyReport : HotelReport<OccupancyReportRow>
    {
        //public ExpectedOcupancyReport(DateTime start, DateTime end)
        //{
        //    this.Start = start;
        //    this.End = end;
        //}

        public override string Title => "Expected Occupancy " + base.Title;

        public override IEnumerable<OccupancyReportRow> Rows
        {
            get
            {
                var dal = new DAL();

                Func<Reservation, bool> _withinDates = r => !(r.End < Start || End < r.Start);

                var days = dal.Read<Day>(filter: d => Start <= d.Date && d.Date <= End);
                var reservations = dal.Read<Reservation>(filter: r => _withinDates(r) && r.Status != ReservationStatus.Cancelled);

                var rows = new List<OccupancyReportRow>();

                for (int i = 0; i < DEFAULT_REPORT_LENGTH; i++)
                {
                    DateTime dateTime = Start.AddDays(i);
                    Day day = days.FirstOrDefault(d => d.Date == dateTime);
                    var row = new OccupancyReportRow(dateTime);

                    if (day == null)
                    {
                        // day not in db, go ahead and add it 
                        dal.Create<Day>(new[] { new Day(dateTime, rooms: null, rate: Day.DEFAULT_RATE) });
                    }
                    else
                    {
                        // get all the reservation type counts for this day by getting the reservation ids for this day
                        IEnumerable<int> resIds = day.Rooms
                            .Where(r => !r.IsOpen && r.ResId.HasValue) // paranoia check here on HasValue
                            .Select(r => r.ResId.Value);

                        // now that we have the reservation ids make sure to ++ the proper row attribute
                        foreach (int id in resIds)
                        {
                            // user First here because if the reservation is marked as booked but is cancelled
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
                
                return rows;
            }
        }

        public override IEnumerable<Statistic<dynamic>> Statistics
        {
            get
            {
                var stats = new List<Statistic<dynamic>>();

                var avgOccupancy = new Statistic<dynamic>()
                {
                    Name = "Average Occupancy Rate",
                    Value = Rows.Select(r => r.NumberRoomsReserved).Sum() / Rows.Count(),
                    Format = avg => string.Format("{0}%", avg),
                };

                stats.Add(avgOccupancy);

                return stats;
            }
        }
    }

    #endregion






    //public interface IReportStatistic
    //{
    //    string Name { get; }
    //    dynamic Value { get; }
    //}

    //public interface IReport
    //{
    //    string Title { get; }
    //    IEnumerable<PropertyInfo> ColumnInfo { get; }
    //    IEnumerable<IReportRow> Rows { get; }
    //    IEnumerable<IReportStatistic> Statistics { get; }
    //}

    //public class ExpectedOccupancyReport : IReport
    //{
    //    public string Title { get => "Expected Occupancy Report"; }

    //    public IEnumerable<PropertyInfo> ColumnInfo { get => typeof(OccupancyReportRow).GetProperties(); }

    //    public IEnumerable<IReportRow> Rows { get => throw new NotImplementedException(); }
    //    public IEnumerable<IReportStatistic> Statistics { get => throw new NotImplementedException(); }
    //}

    //public abstract class Report
    //{
    //    public abstract IReportRow[] Rows { get; set; }
    //    public abstract string Title { get; set; }
    //    public static readonly int DEFAULT_REPORT_LENGTH = 30;

    //    public IEnumerable<string> GetLines()
    //    {
    ////        var properties 
    //        // return each line of the report as a string
    //        for (int i = 0; i < this.Rows.Length; i++)
    //        {
    //            // dont serialize but make it look nice. 
    //        }


    //        throw new NotImplementedException();
    //    }
    //}

    //public class OccupancyReport : Report
    //{
    //   // public OccupancyReportRow[] Rows;
    //    private double? _AverageOccupancyRate;
    //    //OccupancyReportRow[] Rows { get; set; }
    //    public override string Title { get; set; }
    //    public override IReportRow[] Rows { get; set; }

    //    public OccupancyReport(DateTime start, DateTime? end = null)
    //    {
    //        if (!end.HasValue)
    //        {
    //            end = start.AddDays(DEFAULT_REPORT_LENGTH);
    //        }

    //        this.Title = string.Format("Expected Occupancy Report for {0} to {1}", start.ToShortDateString(), end?.ToShortDateString());
    //        this.Rows = new OccupancyReportRow[DEFAULT_REPORT_LENGTH];

    //        DAL dal = new DAL();

    //        IEnumerable<Day> days = dal.Read<Day>(filter: d => start <= d.Date && d.Date <= end);

    //        // todo impliment and test this filter
    //        // IEnumerable<Reservation> reservations = dal.Read<Reservation>(filter: r => !(r.End < start || end < r.Start));
    //        IEnumerable<Reservation> reservations = dal.Read<Reservation>(filter: r => r.Status != ReservationStatus.Cancelled);

    //        for (int i = 0; i < this.Rows.Length; i++)
    //        {
    //            DateTime dateTime = start.AddDays(i);
    //            Day day = days.Where(d => d.Date == dateTime).FirstOrDefault();

    //            var row = new OccupancyReportRow(dateTime);

    //            if (day != null)
    //            { 
    //                for (int j = 0; j < day.Rooms.Length; j++)
    //                {
    //                    Room room = day.Rooms[j];

    //                    if (!room.IsOpen && room.ResId.HasValue)
    //                    {
    //                        int resId = day.Rooms[j].ResId.Value;

    //                        Reservation res = reservations.Where(r => r.Id == resId).FirstOrDefault();

    //                        if (res == null)
    //                        {
    //                            // well we have a big problem here
    //                            throw new Exception(string.Format("Reservation {0} is invalidly stored in the Day record", resId));
    //                        }

    //                        switch (res.Type)
    //                        {
    //                            case ReservationType.Prepaid:
    //                                row.Prepaid++;
    //                                break;
    //                            case ReservationType.SixtyDay:
    //                                row.SixtyDay++;
    //                                break;
    //                            case ReservationType.Conventional:
    //                                row.Conventional++;
    //                                break;
    //                            case ReservationType.Incentive:
    //                                row.Incentive++;
    //                                break;
    //                            default:
    //                                throw new Exception("Unknown ReservationType " + res.Type);
    //                        }
    //                    }
    //                }
    //            }

    //            this.Rows[i] = row;
    //        }
    //    }

    //    public double AverageOccupancyRate
    //    {
    //        get
    //        {
    //            if (_AverageOccupancyRate.HasValue)
    //            {
    //                int sum = 0;

    //                for (int i = 0; i < this.Rows.Length; i++)
    //                {
    //                    var row = (OccupancyReportRow)Rows[i];

    //                    sum += row.Prepaid;
    //                    sum += row.SixtyDay;
    //                    sum += row.Conventional;
    //                    sum += row.Incentive;
    //                }

    //                return sum / Rows.Count();
    //            }
    //            return _AverageOccupancyRate.Value;
    //        }
    //    }

    //}
}
