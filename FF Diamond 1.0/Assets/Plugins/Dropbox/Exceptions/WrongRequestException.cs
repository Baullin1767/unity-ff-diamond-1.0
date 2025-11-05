using System.Threading.Tasks;

namespace Plugins.Dropbox.Exceptions
{
    internal class WrongRequestException : TaskCanceledException
    {
        private readonly string _request;

        public WrongRequestException(string request) => _request = request;

        public override string Message => $"Wrong request: {_request}";
    }
}