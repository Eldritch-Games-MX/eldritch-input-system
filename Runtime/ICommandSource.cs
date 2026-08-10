using System.Collections.Generic;

namespace EldritchGames.InputSystem
{
    /// <summary>
    /// Produces <see cref="ICommand"/> objects into a caller-owned buffer.
    /// The buffer pattern avoids a per-frame allocation compared to returning a new list.
    /// </summary>
    /// <remarks>
    /// Implementations can represent human input, AI decision-making, replays, or any
    /// other command source — the consuming controller does not distinguish between them.
    /// </remarks>
    public interface ICommandSource
    {
        /// <summary>
        /// Appends commands generated this frame into <paramref name="buffer"/>.
        /// Does not clear the buffer before writing; callers are responsible for clearing.
        /// </summary>
        void CollectCommands(IList<ICommand> buffer);
    }
}
