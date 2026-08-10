namespace EldritchGames.InputSystem
{
    /// <summary>
    /// Exposes a pawn's supported capabilities without revealing its concrete type.
    /// Commands call <see cref="TryGet{T}"/> to request a capability; the pawn returns
    /// itself cast to that interface, or null if unsupported.
    /// </summary>
    /// <remarks>
    /// The standard implementation is a single line:
    /// <code>
    /// public bool TryGet&lt;T&gt;(out T capability) where T : class
    /// {
    ///     capability = this as T;
    ///     return capability != null;
    /// }
    /// </code>
    /// <see cref="PawnController"/> provides this implementation by default.
    /// </remarks>
    public interface ICapabilityProvider
    {
        /// <summary>
        /// Attempts to retrieve the specified capability from this provider.
        /// </summary>
        /// <typeparam name="T">The capability interface to query (e.g. a game-defined <c>IMovable</c>).</typeparam>
        /// <param name="capability">The resolved capability, or <c>null</c> if unsupported.</param>
        /// <returns><c>true</c> if the capability is supported; otherwise <c>false</c>.</returns>
        bool TryGet<T>(out T capability) where T : class;
    }
}
