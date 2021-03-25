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
    internal class Auction : IAuction
    {
        private readonly string _connectionString;
        private readonly IAlarmClock _alarmClock;
        public int Id { get; }
        public IUser Seller { get; }
        public string Description { get; }
        public DateTime EndsOn { get; }

        public Auction(int id, IUser seller, string description, DateTime endsOn, IAlarmClock clock,
            string connectionstring)
        {
            Id = id;
            Seller = seller;
            Description = description;
            EndsOn = endsOn;
            _alarmClock = clock;
            _connectionString = connectionstring;
        }

        public IUser CurrentWinner()
        {
            Exist();
            try { 
                using (var c = new AuctionContext(_connectionString))
                {
                    var currWin = c.Auctions.Find(Id).CurrentWinner;
                    return currWin == null ? null : new User(currWin.Name, currWin.SiteName, _alarmClock, _connectionString);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("Connection error!");
            }
}

        public double CurrentPrice()
        {
            Exist();
            try { 
                using (var c = new AuctionContext(_connectionString))
                    return  c.Auctions.Find(Id).CurrentPrice;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("Connection error!");
            }

}

        public void Delete()
        {
            Exist();
            try
            {
                using (var c = new AuctionContext(_connectionString))
                {
                    var auction = c.Auctions.Find(Id);
                    c.Auctions.Remove(auction);
                    c.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("Connection error!");
            }
}

        public bool BidOnAuction(ISession session, double offer)
        {
            Exist();
            CheckNull(session, "session");
            if (!session.IsValid())
                throw new ArgumentException("Session not valid anymore");
            CheckOffer(offer);
            CheckEndsOn(EndsOn, _alarmClock.Now, false);
            CheckNotEquals(Seller, session.User);
            try
            {
                using (var c = new TotalContext(_connectionString))
                {
                    var auct = c.Auctions.Find(Id);
                    var bidderSess = c.Sessions.Find(session.Id);
                    var sel = (from u in c.Users
                        where u.Name == Seller.Username && bidderSess.User.SiteName == u.SiteName
                        select u).AsNoTracking().SingleOrDefault();

                    var bidder = new User(session.User.Username, auct.Site.Name, _alarmClock, _connectionString);

                    if (sel == null)
                        throw new ArgumentException("User from another site can't bid in this auction!", nameof(bidderSess.UserId));

                    bidderSess.ValidUntil = _alarmClock.Now.AddSeconds(auct.Site.SessionExp);
                    var mySession = (Session) session;
                    mySession.ValidUntil = bidderSess.ValidUntil;

                    if (!ControlBidder(auct,bidder, offer)) return false;

                    UpdateAuctionBid(auct,bidderSess,offer);

                    c.SaveChanges();
                }
            }
            catch (Exception e) when (!(e is ArgumentException || e is InvalidOperationException))
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("connection error!");
            }
            return true;
        }

        private void Exist()
        {
            try
            {
                using (var c = new AuctionContext(_connectionString))
                {
                    var existingAuction = c.Auctions.Find(Id);
                    if (existingAuction == null)
                        throw new InvalidOperationException($"the auction {Id} doesn't exist!");
                }
            }
            catch (Exception e) when (!(e is InvalidOperationException))
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("Connection error!");
            }

        }

        /// <summary>
        /// Return false  if :
        ///• the bidder is (already) the current winner and offer is lower than the maximum offer increased by
        ///minimumBidIncrement
        ///• the bidder is not the current winner and offer is lower than the current price
        ///• the bidder is not the current winner and offer is lower than the current price increased by minimumBid - Increment AND this is not the first bid
        /// </summary>
        /// <exception cref="UnavailableDbException"></exception>
        /// <param name="auct"></param>
        /// <param name="bidder"></param>
        /// <param name="offer"></param>
        /// <returns></returns>
        private bool ControlBidder(AuctionEntity auct,User bidder, double offer)
        {
            try
            {
                return (!bidder.Equals(CurrentWinner())
                        || !(offer < auct.Cmo + auct.Site.MinBidIncr)) && (bidder.Equals(CurrentWinner())
                                                                           || !(offer < auct.CurrentPrice)) &&
                       ((bidder.Equals(CurrentWinner())
                         || !(offer < auct.CurrentPrice + auct.Site.MinBidIncr)
                         || !(auct.Cmo > 0)));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("Connection error!");
            }
        }

        /// <summary>
        /// Updates the fields of the auction with the specified rules:
        /// • if this is the first bid, then the maximum offer is set to offer , the current price is not changed (that is, it
        /// remains the starting price), and the bidder becomes the current winner;
        /// • if the bidder was already winning this auction, the maximum offer is set to offer , current price and current
        /// winner are unchanged;
        /// • if this is NOT the first bid, the bidder is NOT the current winner, and offer is higher than the current
        /// maximum offer, in the following denoted by CMO, then the current price is set to the minimum between
        /// offer and CMO+minimumBidIncrement, the maximum offer is set to offer , and the bidder becomes the
        /// current winner;
        /// • if this is NOT the first bid, the bidder is NOT the current winner, and offer is NOT higher than the current
        /// maximum offer, in the following denoted by CMO, then the current price is set to the minimum between
        /// CMO and offer +minimumBidIncrement, and the current winner does not change.
        /// </summary>
        /// <exception cref="UnavailableDbException"></exception>
        /// <param name="auct"></param>
        /// <param name="bidderSess"></param>
        /// <param name="offer"></param>
        private static void UpdateAuctionBid(AuctionEntity auct,SessionEntity bidderSess,double offer)
        {
            try
            {

                if (auct.Cmo == 0 || (auct.Cmo > 0 && auct.CurrentWinner == bidderSess.User))
                {
                    auct.Cmo = offer;
                    auct.CurrentWinner = bidderSess.User;
                }

                if (!(auct.Cmo > 0) || auct.CurrentWinner.Equals(bidderSess.User)) return;
                if (offer > auct.Cmo)
                {
                    auct.CurrentWinner = bidderSess.User;
                    auct.CurrentPrice = Math.Min(offer, auct.Cmo + auct.Site.MinBidIncr);
                }
                else if (offer < auct.Cmo)
                    auct.CurrentPrice = Math.Min(auct.Cmo, offer + auct.Site.MinBidIncr);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new UnavailableDbException("Connection error!");
            }
        }

        //EQUALS AND GETHASHCODE OVERRIDE//
        public override bool Equals(object obj) =>
            !(obj is null) && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((Auction)obj));

        protected bool Equals(Auction other) =>
            string.Equals(_connectionString, other._connectionString) &&  
            _alarmClock.Equals(other._alarmClock) &&
            string.Equals(Description, other.Description) &&
            Equals(Id, other.Id) &&
            Seller.Equals(other.Seller) &&
            CheckEqualDate(EndsOn, other.EndsOn);

        public override int GetHashCode() 
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + _connectionString.GetHashCode();
                hash = hash * 23 + _alarmClock.GetHashCode();
                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + Seller.GetHashCode();
                hash = hash * 23 + Description.GetHashCode();
                hash = hash * 23 + EndsOn.GetHashCode();
                return hash;
            }
        }
    }
}