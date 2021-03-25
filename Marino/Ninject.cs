using System.Data.Entity;
using System.Security;
using Marino.DB;
using Marino.Implementation;
using Ninject.Modules;
using TAP2018_19.AuctionSite.Interfaces;

namespace Marino
{
    [SecurityCritical]
    public class AuctionSiteNinjectModule : NinjectModule
    {
        [SecurityCritical]
        public override void Load()
        {
            Database.SetInitializer<BaseContext>(null);
            Bind<ISiteFactory>().To<SiteFactory>();
            Bind<ISite>().To<Site>();
            Bind<IAuction>().To<Auction>();
            Bind<IUser>().To<User>();
            Bind<ISession>().To<Session>();  
        }
    }
}
