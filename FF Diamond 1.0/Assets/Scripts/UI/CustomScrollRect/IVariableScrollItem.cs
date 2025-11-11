namespace UI.CustomScrollRect
{
    /// <summary>
    /// Implement on item views that need to cooperate with <see cref="VariablePooledScroll"/>.
    /// </summary>
    public interface IVariableScrollItem
    {
        void SetScrollOwner(VariablePooledScroll scroll);
        void ApplyHeight(float height);
        void ApplyState(bool expanded);
    }
}
