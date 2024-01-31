using DSharpPlus.Entities;

namespace TSModMail.Core.Entities.Tickets;

public struct TicketCreateRequest
{
    public readonly DiscordMember Member;

    public readonly TicketRegion Region;

    public readonly string MenuName;

    public TicketCreateRequest(DiscordMember member, TicketRegion region, string menuName)
    {
        Member = member;
        Region = region;
        MenuName = menuName;
    }
}
