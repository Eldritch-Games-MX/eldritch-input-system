namespace EldritchGames.InputSystem
{
    /// <summary>
    /// A controllable entity that receives and executes <see cref="ICommand"/> objects.
    /// Extends <see cref="ICapabilityProvider"/> so commands can query which capabilities
    /// the pawn supports without depending on its concrete type.
    /// </summary>
    /// <remarks>
    /// Implement this interface (or extend <see cref="PawnController"/>) once per
    /// gameplay mode. Declare only the game-defined capability interfaces your pawn supports:
    /// <code>
    /// public class FPSPawn : PawnController, IMovable, ILookable, IJumpable { ... }
    /// public class DrivingPawn : PawnController, IDriveable { ... }
    /// </code>
    /// Capability interfaces (IMovable, etc.) are defined by the consuming game, not this package.
    /// </remarks>
    public interface IPawnController : ICapabilityProvider
    {
        /// <summary>Dispatches a command to this pawn for execution.</summary>
        void ExecuteCommand(ICommand command);
    }
}
