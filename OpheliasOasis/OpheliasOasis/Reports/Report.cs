using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Oasis.Reports
{
    public interface IReportRow
    {
        DateTime Date { get; set; }
    }

    public interface IReport
    {
        string Title { get; }
        IEnumerable<string> ColumnNames { get; }
        IEnumerable<IReportRow> Rows { get; }
        IEnumerable<Statistic<dynamic>> Statistics { get; }
    }

    public class Statistic<T>
    {
        public string Name;
        public T Value;
        public Func<T, string> Format = null;
    }

    public abstract class HotelReport<TRow> : IReport 
        where TRow : IReportRow
    {
        public DateTime Start;
        public DateTime? End;
        public static readonly short DEFAULT_REPORT_LENGTH = 30;

        private IEnumerable<Models.Day> _Days;
        private IEnumerable<Models.Reservation> _Reservations;
        public IEnumerable<Models.Day> Days
        {
            get
            {
                if (_Days == null)
                {
                    _Days = new IO.DAL().Read<Models.Day>(filter: d => Start <= d.Date && d.Date <= End);
                }

                return _Days;
            }
        }
        public IEnumerable<Models.Reservation> Reservations
        {
            get
            {
                if (_Reservations == null)
                {
                    _Reservations = new IO.DAL().Read<Models.Reservation>(filter: r =>
                        !(r.End < Start || End < r.Start) 
                        && r.Status == Models.ReservationStatus.Active);
                }

                return _Reservations;
            }
        }
        public virtual IEnumerable<IReportRow> Rows { get; }
        public virtual IEnumerable<Statistic<dynamic>> Statistics { get; }

        private IEnumerable<PropertyInfo> RowProperties
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

        public virtual IEnumerable<string> SampleOutput
        {
            get
            {
                var lines = new List<string>();

                lines.Add(Title);
                lines.Add(String.Join(", ", ColumnNames));
                lines.AddRange(Rows.Select(r => JsonConvert.SerializeObject(r)));
                lines.AddRange(this.Statistics.Select(s => (string)(s.Name + ": " + s.Format(s.Value))));

                return lines;
            }
        }
    }
}