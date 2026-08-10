namespace EldritchGames.InputSystem
{
    /// <summary>
    /// An action that can be dispatched to any pawn that supports the required capability.
    /// Implementations query the provider for a specific capability interface rather than
    /// casting to a concrete pawn type, keeping commands decoupled from pawn implementations.
    /// </summary>
    /// <example>
    /// <code>
    /// public class JumpCommand : ICommand
    /// {
    ///     public void Execute(ICapabilityProvider provider)
    ///     {
    ///         if (provider.TryGet&lt;IJumpable&gt;(out var jumpable))
    ///             jumpable.Jump();
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface ICommand
    {
        /// <summary>
        /// Executes the command against the given capability provider.
        /// Silently no-ops if the provider does not support the required capability.
        /// </summary>
        void Execute(ICapabilityProvider provider);
    }
}
