using System.Runtime.InteropServices;

namespace SagaSeminar.Shared.Utils
{
    public static class RuntimeHelper
    {
        public static bool IsWebAssembly()
        {
            return RuntimeInformation.OSDescription.Equals("browser", StringComparison.OrdinalIgnoreCase);
        }
    }
}
