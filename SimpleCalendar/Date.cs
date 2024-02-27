using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCalendar {
    //object to save dates and check their validity
    internal class Date {
        public int year;
        public Month month;
        public int day;

        public Date(int year, Month month, int day) {
            this.year = year;
            this.month = month;
            this.day = day;
        }

        public bool IsValidDate(int year, Month month, int day) {
            if(day > 0 && day <= DaysInMonth(year, month)) {
                return true;
            }
            else {
                return false;
            }
        }

        public int DaysInMonth(int year, Month month) {
            switch(month) {
                case Month.January:
                    return 31;
                case Month.February:
                    if (year % 4 == 0) { return 29; }
                    else { return 28; }
                case Month.March:
                    return 31;
                case Month.April:
                    return 30;
                case Month.May:
                    return 31;
                case Month.June:
                    return 30;
                case Month.Juli:
                    return 31;
                case Month.August:
                    return 31;
                case Month.September:
                    return 30;
                case Month.October:
                    return 31;
                case Month.November:
                    return 30;
                case Month.December:
                    return 31;

                //this case cant be reached and is there solely for error supression
                default: 
                    return 0; 
            }
        }
    }

    public enum Month {
        January,
        February,
        March,
        April,
        May,
        June,
        Juli,
        August,
        September,
        October,
        November,
        December
    }

    public enum WeekDay {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }
}
