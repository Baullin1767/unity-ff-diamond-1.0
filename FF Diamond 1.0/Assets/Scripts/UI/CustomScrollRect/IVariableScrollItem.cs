namespace UI.CustomScrollRect
{
    /// <summary>
    /// Implement on item views that need to cooperate with <see cref="IVariableScrollOwner"/>.
    /// </summary>
    public interface IVariableScrollItem
    {
        void SetScrollOwner(IVariableScrollOwner scroll);
        void ApplyHeight(float height);
        void ApplyState(bool expanded);
    }
}
