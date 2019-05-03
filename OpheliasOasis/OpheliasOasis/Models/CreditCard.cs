namespace Oasis.Models
{
    /// <summary>
    ///     Credit card numbers stored as string formatted XXXXXXXXXXXXXXXX
    /// </summary>
    public class CreditCard
    {
        public int Id;
        //public int ResId;
        public string Name;
        public string Number;
        public System.DateTime Expiration;
        public string CVC;
        public string Address;
        public string City;
        public string State;
        public string Zip;

        public bool IsValid()
        {
            //only checking expiration
            return System.DateTime.Now < Expiration;
        }
    }
}
