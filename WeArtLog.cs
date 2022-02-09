/**
*	WEART - Log utility 
*	https://www.weart.it/
*/

using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace WeArt.Utils
{
    /// <summary>
    /// Utility class used to log events and messages in the <see cref="WeArt"/> framework
    /// </summary>
    public static class WeArtLog
    {
        /// <summary>Logs a message in the debug console</summary>
        /// <param name="message">The string message</param>
        /// <param name="callerPath">The path of the caller (optional)</param>
        public static void Log(object message)
        {
            Debug.WriteLine(message);
        }
    }
}