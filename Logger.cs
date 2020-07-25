using System;

namespace JapanSaber
{
    internal static class Logger
    {
        internal static IPA.Logging.Logger IPALogger { get; set; }

        internal static void Debug(string message)
        {
#if DEBUG
            IPALogger?.Debug(message);
#endif
        }
        internal static void Debug(Exception ex)
        {
#if DEBUG
            IPALogger?.Debug(ex);
#endif
        }
    }
}
