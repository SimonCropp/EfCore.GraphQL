﻿using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

class Configuration
{
    void RegisterInContainerServiceCollectionUsage(IServiceCollection serviceCollection)
    {
        #region RegisterInContainerServiceCollectionUsage

        var builder = new DbContextOptionsBuilder();
        builder.UseSqlServer("fake");
        using (var context = new MyDbContext(builder.Options))
        {
            EfGraphQLConventions.RegisterInContainer(serviceCollection, context,x=>(MyDbContext)x);
        }

        #endregion
    }

    public class MyDbContext :
        DbContext
    {
        public MyDbContext(DbContextOptions options):
            base(options)
        {
        }
    }
}