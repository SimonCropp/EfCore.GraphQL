﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Common.Request;
using GraphQL.EntityFramework.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

#region GraphQlControllerTests

public class GraphQlControllerTests :
    XunitLoggingBase
{
    static HttpClient client;
    static WebSocketClient websocketClient;
    static Task startTask;

    static GraphQlControllerTests()
    {
        startTask = Start();
    }

    static async Task Start()
    {
        await DbContextBuilder.Start();
        var server = GetTestServer();
        client = server.CreateClient();
        websocketClient = server.CreateWebSocketClient();
        websocketClient.ConfigureRequest =
            request => { request.Headers["Sec-WebSocket-Protocol"] = "graphql-ws"; };
    }

    [Fact]
    public async Task Get()
    {
        await startTask;
        var query = @"
{
  companies
  {
    id
  }
}";
        using (var response = await ClientQueryExecutor.ExecuteGet(client, query))
        {
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains(
                "{\"companies\":[{\"id\":1},{\"id\":4},{\"id\":6},{\"id\":7}]}",
                result);
        }
    }

    [Fact]
    public async Task Get_single()
    {
        await startTask;
        var query = @"
query ($id: ID!)
{
  company(id:$id)
  {
    id
  }
}";
        var variables = new
        {
            id = "1"
        };

        using (var response = await ClientQueryExecutor.ExecuteGet(client, query, variables))
        {
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains(@"{""data"":{""company"":{""id"":1}}}", result);
        }
    }

    [Fact]
    public async Task Get_single_not_found()
    {
        await startTask;
        var query = @"
query ($id: ID!)
{
  company(id:$id)
  {
    id
  }
}";
        var variables = new
        {
            id = "99"
        };

        using (var response = await ClientQueryExecutor.ExecuteGet(client, query, variables))
        {
            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains("Not found", result);
        }
    }

    [Fact]
    public async Task Get_variable()
    {
        await startTask;
        var query = @"
query ($id: ID!)
{
  companies(ids:[$id])
  {
    id
  }
}";
        var variables = new
        {
            id = "1"
        };

        using (var response = await ClientQueryExecutor.ExecuteGet(client, query, variables))
        {
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains("{\"companies\":[{\"id\":1}]}", result);
        }
    }

    [Fact]
    public async Task Get_companies_paging()
    {
        await startTask;
        var after = 1;
        var query = @"
query {
  companiesConnection(first:2, after:""" + after + @""") {
    edges {
      cursor
      node {
        id
      }
    }
    pageInfo {
      endCursor
      hasNextPage
    }
  }
}";
        using (var response = await ClientQueryExecutor.ExecuteGet(client, query))
        {
            response.EnsureSuccessStatusCode();
            var result = JObject.Parse(await response.Content.ReadAsStringAsync());
            var page = result.SelectToken("..data..companiesConnection..edges[0].cursor")
                .Value<string>();
            Assert.NotEqual(after.ToString(), page);
        }
    }

    [Fact]
    public async Task Get_employee_summary()
    {
        await startTask;
        var query = @"
query {
  employeeSummary {
    companyId
    averageAge
  }
}";
        using (var response = await ClientQueryExecutor.ExecuteGet(client, query))
        {
            response.EnsureSuccessStatusCode();
            var result = JObject.Parse(await response.Content.ReadAsStringAsync());
            var expected = JObject.FromObject(new
            {
                data = new
                {
                    employeeSummary = new[]
                    {
                        new {companyId = 1, averageAge = 28.0},
                        new {companyId = 4, averageAge = 34.0}
                    }
                }
            });
            Assert.Equal(expected.ToString(), result.ToString());
        }
    }

    [Fact]
    public async Task Post()
    {
        await startTask;
        var query = @"
{
  companies
  {
    id
  }
}";
        using (var response = await ClientQueryExecutor.ExecutePost(client, query))
        {
            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains(
                "{\"companies\":[{\"id\":1},{\"id\":4},{\"id\":6},{\"id\":7}]}",
                result);
            response.EnsureSuccessStatusCode();
        }
    }

    [Fact]
    public async Task Post_variable()
    {
        await startTask;
        var query = @"
query ($id: ID!)
{
  companies(ids:[$id])
  {
    id
  }
}";
        var variables = new
        {
            id = "1"
        };
        using (var response = await ClientQueryExecutor.ExecutePost(client, query, variables))
        {
            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains("{\"companies\":[{\"id\":1}]}", result);
            response.EnsureSuccessStatusCode();
        }
    }

    [Fact]
    public async Task Should_subscribe_to_companies()
    {
        await startTask;
        var resetEvent = new AutoResetEvent(false);

        var result = new GraphQLHttpSubscriptionResult(
            new Uri("http://example.com/graphql"),
            new GraphQLRequest
            {
                Query = @"
subscription
{
  companyChanged
  {
    id
  }
}"
            },
            websocketClient);

        result.OnReceive +=
            res =>
            {
                if (res == null)
                {
                    return;
                }
                Assert.Null(res.Errors);

                if (res.Data != null)
                {
                    resetEvent.Set();
                }
            };

        var cancellationSource = new CancellationTokenSource();

        var task = result.StartAsync(cancellationSource.Token);

        Assert.True(resetEvent.WaitOne(TimeSpan.FromSeconds(10)));

        cancellationSource.Cancel();

        await task;
    }

    static TestServer GetTestServer()
    {
        var hostBuilder = new WebHostBuilder();
        hostBuilder.UseStartup<Startup>();
        return new TestServer(hostBuilder);
    }

    public GraphQlControllerTests(ITestOutputHelper output) :
        base(output)
    {
    }
}

#endregion