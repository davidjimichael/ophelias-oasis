using System;
using System.Collections.Generic;
using System.Linq;

namespace Oasis.Reports
{
    public class ExpectedRoomIncomeReportRow : IReportRow
    {
        public DateTime Date { get; set; }
        public double NightlyIncome { get; set; }
    }
    public class ExpectedRoomIncomeReport : HotelReport<ExpectedRoomIncomeReportRow>
    {
        public override string Title => "Expected Room Income " + base.Title;

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
                var rows = new List<ExpectedRoomIncomeReportRow>();
                var days = DAL.Read<Models.Day>(filter: d => Start <= d.Date && d.Date <= End);

                for (int i = 0; i < days.Count(); i++)
                {
                    var ids = days.ElementAt(i).Rooms
                        .Where(r => !r.IsOpen())
                        .Select(r => r.ResId);

                    var reservations = DAL.Read<Models.Reservation>(filter: r => ids.Contains(r.Id));

                    var income = reservations.Select(r => r.BaseRates.Sum() * r.Multiplier).Sum();

                    rows.Add(new ExpectedRoomIncomeReportRow
                    {
                        Date = Start.AddDays(i),
                        NightlyIncome = income,
                    });
                }

                _Rows = rows.OrderBy(r => r.NightlyIncome);

                return _Rows;
            }
        }
        public override IEnumerable<Summary<dynamic>> Summaries
        {
            get
            {
                var stats = new List<Summary<dynamic>>();

                var tis = GetTotalIncomeSummary();
                var ais = GetAverageIncomeSummary(tis.Value);

                return stats;
            }
        }

        private Summary<dynamic> GetAverageIncomeSummary(double totalIncomeForPeriod)
        {
            var length = (End - Start).Value.Days + 1;

            var averageIncomeForPeriod =  totalIncomeForPeriod / length;

            var summary = new Summary<dynamic>()
            {
                Value = averageIncomeForPeriod,
                Format = avg => string.Format("{0:N2}%", (double)avg * 100.0),
                Name = "Average Income",
            };

            return summary;
        }

        private Summary<dynamic> GetTotalIncomeSummary()
        {
            double totalIncome = 0;

            for (int i = 0; i < Rows.Count(); i++)
            {
                var row = (ExpectedRoomIncomeReportRow)Rows.ElementAt(i);

                totalIncome += row.NightlyIncome;
            }

            var totalIncomeForPeriod = new Summary<dynamic>()
            {
                Name = "Total Income",
                Value = totalIncome,
                Format = ti => string.Format("{0:N2}", (double)ti),
            };

            return totalIncomeForPeriod;
        }
    }
}
