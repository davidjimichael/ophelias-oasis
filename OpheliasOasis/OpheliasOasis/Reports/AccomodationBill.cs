using System;
using System.Collections.Generic;
using System.Linq;

namespace Oasis.Reports
{
    public class AccomodationBillReportRow : IReportRow
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public int Room { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public DateTime? PaidOn { get; set; }
        public int Nights { get; set; }
        public double BillAmount { get; set; }
        public double AmountPaid { get; set; }
    }

    public class AccomodationBill : HotelReport<AccomodationBillReportRow>
    {
        public override string Title => "Accomodation (Bill) " + base.Title;

        private IEnumerable<IReportRow> _Rows { get; set; }

        public int ResId { get; set; }

        public override IEnumerable<IReportRow> Rows
        {
            get
            {
                if(_Rows != null)
                {
                    return _Rows;
                }

                var dal = new IO.DAL();
                int length = (End - Start).Value.Days + 1;
                var rows = new AccomodationBillReportRow[length];
                int offset = 0;

                var res = this.Reservations.Where(r => r.Id == this.ResId).First();

                rows[offset] = new AccomodationBillReportRow()
                {
                    Date = Start.AddDays(offset),
                    BillAmount = (res.PenaltyCharge ?? 0) + (res.Multiplier * res.BaseRates.Sum()),
                    AmountPaid = (res.AmountPaid ?? 0.0),
                    CheckIn = res.CheckIn,
                    CheckOut = res.CheckOut,
                    Name = res.Name,
                    Nights = length,
                    PaidOn = res.PaidOn,
                    Room = res.Room,
                };

                _Rows = _Rows.Concat(rows);

                return _Rows;
            }
        }
    }
}
