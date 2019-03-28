using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oasis.Models
{
    class Room
    {
        public LinkedList<Reservation> Reservations;

        public Room()
        {
            Reservations = new LinkedList<Reservation>();
        }

        public bool IsOccupiedOn(DateTime date)
        {
            return Reservations.Any(r => r.Start <= date && date <= r.End);
        }

        public bool IsAvailibleFor(DateTime checkStart, DateTime checkEnd)
        {
            return !Reservations.Any(r => checkStart < r.Start && r.End < checkEnd);
        }

        public Reservation GetReservationFor(DateTime date)
        {
            return Reservations.Where(r => r.Start <= date && date <= r.End).FirstOrDefault();
        }

        public bool ContainsReservation(string search)
        {
            return Reservations.Any(r => r.Matches(search, search, search));
        }

        public Reservation GetReservation(string search)
        {
            return Reservations.Where(r => r.Matches(search, search, search)).FirstOrDefault();
        }
    }
}
