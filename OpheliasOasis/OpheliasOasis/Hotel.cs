using Oasis.IO;
using Oasis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Oasis
{
    public class Hotel
    {
        private int NextId;
        private IDAL Dal;
        private Timer Timer;
        private static readonly int LATE_CHECK_IN_PENALTY = 10;
        // room count
        // defaults etc....

        public Hotel()
        {
            // todo make sure to set nextId to the current max id value + 1
            NextId = 0;
            Dal = new DAL();
            // todo - there are 86400000 milliseconds in one day uncomment this and change the value
            // timer = new Timer(DailyActivities, null, 0, 86400000);
        }

        // todo remove after testing
        public void TriggerDailyActivities()
        {
            DailyActivities(null);
        }

        private void DailyActivities(object state)
        {
            if (Program.Employee == 2)
            {
                Console.WriteLine("Tick: {0}, Timer State: {1}", DateTime.Now, state);
            }
            // get all current reservations split by if they are checked in
            var reservations = Dal.Read<Reservation>();
            //var bookedReservations = reservations.Where(r => r.CheckIn == null);
            //var checkedInReservations = reservations.Where(r => r.CheckIn != null);
            // perform daily activities for each booked not checked in reservation

            // it is after reservation start and the guest has not check in.
            Func<Reservation, bool> _isLateCheckIn = r => r.CheckIn == null && r.Start < DateTime.Now;
            Func<Reservation, Reservation> _addPenalty = r =>
            {
                r.Penalty += LATE_CHECK_IN_PENALTY;
                return r;
            };

            Dal.Update<Reservation>(_addPenalty, _isLateCheckIn);

            // todo add sixty day penalities and reminder emails
        }

        public bool Reset()
        {
            Dal.Delete<Reservation>();
            Dal.Delete<CreditCard>();

            bool updated = Dal.Update<Day>(
                update: day =>
                {
                    day.Rooms = day.Rooms.Select(r => new Room()).ToArray();
                    day.Rate = 0;
                    return day;
                });

            return updated;
            //try
            //{
                //var days = IOBoundary.Get<Day
                
                //foreach (Day day in days)
                //{
                //    day.Rate = 0;

                //    foreach (Room room in day.Rooms)
                //    {
                //        room.ResId = null;
                //    }
                //}

                //return IOBoundary.Set<Day>(days) && IOBoundary.Set<Reservation>(new Reservation[] { });
            //}
            //catch (Exception e)
            //{
            //    if (Program.Employee == 2)
            //    {
            //        Console.WriteLine(e.Message);
            //    }
            //    return false;
            //}
        }
        
        public bool SetBaseRates(DateTime start, DateTime end, int rate)
        {
            // add validation for changing this one year in advance


            /* return IOBoundary.Set<Day, DateTime>(
                 filter: d => start <= d.Date && d.Date <= end,
                 update: d =>
                 {
                     d.Rate = rate;
                     return d;
                 },
                 order: d => d.Date);
                 */
            //var days = IOBoundary.Get<Day>(filter: d => start <= d.Date && d.Date <= end)
            //    .Select(d => {
            //        d.Rate = rate;
            //        return d;
            //    })                           
            //    .ToList();

            //int length = (end-start).Days + 1;

            //for (int i = 0; i < length; i++)
            //{
            //    var date = start.AddDays(i);
            //    if (!days.Any(d => d.Date == date))
            //    {
            //        days.Add(new Day(date, null, rate));
            //    }
            //}

            //return IOBoundary.Set<Day>(days);

            Func<Day, bool> _filter = day =>
            {
                return start <= day.Date && day.Date <= end;
            };

            IEnumerable<Day> prev = Dal.Read<Day>(_filter);

            // update all current days baserates
            var updated = Dal.Update<Day>(
               update: day =>
               {
                   day.Rate = rate;
                   return day;
               },
               filter: _filter);

            if (!updated)
            {
                // failed to update current days
                return false;
            }

            int numberOfDaysToChange = (end - start).Days;
            if (prev.Count() < numberOfDaysToChange)
            {
                // we currently do not have all the baserates we need saved bc the days dont exist
                // figure out which ones are missing from the file and then add them

                List<Day> toAdd = new List<Day>();
                var dates = prev.Select(day => day.Date);

                for (int i = 0; i < numberOfDaysToChange; i++)
                {
                    var date = start.AddDays(i);

                    if (!dates.Contains(date))
                    {
                        // create a new day, null out the rooms because they should not have any reservations
                        toAdd.Add(new Day(date, null, rate));
                    }
                }

                return Dal.Create<Day, DateTime>(toAdd, orderBy: day => day.Date);
            }

            return true;
        }

        /// <summary>
        ///     Return reservations that start within or on this timeframe
        /// </summary>
        public IEnumerable<Reservation> GetReservationsDuring(DateTime start, DateTime? end = null)
        {
            if (end == null)
            {
                end = start;
            }
            return Dal.Read<Reservation>(filter: r => start <= r.Start && r.End <= end);
            //return dal.Read<Reservation>(filter: r => start <= r.Start && r.Start <= end);
        }

        public bool CancelReservation(int id)
        {
            if (id < 0)
            {
                // invalid id 
                return false;
            }
            // get all reservations to save
            // var reservations = IOBoundary.Get<Reservation>(filter: r => r.Id != id);
            var res = Dal.Read<Reservation>(filter: r => r.Id == id).FirstOrDefault();

            if (res == null)
            {
                // reservation didnt exist
                return false;
            }

            bool updated = Dal.Update<Day>(
                update: day =>
                {
                    for (int i = 0; i < day.Rooms.Length; i++)
                    {
                        if (day.Rooms[i].ResId == res.Id)
                        {
                            day.Rooms[i].ResId = null;
                        }
                    }
                    return day;
                });
            
            // return that one reservation is deleted
            bool deleted = Dal.Delete<Reservation>(filter: r => r.Id == id).Count() == 1;

            return updated && deleted;
            // get all days within this daterange
            //var days = IOBoundary.Get<Day>(filter: d => d.Rooms.Any(r => r.ResId == id));

            // reset the room occupancies 
            //foreach (Day day in days)
            //{
            //    foreach (Room room in day.Rooms)
            //    {
            //        if (room.ResId == id)
            //        {
            //            room.ResId = null;
            //        }
            //    }
            //}

            //var dates = days.Select(d => d.Date);
            //var unmodifiedDays = IOBoundary.Get<Day>(filter: d => !dates.Contains(d.Date));
            //days = days.Concat(unmodifiedDays).OrderBy(d => d.Date);

            // update both report success status
            // todo fix error where days are set only to reset days
            //return IOBoundary.Set<Day>(days) && IOBoundary.Set<Reservation>(res);
            // return false;
        }

        public bool AddCreditCard(int resId, CreditCard card)
        {
            if (resId < 0)
            {
                // invalid id 
                return false;
            }

            // check red id has card
            Reservation res = Dal.Read<Reservation>(r => r.Id == resId).FirstOrDefault();

            if (res == null)
            {
                // unable to find reservation
                return false;
            }

            if (!card.IsValid())
            {
                // credit card is invalid by its own definition 
                return false;
            }

            // attach this card to the reservation
            card.ResId = res.Id;
            
            return Dal.Create(new[] { card });
        }

        public bool BookReservation(string name, string email, DateTime start, DateTime end)
        {
            // todo commented out for testing purposes
            // if (start < DateTime.Now || end < start || name == "" || email == "")
            // {
            //     return false;
            // }

            // todo need to set the reservation base rates and  what not 
            var res = new Reservation(NextId++, name, email, start, end);

            // create filters for daterange and room availibility
            Func<Day, bool> _withinDateRange = d => start <= d.Date && d.Date <= end;
            Func<Day, bool> _hasOpenRoom = d => d.Rooms.Any(r => r.IsOpen);
            Func<Day, bool> _filter = d => _hasOpenRoom(d) && _withinDateRange(d);
            // todo add the functionality so a reservation thats attempted for a un saved base rate date is then added

            // var days = IOBoundary.Get<Day>(_filter);
            IEnumerable<Day> days = Dal.Read<Day>(_filter);
            // the below line should be part of the reservation constructor...
            //var baseRates = days.Select(d => d.Rate);
            // add one to compensate for exclusive vs inclusive inequalities in date comparison 
            if (days.Count() != (end - start).Days + 1)
            {
                // not enough rooms availible for length of reservation
                return false;
            }

            // add all availible room numbers (assume ever room is open until proven otherwise)
            var availibleRooms = new List<int>();

            for (int i = 0; i < 45; i++)
            {
                availibleRooms.Add(i);
            }

            // insert the reservation into the same room (by number) across all days of the stay
            // loop over each day of the reservation
            for (int i = 0; i < days.Count(); i++)
            {
                var day = days.ElementAt(i);

                for (int j = 0; j < day.Rooms.Length; j++)
                {
                    if (!day.Rooms[j].IsOpen)
                    {
                        availibleRooms.Remove(j);
                    }
                }
            }

            // at this point availibleRooms will have the index (room number) of an availible room for the hotel
            if (availibleRooms.Count() == 0)
            {
                // no rooms availible for the ENTIRE length of the stay
                return false;
            }

            int roomNumber = availibleRooms.First();

            // reserve room for this reservation
            // foreach (var day in days)
            // {
            //     day.Rooms[roomNumber].ResId = res.Id;
            // }
            // 
            //IOBoundary.Set<Day>()
            //IOBoundary.Set<Day, DateTime>(
            //    _filter,
            //    update: d =>
            //    {
            //        d.Rooms[roomNumber].ResId = res.Id;
            //        return d;
            //    },
            //    order: d => d.Date);

            Dal.Update(day =>
            {
                day.Rooms[roomNumber].ResId = res.Id;
                return day;
            }, _filter);

            //IOBoundary.Add<Reservation, int>(
            //    items: new [] { res },
            //    order: r => r.Id);
            Dal.Create(new[] { res }, orderBy: r => r.Id);
            // todo set the updated days
            // throw new NotImplementedException();
            return true;
        }

        public bool ChangeReservation(int id, DateTime start, DateTime end)
        {
            if (id < 0)
            {
                // invalid id 
                return false;
            }

            var reservation = Dal.Read<Reservation>(filter: r => r.Id == id).FirstOrDefault();

            if (reservation == null)
            {
                // error finding reservation
                return false;
            }

            // try to cancel the reservation first incase the change is an extension
            if (!CancelReservation(id))
            {
                // unable to cancel, possibly cant find reservation
                return false;
            }

            if (!BookReservation(reservation.Name, reservation.Email, start, end))
            {
                // unable to book the new reservation
                return false;
            }

            return true;
        }
    }
}