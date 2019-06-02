# Upgrade Guide 5 to 6


## Changes to RegisterInContainer

`EfGraphQLConventions.RegisterInContainer` had been changed from: 
```cs
public static void RegisterInContainer(IServiceCollection services, IModel model, GlobalFilters filters = null)
```

To:

```cs
public static void RegisterInContainer<TDbContext>(
    IServiceCollection services,
    TDbContext dbContext,
    DbContextFromUserContext<TDbContext> dbContextFromUserContext,
    GlobalFilters filters = null)
    where TDbContext : DbContext
    ```

 * It now accepts a `DbContext` instead of an `IModel`.
 * A delegate `DbContextFromUserContext<DbContext>` has been added. This is used when resolving field context. This means the current `DbContext` no longer needs to be extracted from the GraphQl UserContext, and instead can be accessed directly using the `DbContext` property.


## Changes to QueryGraphType

A generic has `TDbContext` been added.

```cs
public class Query : QueryGraphType
```

Is changed to:

```cs
public class Query : QueryGraphType<MyDbContext>
```


## Changes to EfObjectGraphType

A generic has `TDbContext` been added.

```cs
public class CompanyGraph : EfObjectGraphType<Company>
```

Is changed to:

```cs
public class CompanyGraph : EfObjectGraphType<MyDbContext,Company>
```


## Changes to IEfGraphQLService

A generic has `TDbContext` been added.

So constructors that take an instance:

```cs
public MyClass(IEfGraphQLService graphQlService)
```

Are changed to:

```cs
public MyClass(IEfGraphQLService<MyDbContext> graphQlService)
```


## Changes to AddFields

The resolve context in AddField methods have been changed to now accept a `ResolveEfFieldContext` instead of a `ResolveFieldContext`. `ResolveEfFieldContext` exposes an extra `DbContext` property. This means the current `DbContext` no longer needs to be extracted from the GraphQl UserContext, and instead can be accessed directly using the `DbContext` property.

For example:

```cs
AddQueryConnectionField(
    name: "companies",
    resolve: context =>
    {
        var dbContext = (MyDbContext) context.UserContext;
        return dbContext.Companies;
    });
```

Is changed to:

```cs
AddQueryConnectionField(
    name: "companies",
    resolve: context => context.DbContext.Companies);
```