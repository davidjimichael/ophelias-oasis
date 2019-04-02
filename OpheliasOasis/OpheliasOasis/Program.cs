using Oasis.IO;
using Oasis.Models;
using System;

namespace Oasis
{
    class Program
    {
        // Developer = 2, Manager = 1, Employee = 0, Logout = -1;
        public static int Employee;
        public static Hotel OpheliasOasis;

        static void Main()
        {
            Employee = -1;
            OpheliasOasis = new Hotel();
            string input = "";

            while (!input.StartsWith("Q") && !input.StartsWith("q"))
            {
                input = Console.ReadLine();

                // todo add help with
                // user <int>
                if (input.StartsWith("help"))
                {
                    Console.WriteLine("Command List:");
                    Console.WriteLine("user <login> <userId>");
                    Console.WriteLine("user <logout>");
                    Console.WriteLine("user display");
                    Console.WriteLine("rate set <startdate> <enddate> <rate>");
                    Console.WriteLine("res create <name> <email> <startdate> <enddate>");
                    Console.WriteLine("res change <id> <startdate> <enddate>");
                    Console.WriteLine("res cancel <id>");
                }

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
                else if (input.StartsWith("rate "))
                {
                    var toks = input.Split(' ');

                    if (toks.Length == 5)
                    {
                        if ((Employee == 1 || Employee == 2) && toks[1].Equals("set"))
                        {
                            var rate = int.TryParse(toks[4], out int r) ? r : -1; // -1 causes booking to fail
                            var start = DateTime.TryParse(toks[2], out DateTime s) ? s : DateTime.Now;
                            var end = DateTime.TryParse(toks[3], out DateTime e) ? e : DateTime.Now.AddDays(1); // prevent error end < start

                            var success = OpheliasOasis.SetBaseRates(start, end, rate);
                            Console.WriteLine("{0}", success);
                        }
                    }
                }
                else if (input.StartsWith("res ")) {
                
                    var toks = input.Split(' ');

                    // todo add this one check
                    if (toks.Length == 6)
                    {
                        if (Employee != -1 && toks[1].Equals("create"))
                        {
                            // name email start end
                            var start = DateTime.Parse(toks[4]);
                            var end = DateTime.Parse(toks[5]);

                            var booked = OpheliasOasis.BookReservation(toks[2], toks[3], start, end);
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
                }
                else if (input.StartsWith("reset"))
                {
                    var toks = input.Split(' ');

                    if (toks.Length == 2)
                    {
                        if (Employee == 2 && toks[1].Equals("hotel"))
                        {
                            var reset = OpheliasOasis.Reset();
                            Console.WriteLine(reset);
                        }
                    }
                }                       
                else if (input.StartsWith("Q") || input.StartsWith("q"))
                {
                    // ignore till loop skip else
                }
                else
                {
                    Console.WriteLine("Command Not Found: Type 'help' for help");
                }
            }

            Console.WriteLine("Press any key to exit.");
            System.Console.ReadKey();
        }
    }
}
