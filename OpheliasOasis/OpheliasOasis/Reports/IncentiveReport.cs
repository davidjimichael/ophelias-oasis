using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oasis.Reports
{
    public class IncentiveReportRow : IReportRow
    {
        public DateTime Date { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public class IncentiveReport : HotelReport<IncentiveReportRow>
    {
        public override string Title => "Incentive " + base.Title;
        public override IEnumerable<IReportRow> Rows => throw new NotImplementedException();

        public override IEnumerable<Summary<dynamic>> Summaries
        {
            get
            {
                throw new NotImplementedException();
            }
        }

    }
}
