using Oasis.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oasis.Models
{
    public class SixtyDayReservation : Reservation
    {
        public SixtyDayReservation(DateTime start, DateTime end, CreditCard card, string name, string email, int roomPreference) : base(start, end, card, name, email, roomPreference)
        {
        }

        public override int CalculateRefundPenalty()
        {
            throw new NotImplementedException();
        }

        public override bool ExecDailyTasks()
        {
            if (DateTime.Now.AddDays(45) == this.Start)
            {
                // send reservation reminder for 15 days
                return true;
            }

            return false;
        }

        public override double Multiplier()
        {
            return 0.85;
        }
    }

    public abstract class Reservation
    {
        public bool IsActive;
        public bool IsCharged;
        public int PenaltyAmount;
        public int RoomNumber;
        public string Name;
        public string Email;
        public DateTime Start;
        public DateTime End;
        public DateTime? CheckIn;
        public BaseRate[] BaseRates;
        public CreditCard CreditCard;

        public Reservation(DateTime start, DateTime end, CreditCard card, string name, string email, int roomPreference)
        {
            this.IsActive = true;
            this.IsCharged = false;
            this.PenaltyAmount = 0; ;
            this.RoomNumber = -1;
            this.Name = name;
            this.Email = email;
            this.Start = start;
            this.End = end;
            this.CheckIn = null;
            this.BaseRates[] = (new BaseRateScheduler(start, end).GetBaseRates());
            this.CreditCard = card;
        }

        // override for different reservation multipliers
        public abstract double Multiplier();

        public abstract bool ExecDailyTasks();

        public abstract int CalculateRefundPenalty();

        public bool Matches(string name, string email, string cardNumber)
        {
            return (this.Name.Equals(name) || this.Email.Equals(email) || this.CreditCard.Number.Equals(cardNumber));
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, Start: {1}, End: {2}", Name, Start.ToShortDateString(), End.ToShortDateString());
        }
    }
}
