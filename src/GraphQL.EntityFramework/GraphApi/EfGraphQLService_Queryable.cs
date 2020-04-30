﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        public FieldType AddQueryField<TReturn>(
            IObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            return AddQueryField(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments, description);
        }

        public FieldType AddQueryField<TReturn>(
            IObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryField(graphType, name, resolve, arguments, description);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            return AddQueryField(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments, description);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryField(graphType, name, resolve, arguments, null);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            IObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            return AddQueryField<TSource, TReturn>(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            IObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryField(graphType, name, resolve, arguments, null);
            return graph.AddField(field);
        }

        FieldType BuildQueryField<TSource, TReturn>(
            Type? graphType,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            IEnumerable<QueryArgument>? arguments,
            string? description)
            where TReturn : class
        {
            return BuildQueryField(name, resolve, arguments, graphType, description);
        }

        FieldType BuildQueryField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            IEnumerable<QueryArgument>? arguments,
            Type? graphType,
            string? description)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);
            //lookup the graph type if not explicitly specified
            graphType ??= GraphTypeFinder.FindGraphType<TReturn>();
            //create a list graph type based on the base field graph type
            var listGraphType = MakeListGraphType(graphType);
            //build the field
            return new FieldType
            {
                Name = name,
                Type = listGraphType,
                Description = description,
                //append the default query arguments to the specified argument list
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
                //custom resolve function
                Resolver = new AsyncFieldResolver<TSource, IEnumerable<TReturn>>(
                    async context =>
                    {
                        var efFieldContext = BuildContext(context);
                        //get field names of the table's primary key(s)
                        var names = GetKeyNames<TReturn>();
                        //run the specified resolve function
                        var query = await resolve(efFieldContext);
                        //include sub tables in the query based on the metadata stored for the requested graph
                        query = includeAppender.AddIncludes(query, context);
                        //apply any query filters specified in the arguments
                        query = query.ApplyGraphQlArguments(context, names);
                        //query the database
                        var list = await query.ToListAsync(context.CancellationToken);
                        //apply the global filter on each individually enumerated item
                        return await efFieldContext.Filters.ApplyFilter(list, context.UserContext);
                    })
            };
        }

        static List<string> emptyList = new List<string>();

        List<string> GetKeyNames<TSource>()
        {
            if (keyNames.TryGetValue(typeof(TSource), out var names))
            {
                return names;
            }

            return emptyList;
        }
    }
}