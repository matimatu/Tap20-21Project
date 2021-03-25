using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using TAP2018_19.AuctionSite.Interfaces;

namespace Marino.DB
{
    public class BaseContext : DbContext 
    {
        public BaseContext(string connectionString) : base(connectionString)
        {
        }
        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (Exception error) when (error.GetBaseException() is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
            {
                Debug.WriteLine(error.Message);
                throw new NameAlreadyInUseException(nameof(error), "There is already another key with this name!");
            }
            catch (DBConcurrencyException error)
            {
                Debug.WriteLine(error.Message);
                throw new ConcurrentChangeException("Concurrency problems!", error);
            }
            catch (Exception error)
            {
                Debug.WriteLine(error.Message);
                throw new UnavailableDbException("Connection error!", error);
            }
           
        }
      
    }
    
}



