using System;

namespace Oasis.Models
{
    public class Day
    {
        public DateTime Date;
        public Room[] Rooms;
        public double Rate;

        public static readonly int DEFAULT_RATE = 100;
        public static readonly int DEFAULT_ROOM_COUNT = 45;

        public Day(DateTime date, Room[] rooms = null, double rate = 0)
        {
            Date = date;

            if (rooms != null)
            {
                Rooms = rooms;
            }
            else
            {
                //just make new rooms by default
                Rooms = new Room[DEFAULT_ROOM_COUNT];

                for (int i = 0; i < Rooms.Length; i++)
                {
                    Rooms[i] = new Room();
                }
            }

            // if passed non positive rate reset to default rate
            // there should be other validation for this but I need something quick and dirty
            Rate = rate > 0 ? rate : DEFAULT_RATE;
        }
    }
}