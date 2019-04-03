using System;

namespace Oasis.Models
{
    public interface IPaymentInfo
    {
        bool IsValid();
    }

    /// <summary>
    ///     Credit card numbers stored as XXXXXXXXXXXXXXXX
    /// </summary>
    public class CreditCard : IPaymentInfo
    {
        public int ResId;
        public string Name;
        public string Number;
        public DateTime Expiration;
        public string CVC;
        public string Address;
        public string City;
        public string State;
        public string Zip;

        public bool IsValid()
        {
            // for right now just assume the information is correct and check if its expired
            return DateTime.Now < Expiration;
        }
    }
}
