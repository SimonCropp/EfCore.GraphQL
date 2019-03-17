﻿using GraphQL.EntityFramework;

public class SkipLevelGraph :
    EfObjectGraphType<Level1Entity>
{
    public SkipLevelGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddNavigationField<Level3Graph, Level3Entity>(
            name: "level3Entity",
            resolve: context => context.Source.Level2Entity.Level3Entity,
            includeNames: new[] { "Level2Entity.Level3Entity"});
    }
}