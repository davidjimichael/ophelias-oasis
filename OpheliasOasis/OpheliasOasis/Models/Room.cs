using System;

namespace Oasis.Models
{
    public class Room
    {
        private int? _resId;

        public Room()
        {
            _resId = null;
        }
        
        public bool IsOpen
        {
            get
            {
                return !_resId.HasValue;
            }
        }

        public int? ResId
        {
            get
            {
                return _resId;
            }
            set
            {
                if (value < 0)
                {
                    throw new InvalidOperationException("Reservation Ids cannot be negative");
                }
                else
                {
                    _resId = value;
                }
            }
        }
    }
}
