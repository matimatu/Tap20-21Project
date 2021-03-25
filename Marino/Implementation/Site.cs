using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Marino.DB;
using Marino.DB.Total;
using static Marino.DB.Auxiliary;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;
using ISite = TAP2018_19.AuctionSite.Interfaces.ISite;
using static Marino.Utilities.Checker;
using static Marino.Utilities.Crypto;

namespace Marino.Implementation
{
    internal class Site : ISite
    {
        private readonly string _connectionString;
        private readonly IAlarmClock _alarmClock;
        public string Name { get; }
        public int Timezone { get; }
        public int SessionExpirationInSeconds { get; }
        public double MinimumBidIncrement { get; }

        public Site(string name, int timezone, int sessionExpirationInSeconds, double minimumBidIncrement, string connectionString, IAlarmClock clock)
        {
            Name = name;
            Timezone = timezone;
            SessionExpirationInSeconds = sessionExpirationInSeconds;
            MinimumBidIncrement = minimumBidIncrement;
            _connectionString = connectionString;
            _alarmClock = clock;
            var alarm = clock.InstantiateAlarm(5 * 60 * 1000);
            alarm.RingingEvent += CleanupSessions;
        }
        public IEnumerable<IUser> GetUsers()
        {
            Exist();
            var users = new List<IUser>();
            try
            {
                using (var userContext = new UserContext(_connectionString))
                {
                    var q = (from u in userContext.Users
                        where u.SiteName == Name
                        select u).AsNoTracking().ToList().AsReadOnly();

                    users.AddRange(q.Select
                        (x => new User(x.Name, Name, _alarmClock, _connectionString)));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("connection failed!", e);
            }

            return users;
        }

        public IEnumerable<ISession> GetSessions()
        {
            Exist();
            var sessions = new List<ISession>();
            try
            {
                using (var userContext = new UserContext(_connectionString))
                {
                    var q = (from u in userContext.Users     //sfrutto la Icollection di sessions dentro a user
                        where u.SiteName == Name
                        select u.Sessions.ToList()).AsNoTracking().ToList();

                    sessions.AddRange(from list in q from dbSess in list let user = new User(dbSess.User.Name, Name, _alarmClock, _connectionString)
                        select new Session(dbSess.Id, dbSess.ValidUntil, user, _alarmClock, _connectionString));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("connection failed!", e);
            }
            return sessions;
        }

        public IEnumerable<IAuction> GetAuctions(bool onlyNotEnded)
        {
            Exist();
            var auctions = new List<Auction>();
            try
            {
                using (var siteContext = new SiteContext(_connectionString))
                {
                    var q = (from s in siteContext.Sites
                        where s.Name == Name
                        select s.Auctions.ToList()).AsNoTracking().SingleOrDefault();
                    
                    if (onlyNotEnded)
                    {
                        auctions.AddRange(from x in q where x.EndsOn > _alarmClock.Now
                            let seller = new User(x.Seller.Name, Name, _alarmClock, _connectionString)
                            select new Auction(x.Id, seller, x.ShortDescription, x.EndsOn, _alarmClock, _connectionString));
                    }
                    else
                    {
                        auctions.AddRange(from x in q
                            let seller = new User(x.Seller.Name, Name, _alarmClock, _connectionString)
                            select new Auction(x.Id, seller, x.ShortDescription, x.EndsOn, _alarmClock, _connectionString));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("connection failed!", e);
            }
            return auctions;
        }

        public ISession Login(string username, string password)
        {
            Exist();
            CheckPw(password);
            CheckUserName(username);
            try
            {
                using (var c = new UserSessionContext(_connectionString))
                {
                    var userMatched =
                        (from u in c.Users where u.Name == username && u.SiteName == Name select u)
                        .SingleOrDefault();

                    if (userMatched == null ||
                        !VerifyHashedPw(userMatched.Password, password, HashedPwSize, SaltSize, IterationNumber))
                        return null;

                    var dbSession = (from s in c.Sessions
                        where s.User.Name == userMatched.Name //prendo la sessione più recente
                        select s).OrderByDescending(s => s.ValidUntil).FirstOrDefault();
                    ISession session;
                    if (dbSession != null && dbSession.ValidUntil > _alarmClock.Now) //se la sessione esiste ed è ancora valida
                    {
                        dbSession.ValidUntil = _alarmClock.Now.AddSeconds(SessionExpirationInSeconds); //aggiorno la sessione a db
                        c.SaveChanges();
                        var user = new User(username, Name, _alarmClock, _connectionString);
                        session = new Session(dbSession.Id, dbSession.ValidUntil, user, _alarmClock,
                            _connectionString); 
                        return session;
                    }
                    else 
                    {
                        var newSession = new SessionEntity()
                        {
                            Id = GenerateSessionId(),
                            ValidUntil = _alarmClock.Now.AddSeconds(SessionExpirationInSeconds)
                        };
                        userMatched.Sessions.Add(newSession);
                        c.Sessions.Add(newSession);
                        c.SaveChanges(); 
                        var user = new User(username, Name, _alarmClock, _connectionString);
                        session = new Session(newSession.Id, newSession.ValidUntil, user, _alarmClock, _connectionString);
                        return session;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("connection failed!", e);
            }
        }

        public ISession GetSession(string sessionId)
        {
            Exist();
            if (sessionId == null)
                throw new ArgumentNullException();
            try
            {
                using (var c = new SessionContext(_connectionString))
                {
                    var q = (from se in c.Sessions 
                        where  se.Id == sessionId &&  se.User.SiteName == Name && se.ValidUntil > _alarmClock.Now
                        select se).AsNoTracking().SingleOrDefault();
                    
                    if (q == null) return null;

                    var user = new User(q.User.Name, Name, _alarmClock, _connectionString);
                    var session = new Session(q.Id, q.ValidUntil, user, _alarmClock, _connectionString);
                    return session;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("connection failed!", e);
            }
        }

        public void CreateUser(string username, string password)
        {
            Exist();
            CheckUserName(username);
            CheckPw(password);
            try
            {
                using (var c = new UserContext(_connectionString)) 
                {
                    var user = new UserEntity()
                    {
                        Name = username,
                        Password = HashPw(password, HashedPwSize, SaltSize, IterationNumber),
                        SiteName = Name
                    };   
                    c.Users.Add(user);
                    c.SaveChanges();
                }
            }
            catch (Exception e) when (!(e is NameAlreadyInUseException))
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("connection failed!", e);
            }
        }


        public void Delete()
        {
            Exist();
            try
            {
                using (var c = new TotalContext(_connectionString))
                {
                    var site = c.Sites.Find(Name);
                
                    var users = site.Users;
                    var auctions = site.Auctions;
                    var sessionList = users.Select(x => x.Sessions.ToList()).ToList().AsReadOnly();
                    foreach (var y in sessionList)
                        c.Sessions.RemoveRange(y);

                    c.Sites.Remove(site);
                    c.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("connection failed!", e);
            }
        }

        public void CleanupSessions()
        {
            Exist();
            try { 
                using (var c = new UserSessionContext(_connectionString))
                {
                    var sessions = (from u in c.Users where u.SiteName == Name select u.Sessions).ToList().AsReadOnly();
                    foreach (var session in from x in sessions from session in x
                        where session.ValidUntil < _alarmClock.Now select session)
                        c.Sessions.Remove(session);
                
                    c.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("connection failed!", e);
            }
        }

        private void Exist()
        {
            try
            {
                using (var c = new SiteContext(_connectionString))
                {
                    var site = c.Sites.Find(Name);
                    if (site == null)
                        throw new InvalidOperationException($"the site {Name} doesn't exist!");
                }
            }
            catch (Exception e) when (!(e is InvalidOperationException))
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("connection failed!", e);
            }
        }

        //EQUALS AND GETHASHCODE OVERRIDE//

        public override bool Equals(object obj) =>
            !(obj is null) && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((Site)obj));

        protected bool Equals(Site other) =>
            string.Equals(_connectionString, other._connectionString) &&
            _alarmClock.Equals(other._alarmClock) &&
            string.Equals(Name, other.Name) &&
            Equals(Timezone, other.Timezone) &&
            Equals(SessionExpirationInSeconds, other.SessionExpirationInSeconds) &&
            Equals(MinimumBidIncrement, other.MinimumBidIncrement);



        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + _connectionString.GetHashCode();
                hash = hash * 23 + _alarmClock.GetHashCode();
                hash = hash * 23 + Name.GetHashCode();
                hash = hash * 23 + Timezone.GetHashCode();
                hash = hash * 23 + SessionExpirationInSeconds.GetHashCode();
                hash = hash * 23 + MinimumBidIncrement.GetHashCode();
                return hash;
            }
        }
    }
}
