using Oasis.BusinessLogic;
using Oasis.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oasis.Models
{
    public class Reservation
    {
        public string Name;
        public string Email;
        public DateTime Start;
        public DateTime End;
        public readonly int Id;

        public Reservation(int id, string name, string email, DateTime start, DateTime end)
        {
            Id = id;
            Name = name;
            Email = email;
            Start = start;
            End = end;
        }
    }
}

/*
namespace Oasis.Models
{
    public class PrepaidReservation : Reservation
    {
        public override bool ExecDailyTasks()
        {
            throw new NotImplementedException();
        }

        public override bool Validate(dynamic required)
        {
            // reservation attempt not made 45 days in advance
            if (DateTime.Now.AddDays(45) > required.Start)
            {
                return false;
            }
        }
    }

    public abstract class Reservation
    {
        public bool IsActive;
        //public bool IsCharged;
        public int Id;
        public int PenaltyAmount;
        public int RoomNumber;
        public string Name;
        public string Email;
        public DateTime Start;
        public DateTime End;
        public DateTime? CheckIn;
        public int[] BaseRates;
        public CreditCard CreditCard;

        public static Reservation Build(ReservationType type, DateTime start, DateTime end, string name, string email, CreditCard card = null)
        {
            // validation checks for each reservation type
        }

        // change to int id, Type t, Details d, Card c
        //public Reservation(int id, DateTime start, DateTime end, CreditCard card, string name, string email)
        //{
        //    var baseRates = IOBoundary.Get<Day, int>(
        //        filter: d => start <= d.Date && d.Date <= end,
        //        map: d => d.Rate
        //    ).ToArray();

        //    this.Id = id;
        //    this.IsActive = true;
        //    this.IsCharged = false;
        //    this.PenaltyAmount = 0;
        //    this.RoomNumber = -1;
        //    this.Name = name;
        //    this.Email = email;
        //    this.Start = start;
        //    this.End = end;
        //    this.CheckIn = null;
        //    this.BaseRates = baseRates;
        //    this.CreditCard = card;
        //}

        // override for different reservation multipliers
        public virtual int ChangeCriticalDate { get => 3; }
        public virtual int BookingDayOffset { get => 0; }
        public virtual double Rate { get => 0.85; }
        public virtual double ChangePenaltyRate { get => 1.10; }

        public virtual bool Validate(DateTime start, dynamic otherInto)
        {
            return start > DateTime.Now.AddDays(BookingDayOffset);
        }

        public bool CheckIn(DateTime checkIn)
        {
            if (Start <= checkIn && checkIn <= End)
            {
                if (checkIn > Start)
                {
                    // check in attempted post required date => charge penalty
                    PenaltyAmount++;
                    CheckIn = checkIn; // still allow them to check in late
                    return true;
                }
            }
            return false;
        }

        public virtual bool ExecDailyTasks()
        {
            // check 
            return false;
        }

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
*/