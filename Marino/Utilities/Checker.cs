using System;
using TAP2018_19.AuctionSite.Interfaces;

namespace Marino.Utilities
{
    class Checker
    {
        /// <summary>
        /// Checks if the connectionString is not null
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        public static void CheckNull(object obj,string name)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), $"{name} must not be null!");
        }
        
        /// <summary>
        /// Checks if the sitename is not null and then if is his length is correct
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="name"></param>
        public static void CheckSiteName(string name)
        {
            CheckNull(name,"sitename");

            if (name.Length < DomainConstraints.MinSiteName)
                throw new ArgumentException($"sitename size must be greater or equal than {DomainConstraints.MinSiteName}!");

            if (name.Length > DomainConstraints.MaxSiteName)
                throw new ArgumentException($"sitename size must be lower or equal than {DomainConstraints.MaxSiteName}!");
        }
        
        /// <summary>
        /// Checks if the username is not null and then if his length is correct
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="name"></param>
        public static void CheckUserName(string name)
        {
            CheckNull(name, "username");

            if (name.Length < DomainConstraints.MinUserName)
                throw new ArgumentException($"username size must be greater or equal than {DomainConstraints.MinUserName}!");

            if (name.Length > DomainConstraints.MaxUserName)
                throw new ArgumentException($"username size must be lower or equal than {DomainConstraints.MaxUserName}!");
        }

        /// <summary>
        /// Checks if the pw is null,and then if his length is correct 
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <param name="pw"></param>
        public static void CheckPw(string pw)
        {
            CheckNull(pw, "password");
            
            if (pw.Length < DomainConstraints.MinUserPassword)
                throw new ArgumentException($"password must be at least {DomainConstraints.MinUserPassword} long!", nameof(pw));

        }
        
        /// <summary>
        /// Checks if the timezone is in range 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <param name="tz"></param>
        public static void CheckRangeTimezone(int tz)
        {
            if (tz < DomainConstraints.MinTimeZone)
                throw new ArgumentOutOfRangeException($"Timezone must be greater or equal than {DomainConstraints.MinTimeZone}!");

            if (tz > DomainConstraints.MaxTimeZone)
                throw new ArgumentOutOfRangeException($"Timezone  must be lower or equal than {DomainConstraints.MaxTimeZone}!");
        }
        
        /// <summary>
        /// Checks if the number is greater than 0
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <param name="n"></param>
        /// <param name="name"></param>
        public static void CheckDouble(double n,string name)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException($"{name} must be greater than 0!");
        }

        /// <summary>
        /// Checks if the session is not null and valid
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// <param name="s"></param>
        public static void CheckIsValid(ISession s)
        {
            if (!s.IsValid())
                throw new InvalidOperationException($"The session {s} is not valid!");
        }

        /// <summary>
        /// Checks if the string is not empty
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <param name="s"></param>
        /// <param name="name"></param>
        public static void CheckIsNotEmpty(string s, string name)
        {
            if (s.Length == 0)
                throw new ArgumentException($"{name} is empty", nameof(s));
        }

        /// <summary>
        /// Checks if the ending time of the auction is sensible
        /// </summary>
        /// <exception cref="UnavailableTimeMachineException"></exception>
        /// <param name="endsOn"></param>
        /// <param name="now"></param>
        /// <param name="timeMachine"></param>
        public static void CheckEndsOn(DateTime endsOn, DateTime now, bool timeMachine)
        {
            if (endsOn >= now) return;
            if (timeMachine)
                throw new UnavailableTimeMachineException($"the auction ends on {endsOn},but the current time is {now}");
            throw new InvalidOperationException($"the auction is not valid!");
        }
        
        /// <summary>
        /// Checks if the seller is not the bidder
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <param name="m"></param>
        /// <param name="n"></param>
        public static void CheckNotEquals(object m, object n)
        {
            if (m.Equals(n))
                throw new ArgumentException("The seller can't bid on his auction!", nameof(m));
        }

        /// <summary>
        /// Checks if the offer is positive
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <param name="offer"></param>
        public static void CheckOffer(double offer)
        {
            if (offer < 0)
                throw new ArgumentOutOfRangeException($"{offer} must be positive!");
        }

        /// <summary>
        /// Checks if the two Datetime values are equals,not comparing Milliseconds and Ticks
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        public static bool CheckEqualDate(DateTime d1, DateTime d2)
        {
            return d1.Year == d2.Year && d1.Month == d2.Month && d1.Day == d2.Day &&
                   d1.Hour == d2.Hour && d1.Minute == d2.Minute && d1.Second == d2.Second;
        }
    }
}
