namespace EldritchGames.InputSystem
{
    /// <summary>
    /// An input context that requires per-frame time advancement.
    /// </summary>
    /// <remarks>
    /// <see cref="IController"/> implementations should call <see cref="Tick"/> on the active
    /// context each frame before <see cref="ICommandSource.CollectCommands"/>, when the context
    /// implements this interface. This ensures the context's internal timeline is current before
    /// input is evaluated in the same frame.
    /// </remarks>
    public interface ITickableContext : IInputContext
    {
        /// <summary>Advances internal time state by <paramref name="deltaTime"/> seconds.</summary>
        void Tick(float deltaTime);
    }
}
