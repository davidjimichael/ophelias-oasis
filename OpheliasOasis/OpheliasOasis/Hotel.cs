using Oasis.IO;
using Oasis.Models;
using OpheliasOasis.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Oasis
{
    /// <summary>
    ///     Handles everything, please don't remove any comments they are my make-shift reminders. Feel free to edit the code though if you introduce a bug
    ///     Let's pretend I'm too good to do that and it's your fault, then fix it that's all I care about rn. 
    /// </summary>
    public class Hotel
    {
        private int NextId;
        private static IDAL Dal;
        private Timer Timer;
        public static readonly int LATE_CHECK_IN_PENALTY = 10;
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
                Console.WriteLine("Tick: {0}, Timer State: {1}", DateTime.Now, state ?? "null");
            }
            // todo add the backup files part here

            // charge late penalties for no shows
            Func<Reservation, bool> _isLateCheckIn = r => r.CheckIn == null && r.Start < DateTime.Now;
            Func<Reservation, Reservation> _addPenalty = r =>
            {
                // already charged late check in penalty (if theyre two days late) just dont charge them a second time
                if (r.Penalty > 0)
                {
                    r.Penalty += LATE_CHECK_IN_PENALTY;
                }
                return r;
            };

            Dal.Update<Reservation>(_addPenalty, _isLateCheckIn);
            // get all current reservations split by if they are checked in
            //var bookedReservations = reservations.Where(r => r.CheckIn == null);
            //var checkedInReservations = reservations.Where(r => r.CheckIn != null);
            // perform daily activities for each booked not checked in reservation

            // it is after reservation start and the guest has not check in.

            // todo add sixty day penalities and reminder emails
            // read all reservations in "again"
            var reservations = Dal.Read<Reservation>();
            
            foreach (Reservation res in reservations)
            {
                PerformDailyActions(res);
            }
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
        }
        
        public bool SetBaseRates(DateTime start, DateTime end, double rate)
        {
            // add set for changing this one year in advance
            
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
                // allows for same day reservations
                end = start;
            }
            return Dal.Read<Reservation>(filter: r => start <= r.Start && r.End <= end);
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
            Func<Day, bool> _hasOpenRoom = d => d.Rooms.Any(r => r.IsOpen());
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
                    if (!day.Rooms[j].IsOpen())
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
            
            Dal.Update(day =>
            {
                day.Rooms[roomNumber].ResId = res.Id;
                return day;
            }, _filter);

            Dal.Create(new[] { res }, orderBy: r => r.Id);
            
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
                // unable to book the new reservation dates possibly due to full capacity 
                return false;
            }

            return true;
        }
        
        public static double CalculateBillTotal(Reservation res)
        {
            return (res.Multiplier * res.BaseRates.Sum()) + res.Penalty;
        }

        public static double OccupancyRateAverage(DateTime start, DateTime end)
        {
            IEnumerable<double> rates = OccupancyRates(start, end);

            return rates.Sum() / rates.Count();
        }
        
        public static IEnumerable<double> OccupancyRates(DateTime start, DateTime end)
        {
            return Dal.Read<Day>(filter: d => start <= d.Date && d.Date <= end)
                .Select(d => d.Rooms.Where(r => !r.IsOpen()).Count() / (double)d.Rooms.Length);
        }
        
        public static bool PerformDailyActions(Reservation res)
        {
            switch (res.Type)
            {
                case ReservationType.Conventional:
                    return PerformDailyActionsConventional(res);
                case ReservationType.Prepaid:
                    return PerformDailyActionsPrepaid(res);
                case ReservationType.SixtyDay:
                    return PerformDailyActionsSixtyDay(res);
                case ReservationType.Incentive:
                    return PerformDailyActionsIncentive(res);
                default:
                    throw new NotImplementedException("Unknown ReservationType");
            }
        }

        private static bool PerformDailyActionsIncentive(Reservation res)
        {
            throw new NotImplementedException();
        }

        private static bool PerformDailyActionsSixtyDay(Reservation res)
        {
            /*
            Forty-five days before their stay is due to begin, an
            e-mail is sent to the guests to inform them that the reservation has to be paid in full
            within 15 days or it will be cancelled.
             */
            throw new NotImplementedException();
        }

        private static bool PerformDailyActionsPrepaid(Reservation res)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Returns true for any reservations that were able to carry out thier daily tasks and are not no shows
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        private static bool PerformDailyActionsConventional(Reservation res)
        {
            bool isNoShow = CheckIsNoShow(res);

            // if cancelled 
            switch (res.Status)
            {
                default: return true && !isNoShow;
            }
        }

        // todo move IsNoShow setting to the get for it within reservation (if this doesn't make sense to you don't worry about it this comment is purely a reminder)
        public static bool CheckIsNoShow(Reservation res)
        {
            // past check in day and not fined already
            if (res.Start < DateTime.Now && !res.IsNoShow)
            {
                res.IsNoShow = true;
            }
            return res.IsNoShow;
        }

        // gotta love message passing smells
        public ExpectedOcupancyReport GetExpectedOcupancyReport(DateTime start, DateTime end)
        {
            return new ExpectedOcupancyReport()
            {
                Start = start,
                End = end,
            };
        }
    }
}