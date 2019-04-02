using System;
using Oasis.Models;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace Oasis.BusinessLogic
{
    public class ReportRow
    {
        public dynamic Data;
    }

    public class ReportColumn
    {
        string Name;
        string Property;
        bool IsAveraged;
        bool IsTotaled;
        double Average;
        double Total;

        public ReportColumn(string name, string property, bool isAveraged, bool isTotaled)
        {
            this.Name = name;
            this.Property = property;
            this.IsAveraged = isAveraged;
            this.IsTotaled = isTotaled;
            this.Average = 0.0;
            this.Total = 0.0;
        }
    }

    public interface IReport
    {
        ReportRow[] Rows { get; set; }
        ReportColumn[] Columns { get; set; }
        ReportRow[] GetData();
        ReportColumn[] GetColumns();
    }
}
