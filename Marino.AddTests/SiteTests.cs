using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Marino.Tests;
using Moq;
using NUnit.Framework;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;

namespace Marino.AddTests
{
    [TestFixture]
    public class SiteTests : InstrumentedAuctionSiteTest
    {
        protected ISite Site;
        protected Mock<IAlarmClock> AlarmClock;

        /// <summary>
        /// Initializes Site:
        /// <list type="table">
        /// <item>
        /// <term>name</term>
        /// <description>working site</description>
        /// </item>
        /// <item>
        /// <term>time zone</term>
        /// <description>5</description>
        /// </item>
        /// <item>
        /// <term>expiration time</term>
        /// <description>3600 seconds</description>
        /// </item>
        /// <item>
        /// <term>minimum bid increment</term>
        /// <description>3.5</description>
        /// </item>
        /// <item>
        /// <term>users</term>
        /// <description>empty list</description>
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
        public void SiteInitialize()
        {
            const string workingSite = "working site";
            const int timeZone = 5;
            Site = CreateAndLoadEmptySite(timeZone, workingSite, 3600, 3.5, out AlarmClock);
        }

        private ISession CreateAndLogin_User(string Username, string Password)
        {
            Site.CreateUser(Username, Password);
            return Site.Login(Username, Password);
        }

        private IEnumerable<IAuction> AddAuctions(DateTime EndsOn1, int howMany1)
        {
            Debug.Assert(howMany1 >= 0);
            var username = "pinco" + DateTime.Now.Ticks;
            Site.CreateUser(username, "pippo.123");
            var sellerSession = Site.Login(username, "pippo.123");
            var result = new List<IAuction>();
            for (int i = 0; i < howMany1; i++)
                result.Add(sellerSession.CreateAuction($"Auction {i} of {howMany1} ending on {EndsOn1}",
                    EndsOn1, 7.7 * i + 11));
            return result;
        }

        /// <summary>
        /// Verify that GetAuctions on a site without users
        /// returns a not null (possibly empty) list
        /// </summary>
        [Test]
        public void GetAuctions_ValidArg_ReturnsNotNull1()
        {
            var auctions = Site.GetAuctions(false);
            Assert.That(auctions, Is.Not.Null);
        }

        /// <summary>
        /// Verify that GetAuctions(true) on a site with only expired auctions
        /// returns a not null (possibly empty) list 
        /// </summary>
        [Test]
        public void GetAuctions_ValidArg_ReturnsNotNull2()
        {
            var now = AlarmClock.Object.Now;
            AddAuctions(now.AddDays(1), 5);
            AlarmClock.Setup(ac => ac.Now).Returns(now.AddHours(25));
            Site = siteFactory.LoadSite(connectionString, Site.Name, AlarmClock.Object); //needed to refresh time
            var auctions = Site.GetAuctions(true);
            Assert.That(auctions, Is.Not.Null);
        }

        /// <summary>
        /// Verify that GetAuctions with ten auctions
        /// returns the ten auctions
        /// </summary>
        [Test]
        public void GetAuctions_ValidArg_Returns10Auctions()
        {
            var now = AlarmClock.Object.Now;
            var expectedAuctions = AddAuctions(now.AddDays(2), 10).ToList();
            var auctionsList = Site.GetAuctions(false).ToList();
            Assert.That(expectedAuctions, Is.EquivalentTo(auctionsList));
        }

        /// <summary>
        /// Verify that GetSessions on correct arguments
        /// returns the empty sequence
        /// </summary>
        [Test]
        public void GetSessions_ValidArg_ReturnsEmpty()
        {
            var sessions = Site.GetSessions();
            Assert.That(sessions, Is.Empty);
        }

        /// <summary>
        /// Verify that GetSessions on correct arguments
        /// returns a not null (possibly empty) list
        /// </summary>
        [Test]
        public void GetSessions_ValidArg_ReturnsNonnull()
        {
            var sessions = Site.GetSessions();
            Assert.That(sessions, Is.Not.Null);
        }

        /// <summary>
        /// Verify that GetSessions with three users
        /// returns the three sessions
        /// </summary>
        [Test]
        public void GetSessions_ValidArg_Returns3Sessions()
        {
            var expectedSessions = new List<ISession>();
            for (var i = 0; i < 3; i++)
            {
                var session = CreateAndLogin_User("user" + i, "pwuser" + i);
                expectedSessions.Add(session);
            }

            var sessionsList = Site.GetSessions().ToList();
            Assert.That(sessionsList, Is.EquivalentTo(expectedSessions));
        }

        /// <summary>
        /// Verify that a call to GetSessions on a
        /// deleted site throws InvalidOperationException
        /// </summary>
        [Test]
        public void GetSessions_OnDeletedObject_Throws()
        {
            Site.Delete();
            Assert.That(() => Site.GetSessions(), Throws.TypeOf<InvalidOperationException>());
        }
        /// <summary>
        /// Verify that a site with a session,when is deleted,goes away with the session too
        /// </summary>
        [Test]
        public void Delete_SiteWithSessions()
        {
            var username = "user";
            var password = "pwuser";
            Site.CreateUser(username, password);
            Site.Login(username, password);
            Site.Delete();
        }
        /// <summary>
        /// Verify that calling a delete on a site with auctions deletes also the auctions
        /// </summary>
        [Test]
        public void Delete_SiteWithAuctions()
        {
            AddAuctions(AlarmClock.Object.Now.AddDays(4), 3);
            Site.Delete();
        }
        /// <summary>
        /// Verify that GetUsers on correct arguments
        /// returns a not null (possibly empty) list
        /// </summary>
        [Test]
        public void GetUsers_ValidArg_ReturnsNonnull()
        {
            var users = Site.GetUsers();
            Assert.That(users, Is.Not.Null);
        }

        /// <summary>
        /// Verify that GetUsers with ten users
        /// returns the ten users
        /// </summary>
        [Test]
        public void GetUsers_ValidArg_Returns10Users()
        {
            var expectedUsers = new List<string>();
            for (var i = 0; i < 10; i++)
            {
                var user = "user" + i;
                Site.CreateUser(user, "pwuser" + i);
                expectedUsers.Add(user);
            }

            var usersList = Site.GetUsers().Select(u => u.Username).ToList();
            Assert.That(usersList, Is.EquivalentTo(expectedUsers));
        }
    }

    [TestFixture]
    public class SiteTestsWithCreateTwoSites : InstrumentedAuctionSiteTest
    {
        protected ISite Site1, Site2;
        protected int SessionExpirationTinInSeconds = 3600;
        protected Mock<IAlarmClock> AlarmClock;

        [SetUp]
        public void TwoSiteInitialize()
        {
            const string workingSite1 = "working site 1";
            const string workingSite2 = "working site 2";
            const int timeZone = 5;
            Site1 = CreateAndLoadEmptySite(timeZone, workingSite1, SessionExpirationTinInSeconds, 3.5, out AlarmClock);
            Site2 = CreateAndLoadEmptySite(timeZone, workingSite2, SessionExpirationTinInSeconds, 3.5, out AlarmClock);
        }

        private IEnumerable<IAuction> AddAuctions(ISite site, DateTime EndsOn1, int howMany1)
        {
            Debug.Assert(howMany1 >= 0);
            var username = "pinco" + DateTime.Now.Ticks;
            site.CreateUser(username, "pippo.123");
            var sellerSession = site.Login(username, "pippo.123");
            var result = new List<IAuction>();
            for (var i = 0; i < howMany1; i++)
                result.Add(sellerSession.CreateAuction($"Auction {i} of {howMany1} ending on {EndsOn1}",
                    EndsOn1, 7.7 * i + 11));
            return result;
        }

        private static ISession CreateAndLogin_User(ISite Site, string Username, string Password)
        {
            Site.CreateUser(Username, Password);
            return Site.Login(Username, Password);
        }

        /// <summary>
        /// Verify that CleanupSessions delete only
        /// expired sessions of a site and it doesn't
        /// delete all expired sessions of the database
        /// </summary>
        [Test]
        public void CleanupSessions_TwoSites()
        {
            var now = AlarmClock.Object.Now;
            CreateAndLogin_User(Site1, "usersite1", "pwsite1");
            CreateAndLogin_User(Site2, "usersite2", "pwsite2");
            
            AlarmClock.Setup(ac => ac.Now).Returns(now.AddSeconds(SessionExpirationTinInSeconds + 1));
            
            Site1.CleanupSessions();
            Assert.That(Site2.GetSessions(), Is.Not.Empty);
        }

        /// <summary>
        /// Verify that Delete a site deletes only that site
        /// </summary>
        [Test]
        public void Delete_TwoSites()
        {
            Site1.Delete();
            Assert.That(siteFactory.GetSiteNames(connectionString).Any(s => s == "working site 2"), Is.True);
        }

        /// <summary>
        /// Verify that GetAuctions returns only the
        /// auctions of a site
        /// </summary>
        [Test]
        public void GetAuctions_TwoSites()
        {
            var now = AlarmClock.Object.Now;
            AddAuctions(Site1, now.AddDays(1), 2);
            AddAuctions(Site2, now.AddDays(1), 1);
            Assert.That(Site1.GetAuctions(false).Count(), Is.EqualTo(2));
        }

        /// <summary>
        /// Verify that GetSession returns session
        /// of a user of the site
        /// </summary>
        [Test]
        public void GetSession_TwoSites()
        {
            var session = CreateAndLogin_User(Site1, "usersite1", "pwsite1");
            Assert.That(Site2.GetSession(session.Id), Is.Null);
        }

        /// <summary>
        /// Verify that GetSessions returns only the
        /// sessions of a site
        /// </summary>
        [Test]
        public void GetSessions_TwoSites()
        {
            CreateAndLogin_User(Site1, "usersite1", "pwsite1");
            Assert.That(Site2.GetSessions(), Is.Empty);
        }

        /// <summary>
        /// Verify that GetUsers returns only the
        /// users of a site
        /// </summary>
        [Test]
        public void GetUsers_TwoSites()
        {
            Site1.CreateUser("usersite1", "pwsite1");
            Site2.CreateUser("usersite2", "pwsite2");
            Assert.That(Site2.GetUsers().Count(), Is.EqualTo(1));
        }

        /// <summary>
        /// Verify that Login returns null if you are
        /// logging in to a site with another site's credentials
        /// </summary>
        [Test]
        public void Login_TwoSites()
        {
            var username = "user";
            var password = "pwuser";
            Site1.CreateUser(username, password);
            Assert.That(Site2.Login(username, password), Is.Null);
        }

    }
}
