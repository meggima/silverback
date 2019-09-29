﻿using Microsoft.EntityFrameworkCore;
using Silverback.Background.Model;

namespace Silverback.Tests.Core.TestTypes.Database
{
    public class TestDbContext : DbContext
    {
        public TestDbContext()
        {
        }

        public TestDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Lock> Locks { get; set; }
    }
}
