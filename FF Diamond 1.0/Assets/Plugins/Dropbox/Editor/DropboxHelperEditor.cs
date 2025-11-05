using UnityEditor;

namespace Plugins.Dropbox.Editor
{
    public static class DropboxHelperEditor
    {
        [MenuItem("Dropbox/GetAuthCode")]
        public static void GetAuthCode()
        {
            DropboxHelper.GetAuthCode();
        }

        [MenuItem("Dropbox/GetRefreshToken")]
        public static void GetRefreshToken()
        {
            DropboxHelper.GetRefreshToken();
        }
    }
}