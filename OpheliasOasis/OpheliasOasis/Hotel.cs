using Oasis.IO;
using Oasis.Models;
using Oasis.Reports;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oasis
{
    public interface IHotel
    {
        bool Login(int userId);
        bool Logout();

        bool Reset();

        bool SetBaseRates(DateTime start, DateTime end, double? rate = null);

        bool BookReservation(int type, DateTime start, DateTime end, string name, string email = null);
        bool BookReservation(ReservationType type, DateTime start, DateTime end, string name, string email = null);
        bool ChangeReservation(int resId, DateTime start, DateTime end);
        bool CancelReservation(int resId);
        bool AddCreditCard(int resId, CreditCard card);
        bool Pay(int resId, int? amount = null);
        bool CheckIn(int resId);
        bool CheckOut(int resId);
        IEnumerable<Reservation> GetReservationsDuring(DateTime start, DateTime? end = null);

        void TriggerDailyActivities();
        void DailyActivities(object state);
        bool PerformDailyActions(Reservation res);
        bool PerformDailyActionsIncentive(Reservation res);
        bool PerformDailyActionsSixtyDay(Reservation res);
        bool PerformDailyActionsPrepaid(Reservation res);
        bool PerformDailyActionsConventional(Reservation res);

        IReport GetExpectedOccupancyReport(DateTime start, DateTime? end = null);
        IReport GetExpectedRoomIncomeReport(DateTime start, DateTime? end = null);
        IReport GetIncentiveReport(DateTime start, DateTime? end = null);
        IReport GetDailyArrivalsReport(DateTime start, DateTime? end = null);
        IReport GetDailyOccupancyReport(DateTime start, DateTime? end = null);
        IReport GetAccomodationBill(int resId, DateTime start, DateTime? end = null);
    }
    public class Hotel : Oasis.IHotel
    {
        private IO.DAL DAL;
        private readonly double StandardRate = 100;
        private readonly double NoShowCharge = 15;
        private int NextResId = 0;

        public Hotel()
        {
            DAL = new IO.DAL();

            var today = DateTime.Now;

            NextResId = int.Parse(string.Format("{0}{1}", today.Year, today.DayOfYear));
        }

        public bool AddCreditCard(int resId, CreditCard card)
        {
            if (resId < 0)
            {
                // invalid id 
                return false;
            }

            // check red id has card
            Reservation res = DAL.Read<Reservation>(r => r.Id == resId && r.Status == ReservationStatus.Active).FirstOrDefault();

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
            //card.ResId = res.Id;
            card.Id = res.Id;

            DAL.Update<Reservation>(update: r => 
            {
                r.PaymentId = card.Id;
                return r;
            }, 
            filter: r => r.Id == resId);
            
            return DAL.Create(new[] { card });
        }

        public bool BookReservation(int type, DateTime start, DateTime end, string name, string email = null)
        {
            return BookReservation((ReservationType)type, start, end, name, email);
        }

        public bool BookReservation(ReservationType type, DateTime start, DateTime end, string name, string email = null)
        {
            try
            {
                var res = Reservation.New(NextResId++, type, start, end, name, email);
                // create filters for daterange and room availibility
                Func<Day, bool> _withinDateRange = d => start <= d.Date && d.Date <= end;
                Func<Day, bool> _hasOpenRoom = d => d.Rooms.Any(r => r.IsOpen());
                Func<Day, bool> _filter = d => _hasOpenRoom(d) && _withinDateRange(d);
                // todo add the functionality so a reservation thats attempted for a un saved base rate date is then added

                // var days = IOBoundary.Get<Day>(_filter);
                IEnumerable<Day> days = DAL.Read<Day>(_filter);
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

                res.Room = roomNumber;

                DAL.Update(day =>
                {
                    day.Rooms[roomNumber].ResId = res.Id;
                    return day;
                }, _filter);

                DAL.Create(new[] { res }, orderBy: r => r.Id);
                Console.WriteLine("Create Reservation. Id: " + res.Id);
                return true;
            }
            catch (Exception e)
            {
                if (Program.Employee == 2)
                {
                    Console.WriteLine(e.Message);
                }
                return false;
            }
        }

        public bool CancelReservation(int resId)
        {
            if (resId < 0)
            {
                // invalid id
                return false;
            }

            var res = DAL.Read<Reservation>(filter: r => r.Id == resId && r.Status == ReservationStatus.Active).FirstOrDefault();

            if (res == null)
            {
                // reservation didnt exist
                return false;
            }

            bool updatedDays = DAL.Update<Day>(
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
            //bool deleted = DAL.Delete<Reservation>(filter: r => r.Id == id).Count() == 1;

            bool updatedReservation = DAL.Update<Reservation>(
                update: r =>
                {
                    if (r.Start < DateTime.Now.AddDays(3))
                    {
                        // cancelling less than three days before start
                        r.PenaltyCharge += NoShowCharge;
                        r.IsNoShow = true;
                        
                    }
                    r.Status = ReservationStatus.Cancelled;
                    return r;
                },
                filter: r => r.Id == res.Id);

            return updatedDays && updatedReservation;
        }

        public bool ChangeReservation(int resId, DateTime start, DateTime end)
        {
            if (resId < 0)
            {
                // invalid id 
                return false;
            }

            var oldRes = DAL.Read<Reservation>(filter: r => r.Id == resId && r.Status == ReservationStatus.Active).FirstOrDefault();

            if (oldRes == null)
            {
                // unable to find reservation
                return false;
            }

            if (!CancelReservation(oldRes.Id))
            {
                // unable to cancel reservation
                return false;
            }

            var newRes = Reservation.New(NextResId++, oldRes.Type, oldRes.Start, oldRes.End, oldRes.Name, oldRes.Email, oldRes.PaymentId, changedFrom: oldRes.Id);

            bool created = DAL.Create(new[] { newRes });

            return created;
        }
   
        #region reports
        public IReport GetAccomodationBill(int resId, DateTime start, DateTime? end = null)
        {
            if (resId < 0)
            {
                // invalid resId
                return null;
            }

            return new AccomodationBill
            {
                ResId = resId,
                Start = start,
                End = end ?? start,
            };
        }

        public IReport GetDailyArrivalsReport(DateTime start, DateTime? end = null)
        {
            return new DailyArrivalsReport
            {
                Start = start,
                End = end ?? start,
            };
        }

        public IReport GetDailyOccupancyReport(DateTime start, DateTime? end = null)
        {
            return new DailyOccupancyReport
            {
                Start = start,
                End = end ?? start,
            };
        }

        public IReport GetExpectedOccupancyReport(DateTime start, DateTime? end = null)
        {
            return new ExpectedOcupancyReport
            {
                Start = start,
                End = end ?? start,
            };
        }

        public IReport GetExpectedRoomIncomeReport(DateTime start, DateTime? end = null)
        {
            return new ExpectedRoomIncomeReport
            {
                Start = start,
                End = end ?? start,
            };
        }

        public IReport GetIncentiveReport(DateTime start, DateTime? end = null)
        {
            return new IncentiveReport
            {
                Start = start,
                End = end ?? start,
            };
        }
        #endregion

        public IEnumerable<Reservation> GetReservationsDuring(DateTime start, DateTime? end = null)
        {
            return DAL.Read<Reservation>(filter: r => !(r.End < start || end < r.Start) && r.Status == ReservationStatus.Active);
        }

        public bool Login(int userId)
        {
            if (userId < 0)
            {
                return false;
            }
            else
            {
                Program.Employee = userId;
                return true;
            }
        }

        public bool Logout()
        {
            Program.Employee = -1;
            return true;
        }

        public bool Pay(int resId)
        {
            throw new NotImplementedException();
        }

        public void TriggerDailyActivities()
        {
            DailyActivities(null);
        }

        public void DailyActivities(object state)
        {
            if (Program.Employee == 2)
            {
                Console.WriteLine("Tick: {0}, Timer State: {1}", DateTime.Now, state ?? "null");
            }
        }

        public bool PerformDailyActions(Reservation res)
        {
            bool result = false;
            bool temp_result;

            if (res.Type == ReservationType.Conventional)
            {
                temp_result = PerformDailyActionsConventional(res);
                result = result | !temp_result;
            }

            if (res.Type == ReservationType.SixtyDay)
            {
                temp_result = PerformDailyActionsSixtyDay(res);
                result = result | !temp_result;
            }
            
            // get reports
            //temp_result = PerformDailyActionsConventional(res);
            //result = result | !temp_result;

            return !result;
        }

        public bool PerformDailyActionsConventional(Reservation res)
        {
            bool isNoShow = res.IsNoShow;

            // if cancelled 
            switch (res.Status)
            {
                default: return true && !isNoShow;
            }
        }

        public bool PerformDailyActionsIncentive(Reservation res)
        {
            // Doesn't look like there is anything for this
            throw new NotImplementedException();
        }

        public bool PerformDailyActionsPrepaid(Reservation res)
        {
            // Doesn't look like there is anything for this
            throw new NotImplementedException();
        }

        public bool PerformDailyActionsSixtyDay(Reservation res)
        {
            // send email 45 days in advance to say that needs to be paid in 15 days
            if ((res.Start - DateTime.Now).TotalDays == 45)
            {
                SendPaymentEmail(res.Email);
            }

            // check if 30 days out and not paid -> cancel res
            if ((res.Start - DateTime.Now).TotalDays == 30)
            {
                res.Status = ReservationStatus.Cancelled;
            }
            return true;
        }

        public bool SendPaymentEmail(string email)
        {
            // send email 45 days in advance to say that needs to be paid in 15 days
            return true;
        }

        public bool Reset()
        {
            DAL.Delete<Reservation>();
            DAL.Delete<CreditCard>();

            bool updated = DAL.Update<Day>(
                update: day =>
                {
                    day.Rooms = day.Rooms.Select(r => new Room()).ToArray();
                    day.Rate = 0;
                    return day;
                });

            return updated;
        }

        public bool SetBaseRates(DateTime start, DateTime end, double? rate = null)
        {
            double _rate = rate ?? StandardRate;

            IEnumerable<Day> prev = DAL.Read<Day>(d => start <= d.Date && d.Date <= end);

            // update all current days baserates
            var updated = DAL.Update<Day>(
               update: day =>
               {
                   day.Rate = _rate;
                   return day;
               },
               filter: d => start <= d.Date && d.Date <= end);

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
                        toAdd.Add(new Day(date, null, _rate));
                    }
                }

                return DAL.Create<Day, DateTime>(toAdd, orderBy: day => day.Date);
            }

            return true;
        }

        public bool Pay(int resId, int? amount = null)
        {
            if (resId < 0)
            {
                // invalid id
                return false;
            }

            var res = DAL.Read<Reservation>(filter: r => r.Id == resId && r.Status == ReservationStatus.Active).FirstOrDefault();

            if (res == null)
            {
                // reservation didnt exist
                return false;
            }

            // they either pay a certain amount or the full amount
            res.AmountPaid = amount ?? res.PenaltyCharge + (res.BaseRates.Sum() * res.Multiplier);
            res.PaidOn = DateTime.Now;

            return true;
        }

        public bool CheckIn(int resId)
        {
            if (resId < 0)
            {
                // invalid id
                return false;
            }

            var res = DAL.Read<Reservation>(filter: r => r.Id == resId && r.Status == ReservationStatus.Active).FirstOrDefault();

            if (res == null)
            {
                // reservation didnt exist
                return false;
            }

            if (res.PaymentId == null)
            {
                // guest did not setup a credit card
                return false;
            }

            res.CheckIn = DateTime.Now;

            return true;
        }

        public bool CheckOut(int resId)
        {
            if (resId < 0)
            {
                // invalid id
                return false;
            }

            var res = DAL.Read<Reservation>(filter: r => r.Id == resId && r.Status == ReservationStatus.Active).FirstOrDefault();

            if (res == null)
            {
                // reservation didnt exist
                return false;
            }

            if (res.CheckIn == null || res.IsNoShow)
            {
                // guest never arrived 
                return false;
            }

            if (res.PaymentId == null || res.PaidOn == null)
            {
                // guest has not paid yet
                return false;
            }

            res.CheckOut = DateTime.Now;

            return true;
        }
    }
}