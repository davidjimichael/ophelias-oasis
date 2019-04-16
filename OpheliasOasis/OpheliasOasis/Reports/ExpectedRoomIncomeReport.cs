using System;
using System.Collections.Generic;

namespace OpheliasOasis.Reports
{
    public class ExpectedRoomIncomeReportRow : IReportRow
    {
        public DateTime Date { get; set; }
        public double NightlyIncome { get; set; }
    }

    public class ExpectedRoomIncomeReport : HotelReport<ExpectedRoomIncomeReportRow>
    {
        public override string Title => "Expected Room Income " + base.Title;
        public override IEnumerable<IReportRow> Rows => throw new NotImplementedException();

        public override IEnumerable<Statistic<dynamic>> Statistics
        {
            get
            {
                var stats = new List<Statistic<dynamic>>();

                // todo calculate total income for each night in the range 

                return stats;
            }
        }

    }
}
