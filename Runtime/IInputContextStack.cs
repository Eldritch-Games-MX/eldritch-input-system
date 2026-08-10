using System.Collections.Generic;

namespace EldritchGames.InputSystem
{
    /// <summary>
    /// Manages a stack of <see cref="IInputContext"/> objects and drives their lifecycle.
    /// Only the top context generates commands each frame.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Push"/> / <see cref="Pop"/> for overlay patterns (menu over gameplay).
    /// Use <see cref="Swap"/> for mode transitions (exploration to combat) where stack depth must stay constant.
    /// </remarks>
    public interface IInputContextStack
    {
        /// <summary>
        /// Pauses the current top context and pushes <paramref name="context"/> as the new active context.
        /// </summary>
        void Push(IInputContext context);

        /// <summary>
        /// Removes and returns the top context, then resumes the context beneath it.
        /// </summary>
        IInputContext Pop();

        /// <summary>
        /// Replaces the top context without changing stack depth.
        /// Prefer over Push+Pop for permanent mode changes (no context to resume beneath).
        /// </summary>
        void Swap(IInputContext context);

        /// <summary>Returns the active top context, or <c>null</c> if the stack is empty.</summary>
        IInputContext Peek();

        /// <summary>Returns <c>true</c> when no contexts are on the stack.</summary>
        bool IsEmpty { get; }

        /// <summary>Read-only view of all contexts from top (active) to bottom.</summary>
        IReadOnlyCollection<IInputContext> Contexts { get; }
    }
}
