using System;

namespace EldritchGames.InputSystem
{
    /// <summary>
    /// An <see cref="IInputContext"/> that self-terminates and reports a boolean result
    /// when finished. Use for bounded input sequences — QTE prompts, attack timing windows,
    /// minigame phases — where the context knows when it is done and whether the player succeeded.
    /// </summary>
    /// <remarks>
    /// The stack owner should subscribe to <see cref="OnCompleted"/> before pushing this context,
    /// then pop it (or let it self-pop) inside the handler. Prefer
    /// <see cref="IController.PushCompletable"/> over manual subscribe + pop:
    /// <code>
    /// controller.PushCompletable(qte, success => HandleQteResult(success));
    /// </code>
    /// Implementations must raise <see cref="OnCompleted"/> exactly once, then stop generating commands.
    /// </remarks>
    public interface ICompletableInputContext : IInputContext
    {
        /// <summary>
        /// Raised exactly once when the context finishes.
        /// <c>true</c> = player succeeded; <c>false</c> = failed or timed out.
        /// </summary>
        event Action<bool> OnCompleted;

        /// <summary>
        /// Forces the context to complete immediately with the given result.
        /// No-ops if already completed. Implementations must fire <see cref="OnCompleted"/> exactly once.
        /// </summary>
        void ForceComplete(bool success);
    }
}
