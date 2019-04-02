
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oasis.Models
{
    public class Day
    {
        public DateTime Date;
        public Room[] Rooms;
        public int Rate;
        public static readonly int DEFAULT_RATE = 100;
        public static readonly int DEFAULT_ROOM_COUNT = 45;

        public Day(DateTime date, Room[] rooms = null, int rate = 0)
        {
            Date = date;

            if (rooms == null)
            {
                // just make new rooms by default
                Rooms = new Room[DEFAULT_ROOM_COUNT];

                for (int i = 0; i < Rooms.Length; i++)
                {
                    Rooms[i] = new Room();
                }
            }
            else
            {
                Rooms = rooms;
            }


            // if passed non positive rate reset to default rate
            Rate = rate > 0 ? rate : DEFAULT_RATE;
        }

        //public bool HasOpenRooms()
        //{
        //    return GetAvailibleRooms().Count() > 0;
        //}

        //public IEnumerable<int> GetAvailibleRoomNumbers(IEnumerable<int> acceptableRooms = null)
        //{
        //    if (acceptableRooms == null)
        //    {
        //        // either check previously open rooms or start checking all of them
        //        acceptableRooms = Rooms.Select((r, i) => i);
                
        //    }

        //    for (int i = 0; i < Rooms.Length; i++)
        //    {
        //        if (!acceptableRooms.Contains(i) || !Rooms[i].IsOpen)
        //        {
        //            acceptableRooms.
        //        }
        //    }

        //    return numbers;
        //}

        //public int[] GetActiveReservationIds(IEnumerable<int> acceptableRooms = null)
        //{
        //    return GetAvailibleRooms(acceptableRooms)
        //        .Where(r => r.IsOpen)
        //        .Select(r => r.ResId.Value).ToArray();
        //}

        /// <summary>
        ///     Returns all rooms that are marked as open for this day as 
        ///     Dictionary
        /// </summary>
        /// <param name="acceptableRooms">Rooms not contained here are filtered out</param>
        /// <returns></returns>
        //private IEnumerable<Room> GetAvailibleRooms(IEnumerable<int> acceptableRooms = null)
        //{
        //    if (acceptableRooms == null)
        //    {
        //        // all rooms acceptable
        //        acceptableRooms = Rooms.Select((r, i) => i).ToArray();
        //    }
            
        //    var rooms = new List<Room>();

        //    for (int i = 0; i < Rooms.Length; i++)
        //    {
        //        if (acceptableRooms.Contains() && Rooms[i].IsOpen)
        //        {
        //            rooms.Add(Rooms[i]);
        //        }
        //    }
        //    return rooms;
        //}
    }
}