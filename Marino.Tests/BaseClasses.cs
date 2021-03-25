namespace Marino.Tests {
    using System;
    using TAP2018_19.AlarmClock.Interfaces;
    using TAP2018_19.AuctionSite.Interfaces;
    using NUnit.Framework;
    using Ninject;

    internal static class Configuration {
        internal const string ImplementationAssembly =
            @"..\..\..\Marino\bin\Debug\Marino.dll";

        internal const string ConnectionString =
            @"Data Source=.\SQLEXPRESS;Initial Catalog=AuctionDb;Integrated Security=True;";
    }

    [TestFixture]
    public abstract class AbstractTest {
        protected static readonly IAlarmClockFactory AnAlarmClockFactory;
        protected static readonly ISiteFactory AnAuctionSiteFactory;
        protected static readonly string ImplementationAssembly = Configuration.ImplementationAssembly;

        public static ISiteFactory LoadSiteFactoryFromModule() {
            var kernel = new StandardKernel();
            ISiteFactory result = null;
            try {
                kernel.Load(Configuration.ImplementationAssembly);
                result = kernel.Get<ISiteFactory>();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }

            return result;
        }

        static AbstractTest() {
            var kernel = new StandardKernel();

            try {
                AnAuctionSiteFactory = LoadSiteFactoryFromModule();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }

    public abstract class AuctionSiteTest : AbstractTest {
        protected ISiteFactory GetSiteFactory() {
            return AbstractTest.AnAuctionSiteFactory;
        }

        protected string GetConnectionString() {
            return Configuration.ConnectionString;
        }
    }
}