using UnityEngine;


namespace ABLC
{
    /// <summary>
    /// Debugging utility class.
    /// </summary>
    internal static class Debugging
    {
        /// <summary>
        /// Prints a single-line debugging message to the Unity output log.
        /// </summary>
        /// <param name="message">Message to log</param>
        internal static void Message(string message)
        {
            Debug.Log("Advanced Building Level Control: " + message + ".");
        }
    }
}
