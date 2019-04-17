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

    /*
    The expected room income report is a one-page report that shows the expected
income from the rooms each night for the next 30 days.  Each line of the report shows the
date and the expected income for that night.  The last two lines of the report are the total
income for that period and the average income for that period.

        Shows the expected income from rooms reserved for the next 30 days
Each line shows:
Date
Expected Income
Last 2 lines are total income for that period and average income for that period

     */
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

                int length = (End - Start).Value.Days + 1;

                for (int i = 0; i < length; i++)
                {
                    
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
