using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PSMSystem.Data;

public class PsmDbContextFactory : IDesignTimeDbContextFactory<PsmDbContext>
{
    public PsmDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PsmDbContext>();

        
        const string connectionString =
            "Server=.\\SQLEXPRESS;Database=PSMSystem;Trusted_Connection=True;TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connectionString);

        return new PsmDbContext(optionsBuilder.Options);
    }
}
