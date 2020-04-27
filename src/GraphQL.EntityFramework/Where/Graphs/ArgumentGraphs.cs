﻿using System;
using System.Collections.Generic;
using GraphQL.EntityFramework;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;

static class ArgumentGraphs
{
    static Dictionary<Type, GraphType> entries = new Dictionary<Type, GraphType>();

    static ArgumentGraphs()
    {
        GraphTypeTypeRegistry.Register(typeof(Comparison), typeof(ComparisonGraph));
        GraphTypeTypeRegistry.Register(typeof(StringComparison), typeof(StringComparisonGraph));
        GraphTypeTypeRegistry.Register(typeof(Connector), typeof(ConnectorGraph));
        Add<StringComparisonGraph>();
        Add<WhereExpressionGraph>();
        Add<OrderByGraph>();
        Add<ComparisonGraph>();
        Add<ConnectorGraph>();
    }

    public static void RegisterInContainer(IServiceCollection services)
    {
        foreach (var entry in entries)
        {
            services.AddSingleton(entry.Key, entry.Value);
        }
    }
    
    public static void RegisterInContainer(Action<Type, GraphType> registerDelegate)
    {
        foreach (var entry in entries)
        {
            registerDelegate(entry.Key, entry.Value);
        }
    }

    static void Add<T>()
        where T : GraphType, new()
    {
        var value = new T();
        entries.Add(typeof(T), value);
    }
}