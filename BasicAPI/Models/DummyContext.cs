using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BasicAPI.Models
{
    public class DummyContext : DbContext
    {
        public DummyContext(DbContextOptions<DummyContext> options) : base(options)
        {
            
        }
        public DbSet<Dummy> DummyItems { get; set; }
    }
}
