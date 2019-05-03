namespace Oasis.Models
{
    public class Room
    {
        private int? _resId;

        public Room()
        {
            _resId = null;
        }
        
        public bool IsOpen()
        {
            return !_resId.HasValue;
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
                    // normally I wouldn't set this but I feel like this is actually really imporant to not mess up.
                    throw new System.InvalidOperationException("Reservation Ids cannot be negative");
                }
                else
                {
                    _resId = value;
                }
            }
        }
    }
}
