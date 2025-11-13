namespace UI.ViewSystem
{
    public interface IUIViewController
    {
        void ShowScreen(UIScreenId id);
        void ShowPopup(UIPopupId id, int reward, string result = "");
        void HidePopup(UIPopupId id);
        void HideAllPopups();
    }
}
