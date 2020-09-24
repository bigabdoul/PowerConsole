﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PowerConsole
{
    /// <summary>
    /// Provides extension methods for instances of a <see cref="SmartConsole"/> class.
    /// </summary>
    public static class SmartConsoleExtensions
    {
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
        /// Writes out a line using a specified Unicode character repeated a 
        /// specified number of times.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="c">A Unicode character.</param>
        /// <param name="count">The number of times <paramref name="c"/> occurs.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than zero.</exception>
        public static SmartConsole Repeat(this SmartConsole console, char c, int count)
        {
            return console.WriteLine(new string(c, count));
        }

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
                var nullableUnderlyingType = Nullable.GetUnderlyingType(typeof(T));
                var isNullable = nullableUnderlyingType != null || typeof(T) == typeof(string);

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
        /// <returns></returns>
        public static TypeCategory GetTypeCategory(this Type type)
        {
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
        /// Reads masked keystrokes from the system's <see cref="Console"/>.
        /// </summary>
        /// <param name="useMask">
        /// true to write out an asterisk on every keystroke, otherwise, false.
        /// </param>
        /// <returns>A string.</returns>
        public static string ReadSecureString(bool useMask = false)
        {
            // borrowed from stackoverflow.com and slightly modified
            const int ENTER = 13, BACKSPACE = 8, CTRL_BACKSPACE = 127;
            int[] filtered = { 0, 27, 9, 10 };

            var password = new System.Security.SecureString();

            char chr;

            while ((chr = Console.ReadKey(true).KeyChar) != ENTER)
            {
                if ((chr == BACKSPACE || chr == CTRL_BACKSPACE) && password.Length > 0)
                {
                    if (useMask) Console.Write("\b \b");
                    password.RemoveAt(password.Length - 1);
                }
                else if (((chr == BACKSPACE) || (chr == CTRL_BACKSPACE)) && (password.Length == 0))
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
                    password.AppendChar(chr);
                    if (useMask) Console.Write('*');
                }
            }

            var ptr = Marshal.SecureStringToBSTR(password);
            var result = Marshal.PtrToStringBSTR(ptr);
            Marshal.ZeroFreeBSTR(ptr);
            return result;
        }
    }
}