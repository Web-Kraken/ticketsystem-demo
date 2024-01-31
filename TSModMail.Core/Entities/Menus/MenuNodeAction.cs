namespace TSModMail.Core.Entities.Menus;

/// <summary>
/// The action to undertake when a <see cref="MenuNode"/> is clicked.
/// </summary>
public class MenuNodeAction
{
    /// <summary>
    /// Continue and print the next children.
    /// </summary>
    public static readonly MenuNodeAction Continue = new MenuNodeAction(MenuNodeMode.Continue);

    /// <summary>
    /// Open the ticket proper.
    /// </summary>
    public static readonly MenuNodeAction OpenTicket = new MenuNodeAction(MenuNodeMode.OpenTicket);

    /// <summary>
    /// Open a ticket properly, with a message.
    /// </summary>
    public static MenuNodeAction FromOpen(string? v = null) => new MenuNodeAction(MenuNodeMode.OpenTicket, v);

    /// <summary>
    /// Displays a modal and then opens the ticket with the information.
    /// </summary>
    public static MenuNodeAction FromModal(string name) => new MenuNodeAction(MenuNodeMode.OpenTicketModal, modalName: name);

    /// <summary>
    /// Abort a questionnaire with a given reason.
    /// </summary>
    public static MenuNodeAction FromCancel(string v) => new MenuNodeAction(MenuNodeMode.Abort, v);

    /// <summary>
    /// The outcome of clicking the <see cref="MenuNode"/>.
    /// </summary>
    public MenuNodeMode Mode { get; private set; }

    private MenuNodeAction(MenuNodeMode mode, string? message = null, string? modalName = null)
    {
        Mode = mode;
        Message = message;
        ModalName = modalName;
    }

    /// <summary>
    /// A message to include with opening or aborting a ticket.
    /// </summary>
    public string? Message { get; private set; }

    /// <summary>
    /// The id of the modal this action opens.
    /// </summary>
    public string? ModalName { get; private set; }
}

/// <summary>
/// The outcome of a <see cref="MenuNodeAction"/>.
/// </summary>
public enum MenuNodeMode
{
    /// <summary>
    /// Continue with the menu.
    /// </summary>
    Continue = 0,
    /// <summary>
    /// Abort the menu and direct the user elsewhere.
    /// </summary>
    Abort,
    /// <summary>
    /// Open the ticket and add TS.
    /// </summary>
    OpenTicket,
    /// <summary>
    /// Open a modal and post the result into the Ticket.
    /// </summary>
    OpenTicketModal
}

