namespace UI.CustomScrollRect
{
    /// <summary>
    /// Receives height/state notifications from variable-height scroll items.
    /// </summary>
    public interface IVariableScrollOwner
    {
        void NotifyItemHeightChanged(BaseItemView view, float newHeight);
        void NotifyItemStateChanged(BaseItemView view, bool expanded);
    }
}
