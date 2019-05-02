﻿using Newtonsoft.Json;
using Oasis.Dev;
using Oasis.IO;
using Oasis.Models;
using Oasis.Reports;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Oasis
{
    class Program
    {
        // unreal security measures.
        // used for the sake of this class to demonstrate
        // Developer = 2, Manager = 1, Employee = 0, Logout = -1;
        public static int Employee; // mandatory field for backend logging and whatnot figured this was the easiest way to set it
        public static Dev.Hotel OpheliasOasis;
        //public static IHotel OpheliasOasis;

        static void Main()
        {
            Program.Employee = -1;
            OpheliasOasis = new Dev.Hotel();
            //OpheliasOasis = new Dev.Hotel(id: 0);
            string input = "";
            try
            {
                Console.WriteLine("Welcome to Ophleias Oasis Hotel System");
                Console.WriteLine("Type a command or help for a command list");
                while (!input.StartsWith("Q") && !input.StartsWith("q"))
                {
                    input = Console.ReadLine();

                    if (input.StartsWith("help"))
                    {
                        Console.WriteLine("Command List:");
                        Console.WriteLine("user <login>, <userId>");
                        Console.WriteLine("user <logout>");
                        Console.WriteLine("user display");
                        Console.WriteLine("rate set <startdate>, <enddate>, <rate>");
                        Console.WriteLine("admin reset");
                        Console.WriteLine("admin daily");
                        Console.WriteLine("res create <name>, <email>, <startdate>, <enddate>");
                        Console.WriteLine("res change <id>, <startdate>, <enddate>");
                        Console.WriteLine("res cancel <id>");
                        Console.WriteLine("res list <startdate>, <enddate>");
                        Console.WriteLine("res card <id>, <name>, <card_number>, <exp>, <cvc>, <address>, <city>, <state>, <zip>");
                        Console.WriteLine("report eor <startdate>, <enddate> (Expected Occupancy Report)");
                        Console.WriteLine("report erir <startdate>, <enddate> (Expected Room Income Report)");
                        Console.WriteLine("report dor <startdate>, <enddate> (Daily Occupancy Report)");
                        Console.WriteLine("report ab <startdate>, <enddate> (Accomodation Bill Occupancy Report)");
                        Console.WriteLine("report ir <startdate>, <enddate> (Incentive Report)");
                        Console.WriteLine("clear");
                    }
                    else if (input.StartsWith("clear"))
                    {
                        Console.Clear();
                    }
                    //change user lever
                    else if (input.StartsWith("user "))
                    {
                        var toks = input.Split(' ');

                        if (toks.Length == 3)
                        {
                            if (toks[1].Equals("login"))
                            {
                                Employee = int.TryParse(toks[2], out int id) ? id : -1;
                            }
                        }
                        else if (toks.Length == 2)
                        {
                            if (toks[1].Equals("display"))
                            {
                                Console.WriteLine(Employee);
                            }
                            else if (toks[1].Equals("logout"))
                            {
                                Employee = -1;
                            }
                        }
                    }
                    //set rate
                    else if (input.StartsWith("rate "))
                    {
                        var toks = input.Split(' ');

                        if (toks.Length == 5)
                        {
                            if ((Employee == 1 || Employee == 2) && toks[1].Equals("set"))
                            {
                                var rate = double.TryParse(toks[4], out double r) ? r : -1; // -1 causes booking to fail
                                var start = DateTime.TryParse(toks[2], out DateTime s) ? s : DateTime.Now;
                                var end = DateTime.TryParse(toks[3], out DateTime e) ? e : DateTime.Now.AddDays(1); // prevent error end < start

                                var success = OpheliasOasis.SetBaseRates(start, end, rate);
                                Console.WriteLine("{0}", success);
                            }
                        }
                    }
                    //create, modify, cancel reservations
                    else if (input.StartsWith("res "))
                    {

                        var toks = input.Split(' ');

                        // todo add this one check
                        if (toks.Length == 6)
                        {
                            if (Employee != -1 && toks[1].Equals("create"))
                            {
                                // name email start end
                                var start = DateTime.Parse(toks[4]);
                                var end = DateTime.Parse(toks[5]);

                                var booked = OpheliasOasis.BookReservation(0, start, end, toks[2], toks[3]);
                                Console.WriteLine(booked);
                            }
                        }
                        else if (toks.Length == 3)
                        {
                            if (Employee != -1 && toks[1].Equals("cancel"))
                            {
                                // id
                                int id = int.TryParse(toks[2], out int result) ? result : -1;
                                var cancelled = OpheliasOasis.CancelReservation(id);
                                Console.WriteLine(cancelled);
                            }
                        }
                        else if (toks.Length == 5)
                        {
                            if (Employee != -1 && toks[1].Equals("change"))
                            {
                                // id start end
                                var id = int.TryParse(toks[2], out int result) ? result : -1;
                                var start = DateTime.Parse(toks[3]);
                                var end = DateTime.Parse(toks[4]);

                                var changed = OpheliasOasis.ChangeReservation(id, start, end);
                                Console.WriteLine(changed);
                            }
                        }
                        else if (toks.Length == 11)
                        {
                            if (Employee != -1 && toks[1].Equals("card"))
                            {
                                // res card name num exp cvc add city state zip
                                CreditCard card = new CreditCard
                                {
                                    Id = int.TryParse(toks[2], out int id) ? id : -1,
                                    Name = toks[3],
                                    Number = toks[4],
                                    Expiration = DateTime.TryParse(toks[5], out var date) ? date : DateTime.Now,
                                    CVC = toks[6],
                                    Address = toks[7],
                                    City = toks[8],
                                    State = toks[9],
                                    Zip = toks[10],
                                };

                                var added = OpheliasOasis.AddCreditCard(card.Id, card);
                                Console.WriteLine(added);
                            }
                        }
                        else if (toks.Length == 4)
                        {
                            if (Employee != -1 && toks[1].Equals("list"))
                            {
                                // res list <startdate> <enddate>
                                DateTime start = DateTime.TryParse(toks[2], out var s) ? s : DateTime.Now;
                                DateTime end = DateTime.TryParse(toks[3], out var e) ? e : DateTime.Now;

                                IEnumerable<Reservation> reservations = OpheliasOasis.GetReservationsDuring(start, end);

                                foreach (Reservation res in reservations)
                                {
                                    Console.WriteLine(JsonConvert.SerializeObject(res));
                                }
                                Console.WriteLine(reservations.Count() > 0);
                            }
                        }
                    }
                    //generate reports
                    else if (input.StartsWith("report "))
                    {
                        var toks = input.Split(' ');

                        if (toks.Length == 4 || toks.Length == 5)
                        {
                            if ((Employee == 1 || Employee == 2))
                            {
                                var start = DateTime.TryParse(toks[2], out DateTime s) ? s : DateTime.Now;
                                var end = DateTime.TryParse(toks[3], out DateTime e) ? e : DateTime.Now.AddDays(1); // prevent error end < start

                                if (toks[1].Equals("eor"))
                                {
                                    var report = (ExpectedOcupancyReport)OpheliasOasis.GetExpectedOccupancyReport(start, end);
                                    Console.WriteLine(string.Join("\n", report?.SampleOutput));
                                }
                                if (toks[1].Equals("erir"))
                                {
                                    var report = (ExpectedRoomIncomeReport)OpheliasOasis.GetExpectedRoomIncomeReport(start, end);
                                    Console.WriteLine(string.Join("\n", report?.SampleOutput));
                                }
                                if (toks[1].Equals("dor"))
                                {
                                    var report = (DailyOccupancyReport)OpheliasOasis.GetDailyOccupancyReport(start, end);
                                    Console.WriteLine(string.Join("\n", report?.SampleOutput));
                                }
                                if (toks[1].Equals("ab"))
                                {
                                    var resId = int.TryParse(toks[4], out int i) ? i : -1;
                                    var report = (AccomodationBill)OpheliasOasis.GetAccomodationBill(resId, start, end);
                                    Console.WriteLine(string.Join("\n", report?.SampleOutput));
                                }
                                if (toks[1].Equals("ir"))
                                {
                                    var report = (IncentiveReport)OpheliasOasis.GetIncentiveReport(start, end);
                                    Console.WriteLine(string.Join("\n", report?.SampleOutput));
                                }
                            }
                        }
                    }
                    //admin activities
                    else if (input.StartsWith("admin "))
                    {
                        var toks = input.Split(' ');

                        if (toks.Length == 2)
                        {
                            if (Employee == 2 && toks[1].Equals("reset"))
                            {
                                var reset = OpheliasOasis.Reset();
                                Console.WriteLine(reset);
                            }
                            else if (Employee == 2 && toks[1].Equals("daily"))
                            {
                                // todo allow an optional date parameter to trigger daily events on a particular day
                                OpheliasOasis.TriggerDailyActivities();
                            }
                        }
                    }
                    else if (input.ToLower().StartsWith("q"))
                    {
                        // ignore till loop skip else
                    }
                    else
                    {
                        Console.WriteLine("Command Not Found: Type 'help' for command list");
                    }
                }
            }
            catch (Exception e)
            {
                if (Program.Employee == 2)
                {
                    Console.WriteLine(e.Message);
                }
            }
            finally
            {
                Console.WriteLine("Press any key to exit.");
            }
            System.Console.ReadKey();
        }
    }
}
