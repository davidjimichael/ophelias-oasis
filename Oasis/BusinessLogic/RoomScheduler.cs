using Oasis.Models;
using System;
using System.Linq;

namespace Oasis.BusinessLogic
{
    class RoomScheduler
    {
        Room[] Rooms;

        public RoomScheduler(int roomCount)
        {
            if (roomCount > 0)
            {
                Rooms = new Room[roomCount];

                for (int i = 0; i < roomCount; i++)
                {
                    Rooms[i] = new Room();
                }
            }
        }

        public bool InsertReservation(Reservation res)
        {
            var roomNumber = FindAvailibleRoom(res);

            if (roomNumber >= 0)
            {
                res.RoomNumber = roomNumber;
                Rooms[roomNumber].Reservations.Append(res);
                return true;
            }
            return false;
        }

        // Change Reservation
        public bool ChangeReservation(DateTime newStart, DateTime newEnd, String search)
        {
            var reservation = CancelReservation(search);
            if (reservation != null)
            {
                // set new reservation specifics
                reservation.Start = newStart;
                reservation.End = newEnd;

                return InsertReservation(reservation);
            }
            return false;
        }

        public Reservation CancelReservation(String search)
        {
            var room = Rooms.Where(r => r.ContainsReservation(search)).FirstOrDefault();
            
            if (room != null)
            {
                // find old reservation and remove
                var reservation = room.Reservations.Where(r => r.Matches(search, search, search)).FirstOrDefault();
                room.Reservations.Remove(room.Reservations.Where(r => r.Matches(search, search, search)).First());

                return reservation;
            }
            return null;
        }

        public Reservation[] GetCheckedInReservations(DateTime date)
        {
            return Rooms.Where(r => r.IsOccupiedOn(date))
                .Select(r => r.GetReservationFor(date))
                .ToArray();
        }

        private int FindAvailibleRoom(Reservation res)
        {
            for (int i = 0; i < Rooms.Length; i++)
            {
                if (Rooms[i].IsAvailibleFor(res.Start, res.End)) {
                    return i;
                }
            }
            return -1;
        }
    }
}
