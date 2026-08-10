using UnityEngine;

namespace EldritchGames.InputSystem
{
    /// <summary>
    /// Convenience base class for scene-placed pawns. Implements the standard
    /// <see cref="ICapabilityProvider.TryGet{T}"/> pattern (<c>this as T</c>) so
    /// subclasses only need to declare capability interfaces without extra boilerplate.
    /// </summary>
    /// <remarks>
    /// Extend this class and declare the game-defined capabilities your pawn supports:
    /// <code>
    /// public class ExplorationPawn : PawnController, IMovable, IInteractor { ... }
    /// </code>
    /// Capability interfaces are defined by the consuming game, not this package.
    /// Pure C# pawns (tests, AI dummies) should implement <see cref="IPawnController"/> directly
    /// without inheriting from <c>MonoBehaviour</c>.
    /// </remarks>
    public abstract class PawnController : MonoBehaviour, IPawnController
    {
        /// <inheritdoc/>
        public abstract void ExecuteCommand(ICommand command);

        /// <inheritdoc/>
        public bool TryGet<T>(out T capability) where T : class
        {
            capability = this as T;
            return capability != null;
        }
    }
}
