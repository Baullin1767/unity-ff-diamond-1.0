using System;

namespace Plugins.Dropbox.Exceptions
{
    internal class DownloadCancelledException : Exception
    {
        public override string Message => "Download was cancelled";
    }
}