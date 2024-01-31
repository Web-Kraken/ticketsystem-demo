using System.Threading.Tasks;

namespace TSModMail.Core.Services;

/// <summary>
/// The command handler service, handles application commands.
/// </summary>
public interface ICommandService
{
    /// <summary>
    /// Must be called before ConnectAsync.
    /// </summary>
    Task RegisterCommands();
}
