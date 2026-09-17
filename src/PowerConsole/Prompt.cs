using System;

namespace PowerConsole
{
    /// <summary>
    /// Represents an object that stores a prompt message and the corresponding
    /// response.
    /// </summary>
    public class Prompt
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Prompt" /> class using
        /// the especified parameter.
        /// </summary>
        /// <param name="message">The prompt message.</param>
        public Prompt(string message)
        {
            Id = message;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Prompt" /> class using
        /// the especified parameter.
        /// </summary>
        /// <param name="message">The prompt message.</param>
        /// <param name="response">The response to the current prompt.</param>
        /// <param name="id">
        /// The identifier of the current prompt. If null will be identical to
        /// <paramref name="message"/>.
        /// </param>
        public Prompt(string message, object response, string id = null)
        {
            Id = id ?? message;
            Message = message;
            Response = response;
        }

        /// <summary>
        /// Gets or sets the identifier of the prompt.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets the prompt message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets or sets the response of the prompt.
        /// </summary>
        public object Response { get; set; }

        /// <summary>
        /// Casts and returns the response as a <see cref="byte"/>.
        /// </summary>
        /// <returns>The <see cref="byte"/> value represented by <see cref="Response"/>.</returns>
        public byte ToByte()
        {
            if (Response == null)
                return 0;
            return (byte)Response;
        }

        /// <summary>
        /// Casts and returns the response as a <see cref="short"/>.
        /// </summary>
        /// <returns>The <see cref="short"/> value represented by <see cref="Response"/>.</returns>
        public short ToShort()
        {
            if (Response == null)
                return 0;
            return (short)Response;
        }

        /// <summary>
        /// Casts and returns the response as an <see cref="int"/>.
        /// </summary>
        /// <returns>The <see cref="int"/> value represented by <see cref="Response"/>.</returns>
        public int ToInt()
        {
            if (Response == null)
                return 0;
            return (int)Response;
        }

        /// <summary>
        /// Casts and returns the response as a <see cref="long"/>.
        /// </summary>
        /// <returns>The <see cref="long"/> value represented by <see cref="Response"/>.</returns>
        public long ToLong()
        {
            if (Response == null)
                return 0L;
            return (long)Response;
        }

        /// <summary>
        /// Casts and returns the response as a <see cref="float"/>.
        /// </summary>
        /// <returns>The <see cref="float"/> value represented by <see cref="Response"/>.</returns>
        public float ToFloat()
        {
            if (Response == null)
                return 0F;
            return (float)Response;
        }

        /// <summary>
        /// Casts and returns the response as a <see cref="double"/>.
        /// </summary>
        /// <returns>The <see cref="double"/> value represented by <see cref="Response"/>.</returns>
        public double ToDouble()
        {
            if (Response == null)
                return 0d;
            return (double)Response;
        }

        /// <summary>
        /// Casts and returns the response as a <see cref="decimal"/>.
        /// </summary>
        /// <returns>The <see cref="decimal"/> value represented by <see cref="Response"/>.</returns>
        public decimal ToDecimal()
        {
            if (Response == null)
                return 0M;
            return (decimal)Response;
        }

        /// <summary>
        /// Attempts to convert <see cref="Response"/> to an instance of
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <param name="provider">
        /// An object that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// An instance of <typeparamref name="T"/> or its default value.
        /// </returns>
        public T As<T>(IFormatProvider provider = null)
        {
            if (Response == null)
            {
                return default;
            }
            
            if (Response.GetType() == typeof(T))
            {
                return (T)Response;
            }

            return Response.ToString().TryConvert<T>(provider, out var result) 
                ? result 
                : default;
        }

        /// <summary>
        /// Determines whether the specified <paramref name="value"/> is equal
        /// to the string representation of <see cref="Response"/> using 
        /// case-insensitive comparison.
        /// </summary>
        /// <param name="value">
        /// The value to compare <see cref="Response"/> against.
        /// </param>
        /// <returns></returns>
        public bool EqualsIgnoreCase(string value)
        {
            return string.Equals($"{Response}", value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the string representation of the current Prompt instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Message} {Response}";
        }
    }
}
