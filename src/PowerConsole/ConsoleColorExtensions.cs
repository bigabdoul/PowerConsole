using System;

namespace PowerConsole
{
    /// <summary>
    /// Provides extension methods to values of the <see cref="ConsoleColor"/> enumeration.
    /// </summary>
    public static class ConsoleColorExtensions
    {
        /// <summary>
        /// Writes out a message in the default <see cref="SmartConsole"/>
        /// instance using the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="color">The used <see cref="ConsoleColor"/>.</param>
        /// <param name="message">The message to write.</param>
        /// <returns>A reference to the default <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole Write(this ConsoleColor color, string message)
            => SmartConsole.Default.Write(message, color);

        /// <summary>
        /// Writes out an object's string representation in the default
        /// <see cref="SmartConsole"/> instance using the specified 
        /// <paramref name="color"/>.
        /// </summary>
        /// <param name="color">The used <see cref="ConsoleColor"/>.</param>
        /// <param name="obj">The object to write.</param>
        /// <returns>A reference to the default <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole Write(this ConsoleColor color, object obj)
            => SmartConsole.Default.Write(obj, color);

        /// <summary>
        /// Writes out a formatted, colored string using the same 
        /// semantics as <see cref="string.Format(string, object[])"/>.
        /// </summary>
        /// <param name="color">The used <see cref="ConsoleColor"/>.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format</param>
        /// <returns>A reference to the default <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole Write(this ConsoleColor color, string format, params object[] args)
            => SmartConsole.Default.Write(string.Format(format, args), color);

        /// <summary>
        /// Writes out a message in the default <see cref="SmartConsole"/>
        /// instance using the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="color">The used <see cref="ConsoleColor"/>.</param>
        /// <param name="message">The message to write.</param>
        /// <returns>A reference to the default <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole WriteLine(this ConsoleColor color, string message)
            => SmartConsole.Default.WriteLine(message, color);

        /// <summary>
        /// Writes out an object's string representation in the default
        /// <see cref="SmartConsole"/> instance using the specified 
        /// <paramref name="color"/>.
        /// </summary>
        /// <param name="color">The used <see cref="ConsoleColor"/>.</param>
        /// <param name="obj">The object to write.</param>
        /// <returns>A reference to the default <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole WriteLine(this ConsoleColor color, object obj)
            => SmartConsole.Default.WriteLine(obj, color);

        /// <summary>
        /// Writes out a formatted, colored string using the same 
        /// semantics as <see cref="string.Format(string, object[])"/>.
        /// </summary>
        /// <param name="color">The used <see cref="ConsoleColor"/>.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format</param>
        /// <returns>A reference to the default <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole WriteLine(this ConsoleColor color, string format, params object[] args)
            => SmartConsole.Default.WriteLine(color, format, args);
    }
}
