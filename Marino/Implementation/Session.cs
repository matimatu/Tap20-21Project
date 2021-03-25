using System;
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
    internal class Session : ISession
    {
        private readonly string _connectionString;
        private readonly IAlarmClock _alarmClock;
        public string Id { get; }
        public DateTime ValidUntil { get; set; }  
        public IUser User { get; }

        public Session(string id, DateTime validUntil, IUser user, IAlarmClock alarmClock, string connectionString)
        {
            Id = id;
            ValidUntil = validUntil;
            User = user;
            _alarmClock = alarmClock;
            _connectionString = connectionString;
        }

        public bool IsValid()
        {
            try { 
                using (var c = new SessionContext(_connectionString))
                {
                    var existingSession = (from s in c.Sessions
                        where s.Id == Id && ValidUntil > _alarmClock.Now
                        select s).AsNoTracking().SingleOrDefault();
                    return existingSession != null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("connection failed!", e);
            }
        }
        public void Logout()
        {
            CheckIsValid(this);
            try
            {
                using (var c = new SessionContext(_connectionString))
                {
                    var session = c.Sessions.Find(Id);
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
        public IAuction CreateAuction(string description, DateTime endsOn, double startingPrice)
        {
            CheckIsValid(this);
            CheckNull(description, "description");
            CheckIsNotEmpty(description, "description");
            CheckDouble(startingPrice, "startingPrice");
            CheckEndsOn(endsOn, _alarmClock.Now,true);
            try { 
                using (var c = new TotalContext(_connectionString))
                {
                    var session = c.Sessions.Find(Id);
                    var site = c.Users.Find(session.UserId).Site;
                    var user = c.Users.Find(session.UserId);
                    
                    var dbAuction = new AuctionEntity()
                    {
                        SellerId = user.Id,
                        SiteName = site.Name,
                        CurrentPrice = startingPrice,
                        ShortDescription = description,
                        EndsOn = endsOn,
                    };
                    c.Auctions.Add(dbAuction);

                    session.ValidUntil = _alarmClock.Now.AddSeconds(site.SessionExp);  
                    c.SaveChanges();
                  
                    ValidUntil = _alarmClock.Now.AddSeconds(site.SessionExp); 
                    return new Auction(dbAuction.Id, User, description, endsOn, _alarmClock, _connectionString);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("connection failed!", e);
            }
        }

        //EQUALS AND GETHASHCODE OVERRIDE//

        public override bool Equals(object obj) =>
            !(obj is null) && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((Session)obj));
        
        protected bool Equals(Session other) =>
            string.Equals(_connectionString, other._connectionString) &&
            _alarmClock.Equals(other._alarmClock) &&
            string.Equals(Id, other.Id) &&
            User.Equals(other.User) &&
            CheckEqualDate(ValidUntil, other.ValidUntil);

        public override int GetHashCode() 
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + _connectionString.GetHashCode();
                hash = hash * 23 + _alarmClock.GetHashCode();
                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + User.GetHashCode();
                hash = hash * 23 + ValidUntil.GetHashCode();
                return hash;
            }
        }

    }
}