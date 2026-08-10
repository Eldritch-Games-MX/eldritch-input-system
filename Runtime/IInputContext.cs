namespace EldritchGames.InputSystem
{
    /// <summary>
    /// An input mode that generates commands and responds to lifecycle events from the
    /// <see cref="IInputContextStack"/>. A context can represent a control scheme
    /// (exploration, combat, driving, UI) or an input source (player, AI, replay).
    /// </summary>
    /// <remarks>
    /// Lifecycle sequence on the stack:
    /// <list type="bullet">
    ///   <item><see cref="OnPush"/> — context becomes active for the first time (enable input).</item>
    ///   <item><see cref="OnPause"/> — a new context was pushed on top (disable input but retain state).</item>
    ///   <item><see cref="OnResume"/> — the context on top was popped; this context is active again (re-enable input).</item>
    ///   <item><see cref="OnPop"/> — context removed from stack permanently (disable and clean up).</item>
    /// </list>
    /// Only the top-of-stack context has <see cref="ICommandSource.CollectCommands"/> called each frame.
    /// </remarks>
    public interface IInputContext : ICommandSource
    {
        /// <summary>Called when this context is first pushed onto the stack.</summary>
        void OnPush();

        /// <summary>Called when this context is removed from the stack.</summary>
        void OnPop();

        /// <summary>Called when a context above this one is popped and this becomes active again.</summary>
        void OnResume();

        /// <summary>Called when a new context is pushed on top of this one.</summary>
        void OnPause();
    }
}
