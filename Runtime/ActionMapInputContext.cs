using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace EldritchGames.InputSystem
{
    /// <summary>
    /// Base class for input contexts driven by a Unity <see cref="InputActionMap"/>.
    /// Handles Enable/Disable lifecycle automatically; subclasses only implement
    /// <see cref="CollectCommands"/>.
    /// </summary>
    /// <remarks>
    /// Pass the specific action map (not the entire asset) to avoid enabling unrelated maps:
    /// <code>
    /// public class PlayerInputContext : ActionMapInputContext
    /// {
    ///     public PlayerInputContext(PlayerInput input)
    ///         : base(input.actions.FindActionMap("Player", throwIfNotFound: true)) { }
    ///
    ///     public override void CollectCommands(IList&lt;ICommand&gt; buffer) { ... }
    /// }
    /// </code>
    /// </remarks>
    public abstract class ActionMapInputContext : IInputContext
    {
        protected readonly InputActionMap ActionMap;

        protected ActionMapInputContext(InputActionMap actionMap) => ActionMap = actionMap;

        public void OnPush()   => ActionMap.Enable();
        public void OnPop()    => ActionMap.Disable();
        public void OnResume() => ActionMap.Enable();
        public void OnPause()  => ActionMap.Disable();

        public abstract void CollectCommands(IList<ICommand> buffer);
    }
}
