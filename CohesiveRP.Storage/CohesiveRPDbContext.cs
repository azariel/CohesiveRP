using Microsoft.EntityFrameworkCore;

namespace CohesiveRP.Storage
{
    public class CohesiveRPDbContext : DbContext
    {
        // ********************************************************************
        //                            Constructors
        // ********************************************************************
        public CohesiveRPDbContext()
        {
            Database.EnsureCreated();
        }
    }
}
