using System;
using System.Linq;

namespace Oasis.Models
{
    public class Reservation
    {
        public int Id;
        public int Room;
        public int? ChangedFrom;
        public double? PenaltyCharge;
        public int? PaymentId;
        public string Name;
        public string Email;
        public DateTime Start;
        public DateTime End;
        public DateTime PayBy;
        public DateTime? CheckIn;
        public DateTime? CheckOut;
        public DateTime? PaidOn;
        public double[] BaseRates;
        public double Multiplier;
        public double? AmountPaid;
        public Models.ReservationType Type;
        public Models.ReservationStatus Status;
        public bool IsNoShow;
        
        public static Reservation New(int id, Models.ReservationType type, DateTime start, DateTime end, string name, string email = "", int? paymentId = null, int? changedFrom = null)
        {
            var res = new Reservation
            {
                Id = id,
                Type = type,
                Start = start,
                End = end,
                Email = email,
                Name = name,
                ChangedFrom = changedFrom,
                PenaltyCharge = null,
                CheckIn = null,
                CheckOut = null,
                PaidOn = null,
                AmountPaid = null,
                PaymentId = null,
                Status = ReservationStatus.Active,
                IsNoShow = start < DateTime.Now,
                PayBy = GetPayByDate(type, start, end),
                Multiplier = GetMultiplier(start, end, type, changedFrom),
                BaseRates = new IO.DAL().Read<Day>(filter: d => start <= d.Date && d.Date <= end).Select(d => d.Rate).ToArray(),
            };

            return res;
        }

        private static double GetMultiplier(DateTime start, DateTime end, ReservationType type, int? changedFrom)
        {
            if (changedFrom != null)
            {
                return 1.1;
            }

            switch (type)
            {
                case ReservationType.Conventional: return 1;
                case ReservationType.Prepaid: return 0.75;
                case ReservationType.SixtyDay: return 0.85;
                case ReservationType.Incentive: return GetRateFor(start, end);
                default: throw new NotImplementedException("Unknown ReservationType: " + type);
            }
        }

        private static double GetRateFor(DateTime start, DateTime end)
        {
            double avgOccupancy = (double)new Hotel().GetExpectedOccupancyReport(start, end).Summaries.First().Value;

            if (avgOccupancy > 0.60)
            {
                return 1.0;
            }

            return 0.80;
        }

        private static DateTime GetPayByDate(ReservationType type, DateTime start, DateTime end)
        {
            switch (type)
            {
                case ReservationType.Conventional: return end;
                case ReservationType.Incentive: return end;
                case ReservationType.Prepaid: return start;
                case ReservationType.SixtyDay: return start.AddDays(-30); // 30 days before start
                default: throw new NotImplementedException("Unknown ReservationType: " + type);
            }
        }
    }
}

namespace Oasis.Models
{
    public enum ReservationStatus
    {
        Active,
        Changed,
        Cancelled,
    }
    public enum ReservationType
    {
        SixtyDay,
        Conventional,
        Prepaid,
        Incentive,
    }
}