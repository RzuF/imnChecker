using System.Threading.Tasks;

namespace imnChecker
{
    public interface INotifier
    {
        string ApiKey { get; set; }
        Task<bool> Authenteticate(string additionalJsonArgs = "");
        Task<string> SendNotification(string message, string additionalJsonArgs = "");
    }

    public enum System
    {
        Android
    }
}
