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
    public class AuctionTests : InstrumentedAuctionSiteTest
    {
        protected ISite Site;
        protected Mock<IAlarmClock> AlarmClock;

        protected IUser Seller;
        protected ISession SellerSession;

        protected IUser Bidder1;
        protected ISession Bidder1Session;

        protected IAuction TheAuction;

        protected const string SiteName = "site for auction tests";
        protected const string OtherSiteName = "other site for auction tests";

        protected const int Timezone = -2;
        /// <summary>
        /// Initializes Site:
        /// <list type="table">
        /// <item>
        /// <term>name</term>
        /// <description>SiteName = "site for auction tests"</description>
        /// </item>
        /// <item>
        /// <term>time zone</term>
        /// <description>-2</description>
        /// </item>
        /// <item>
        /// <term>expiration time</term>
        /// <description>300 seconds</description>
        /// </item>
        /// <item>
        /// <term>minimum bid increment</term>
        /// <description>7</description>
        /// </item>
        /// <item>
        /// <term>users</term>
        /// <description>Seller, Bidder1</description>
        /// </item>
        /// <item>
        /// <term>auctions</term>
        /// <description>TheAuction ("Beautiful object to be desired by everybody",
        /// starting price 5, ends in 7 days)</description>
        /// </item>
        /// <item>
        /// <term>sessions</term>
        /// <description>SellerSession, Bidder1Session</description>
        /// </item>
        /// </list>  
        /// </summary>

        [SetUp]
        public void SiteUSersAuctionInitialize()
        {
            siteFactory.CreateSiteOnDb(connectionString, SiteName, Timezone, 300, 7);
            AlarmClockMock(Timezone, out AlarmClock);
            Site = siteFactory.LoadSite(connectionString, SiteName, AlarmClock.Object);
            Seller = CreateAndLogUser("seller", out SellerSession, Site);
            Bidder1 = CreateAndLogUser("bidder1", out Bidder1Session, Site);
            TheAuction = SellerSession.CreateAuction("Lidl socks",
                AlarmClock.Object.Now.AddDays(7), 5);
        }

        /// <summary>
        /// Verify that BidOnAuction with the
        /// auction expired throws InvalidOperationException
        /// </summary>
        [Test]
        public void BidOnAuctions_EndsOn_Throws()
        {
            var now = AlarmClock.Object.Now;
            AlarmClock.Setup(ac => ac.Now).Returns(now.AddDays(8));
            Bidder1Session = Site.Login("bidder1", "bidder1");
            Assert.That(() => TheAuction.BidOnAuction(Bidder1Session, 8), Throws.TypeOf<InvalidOperationException>());
        }

        /// <summary>
        /// Verify that BidOnAuction from seller
        /// throws ArgumentException
        /// </summary>
        [Test]
        public void BidOnAuction_Seller_Throws()
        {
            Assert.That(() => TheAuction.BidOnAuction(SellerSession, 8), Throws.TypeOf<ArgumentException>());
        }

        /// <summary>
        /// Verify that BidOnAuction from a user
        /// of other site throws ArgumentException
        /// </summary>
        [Test]
        public void BidOnAuction_OtherSite_Throws()
        {
            siteFactory.CreateSiteOnDb(connectionString, OtherSiteName, Timezone, 300, 7);
            var otherSite = siteFactory.LoadSite(connectionString, OtherSiteName, AlarmClock.Object);

            CreateAndLogUser("userothersite", out var session, otherSite);
            Assert.That(() => TheAuction.BidOnAuction(session, 8), Throws.TypeOf<ArgumentException>());
        }

        /// <summary>
        /// Verify that BidOnAuction with offer zero returns false
        /// </summary>
        [Test]
        public void BidOnAuction_OfferZero_False()
        {
            var accepted = TheAuction.BidOnAuction(Bidder1Session, 0);
            Assert.That(!accepted);
        }

        /// <summary>
        /// Verify that a call to CurrentPrice on a
        /// deleted auction throws InvalidOperationException
        /// </summary>
        [Test]
        public void CurrentPrice_OnDeletedObject_Throws()
        {
            TheAuction.Delete();
            Assert.That(() => TheAuction.CurrentPrice(), Throws.TypeOf<InvalidOperationException>());
        }

        /// <summary>
        /// Verify that a call to CurrentWinner on a
        /// deleted auction throws InvalidOperationException
        /// </summary>
        [Test]
        public void CurrentWinner_OnDeleteObject_Throws()
        {
            TheAuction.Delete();
            Assert.That(() => TheAuction.CurrentWinner(), Throws.TypeOf<InvalidOperationException>());
        }
        protected IUser CreateAndLogUser(string username, out ISession session, ISite site)
        {
            site.CreateUser(username, username);
            session = site.Login(username, username);
            return site.GetUsers().FirstOrDefault(u => u.Username == username);
        }
    }
}
