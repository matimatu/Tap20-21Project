using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Marino.DB;
using Marino.DB.Total;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;
using static Marino.Utilities.Checker;

namespace Marino.Implementation
{
    internal class SiteFactory : ISiteFactory
    {
        public void Setup(string connectionString)  
        {
            CheckNull(connectionString,"connectionString");
            try
            {
                using (var database = new TotalContext(connectionString))
                {
                    database.Database.Delete();
                    database.Database.Create();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("setup failed!", e);
            }
        }

        public IEnumerable<string> GetSiteNames(string connectionString)
        {
            CheckNull(connectionString, "connectionString");
            IEnumerable<string> siteNames;
            try
            {
                using (var siteContext = new SiteContext(connectionString))
                {
                    var q = (from s in siteContext.Sites select s.Name).AsNoTracking().ToList().AsReadOnly();
                    siteNames = q;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("connection failed!", e);
            }
            return siteNames;
        }

        public void CreateSiteOnDb(string connectionString, string name, int timezone, int sessionExpirationTimeInSeconds, double minimumBidIncrement)
        {
            CheckNull(connectionString, "connectionString");
            CheckSiteName(name);
            CheckRangeTimezone(timezone);
            CheckDouble(minimumBidIncrement, "timezone");
            CheckDouble(sessionExpirationTimeInSeconds, "sessionExpirationTimeInSeconds");
            try
            {
                using (var siteContext = new SiteContext(connectionString))
                {
                    var siteEntity = new SiteEntity()
                    {
                        Name = name, MinBidIncr = minimumBidIncrement,
                        SessionExp = sessionExpirationTimeInSeconds,
                        Timezone = timezone
                    };
                    siteContext.Sites.Add(siteEntity);
                    siteContext.SaveChanges();
                }
            }
            catch (Exception e) when (!(e is NameAlreadyInUseException))
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("connection failed!", e);
            }
        }

        public ISite LoadSite(string connectionString, string name, IAlarmClock alarmClock)
        {
            CheckNull(connectionString, "connectionString");
            CheckNull(alarmClock,"alarmClock");
            CheckSiteName(name);
            ISite site;
            try
            {
                using (var siteContext = new SiteContext(connectionString))
                {
                    var q = (from s in siteContext.Sites
                        where s.Name == name
                        select s).AsNoTracking().Single();
                    site = new Site(q.Name, q.Timezone, q.SessionExp, q.MinBidIncr, connectionString, alarmClock);
                }
            }
            catch (InvalidOperationException e)
            {
                throw new InexistentNameException(nameof(name), "This sitename doesn't exist!", e);
            }
            catch (Exception e) when(!(e is InexistentNameException))
            {
                throw new UnavailableDbException("connection failed!", e);
            }
            if (alarmClock.Timezone != site.Timezone)
                throw new ArgumentException();
            return site;
        }

        public int GetTheTimezoneOf(string connectionString, string name)
        {
            CheckNull(connectionString, "connectionString");
            CheckSiteName(name);
            int timezone;
            try
            {
                using (var siteContext = new SiteContext(connectionString))
                {
                    timezone = (from s in siteContext.Sites
                        where s.Name == name
                        select s.Timezone).Single();
                }
            }
            catch (InvalidOperationException e)
            {
                throw new InexistentNameException(nameof(name), $"The sitename '{name}' doesn't exist!", e);
            }
            catch (Exception e) when (!(e is InexistentNameException))
            {
                throw new UnavailableDbException("connection failed!", e);
            }
            return timezone;
        }
    }
}
