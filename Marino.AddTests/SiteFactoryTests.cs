using System;
using System.Linq;
using Marino.Tests;
using Moq;
using NUnit.Framework;
using TAP2018_19.AlarmClock.Interfaces;

namespace Marino.AddTests
{
    [TestFixture]
    public class SiteFactoryBasicTest : InstrumentedAuctionSiteTest
    {
        /// <summary>
        /// Verify that CreateSiteOnDb on a 0 minimum bid increment
        /// throws ArgumentOutOfRangeException
        /// </summary>
        [Test]
        public void CreateSiteOnDb_ZeroMinimumBidIncrement_Throws()
        {
            Assert.That(() => siteFactory.CreateSiteOnDb(connectionString, "troppo giusto", 1, 600, 0),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Verify that CreateSiteOnDb on a 0 session expiration time
        /// throws ArgumentOutOfRangeException
        /// </summary>
        [Test]
        public void CreateSiteOnDb_ZeroSessionExpirationTime_Throws()
        {
            Assert.That(() => siteFactory.CreateSiteOnDb(connectionString, "troppo giusto", 1, 0, 0.01),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Verify that CreateSiteOnDb on a name shorter that
        /// DomainConstraints.MinSiteName throws ArgumentException
        /// </summary>
        [Test]
        public void CreateSiteOnDb_TooShort_name_Throws()
        {
            Assert.That(() => siteFactory.CreateSiteOnDb(connectionString, "", 1, 600, 0.01),
                Throws.TypeOf<ArgumentException>());
        }

        /// <summary>
        /// Verify that CreateSiteOnDb on a name longer that
        /// DomainConstraints.MaxSiteName throws ArgumentException
        /// </summary>
        [Test]
        public void CreateSiteOnDb_TooLong_name_Throws()
        {
            Assert.That(() => siteFactory.CreateSiteOnDb(connectionString, "MP6rVLwFsoy1WYITJjrY1oETEy3kvLRkMCAAZvikN47O8h6ak8dYSS1eh9a2SOslqeXbOW6ry63oqwyUu8N4MmjYY7FHO5aJ9OjwvTosg9Tdc5Xxlu1gpmM13lME1wUTm", 1, 600, 0.01),
                Throws.TypeOf<ArgumentException>());
        }
    }

    public class SiteFactoryTestsCreateEmptySite : InstrumentedAuctionSiteTest
    {
        private Mock<IAlarmClock> _alarmClockMock;

        [SetUp]
        public void CreateSite()
        {
            siteFactory.CreateSiteOnDb(connectionString, "questo va", 3, 673, 2.8);
            AlarmClockMock(3, out _alarmClockMock);
        }

        /// <summary>
        /// Verify that GetTheTimezoneOf on a name shorter that
        /// DomainConstraints.MinSiteName throws ArgumentException
        /// </summary>
        [Test]
        public void GetTheTimezoneOf_TooShort_name_Throws()
        {
            Assert.That(() => siteFactory.GetTheTimezoneOf(connectionString, ""), Throws.TypeOf<ArgumentException>());
        }

        /// <summary>
        /// Verify that GetTheTimezoneOf on a name longer that
        /// DomainConstraints.MaxSiteName throws ArgumentException
        /// </summary>
        [Test]
        public void GetTheTimezoneOf_TooLong_name_Throws()
        {
            Assert.That(
                () => siteFactory.GetTheTimezoneOf(connectionString,
                    "MP6rVLwFsoy1WYITJjrY1oETEy3kvLRkMCAAZvikN47O8h6ak8dYSS1eh9a2SOslqeXbOW6ry63oqwyUu8N4MmjYY7FHO5aJ9OjwvTosg9Tdc5Xxlu1gpmM13lME1wUTm"),
                Throws.TypeOf<ArgumentException>());
        }

        /// <summary>
        /// Verify that GetTheTimezoneOf returns 
        /// correct timezone of the two sites
        /// </summary>
        [Test]
        public void GetTheTimezoneOf_ValidArgs_TwoSites()
        {
            siteFactory.CreateSiteOnDb(connectionString, "questo vaaa", 2, 609, 0.01);
            Assert.Multiple(() =>
            {
                Assert.That(siteFactory.GetTheTimezoneOf(connectionString, "questo va"), Is.EqualTo(3));
                Assert.That(siteFactory.GetTheTimezoneOf(connectionString, "questo vaaa"), Is.EqualTo(2));
            });
        }

        /// <summary>
        /// Verify that LoadSite on a name shorter that
        /// DomainConstraints.MinSiteName throws ArgumentException
        /// </summary>
        [Test]
        public void LoadSite_TooShort_name_Throws()
        {
            Assert.That(() => siteFactory.LoadSite(connectionString, "", _alarmClockMock.Object),
                Throws.TypeOf<ArgumentException>());
        }

        /// <summary>
        /// Verify that LoadSite on a name longer that
        /// DomainConstraints.MaxSiteName throws ArgumentException
        /// </summary>
        [Test]
        public void LoadSite_TooLong_name_Throws()
        {
            Assert.That(
                () => siteFactory.LoadSite(connectionString,
                    "MP6rVLwFsoy1WYITJjrY1oETEy3kvLRkMCAAZvikN47O8h6ak8dYSS1eh9a2SOslqeXbOW6ry63oqwyUu8N4MmjYY7FHO5aJ9OjwvTosg9Tdc5Xxlu1gpmM13lME1wUTm",
                    _alarmClockMock.Object), Throws.TypeOf<ArgumentException>());
        }
    }

    public class SiteFactoryTestsWithTestCase : InstrumentedAuctionSiteTest
    {
        private const int Timezone = 3;

        /// <summary>
        /// Verify that CreateSiteOnDb on name of
        /// 1 and 128 chars is successful and the
        /// GetSiteNames returns count is equal 1
        /// </summary>
        /// <param name="name">Site name</param>
        [Test]
        [TestCase("n")]
        [TestCase("gx2YvQrLC5y3vZuDagfF3flPBuoUEXX5NTj3sNt34iFzumXCg0qUX4uH6uIUXPhbhvRjnXqdYOlTPpDGxomn3kBWo7Rg32560eEP5bnHqtstxwY0widpZcuEdLXKevzi")]
        public void CreateSiteOnDb_ValidArg_name(string name)
        {
            siteFactory.CreateSiteOnDb(connectionString, name, Timezone, 673, 2.8);
            var expectedCount = siteFactory.GetSiteNames(connectionString).Count();
            Assert.That(expectedCount == 1);
        }

        /// <summary>
        /// Verify that CreateSiteOnDb on timezone
        /// -12 and 12 is successful and the
        /// GetSiteNames returns count is equal 1
        /// </summary>
        /// <param name="timezone">Timezone of the site</param>
        [Test]
        [TestCase(-12)]
        [TestCase(12)]
        public void CreateSiteOnDb_ValidArgTimezone(int timezone)
        {
            siteFactory.CreateSiteOnDb(connectionString, "questo va", timezone, 673, 2.8);
            var expectedCount = siteFactory.GetSiteNames(connectionString).Count();
            Assert.That(expectedCount == 1);
        }

        /// <summary>
        /// Verify that GetTheTimezoneOf on name of
        /// 1 and 128 chars is successful and
        /// returns the corresponding timezone
        /// </summary>
        /// <param name="name">Site name</param>
        [Test]
        [TestCase("n")]
        [TestCase("gx2YvQrLC5y3vZuDagfF3flPBuoUEXX5NTj3sNt34iFzumXCg0qUX4uH6uIUXPhbhvRjnXqdYOlTPpDGxomn3kBWo7Rg32560eEP5bnHqtstxwY0widpZcuEdLXKevzi")]
        public void GetTheTimezoneOf_ValidArg_name(string name)
        {
            siteFactory.CreateSiteOnDb(connectionString, name, Timezone, 673, 2.8);

            var expectedTimezone = siteFactory.GetTheTimezoneOf(connectionString, name);
            Assert.That(expectedTimezone == Timezone);
        }

        /// <summary>
        /// Verify that LoadSite on name of
        /// 1 and 128 chars is successful and
        /// the site name matches
        /// </summary>
        /// <param name="name">Site name</param>
        [Test]
        [TestCase("n")]
        [TestCase("gx2YvQrLC5y3vZuDagfF3flPBuoUEXX5NTj3sNt34iFzumXCg0qUX4uH6uIUXPhbhvRjnXqdYOlTPpDGxomn3kBWo7Rg32560eEP5bnHqtstxwY0widpZcuEdLXKevzi")]
        public void LoadSite_ValidArg_name(string name)
        {
            siteFactory.CreateSiteOnDb(connectionString, name, Timezone, 673, 2.8);

            AlarmClockMock(Timezone, out var alarmClock);
            var newSite = siteFactory.LoadSite(connectionString, name, alarmClock.Object);
            Assert.That(newSite.Name == name);
        }
    }
}
