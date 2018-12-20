﻿using System.Linq;
using GraphQL.EntityFramework;

public class Query : EfObjectGraphType
{
    public Query(IEfGraphQLService efGraphQlService) : base(efGraphQlService)
    {
        AddQueryField<CustomTypeGraph, CustomTypeEntity>(
            name: "customType",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.CustomTypeEntities;
            });

        AddQueryField<SkipLevelGraph, Level1Entity>(
            name: "skipLevel",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Level1Entities;
            });

        AddQueryField<WithManyChildrenGraph, WithManyChildrenEntity>(
            name: "manyChildren",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.WithManyChildrenEntities;
            });

        AddQueryField<Level1Graph, Level1Entity>(
            name: "level1Entities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Level1Entities;
            });

        efGraphQlService.AddQueryField<WithNullableGraph, WithNullableEntity>(this,
            name: "withNullableEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.WithNullableEntities;
            });

        efGraphQlService.AddQueryField<WithMisNamedQueryParentGraph, WithMisNamedQueryParentEntity>(this,
            name: "misNamed",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.WithMisNamedQueryParentEntities;
            });

        efGraphQlService.AddQueryField<ParentGraph, ParentEntity>(this,
            name: "parentEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            });

        efGraphQlService.AddQueryField<ChildGraph, ChildEntity>(this,
            name: "childEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ChildEntities;
            });

        efGraphQlService.AddQueryConnectionField<ParentGraph, ParentEntity>(this,
            name: "parentEntitiesConnection",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            });

        efGraphQlService.AddQueryConnectionField<ChildGraph, ChildEntity>(this,
            name: "childEntitiesConnection",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ChildEntities;
            });

        efGraphQlService.AddQueryField<FilterParentGraph, FilterParentEntity>(this,
            name: "parentEntitiesFiltered",
            resolve: context =>
            {
                var dataContext = (MyDataContext)context.UserContext;
                return dataContext.FilterParentEntities;
            });

        efGraphQlService.AddQueryConnectionField<FilterParentGraph, FilterParentEntity>(this,
            name: "parentEntitiesConnectionFiltered",
            resolve: context =>
            {
                var dataContext = (MyDataContext)context.UserContext;
                return dataContext.FilterParentEntities;
            });

        efGraphQlService.AddSingleField<ParentGraph, ParentEntity>(this,
            name: "parentEntity",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            }
        );
    }
}