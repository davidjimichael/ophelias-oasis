using System;
using System.Collections.Generic;
using System.Linq;

namespace Oasis.Reports
{
    public class IncentiveReportRow : IReportRow
    {
        public DateTime Date { get; set; }
        public double Discount { get; set; }
    }

    public class IncentiveReport : HotelReport<IncentiveReportRow>
    {
        public override string Title => "Incentive " + base.Title;

        private List<IncentiveReportRow> _Rows;

        public override IEnumerable<IReportRow> Rows
        {
            get
            {
                if (_Rows != null)
                {
                    return _Rows;
                }

                var rows = new List<IncentiveReportRow>();
                
                var length = (End - Start).Value.Days + 1;

                for (int i = 0; i < length; i++)
                {
                    double rate = Days.ElementAt(i).Rate;

                    rows.Add(new IncentiveReportRow()
                    {
                        Date = Start.AddDays(length),
                        Discount = rate * 0.20,
                    });
                }
                
                _Rows = rows.OrderBy(r => r.Date).ToList();

                return _Rows;
            }
        }
        
        public override IEnumerable<Summary<dynamic>> Summaries
        {
            get
            {
                var stats = new List<Summary<dynamic>>();

                var tid = GetTotalIncentiveDiscount();
                var aid = GetAverageIncentiveDiscount(tid.Value);

                return stats;
            }
        }

        private Summary<dynamic> GetAverageIncentiveDiscount(double tid)
        {
            var length = (End - Start).Value.Days + 1;

            var aid = tid / length;

            var summary = new Summary<dynamic>()
            {
                Value = aid,
                Format = avg => string.Format("{0:N2}", (double)avg),
                Name = "Average Incentive Discount",
            };

            return summary;
        }

        private Summary<dynamic> GetTotalIncentiveDiscount()
        {
            double tid = 0;

            for (int i = 0; i < Rows.Count(); i++)
            {
                var row = (IncentiveReportRow)Rows.ElementAt(i);

                tid += row.Discount;
            }

            var totalIncentiveDiscountForPeriod = new Summary<dynamic>()
            {
                Name = "Total Incentive Discount",
                Value = tid,
                Format = ti => string.Format("{0:N2}", (double)ti),
            };

            return totalIncentiveDiscountForPeriod;
        }
    }
}
