namespace Oasis.Models
{
    public class Day
    {
        public System.DateTime Date;
        public Room[] Rooms;
        public double Rate;

        public static readonly int DEFAULT_RATE = 100;
        public static readonly int DEFAULT_ROOM_COUNT = 45;

        public Day(System.DateTime date, Room[] rooms = null, double rate = 0)
        {
            Date = date;

            if (rooms != null)
            {
                Rooms = rooms;
            }
            else
            {
                // new rooms by default
                Rooms = new Room[DEFAULT_ROOM_COUNT];

                for (int i = 0; i < Rooms.Length; i++)
                {
                    Rooms[i] = new Room();
                }
            }
            Rate = rate > 0 ? rate : DEFAULT_RATE;
        }
    }
}