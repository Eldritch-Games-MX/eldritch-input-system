using System;

namespace EldritchGames.InputSystem
{
    /// <summary>
    /// Drives the input-to-pawn pipeline for a single player slot.
    /// Each frame it drains the active <see cref="IInputContext"/> into a command buffer
    /// and dispatches every command to the current <see cref="IPawnController"/>.
    /// </summary>
    /// <remarks>
    /// One <c>IController</c> instance per player. For local multiplayer, instantiate
    /// N controllers each initialized with their own <see cref="IInputContextStack"/>
    /// and <see cref="IPawnController"/>.
    /// </remarks>
    public interface IController
    {
        /// <summary>Zero-based player slot index. Set once at <see cref="Initialize"/>.</summary>
        int PlayerIndex { get; }

        /// <summary>
        /// Binds the controller to a player slot, pawn, and context stack.
        /// Must be called before <see cref="Tick"/> or any context/pawn mutations.
        /// </summary>
        void Initialize(int playerIndex, IPawnController pawn, IInputContextStack stack);

        /// <summary>Replaces the active pawn without affecting the context stack.</summary>
        void SetPawn(IPawnController pawn);

        /// <summary>
        /// Changes the active context. Uses <see cref="IInputContextStack.Swap"/> if the
        /// stack is non-empty, otherwise <see cref="IInputContextStack.Push"/>.
        /// </summary>
        void SwitchContext(IInputContext context);

        /// <summary>Pushes a context on top of the current one (e.g. opening a menu).</summary>
        void PushContext(IInputContext context);

        /// <summary>Pops the top context, resuming the one beneath it (e.g. closing a menu).</summary>
        void PopContext();

        /// <summary>Processes one frame: collects commands from the active context and executes them on the pawn.</summary>
        void Tick(float deltaTime);

        /// <summary>Returns the currently active context, or <c>null</c> if the stack is empty.</summary>
        IInputContext CurrentContext { get; }

        /// <summary>
        /// Pushes a completable context and automatically pops it when <see cref="ICompletableInputContext.OnCompleted"/> fires.
        /// The optional callback receives the completion result after the pop.
        /// Prefer this over manual Push + subscribe + PopContext to avoid lifecycle bugs.
        /// </summary>
        void PushCompletable(ICompletableInputContext context, Action<bool> onCompleted = null);
    }
}
