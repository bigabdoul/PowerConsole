using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace PowerConsole
{
    /// <summary>
    /// Provides extension methods for instances of a <see cref="SmartConsole"/> class.
    /// </summary>
    public static class SmartConsoleExtensions
    {
        #region fields

        const int ENTER = 13, BACKSPACE = 8, CTRL_BACKSPACE = 127;

        private static readonly Dictionary<string, char[]> NativeDigitsCache =
            new Dictionary<string, char[]>();

        #endregion

        /// <summary>
        /// Adds a format provider for the type <typeparamref name="T"/> used
        /// when converting a user input.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole AddFormatProvider<T>(this SmartConsole console, IFormatProvider provider)
        {
            console.Formatters.TryAdd(typeof(T), provider);
            return console;
        }

        /// <summary>
        /// Attempts to remove an instance of <see cref="IFormatProvider"/>
        /// of type <typeparamref name="T"/> from the dictionary.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole RemoveFormatProvider<T>(this SmartConsole console)
        {
            console.Formatters.TryRemove(typeof(T), out _);
            return console;
        }

        /// <summary>
        /// Writes out a specified Unicode character repeated a specified 
        /// number of times.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="c">A Unicode character.</param>
        /// <param name="count">The number of times <paramref name="c"/> occurs.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than zero.</exception>
        public static SmartConsole Repeat(this SmartConsole console, char c, int count)
        {
            return console.Write(new string(c, count));
        }

        /// <summary>
        /// Writes out a lspecified Unicode character repeated a specified 
        /// number of times, and appends a line terminator.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="c">A Unicode character.</param>
        /// <param name="count">The number of times <paramref name="c"/> occurs.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than zero.</exception>
        public static SmartConsole RepeatLine(this SmartConsole console, char c, int count)
            => console.Repeat(c, count).WriteLine();

        /// <summary>
        /// Writes out a line using a specified Unicode character repeated a 
        /// specified number of times.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="s">A series of Unicode characters to repeat.</param>
        /// <param name="count">The number of times <paramref name="s"/> occurs.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than zero.</exception>
        public static SmartConsole Repeat(this SmartConsole console, string s, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count),
                    $"{nameof(count)} must be greater than or equal to 0.");

            var sb = new StringBuilder();

            for (int i = 0; i < count; i++)
                sb.Append(s);

            console.Write(sb.ToString());
            sb.Clear();

            return console;
        }

        /// <summary>
        /// Writes out a line using a specified formatted Unicode string 
        /// repeated a specified number of times.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="count">The number of times the formatted string occurs.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format and write.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole Repeat(this SmartConsole console, int count, string format, params object[] args)
        {
            return console.Repeat(string.Format(format, args), count);
        }

        /// <summary>
        /// Creates a new object of type <typeparamref name="T"/> whose 
        /// properties' values were collected through a series of prompts.
        /// </summary>
        /// <typeparam name="T">The type of the object to create.</typeparam>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <returns>An initialized instance of the type <typeparamref name="T"/>.</returns>
        public static T CreateObject<T>(this SmartConsole console) where T : new()
        {
            T obj = new T();

            foreach (var pi in typeof(T).GetProperties())
            {
                if (pi.CanWrite && console.FindPrompt(pi.Name, out var prompt))
                {
                    pi.SetValue(obj, prompt.Response);
                }
            }

            return obj;
        }

        /// <summary>
        /// Finds the first <see cref="Prompt"/> whose <see cref="Prompt.Id"/>
        /// matches the specified <paramref name="idOrMessage"/> parameter.
        /// The comparison is case-sensitive.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="idOrMessage">The <see cref="Prompt.Id"/> or <see cref="Prompt.Message"/> value to match.</param>
        /// <param name="result">Returns the matched <see cref="Prompt"/> or null.</param>
        /// <returns>true if the <see cref="SmartConsole.Prompts"/> collection contains a match, otherwise false.</returns>
        public static bool FindPrompt(this SmartConsole console, string idOrMessage, out Prompt result)
        {
            result =
                console.Prompts.FirstOrDefault(p => p.Id == idOrMessage) ??
                console.Prompts.FirstOrDefault(p => p.Message == idOrMessage);
            return result != null;
        }

        /// <summary>
        /// Writes out all collected prompts with their respective responses to
        /// a file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/> instance.</param>
        /// <param name="path">The file to write to.</param>
        /// <param name="encoding">The encoding to apply to the string. Can be 
        /// null, which then resolves to <see cref="Encoding.UTF8"/>.</param>
        /// <returns>A reference to the specified <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole WriteFile(this SmartConsole console, string path, Encoding encoding = null)
        {
            File.WriteAllText(path, console.Prompts.AsString(), encoding ?? Encoding.UTF8);
            return console;
        }

        /// <summary>
        /// Writes the comobined string representation of all <see cref="Prompt"/>
        /// elements contained in the <see cref="SmartConsole.Prompts"/> collection.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/> instance.</param>
        /// <param name="stream">A writable <see cref="Stream"/> to write to.</param>
        /// <param name="encoding">The encoding to use. If null, <see cref="Encoding.UTF8"/> will be used.</param>
        /// <returns></returns>
        public static SmartConsole WriteTo(this SmartConsole console, Stream stream, Encoding encoding = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (!stream.CanWrite)
                throw new InvalidOperationException("Cannot write to the provided stream.");

            if (encoding == null)
                encoding = Encoding.UTF8;

            var bytes = encoding.GetBytes(console.Prompts.AsString());

            stream.Write(bytes, 0, bytes.Length);
            return console;
        }

        /// <summary>
        /// Reads masked keystrokes from the system's <see cref="Console"/>.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/> instance.</param>
        /// <param name="message">The optional prompt message to display.</param>
        /// <param name="useMask">true to write out an asterisk on every keystroke, otherwise, false.</param>
        /// <returns></returns>
        public static string GetSecureInput(this SmartConsole console, string message = null, bool useMask = false)
        {
            if (!string.IsNullOrWhiteSpace(message))
                console.Write(message);

            return ReadSecureString(useMask);
        }

        #region SetResponse / TrySetResponse

        /// <summary>
        /// Collects user input as a string and passes it to the specified 
        /// <paramref name="action"/>.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="action">The action that the retrieved value is passed to.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole SetResponse(this SmartConsole console, Action<string> action,
                                               Func<string, bool> validator = null, string validationMessage = null)
            => console.SetResponse(action, validator, validationMessage);

        /// <summary>
        /// Writes out a message, collects user input as a string value and
        /// passes it to the specified <paramref name="action"/>.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="action">The action that the retrieved value is passed to.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole SetResponse(this SmartConsole console, string message, Action<string> action,
                                               Func<string, bool> validator = null, string validationMessage = null)
            => console.Write(message).SetResponse(action, validator, validationMessage);

        /// <summary>
        /// Writes out a message, collects user input as a strongly-typed
        /// value and passes it to the specified <paramref name="action"/>.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="action">The action that the retrieved value is passed to.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole SetResponse<T>(this SmartConsole console, string message, Action<T> action,
                                                  Func<T, bool> validator = null, string validationMessage = null)
            => console.Write(message).SetResponse(action, validator, validationMessage);

        /// <summary>
        /// Attempts to collect user input as a string value and passes it to
        /// the specified <paramref name="action"/>, or reports an error if the
        /// method fails.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="action">The action that the retrieved value is passed to.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <param name="onError">A callback delegate to invoke when an exception is catched.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole TrySetResponse(this SmartConsole console, Action<string> action,
                                                  Func<string, bool> validator = null, string validationMessage = null,
                                                  Action<Exception> onError = null)
        {
            try
            {
                return console.SetResponse(action, validator, validationMessage);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return console;
            }
        }

        /// <summary>
        /// Writes out a message, attempts to collect user input as a string 
        /// value, and passes it to the specified <paramref name="action"/>,
        /// or reports an error if the method fails.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="action">The action that the retrieved value is passed to.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <param name="onError">A callback delegate to invoke when an exception is catched.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole TrySetResponse(this SmartConsole console, string message, Action<string> action,
                                                  Func<string, bool> validator = null, string validationMessage = null,
                                                  Action<Exception> onError = null)
            => console.Write(message).TrySetResponse(action, validator, validationMessage, onError);

        /// <summary>
        /// Attempts to collect user input as a string value and passes it to
        /// the specified <paramref name="action"/>, or reports an error of type
        /// <typeparamref name="TException"/> if the method fails. Other exception 
        /// types are rethrown.
        /// </summary>
        /// <typeparam name="TException">The type of exception to handle.</typeparam>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="action">The action that the retrieved value is passed to.</param>
        /// <param name="onError">A callback delegate to invoke when an exception is catched.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole TrySetResponse<TException>(this SmartConsole console, Action<string> action,
                                                              Action<TException> onError = null,
                                                              Func<string, bool> validator = null,
                                                              string validationMessage = null) where TException : Exception
        {
            try
            {
                return console.SetResponse(action, validator, validationMessage);
            }
            catch (Exception ex)
            {
                if (ex is TException error)
                {
                    onError?.Invoke(error);
                    return console;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Writes out a message, attempts to collect user input as a string
        /// value, and passes it to the specified <paramref name="action"/>,
        /// or reports an error of type <typeparamref name="TException"/> 
        /// if the method fails. Other exception types are rethrown.
        /// </summary>
        /// <typeparam name="TException">The type of exception to handle.</typeparam>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="action">The action that the retrieved value is passed to.</param>
        /// <param name="onError">A callback delegate to invoke when an exception is catched.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole TrySetResponse<TException>(this SmartConsole console, string message,
                                                              Action<string> action, Action<TException> onError = null,
                                                              Func<string, bool> validator = null,
                                                              string validationMessage = null) where TException : Exception
            => console.Write(message).TrySetResponse(action, onError, validator, validationMessage);

        /// <summary>
        /// Attempts to collect user input as a strongly-typed value and passes it to
        /// the specified <paramref name="action"/>, or reports an error if the method fails.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="action">The action that the retrieved value is passed to.</param>
        /// <param name="onError">A callback delegate to invoke when an exception is catched.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole TrySetResponse<T>(this SmartConsole console, Action<T> action,
                                                     Action<Exception> onError = null, Func<T, bool> validator = null,
                                                     string validationMessage = null)
        {
            try
            {
                return console.SetResponse(action, validator, validationMessage);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
                return console;
            }
        }

        /// <summary>
        /// Writes out a message, attempts to collect user input as a strongly-typed
        /// value, and passes it to the specified <paramref name="action"/>, or
        /// reports an error if the method fails.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="action">The action that the retrieved value is passed to.</param>
        /// <param name="onError">A callback delegate to invoke when an exception is catched.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole TrySetResponse<T>(this SmartConsole console, string message, Action<T> action,
                                                     Action<Exception> onError = null, Func<T, bool> validator = null,
                                                     string validationMessage = null)
            => console.Write(message).TrySetResponse(action, onError, validator, validationMessage);

        /// <summary>
        /// Attempts to collect user input as a strongly-typed value and passes it
        /// to the specified <paramref name="action"/>, or reports an error of type
        /// <typeparamref name="TException"/> if the method fails. Other exception
        /// types are rethrown.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <typeparam name="TException">The type of exception to handle.</typeparam>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="action">The action that the retrieved value is passed to.</param>
        /// <param name="onError">A callback delegate to invoke when an exception is catched.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole TrySetResponse<T, TException>(this SmartConsole console, Action<T> action,
                                                                 Action<TException> onError = null,
                                                                 Func<T, bool> validator = null,
                                                                 string validationMessage = null) where TException : Exception
        {
            try
            {
                return console.SetResponse(action, validator, validationMessage);
            }
            catch (Exception ex)
            {
                if (ex is TException error)
                {
                    onError?.Invoke(error);
                    return console;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Writes out a message, attempts to collect user input as a strongly-typed 
        /// value, and passes it to the specified <paramref name="action"/>,
        /// or reports an error of type <typeparamref name="TException"/> if 
        /// the method fails. Other exception types are rethrown.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <typeparam name="TException">The type of exception to handle.</typeparam>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="action">The action that the retrieved value is passed to.</param>
        /// <param name="onError">A callback delegate to invoke when an exception is catched.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole TrySetResponse<T, TException>(this SmartConsole console, string message,
                                                                 Action<T> action, Action<TException> onError = null,
                                                                 Func<T, bool> validator = null,
                                                                 string validationMessage = null) where TException : Exception
            => console.Write(message).TrySetResponse(action, onError, validator, validationMessage);

        #endregion

        /// <summary>
        /// Invokes the specified <paramref name="action"/>.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="action">The delegate to invoke.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole Then(this SmartConsole console, Action action)
        {
            action.Invoke();
            return console;
        }

        /// <summary>
        /// Invokes the specified <paramref name="action"/> and returns a reference
        /// to the current <see cref="SmartConsole"/> instance.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="action">The delegate to invoke.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole Then(this SmartConsole console, Action<SmartConsole> action)
        {
            action.Invoke(console);
            return console;
        }

        /// <summary>
        /// Creates a timer that executes the specified <paramref name="callback"/>
        /// at a regular interval specified by <paramref name="millisecondsInterval"/>.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="callback">The action to invoke on each timer tick.</param>
        /// <param name="millisecondsInterval">The number of milliseconds that 
        /// should elapse between two consecutive ticks.</param>
        /// <param name="name">The name of the associated timer. Useful when 
        /// calling <see cref="ClearInterval(SmartConsole, string)"/>.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole SetInterval(this SmartConsole console, Action<TimerEventArgs> callback, double millisecondsInterval, string name = null)
        {
            TimerManager.Add(console, callback, millisecondsInterval, name, repeat: true);
            return console;
        }

        /// <summary>
        /// Creates a timer that executes the specified <paramref name="callback"/>
        /// once after the delay specified by <paramref name="millisecondsDelay"/>.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="callback">The action to invoke on each timer tick.</param>
        /// <param name="millisecondsDelay">The number of milliseconds to wait 
        /// before calling the callback.</param>
        /// <param name="name">The name of the associated timer. Useful when 
        /// calling <see cref="ClearTimeout(SmartConsole, string)"/>.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole SetTimeout(this SmartConsole console, Action<TimerEventArgs> callback, double millisecondsDelay, string name = null)
        {
            TimerManager.Add(console, callback, millisecondsDelay, name, repeat: false);
            return console;
        }

        /// <summary>
        /// Disposes off a timer previously created with the method
        /// <see cref="SetInterval(SmartConsole, Action{TimerEventArgs}, double, string)"/>.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="name">The name of the associated timer to dispose.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole ClearInterval(this SmartConsole console, string name)
        {
            TimerManager.Remove(name);
            return console;
        }

        /// <summary>
        /// Disposes off a timer previously created with the method
        /// <see cref="SetTimeout(SmartConsole, Action{TimerEventArgs}, double, string)"/>.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="name">The name of the associated timer to dispose.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole ClearTimeout(this SmartConsole console, string name)
        {
            TimerManager.Remove(name);
            return console;
        }

        /// <summary>
        /// Disposes off all timers previously created with either of the methods
        /// <see cref="SetTimeout(SmartConsole, Action{TimerEventArgs}, double, string)"/>
        /// and <see cref="SetInterval(SmartConsole, Action{TimerEventArgs}, double, string)"/>.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static SmartConsole ClearTimers(this SmartConsole console)
        {
            TimerManager.Clear();
            return console;
        }

        /// <summary>
        /// Invokes the specified delegate function and returns its result.
        /// </summary>
        /// <typeparam name="T">The delegate's return type.</typeparam>
        /// <param name="_">The used <see cref="SmartConsole"/>. Is not used.</param>
        /// <param name="func">The delegate to invoke.</param>
        /// <returns><typeparamref name="T"/> which represents the result of the delegate <paramref name="func"/>.</returns>
        public static T Result<T>(this SmartConsole _, Func<T> func) => func.Invoke();

        /// <summary>
        /// Returns the combined string representation of all <see cref="Prompt"/>
        /// elements contained in the specified collection.
        /// </summary>
        /// <param name="collection">The collection to transform.</param>
        /// <param name="separator">The string to use as a separator. If null,
        /// <see cref="Environment.NewLine"/> will be used.</param>
        /// <returns></returns>
        public static string AsString(this IEnumerable<Prompt> collection, string separator = null)
        {
            return string.Join(separator ?? Environment.NewLine, collection.Select(p => p));
        }

        /// <summary>
        /// Attempts to change the type of the specified <paramref name="response"/>.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <param name="response">The response to convert.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="result">Returns the converted <paramref name="response"/>.
        /// </param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public static bool TryConvert<T>(this string response, IFormatProvider provider, out T result)
        {
            try
            {
                var isNullable = typeof(T).IsNullable(out var nullableUnderlyingType) || typeof(T) == typeof(string);

                if (string.IsNullOrWhiteSpace(response))
                {
                    result = default;
                    return isNullable;
                }

                result = (T)Convert.ChangeType(response, nullableUnderlyingType ?? typeof(T), provider);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Returns the <see cref="TypeCategory"/> value for the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>A <see cref="TypeCategory"/> value that represents the 
        /// category of the specified <paramref name="type"/>.</returns>
        public static TypeCategory GetTypeCategory(this Type type)
        {
            if (type.IsNullable(out var underlyingType))
            {
                type = underlyingType;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return TypeCategory.Boolean;
                case TypeCode.DateTime:
                    return TypeCategory.DateTime;
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return TypeCategory.IntegralNumber;
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return TypeCategory.FloatingPointNumber;
                case TypeCode.DBNull:
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.String:
                default:
                    return TypeCategory.Other;
            }
        }

        /// <summary>
        /// Returns a tuple that indicates whether the specified type is a 
        /// number and, if it is, what precise category it belongs to.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>A tuple that indicates the category of the specified <paramref name="type"/>.</returns>
        public static (bool integral, bool floatingPoint) GetNumberCategory(this Type type)
        {
            var category = type.GetTypeCategory();
            return (category == TypeCategory.IntegralNumber, category == TypeCategory.FloatingPointNumber);
        }

        /// <summary>
        /// Determines whether the specified type is <see cref="Nullable"/>.
        /// </summary>
        /// <param name="type">The type to test.</param>
        /// <param name="underlyingType">Returns the underlying type</param>
        /// <returns>true if <paramref name="type"/> is nullable, otherwise false.</returns>
        public static bool IsNullable(this Type type, out Type underlyingType)
        {
            underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType != null;
        }

        /// <summary>
        /// Reads masked keystrokes from the system's <see cref="Console"/>.
        /// </summary>
        /// <param name="useMask">
        /// true to write out an asterisk on every keystroke, otherwise, false.
        /// </param>
        /// <returns>A string.</returns>
        public static string ReadSecureString(bool useMask = false)
        {
            // borrowed from stackoverflow.com and slightly modified
            int[] filtered = { 0, 27, 9, 10 };

            var secureStr = new System.Security.SecureString();

            char chr;

            while ((chr = Console.ReadKey(true).KeyChar) != ENTER)
            {
                if ((chr == BACKSPACE || chr == CTRL_BACKSPACE) && secureStr.Length > 0)
                {
                    if (useMask) Console.Write("\b \b");
                    secureStr.RemoveAt(secureStr.Length - 1);
                }
                else if (((chr == BACKSPACE) || (chr == CTRL_BACKSPACE)) && (secureStr.Length == 0))
                {
                    // don't append * when length is 0 and backspace is selected
                }
                else if (filtered.Count(x => chr == x) > 0)
                {
                    // don't append when a filtered char is detected
                }
                else
                {
                    // append and eventually write * mask
                    secureStr.AppendChar(chr);
                    if (useMask) Console.Write('*');
                }
            }

            var ptr = Marshal.SecureStringToBSTR(secureStr);
            var result = Marshal.PtrToStringBSTR(ptr);
            Marshal.ZeroFreeBSTR(ptr);
            return result;
        }

        /// <summary>
        /// Allows only numbers to be entered.
        /// </summary>
        /// <param name="allowDecimal">true to allow a single decimal-point (period), otherwise false.</param>
        /// <param name="culture">The culture to use. If null, the culture of 
        /// the current thread or <see cref="CultureInfo.InvariantCulture"/> 
        /// will be used.</param>
        /// <param name="allowNegative">true to allow negative numbers, otherwise allow only positive numbers.</param>
        /// <returns>A string that represents the typed number.</returns>
        public static string ReadNumber(bool allowDecimal = false, CultureInfo culture = null, bool allowNegative = true)
        {
            var sb = new StringBuilder();
            char chr;
            var minus = false;
            var period = false;

            if (culture == null)
            {
                culture = Thread.CurrentThread.CurrentCulture ?? CultureInfo.InvariantCulture;
            }

            var numberFormat = culture.NumberFormat;

            if (!NativeDigitsCache.TryGetValue(culture.EnglishName, out var nativeDigits))
            {
                nativeDigits = numberFormat.NativeDigits.Select(d => d[0]).ToArray();
                NativeDigitsCache.Add(culture.EnglishName, nativeDigits);
            }

            var negativeChar = numberFormat.NegativeSign[0];
            var decimalChar = numberFormat.NumberDecimalSeparator[0];

            while ((chr = Console.ReadKey(true).KeyChar) != ENTER)
            {
                var len = sb.Length;
                if (chr == BACKSPACE || chr == CTRL_BACKSPACE)
                {
                    if (len > 0)
                    {
                        Console.Write("\b \b");

                        // store the char being removed
                        len--;
                        chr = sb[len];
                        sb.Remove(len, 1);
                    }

                    if (minus && (len == 0 || chr == negativeChar))
                        minus = false;

                    // has the period been removed?
                    if (period && (len == 0 || chr == decimalChar))
                        period = false;
                }
                else if (nativeDigits.Contains(chr))
                {
                    // append only digits
                    _AppendChar();
                }
                else if (chr == negativeChar && allowNegative && !minus && len == 0)
                {
                    // allow minus only at the very beginning of the sequence
                    _AppendChar();
                    minus = true;
                }
                else if (chr == decimalChar && allowDecimal && !period)
                {
                    _AppendChar();
                    period = true;
                }
            }

            // since ENTER was pressed, write a line in the console
            Console.WriteLine();

            return sb.ToString();

            void _AppendChar()
            {
                sb.Append(chr);
                Console.Write(chr);
            }
        }
    }
}
