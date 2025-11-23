namespace UI.CustomScrollRect
{
    /// <summary>
    /// Implement on items that should react to entering or leaving the viewport.
    /// </summary>
    public interface IScrollVisibilityHandler
    {
        void OnVisibilityChanged(bool isVisible);
    }
}
