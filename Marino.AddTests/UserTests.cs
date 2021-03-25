using System;
using System.Linq;
using Marino.Tests;
using Moq;
using NUnit.Framework;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;

namespace Marino.AddTests
{
    [TestFixture]
    public class UserTests : InstrumentedAuctionSiteTest
    {
        protected ISite Site;
        protected ISite OtherSite;
        protected const string SiteName = "site for user tests";
        protected const string OtherSiteName = "other site for user tests";
        protected const int Timezone = -2;
        
        protected Mock<IAlarmClock> AlarmClock;
        protected Mock<IAlarmClock> OtherAlarmClock;
        
        protected IUser User;
        protected ISession UserSession;
        protected const string UserName = "TopoGigio";

        protected IUser Seller;
        protected const string SellerName = "Seller";
        protected ISession SellerSession;

        protected IUser Bidder1;
        protected ISession Bidder1Session;
        
        protected IUser Bidder2;
        protected ISession Bidder2Session;


        protected IAuction TheAuction;

        protected int SecondsInADay = 86400;

        /// <summary>
        /// Initializes Site:
        /// <list type="table">
        /// <item>
        /// <term>name</term>
        /// <description>site for user tests</description>
        /// </item>
        /// <item>
        /// <term>time zone</term>
        /// <description>-5</description>
        /// </item>
        /// <item>
        /// <term>expiration time</term>
        /// <description>360 seconds</description>
        /// </item>
        /// <item>
        /// <term>minimum bid increment</term>
        /// <description>7</description>
        /// </item>
        /// <item>
        /// <term>users</term>
        /// <description>username = "My Dear Friend", pw = "f86d 78ds6^^^55"</description>
        /// </item>
        /// <item>
        /// <term>auctions</term>
        /// <description>empty list</description>
        /// </item>
        /// <item>
        /// <term>sessions</term>
        /// <description>empty list</description>
        /// </item>
        /// </list>  
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            Site = CreateAndLoadEmptySite(Timezone, SiteName, 360, 7, out AlarmClock);
            Seller = CreateAndLogUser(SellerName, out SellerSession, Site);
            TheAuction = SellerSession.CreateAuction("lidl socks", AlarmClock.Object.Now.AddDays(3), 2);
            Assert.That(Seller, Is.Not.Null, "Set up should be successful");
        }
        /// <summary>
        /// verify that deleting a creator of an auction not ended yet
        /// throws InvalidOperationException
        /// </summary>
        [Test]
        public void OnDelete_CreatorOf_Auction_NotEnded_Throws()
        {
            Assert.That(() => Seller.Delete(), Throws.TypeOf<InvalidOperationException>());
        }
        /// <summary>
        /// verify that deleting a creator of an auction ended
        /// also deletes the auction
        /// </summary>
        [Test]
        public void Delete_CreatorOf_Auction_Ended()
        {
            SetNowToFutureTime(SecondsInADay * 4,AlarmClock);
            Seller.Delete();
            Assert.That(Site.GetAuctions(false),Is.Empty);
        }
        /// <summary>
        /// verify that after deleting a user
        /// also deletes his sessions
        /// </summary>
        [Test]
        public void OnDelete_Delete_User_Session()
        {
            SetNowToFutureTime(SecondsInADay*3+1, AlarmClock);
            Seller.Delete();
            Assert.That(Site.GetSessions(), Is.Empty);
        }

        /// <summary>
        /// verify that after deleting a user that is a winner of an auction
        /// the current winner of the auction will be null
        /// </summary>

        [Test]
        public void OnCreatorDelete_Null_CurrentWinner()
        {
            Bidder1 = CreateAndLogUser("bidder1", out Bidder1Session, Site);
            TheAuction = SellerSession.CreateAuction("lidl socks", AlarmClock.Object.Now.AddDays(3), 2);
            TheAuction.BidOnAuction(Bidder1Session, 8);
            var now = AlarmClock.Object.Now;
            AlarmClock.Setup(ac => ac.Now).Returns(now.AddDays(8));
            Bidder1.Delete();
            Assert.That(TheAuction.CurrentWinner(), Is.Null);
        }
        
        /// <summary>
        /// verify that the delete of a user in a site,delete only the user of that site
        /// and not other users that have the same username but are in other sites!
        /// </summary>
        [Test]
        public void TwoSites_UsersDelete()
        {
            OtherSite = CreateAndLoadEmptySite(Timezone, OtherSiteName, 360, 7, out OtherAlarmClock);
            var evenAnotherSite = CreateAndLoadEmptySite(Timezone, "n'altro? mo basta", 360, 7, out OtherAlarmClock);
            User = CreateAndLogUser(SellerName, out UserSession, OtherSite);
            var user2 = CreateAndLogUser(SellerName, out UserSession, evenAnotherSite);
            User.Delete();
            user2.Delete();
            Assert.Multiple(() =>
                {
                    Assert.That(OtherSite.GetUsers(), Is.Empty);
                    Assert.That(evenAnotherSite.GetUsers(), Is.Empty);
                });
        }
        /// <summary>
        /// verify that calling WonAuctions on a deleted user
        /// throws InvalidOperationException
        /// </summary>
        [Test]
        public void WonAuctions_On_DeletedUser_Throws()
        {
            User = CreateAndLogUser(UserName, out UserSession, Site);
            User.Delete();
            Assert.That(() => User.WonAuctions(), Throws.TypeOf<InvalidOperationException>());
        }
        
        /// <summary>
        /// verify that a call of wonAuctions of a user in a site
        ///returns only his auction and not other auctions won by
        /// users in other sites with same username !
        /// </summary>
        [Test]
        public void TwoSites_Users_WonAuctions()
        {
            OtherSite = CreateAndLoadEmptySite(Timezone, OtherSiteName, 360, 7, out OtherAlarmClock);
            User = CreateAndLogUser(UserName, out UserSession, OtherSite);
            Bidder1 = CreateAndLogUser("bidder1", out Bidder1Session, OtherSite);
            
            Bidder2 = CreateAndLogUser("bidder2", out Bidder2Session, Site);
            TheAuction.BidOnAuction(Bidder2Session, 8);
            
            var otherAuction = UserSession.CreateAuction("lidl socks", AlarmClock.Object.Now.AddDays(3), 2);
            otherAuction.BidOnAuction(Bidder1Session, 8);
            SetNowToFutureTime(86400*8, AlarmClock);
            SetNowToFutureTime(86400 * 8, OtherAlarmClock);
            Assert.Multiple(() =>
            {
                Assert.That(Bidder1.WonAuctions().Count() == 1);
                Assert.That(Bidder2.WonAuctions().Count() == 1);
            });

        }

        /// <summary>
        /// verify that a call of wonAuctions of a user
        /// when the auction is not ended,returns an empty list
        /// </summary>
        [Test]
        public void WonAuctions_On_StillOpenAuction_Returns_Empty()
        {
            Bidder1 = CreateAndLogUser("bidder", out Bidder1Session, Site);
            TheAuction.BidOnAuction(Bidder1Session, 8);
            Assert.That(Bidder1.WonAuctions(), Is.Empty);
        }
        /// <summary>
        /// verify that a call of wonAuctions of a winner user
        /// doesn't return null
        /// </summary>
        [Test]
        public void WonAuctions_On_Normal_User()
        {
            Assert.That(Seller.WonAuctions(),Is.Not.Null);
        }
        protected IUser CreateAndLogUser(string username, out ISession session, ISite site)
        {
            site.CreateUser(username, username);
            session = site.Login(username, username);
            return site.GetUsers().FirstOrDefault(u => u.Username == username);
        }
    }
}
