namespace UI.CustomScrollRect
{
    /// <summary>
    /// Optional interface for pooled scroll items that want to know their absolute data index.
    /// </summary>
    public interface IIndexedScrollItem
    {
        void SetDataIndex(int index);
    }
}
