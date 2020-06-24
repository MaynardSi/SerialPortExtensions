using System.Threading.Tasks;

namespace ODValueHelperProject
{
    public interface ISensorHelper
    {
        void OpenSerialPort();

        void CloseSerialPort();

        void CommandProcess(string command);

        Task CommandProcessAsync(string command);
    }
}