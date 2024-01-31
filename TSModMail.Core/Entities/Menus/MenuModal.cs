using DSharpPlus.Entities;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TSModMail.Core.Entities.Menus;

public class MenuModal : Entity<string>
{
    public MenuModal(string id, string title) : base(id) 
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException($"'{nameof(title)}' cannot be null or whitespace.", nameof(title));
        }

        Title = title;
    }

    public string Title { get; private set; }

    [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
    public Dictionary<int, TextInputComponent> TextComponents { get; set; } = new();

    [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
    public Dictionary<int, DiscordSelectComponent> SelectComponents { get; set; } = new();

    public DiscordInteractionResponseBuilder CreateInteraction()
    {
        Dictionary<int, DiscordComponent> components = new();
        TextComponents.ToList().ForEach(kvp => components[kvp.Key] = kvp.Value);
        SelectComponents.ToList().ForEach(kvp => components[kvp.Key] = kvp.Value);

        var builder = new DiscordInteractionResponseBuilder();

        builder
            .WithTitle(Title)
            .WithCustomId(Id);

        foreach (var kvp in components.OrderBy(x => x.Key))
        {
            builder.AddComponents(kvp.Value);
        }

        return builder;
    }
}
