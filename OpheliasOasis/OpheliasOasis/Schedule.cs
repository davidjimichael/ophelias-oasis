//using Oasis.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Oasis.IO;

//namespace Oasis.BusinessLogic
//{
//    public static class Schedule
//    {
//        static IEnumerable<Day> Days;

//        public static bool SetBaseRates(DateTime start, DateTime end, int rate)
//        {
//            if (rate <= 0)
//            {
//                //return FileBoundary.SetBaseRates(start, end, rate);
//            }

//            return false;
//        }

//        public static BaseRate[] GetBaseRates(DateTime start, DateTime end)
//        {
//            try
//            {
//                //var baseRates = FileBoundary.GetBaseRates();

//                return new BaseRate[] { };
//            }
//            catch (Exception e)
//            {
//                if (Program.Employee == 2)
//                {
//                    Console.WriteLine(e.Message);
//                }
//                return new BaseRate[] { };
//            }
//        }

//        public static bool BookReservation(Reservation res)
//        {
//            if (GetFirstAvailibleRoom(res.Start, res.End) is int roomNumber)
//            {
//                var day = Days.Where(d => d.Date.Equals(res.Start)).FirstOrDefault();
//                if (day == null)
//                {
//                    // we do not have records for the scheduled reservation date
//                    // cannot insert the reservation where the hotel "doesn't exist"
//                    return false;
//                }

//                // next step is to insert the reservation to a room for each day res is there
//                for (int i = 0; i < (res.End - res.Start).Days; i++)
//                {
//                    var room = Days.ElementAt(i).Rooms.ElementAt(roomNumber);

//                    room.ResId = res.Id;

//                    res.IsActive = true;
//                }

//                // room is booked with attached reservation 
//                return true;
//            }
//            return false;
//        }

//        public static bool CancelReservation(Reservation res)
//        {
//            throw new NotImplementedException();
//            // loop over days during the reservation
//            foreach (Day day in Days.Where(d => res.Start <= d.Date && d.Date <= res.End))
//            {
//                //var ids = day.GetActiveReservationIds();
                
//                //if (ids.Contains(res.Id))
//                //{
//                //    var room = day.Rooms[res.RoomNumber];

//                //    room.ReservationId = null;

//                //    res.IsActive = false;
//                //    res.RoomNumber = -1;
//                //}
//            }

//            return false;
//        }

//        public static bool ChangeReservation(Reservation res, DateTime newStart, DateTime newEnd)
//        {
//            // check for an open room and dont change if there is no open rooms
//            if (GetFirstAvailibleRoom(newStart, newEnd) is int roomNumber)
//            {
//                // just cancel anything related to this one
//                CancelReservation(res);

//                // change the dates and rebook
//                res.Start = newStart;
//                res.End = newEnd;

//                BookReservation(res);

//                return true;
//            }
//            return false;
//        }

//        public static int? GetFirstAvailibleRoom(DateTime start, DateTime end)
//        {
//            throw new NotImplementedException();
//            int dayLength = (end - start).Days;

//            IEnumerable<int> roomNumbers = null; // leave as null

//            // filter out unavailible rooms
//            for (int i = 0; i < dayLength; i++)
//            {
//                var dayDuringReservation = Days.ElementAt(i);

//                //if (dayDuringReservation.HasOpenRooms())
//                //{
//                //    roomNumbers = dayDuringReservation.GetAvailibleRoomNumbers(roomNumbers);
//                //}

//                if (roomNumbers.Count() < 1)
//                { 
//                    // escape early there's no availible rooms for the whole time span
//                    return null;
//                }
//            }

//            // at this point roomNumbers should have all rooms (>=1) open for the length of the reservation
//            // so grab the first open room
//            return roomNumbers.FirstOrDefault();
//        }
//    }
//}