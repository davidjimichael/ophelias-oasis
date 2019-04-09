using Oasis.IO;
using System;
using System.Linq;

namespace Oasis.Models
{
    public enum ReservationStatus
    {
        Active,
        Changed,
        Cancelled,
    }

    // we're keeping this around because subclassing is more complicated to setup and with our time span im not feeling it right now, might change later (probably not)
    public enum ReservationType
    {
        SixtyDay,
        Conventional,
        Prepaid,
        Incentive,
    }

    public class Reservation
    {
        public string Name;
        public string Email;
        public DateTime Start;
        public DateTime End;
        public DateTime? CheckIn;
        public DateTime? CheckOut;
        public DateTime? Paid;
        public readonly int Id;
        public int Penalty;
        public double[] BaseRates;
        public double Multiplier;
        public ReservationType Type;
        public ReservationStatus Status;
        private bool _IsNoShow;

        public bool IsNoShow
        {
            get
            {
                return _IsNoShow;
            }
            
            set
            {
                // check prevents double charging for guests who don't show up for >1 days past check in.
                if (!this._IsNoShow)
                {
                    this._IsNoShow = value;

                    if (this._IsNoShow)
                    {
                        this.Penalty += Hotel.LATE_CHECK_IN_PENALTY;
                    }
                }
            }
        }

        // todo remove default reservation type parameter, also there dztbhneeds to be a lot more validation on what happens with this
        // reason I've been holding off on reservation validation is because I'm not sure what the best way to "build"
        // reservations is, we might want to look into a factory of sorts because the credit card rules mess with having a public constructor like this. 
        // at least as surface level because yes there are other ways to do it (e.g. more constructors for each type or something like that)
        public Reservation(int id, string name, string email, DateTime start, DateTime end, ReservationType type = ReservationType.Conventional)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException("Reservation Id cannot be negative");
            }

            // get baserates for length of stay
            var rates = new DAL().Read<Day>(filter: d => start <= d.Date && d.Date <= end)
                .Select(d => d.Rate)
                .ToArray();

            Id = id;
            Name = name;
            Email = email;
            Start = start;
            End = end;
            Penalty = 0;
            BaseRates = rates;
            // todo add validation on creating a reservation with prepaid types at certain number of days away from start. 
            Type = type;
            Status = ReservationStatus.Active;
            Multiplier = GetMultiplier(type);
        }


        private static double GetMultiplier(ReservationType type)
        {
            // todo change these to represent accurate multipliers
            switch (type)
            {
                case ReservationType.Conventional:
                    return 1;
                case ReservationType.Prepaid:
                    return 0.75;
                case ReservationType.SixtyDay:
                    return 0.85;
                case ReservationType.Incentive:
                    return 1; // todo incentive calculation here
                default:
                    throw new NotImplementedException("Unknown ReservationType: " + type);
            }
        }
    }
}