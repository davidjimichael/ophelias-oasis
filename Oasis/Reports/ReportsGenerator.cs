using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oasis.Reports
{
    public class ReportsGenerator
    {
        // one static create the right report using 
        // methods to get/call gets for data to report
    }

    public class ReportRow
    {
        public dynamic data;
    }

    public abstract class Report
    {
        public Report[] Rows;

        // takes data and formats to line by line ready for printing by an output
        // it will take the headers as columns and properties as values in rows under the headers
        public abstract string[] Format(Dictionary<string, string> headerPropertyPairs);

        // because I havent figured out how to impliment this yet this method gets called
        // when i will eventually ask each report to generate itself
        public abstract Report GetData();
    }
}
