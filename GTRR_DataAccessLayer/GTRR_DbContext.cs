using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRR_DataAccessLayer
{
    public  class GTRR_DbContext : DbContext
    {
   


        public GTRR_DbContext(DbContextOptions<GTRR_DbContext> options)
      : base(options)
        {
        }

        public DbSet<PalletManagement> PalletManagements { get; set; }
    }
}
