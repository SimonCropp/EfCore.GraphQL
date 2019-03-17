﻿using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public class EfObjectGraphType :
        ObjectGraphType
    {
        IEfGraphQLService efGraphQlService;

        public EfObjectGraphType(IEfGraphQLService efGraphQlService)
        {
            Guard.AgainstNull(nameof(efGraphQlService), efGraphQlService);
            this.efGraphQlService = efGraphQlService;
        }

        protected ConnectionBuilder<TGraph, object> AddNavigationConnectionField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return efGraphQlService.AddNavigationConnectionField<TGraph, TReturn>(this, name, resolve, arguments, includeNames, pageSize);
        }

        protected FieldType AddNavigationField<TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class
        {
            return efGraphQlService.AddNavigationField(this, graphType, name, resolve, arguments, includeNames);
        }

        protected FieldType AddNavigationField<TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class
        {
            return efGraphQlService.AddNavigationField(this, graphType, name, resolve, arguments, includeNames);
        }

        protected ConnectionBuilder<TGraph, object> AddQueryConnectionField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return efGraphQlService.AddQueryConnectionField<TGraph, TReturn>(this, name, resolve, arguments, pageSize);
        }

        protected FieldType AddQueryField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return efGraphQlService.AddQueryField(this, name: name, resolve: resolve, graphType: typeof(TGraph), arguments: arguments);
        }

        protected FieldType AddQueryField<TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            return efGraphQlService.AddQueryField(this, graphType, name, resolve, arguments);
        }

        protected FieldType AddSingleField<TGraph, TReturn>(
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            string name = nameof(TReturn))
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return efGraphQlService.AddSingleField(this, name, resolve: resolve, graphType: typeof(TGraph));
        }

        protected FieldType AddSingleField<TReturn>(
            Type graphType,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            string name = nameof(TReturn))
            where TReturn : class
        {
            return efGraphQlService.AddSingleField(this, name, graphType, resolve);
        }
    }
}