using Oasis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oasis.BusinessLogic
{
    class BaseRateScheduler
    {
        BaseRate[] BaseRates;

        public BaseRateScheduler(DateTime start, DateTime end)
        {
            int days = end.Subtract(start).Days;

            BaseRates = new BaseRate[days];

            // read 
        }

        public void SetBaseRate(int rate)
        {
            foreach (BaseRate br in BaseRates)
            {
                br.Rate = rate;
            }

            // save
        }

        public BaseRate[] GetBaseRates()
        {
            return BaseRates;
        }
    }
}
