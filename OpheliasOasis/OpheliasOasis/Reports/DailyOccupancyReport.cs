using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oasis.Reports
{
    public class DailyOccupancyReportRow : IReportRow
    {
        public DateTime Date { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public class DailyOccupancyReport : HotelReport<DailyOccupancyReportRow>
    {
        public override string Title => "Incentive " + base.Title;
        public override IEnumerable<IReportRow> Rows => throw new NotImplementedException();

        public override IEnumerable<Statistic<dynamic>> Statistics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

    }
}
