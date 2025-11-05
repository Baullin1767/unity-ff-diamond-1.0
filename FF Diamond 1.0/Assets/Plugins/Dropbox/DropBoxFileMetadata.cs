using System;

namespace Plugins.Dropbox
{
    [Serializable]
    public struct DropBoxFileMetadata
    {
        public string tag;
        public string name;
        public string path_lower;
        public string path_display;
        public string id;
        public string client_modified;
        public string server_modified;
        public string rev;
        public int size;
        public bool is_downloadable;
        public string content_hash;
    }
}