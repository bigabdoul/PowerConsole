using System;
using System.Runtime.Serialization;

namespace PowerConsole
{
    /// <summary>
    /// Represents an exception that is thrown when the condition required for
    /// continuing code execution has not been met.
    /// </summary>
    [Serializable]
    public sealed class ContinuationException : Exception
    {
        /// <summary>
        /// Intializes a new instance of the <see cref="ContinuationException"/> class.
        /// </summary>
        public ContinuationException()
        {
        }

        /// <inheritdoc/>
        public ContinuationException(string message) : base(message)
        {
        }

        /// <inheritdoc/>
        public ContinuationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}