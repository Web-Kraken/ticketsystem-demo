using DSharpPlus;
using System;
using System.Collections.Generic;

namespace TSModMail.Core.Entities.Menus;

/// <summary>
/// A Menu Node.
/// </summary>
public class MenuNode : Entity<string>
{
    public MenuNode(
        string id,
        string content,
        MenuNodeAction action,
        ButtonStyle style = ButtonStyle.Primary
    ) : base(id)
    {
        if (id.Contains('.') || id.Contains(":"))
        {
            throw new ArgumentException($"Forbidden name {id} for MenuNode.");
        }

        if (id.Length >= 100)
        {
            throw new ArgumentException("Id is too long.");
        }

        if (content.Length >= 80)
        {
            throw new ArgumentException($"Content \"{content}\" too long.");
        }

        Content = content;
        Action = action;
        Style = style;

        Children = new List<MenuNode>();
    }

    /// <summary>
    /// Children nodes of this menu.
    /// </summary>
    public List<MenuNode> Children { get; private set; }

    /// <summary>
    /// What should the button read.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// The action to be undertaken when the button is pressed.
    /// </summary>
    public MenuNodeAction Action { get; set; }

    /// <summary>
    /// The button style, determines the colour of the button.
    /// </summary>
    public ButtonStyle Style { get; set; }

    /// <summary>
    /// Fluently add content to a menu.
    /// </summary>
    public MenuNode WithContent(string content)
    {
        Content = content;
        return this;
    }

    /// <summary>
    /// Fluently add a child option.
    /// </summary>
    public MenuNode WithChild(MenuNode child)
    {
        Children.Add(child);
        return this;
    }
}
