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
    public class SessionTests : InstrumentedAuctionSiteTest
    {
        protected ISite Site;
        protected Mock<IAlarmClock> AlarmClock;

        protected IUser User;
        protected ISession UserSession;

        protected IUser Seller;
        protected ISession SellerSession;

        protected IAuction TheAuction;
        protected string Description = "Beautiful object to be desired by everybody";
        protected DateTime EndsOn;
        protected double StartingPrice = 5;

        protected const string UserName = "My Dear Friend";

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
        /// <description>User (with UserName = "My Dear Friend"), Seller</description>
        /// </item>
        /// <item>
        /// <term>auctions</term>
        /// <description>empty list</description>
        /// </item>
        /// <item>
        /// <term>sessions</term>
        /// <description>UserSession, SellerSession</description>
        /// </item>
        /// </list>  
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            Site = CreateAndLoadSite(-5, "site for user tests", 360, 7, out AlarmClock);
            User = CreateAndLogUser(UserName, out UserSession, Site);
            Seller = CreateAndLogUser("seller", out SellerSession, Site);
            EndsOn = AlarmClock.Object.Now.AddDays(7);
            TheAuction = SellerSession.CreateAuction(Description, EndsOn, StartingPrice);
        }

        protected IUser CreateAndLogUser(string username, out ISession session, ISite site)
        {
            site.CreateUser(username, username);
            session = site.Login(username, username);
            return site.GetUsers().FirstOrDefault(u => u.Username == username);
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. TheAuction id
        /// </summary>
        [Test]
        public void CreateAuction_DbValidArg_Id()
        {
            Assert.That(Site.GetAuctions(false).Select(a => a.Id), Is.Not.Null);
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. TheAuction seller
        /// </summary>
        [Test]
        public void CreateAuction_DbValidArg_Seller()
        {
            Assert.That(Site.GetAuctions(false).Any(a => a.Seller.Equals(Seller)));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. TheAuction description
        /// </summary>
        [Test]
        public void CreateAuction_DbValidArg_Description()
        {
            Assert.That(Site.GetAuctions(false).Any(a => a.Description == Description));
        }

        /// <summary>
        /// Verify that the setup is correct w.r.t. TheAuction endsOn
        /// </summary>
        [Test]
        public void CreateAuction_DbValidArg_EndsOn()
        {
            Assert.That(Site.GetAuctions(false).Any(a => SameDateTime(a.EndsOn, EndsOn)));
        }


    }
}
