using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Marino.DB;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;

namespace Marino.Implementation
{
    internal class User :IUser
    {
        private readonly string _connectionString;
        private readonly IAlarmClock _alarmClock;
        public string Username { get; }
        public string Sitename { get; }

        public User(string name, string siteName, IAlarmClock clock, string connectionString)
        {
            _connectionString = connectionString;
            Username = name;
            Sitename = siteName;
            _alarmClock = clock;
        }

        public IEnumerable<IAuction> WonAuctions()
        {
            Exist();
            var auctions = new List<IAuction>();
            try { 
                using (var database = new AuctionContext(_connectionString))
                {
                    var auctionsWon = (from a in database.Auctions
                        where a.Site.Name == Sitename && a.CurrentWinner.Name == Username && (a.EndsOn < _alarmClock.Now)
                        select a).AsNoTracking().ToList().AsReadOnly();
                    auctions.AddRange(from x in auctionsWon
                        let seller = new User(x.Seller.Name, Sitename, _alarmClock, _connectionString)
                        select new Auction(x.Id, seller, x.ShortDescription, x.EndsOn, _alarmClock, _connectionString));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("Connection error!");
            }
            return auctions;
        }

        public void Delete()
        {
            Exist();
            try { 
                using (var c = new AuctionUserContext(_connectionString))
                {
                    var auctionsNotEnded = (from a in c.Auctions
                        where a.Site.Name == Sitename &&
                              (a.CurrentWinner.Name == Username || a.Seller.Name == Username) &&
                              a.EndsOn > _alarmClock.Now
                        select a).ToList().AsReadOnly();
                    if (auctionsNotEnded.Any())
                        throw new InvalidOperationException($"The user {Username} is winner or creator of one or more auctions still open!");

                    var dbUser =
                        (from u in c.Users where u.SiteName == Sitename && u.Name == Username select u)
                        .SingleOrDefault();
                    var auctWon = dbUser.AuctionsWon;
                    c.Users.Remove(dbUser);
                    c.SaveChanges();
                }
            }
            catch (Exception e) when (!(e is InvalidOperationException))
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("Connection error!");
            }
        }
        private void Exist()
        {
            try
            {
                using (var c = new UserContext(_connectionString))
                {
                    var existingUser =
                        (from u in c.Users where u.SiteName == Sitename && u.Name == Username select u)
                        .AsNoTracking().SingleOrDefault();
                    if (existingUser == null)
                        throw new InvalidOperationException($"the user {Username} doesn't exist!");
                }
            }
            catch (Exception e) when (!(e is InvalidOperationException))
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("Connection error!");
            }
        }

        //EQUALS AND GETHASHCODE OVERRIDE//
        public override bool Equals(object obj) => 
            !(obj is null) && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((User) obj));

        protected bool Equals(User other) =>
            string.Equals(_connectionString, other._connectionString) &&
            _alarmClock.Equals(other._alarmClock) &&
            string.Equals(Username, other.Username) &&
            string.Equals(Sitename, other.Sitename);

        public override int GetHashCode() 
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + _connectionString.GetHashCode();
                hash = hash * 23 + _alarmClock.GetHashCode();
                hash = hash * 23 + Username.GetHashCode();
                hash = hash * 23 + Sitename.GetHashCode();
                return hash;
            }
        }
    }
}