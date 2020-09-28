using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace PowerConsole
{
    /// <summary>
    /// Represents an object that makes strongly-typed user input collection
    /// and validation through the <see cref="Console"/> easier.
    /// </summary>
    public class SmartConsole
    {
        #region private fields

        private readonly TextReader _instream;
        private readonly TextWriter _outstream;
        private readonly Dictionary<string, Prompt> _history;
        private ConcurrentDictionary<Type, IFormatProvider> _formatters;
        private ConsoleColor _currentForegroundColor;
        private ConsoleColor _backgroundColor;
        private bool? _storePrompts;
        private HashSet<Action<SmartConsole, ConsoleCancelEventArgs>> _cancelActions;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartConsole" /> class.
        /// </summary>
        public SmartConsole() : this(Console.In, Console.Out)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartConsole" /> class
        /// using the specified parameter.
        /// </summary>
        /// <param name="reader">The text reader used to scan user inputs.</param>
        public SmartConsole(TextReader reader) : this(reader, Console.Out)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartConsole" /> class
        /// using the specified parameters.
        /// </summary>
        /// <param name="inReader">A <see cref="TextReader"/> that represents the standard input stream.</param>
        /// <param name="outWriter">A <see cref="TextWriter"/> that represents the standard output stream.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="inReader"/> or <paramref name="outWriter"/> is null.
        /// </exception>
        public SmartConsole(TextReader inReader, TextWriter outWriter)
        {
            _instream = inReader ?? throw new ArgumentNullException(nameof(inReader));
            _outstream = outWriter ?? throw new ArgumentNullException(nameof(outWriter));

            if (Console.In != _instream)
                Console.SetIn(_instream);

            if (Console.Out != _outstream)
                Console.SetOut(_outstream);

            _history = new Dictionary<string, Prompt>();
            _currentForegroundColor = Console.ForegroundColor;
            _backgroundColor = Console.BackgroundColor;

            Culture = Thread.CurrentThread.CurrentCulture;
        }

        #endregion

        #region properties

        /// <summary>
        /// Returns the last <see cref="PowerConsole.Prompt" /> instance or null.
        /// </summary>
        /// <returns>An instance of the <see cref="PowerConsole.Prompt" /> class or null.</returns>
        public Prompt LastPrompt { get; protected set; }

        /// <summary>
        /// Returns a collection containing all <see cref="PowerConsole.Prompt"/> objects
        /// added to the underlying history dictionary.
        /// </summary>
        /// <returns>A read-only collection of <see cref="PowerConsole.Prompt"/> elements.</returns>
        public IReadOnlyCollection<Prompt> Prompts
        {
            get => _history.Values;
        }

        /// <summary>
        /// Gets a dictionary of <see cref="IFormatProvider"/> elements.
        /// </summary>
        public ConcurrentDictionary<Type, IFormatProvider> Formatters
        {
            get
            {
                if (_formatters == null)
                {
                    lock (this)
                    {
                        if (_formatters == null)
                            _formatters = new ConcurrentDictionary<Type, IFormatProvider>();
                    }
                }
                return _formatters;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ConsoleColor"/> to use for writing out
        /// info messages. The default is <see cref="ConsoleColor.Blue"/>.
        /// </summary>
        public ConsoleColor InfoColor { get; set; } = ConsoleColor.Blue;

        /// <summary>
        /// Gets or sets the <see cref="ConsoleColor"/> to use for writing out
        /// warning messages. The default is <see cref="ConsoleColor.Yellow"/>.
        /// </summary>
        public ConsoleColor WarningColor { get; set; } = ConsoleColor.Yellow;

        /// <summary>
        /// Gets or sets the <see cref="ConsoleColor"/> to use for writing out
        /// error messages. The default is <see cref="ConsoleColor.Red"/>.
        /// </summary>
        public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;

        /// <summary>
        /// Gets or sets the <see cref="ConsoleColor"/> to use for writing out
        /// success messages. The default is <see cref="ConsoleColor.Green"/>.
        /// </summary>
        public ConsoleColor SuccessColor { get; set; } = ConsoleColor.Green;
        
        /// <summary>
        /// Gets the culture used for this console.
        /// </summary>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Gets or sets validation messages for type categories.
        /// </summary>
        public ValidationMessages ValidationMessages { get; set; }

        /// <summary>
        /// Gets the left inner margin size.
        /// </summary>
        public byte MarginLeft { get; private set; }

        /// <summary>
        /// Gets or sets the default responses for "No" prompts.
        /// </summary>
        public static string[] DefaultNoResponses { get; set; } = { string.Empty, "n", "no" };

        /// <summary>
        /// Gets or sets the default responses for "Yes" prompts.
        /// </summary>
        public static string[] DefaultYesResponses { get; set; } = { string.Empty, "y", "yes" };
        
        /// <summary>
        /// Indicates whether the <see cref="Console.CancelKeyPress"/> event
        /// was invoked.
        /// </summary>
        public bool CancelRequested { get; private set; }

        #endregion

        #region Prompt

        /// <summary>
        /// Returns the default instance of the <see cref="SmartConsole"/> class.
        /// </summary>
        /// <returns>A reference to the default <see cref="SmartConsole" /> instance.</returns>
        public static readonly SmartConsole Default = new SmartConsole();

        /// <summary>
        /// Writes out a message and collects user input.
        /// </summary>
        /// <param name="message">The prompt message to display.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole Prompt(string message)
        {
            return Prompt(message, false);
        }

        /// <summary>
        /// Writes out a message, collects user input, and optionally stores the 
        /// message into the prompt history.
        /// </summary>
        /// <param name="message">The prompt message to display.</param>
        /// <param name="store">true to store the message into the prompt history, otherwise false.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole Prompt(string message, bool store)
        {
            return Prompt(message, null, store);
        }

        /// <summary>
        /// Writes out a message, collects user input, and optionally stores the 
        /// message into the prompt history.
        /// </summary>
        /// <param name="message">The prompt message to display.</param>
        /// <param name="historyLabel">An alternative message to use for history replay.</param>
        /// <param name="store">true to store the message into the prompt history, otherwise false.</param>
        /// <param name="promptId">A unique identifier of the prompt.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public virtual SmartConsole Prompt(string message, string historyLabel = null, bool store = false,
                                           string promptId = null, string validationMessage = null,
                                           Func<string, bool> validator = null)
        {
            _outstream.Write(message);
            return RecallResponse(message)
                .GetInput(message, historyLabel, store, validationMessage, validator, promptId);
        }

        /// <summary>
        /// Writes out a message and terminates the line, and collects user input.
        /// </summary>
        /// <param name="message">The prompt message to display.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole PromptLine(string message)
        {
            return PromptLine(message, false);
        }

        /// <summary>
        /// Writes out a message and terminates the line, collects user input, and
        /// optionally stores the message into the prompt history.
        /// </summary>
        /// <param name="message">The prompt message to display.</param>
        /// <param name="store">true to store the message into the prompt history, otherwise false.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole PromptLine(string message, bool store)
            => PromptLine(message, null, store);

        /// <summary>
        /// Writes out a message, terminates the line, and collects user input.
        /// </summary>
        /// <param name="message">The prompt message to display.</param>
        /// <param name="historyLabel">An alternative message to use for history replay.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole PromptLine(string message, string historyLabel)
            => PromptLine(message, historyLabel, store: false);

        /// <summary>
        /// Writes out a message, terminates the line, collects user input, and
        /// optionally stores the message into the prompt history.
        /// </summary>
        /// <param name="message">The prompt message to display.</param>
        /// <param name="historyLabel">An alternative message to use for history replay.</param>
        /// <param name="store">true to store the message into the prompt history, otherwise false.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public virtual SmartConsole PromptLine(string message, string historyLabel, bool store)
        {
            _outstream.Write(message);

            return RecallResponse($"{message}{Environment.NewLine}")
                .GetInput<string>(message, historyLabel, store);
        }

        /// <summary>
        /// Writes out a message, collects user input as a strongly-typed value,
        /// and optionally stores the response into the prompt history.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <param name="message">The prompt message to display.</param>
        /// <param name="historyLabel">An alternative message to use for history replay.</param>
        /// <param name="store">true to store the message into the prompt history, otherwise false.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="promptId">A unique identifier of the prompt.</param>
        /// <param name="converter">A function that converts the user input to the specified type <typeparamref name="T"/>.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public virtual SmartConsole Prompt<T>(string message, string historyLabel = null, bool store = false,
                                               string validationMessage = null, Func<T, bool> validator = null,
                                               string promptId = null, Func<string, IFormatProvider, T> converter = null)
        {
            _outstream.Write(message);

            return RecallResponse(message)
                .GetInput(message, historyLabel, store, validationMessage, validator, promptId, converter);
        }

        /// <summary>
        /// Writes out a message and collects user input as a boolean where the
        /// default response is affirmative (Yes).
        /// </summary>
        /// <param name="message">The prompt message to display.</param>
        /// <param name="defaultResponses">
        /// Zero or more strings accepted as the default response. If the
        /// argument is null or empty, an empty string, "y", and "yes" are 
        /// the accepted case-insensitive default responses.
        /// </param>
        /// <returns>true if any of the <paramref name="defaultResponses"/> is entered, otherwise false.</returns>
        public bool PromptYes(string message, params string[] defaultResponses)
            => ConvertResponse(message, (input, _) => YesDefault(input, defaultResponses));

        /// <summary>
        /// Writes out a message and collects user input as a boolean where the
        /// default response is negative (No).
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="defaultResponses">
        /// Zero or more strings accepted as the default response. If the
        /// argument is null or empty, an empty string, "n", and "no" are 
        /// the accepted case-insensitive default responses.
        /// </param>
        /// <returns>true if any of the <paramref name="defaultResponses"/> is entered, otherwise false.</returns>
        public bool PromptNo(string message, params string[] defaultResponses)
            => ConvertResponse(message, (input, _) => NoDefault(input, defaultResponses));

        #endregion

        #region GetResponse / SetResponse / ConvertResponse

        /// <summary>
        /// Writes out a message and collects user input as a string.
        /// The response is not stored into the prompt history.
        /// </summary>
        /// <param name="message">The prompt message to display.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="formatter"></param>
        /// <returns>The string response of the last prompt.</returns>
        public string GetResponse(string message, string validationMessage = null, Func<string, bool> validator = null,
                                  Func<string, IFormatProvider, string> formatter = null)
            => GetResponse<string>(message, validationMessage, validator, formatter);

        /// <summary>
        /// Writes out a message and collects user input as a strongly-typed value.
        /// The response is not stored into the prompt history.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <param name="message">The prompt message to display.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="converter">A function that converts the user input to the specified type <typeparamref name="T"/>.</param>
        /// <returns>The converted response of the last prompt.</returns>
        public virtual T GetResponse<T>(string message,
                                        string validationMessage = null,
                                        Func<T, bool> validator = null,
                                        Func<string, IFormatProvider, T> converter = null)
        {
            return Prompt(message,
                          historyLabel: null,
                          store: false,
                          validationMessage,
                          validator,
                          converter: converter).LastPrompt.As<T>(Culture);
        }

        /// <summary>
        /// Writes a message and collects user input as a strongly-typed value by allowing custom conversion.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <param name="message">The prompt message to display.</param>
        /// <param name="converter">A function that converts the user input to the specified type <typeparamref name="T"/>.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <returns></returns>
        public virtual T ConvertResponse<T>(string message, Func<string, IFormatProvider, T> converter,
                                            string validationMessage = null, Func<T, bool> validator = null)
        {
            return Prompt(message,
                          historyLabel: null,
                          store: false,
                          validationMessage,
                          validator,
                          converter: converter).LastPrompt.As<T>(Culture);
        }

        /// <summary>
        /// Collects user input as a strongly-typed value and passes it to the
        /// specified <paramref name="action"/>.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <param name="action">The action that the retrieved value is passed to.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public virtual SmartConsole SetResponse<T>(Action<T> action, Func<T, bool> validator = null,
                                                   string validationMessage = null)
        {
            var result = GetInput(message: null,
                                  validationMessage: validationMessage,
                                  validator: validator)
                .LastPrompt.As<T>(Culture);

            action.Invoke(result);
            return this;
        }

        #endregion

        #region Write...

        /// <summary>
        /// Writes out a character.
        /// </summary>
        /// <param name="value">The character to write to the text stream.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole Write(char value)
        {
            _outstream.Write(value);
            return this;
        }

        /// <summary>
        /// Writes out a message.
        /// </summary>
        /// <param name="message">The prompt message to write.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole Write(string message)
        {
            _outstream.Write(message);
            return this;
        }

        /// <summary>
        /// Writes out a message.
        /// </summary>
        /// <param name="message">The prompt message to write.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole Write(object message)
        {
            _outstream.Write(message);
            return this;
        }

        /// <summary>
        /// Writes out a formatted string, using the same semantics as 
        /// <see cref="string.Format(string, object[])"/>.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">
        /// An object array that contains zero or more objects to format and write.
        /// </param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole Write(string format, params object[] args)
        {
            _outstream.Write(format, args);
            return this;
        }

        /// <summary>
        /// Writes a line terminator.
        /// </summary>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole WriteLine()
        {
            _outstream.WriteLine();
            return this;
        }

        /// <summary>
        /// Writes out a message and a new line.
        /// </summary>
        /// <param name="message">The prompt message to display.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole WriteLine(string message)
        {
            _outstream.WriteLine(message);
            return this;
        }

        /// <summary>
        /// Writes out a message and a new line.
        /// </summary>
        /// <param name="message">The prompt message to display.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole WriteLine(object message)
        {
            _outstream.WriteLine(message);
            return this;
        }

        /// <summary>
        /// Writes out a formatted string and a new line, using the same 
        /// semantics as <see cref="string.Format(string, object[])"/>.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">
        /// An object array that contains zero or more objects to format and write.
        /// </param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole WriteLine(string format, params object[] args)
        {
            _outstream.WriteLine(format, args);
            return this;
        }

        /// <summary>
        /// Writes out an array of objects, each one followed by a line terminator.
        /// </summary>
        /// <param name="args">An object array that contains zero or more objects to write.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole WriteLines(params object[] args)
        {
            if (args?.Length > 0)
                for (int i = 0; i < args.Length; i++)
                    _outstream.WriteLine(args[i]);
            return this;
        }

        /// <summary>
        /// Writes out a collection of object, each one followed by a line terminator.
        /// </summary>
        /// <param name="collection">A collection that contains zero or more objects to write.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole WriteLines(IEnumerable<object> collection)
        {
            foreach (var line in collection)
            {
                _outstream.WriteLine(line);
            }
            return this;
        }

        #region ConsoleColor management

        /// <summary>
        /// Writes out each element contained in <paramref name="args"/> using
        /// the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="color">The color to use.</param>
        /// <param name="separator">A string that separates each element in <paramref name="args"/>.</param>
        /// <param name="args">A one-dimensional array of objects to write out.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole WriteList(ConsoleColor color, string separator, params object[] args)
        {
            var length = args.Length;

            for (int i = 0; i < length; i++)
            {
                SetForegroundColor(color);
                _outstream.Write(args[i]);
                RestoreForegroundColor();

                if (i < length - 1)
                {
                    _outstream.Write(separator);
                }
            }

            return this;
        }

        /// <summary>
        /// Writes out a message in the system's <see cref="Console"/> using
        /// the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="color">The <see cref="ConsoleColor"/> to use for writing the message.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public virtual SmartConsole Write(string message, ConsoleColor color)
            => SetForegroundColor(color).Write(message).RestoreForegroundColor();

        /// <summary>
        /// Writes out a colored and formatted string, using the same semantics
        /// as <see cref="string.Format(string, object[])"/>.
        /// </summary>
        /// <param name="color">The foreground color to set.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">
        /// An object array that contains zero or more objects to format and write.
        /// </param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole Write(ConsoleColor color, string format, params object[] args)
        {
            return SetForegroundColor(color).Write(format, args).RestoreForegroundColor();
        }

        /// <summary>
        /// Writes out a message in the system's <see cref="Console"/> using
        /// the value of the <see cref="InfoColor"/> property.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole WriteInfo(string message)
        {
            return Write(message, ConsoleColor.Blue);
        }

        /// <summary>
        /// Writes out a message in the system's <see cref="Console"/> using
        /// the value of the <see cref="SuccessColor"/> property.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole WriteSuccess(string message)
        {
            return Write(message, SuccessColor);
        }

        /// <summary>
        /// Writes out a message in the system's <see cref="Console"/> using
        /// the value of the <see cref="WarningColor"/> property.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole WriteWarning(string message)
        {
            return Write(message, WarningColor);
        }

        /// <summary>
        /// Writes out a message in the system's <see cref="Console.Error"/>
        /// standard output stream using the value of the <see cref="ErrorColor"/>
        /// property.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole WriteError(string message)
        {
            SetForegroundColor(ErrorColor);
            Console.Error.Write(message);
            return RestoreForegroundColor();
        }

        /// <summary>
        /// Writes out the message of the specified <paramref name="error"/>.
        /// </summary>
        /// <param name="error">The exception that occurred.</param>
        /// <returns></returns>
        public SmartConsole WriteError(Exception error)
        {
            var sb = new StringBuilder(error.Message);
            var inner = error.InnerException;

            while (inner != null)
            {
                sb.AppendLine(inner.Message);
                inner = inner.InnerException;
            }

            sb.AppendLine();
            return WriteError(sb.ToString());
        }

        #endregion
        
        /// <summary>
        /// Sets the background color of the system's <see cref="Console"/>
        /// to the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="color">The background color to set.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole SetBackgroundColor(ConsoleColor color)
        {
            _backgroundColor = Console.BackgroundColor;
            Console.BackgroundColor = color;
            return this;
        }

        /// <summary>
        /// Sets the foreground color of the system's <see cref="Console"/>
        /// to the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="color">The foreground color to set.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole SetForegroundColor(ConsoleColor color)
        {
            _currentForegroundColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            return this;
        }

        /// <summary>
        /// Restores the background color of the system's <see cref="Console"/>.
        /// </summary>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole RestoreBackgroundColor()
        {
            Console.BackgroundColor = _backgroundColor;
            return this;
        }

        /// <summary>
        /// Restores the foreground color of the system's <see cref="Console"/>.
        /// </summary>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole RestoreForegroundColor()
        {
            Console.ForegroundColor = _currentForegroundColor;
            return this;
        }

        #endregion

        #region Misc

        /// <summary>
        /// Throws a <see cref="ContinuationException"/> if the specified 
        /// <paramref name="condition"/> is not true. This is a crucial part
        /// of the fluent design of this <see cref="SmartConsole"/> to avoid
        /// the next method call under some circumstances. It is recommended to
        /// call this method from within a try-catch block with a 
        /// <see cref="ContinuationException"/> filter.
        /// </summary>
        /// <param name="condition">The condition required to continue code execution.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole ContinueWhen(bool condition)
            => !condition ? throw new ContinuationException() : this;

        /// <summary>
        /// Writes out a message and collects a masked user input.
        /// <para>
        /// Caution: This method only changes the foreground color of
        /// <see cref="Console"/> to be the same as its current background
        /// color. This means that the input, although appearing invisible, 
        /// may be copied from the console. For password retrieval, use
        /// <see cref="SmartConsoleExtensions.GetSecureInput(SmartConsole, string, bool)"/> 
        /// or <see cref="SmartConsoleExtensions.ReadSecureString(bool)"/>.
        /// </para>
        /// </summary>
        /// <param name="message">The prompt message to display.</param>
        /// <returns>The string response of the last prompt.</returns>
        public virtual string GetMaskedInput(string message)
        {
            _outstream.Write(message);

            var result = SetForegroundColor(Console.BackgroundColor)
                .GetInput<string>(message)
                .LastPrompt.As<string>();

            RestoreForegroundColor();
            return result;
        }

        /// <summary>
        /// Prints the history of all prompts.
        /// </summary>
        /// <param name="prefix">A string to prepend to each <see cref="PowerConsole.Prompt"/>'s string representation.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole Recall(string prefix = null)
        {
            foreach (var prompt in _history.Values)
                _outstream.WriteLine($"{prefix}{prompt}");
            return this;
        }

        /// <summary>
        /// Removes all prompts from the prompt history.
        /// </summary>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public virtual SmartConsole Clear()
        {
            _history.Clear();
            return this;
        }

        /// <summary>
        /// Adds the specified action to the collection of handlers that listen
        /// for the <see cref="Console.CancelKeyPress"/> event. This is a 
        /// convenient way to add a handler for the <see cref="Console.CancelKeyPress"/>
        /// event without breaking the fluent nature of the <see cref="SmartConsole"/>.
        /// </summary>
        /// <param name="action">The action to add to the collection.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        public virtual SmartConsole OnCancel(Action<SmartConsole, ConsoleCancelEventArgs> action)
        {
            AddCancelAction(action);
            return this;
        }

        /// <summary>
        /// Obtains the next character or function key pressed by the user.
        /// The pressed key is optionally displayed in the console window.
        /// </summary>
        /// <param name="intercept">Determines whether to display the pressed 
        /// key in the console window. true to not display the pressed key; 
        /// otherwise, false.</param>
        /// <returns>See <see cref="Console.ReadKey(bool)"/>.</returns>
        public ConsoleKeyInfo ReadKey(bool intercept = false)
        {
            return Console.ReadKey(intercept);
        }

        /// <summary>
        /// Adds an anonymous <see cref="UnhandledExceptionEventHandler"/> to
        /// the current <see cref="AppDomain.UnhandledException"/> event.
        /// This method acts a global error handler for the current application.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public SmartConsole Catch(Action<SmartConsole, UnhandledExceptionEventArgs> handler)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => handler(this, e);
            return this;
        }

        #endregion

        #region property setters

        /// <summary>
        /// Sets the <see cref="Console.Title"/> property value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole SetTitle(string value)
        {
            Console.Title = value;
            return this;
        }

        /// <summary>
        /// Sets the encoding the <see cref="Console"/> uses to read input and write output.
        /// </summary>
        /// <param name="encoding">The character encoding to set.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole SetEncoding(Encoding encoding)
        {
            Console.InputEncoding = Console.OutputEncoding = encoding;
            return this;
        }

        /// <summary>
        /// Sets the values of the <see cref="Culture"/> and
        /// <see cref="Thread.CurrentCulture"/> properties.
        /// </summary>
        /// <param name="value">The name of the culture.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        /// <exception cref="CultureNotFoundException">
        /// <paramref name="value"/> is not a valid culture name.
        /// </exception>
        public SmartConsole SetCulture(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Culture = CultureInfo.CreateSpecificCulture(value);
                Thread.CurrentThread.CurrentCulture = Culture;
            }
            return this;
        }

        /// <summary>
        /// Instructs this <see cref="SmartConsole"/> instance to force-store
        /// all prompts or leave the decision to individual method calls.
        /// </summary>
        /// <param name="value">true to force-store all prompts, otherwise
        /// false to leave the decision to method calls.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole Store(bool value = true)
        {
            if (value)
                _storePrompts = true;
            else
                _storePrompts = null;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="InfoColor"/> property value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole SetInfoColor(ConsoleColor value)
        {
            InfoColor = value;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="WarningColor"/> property value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole SetWarningColor(ConsoleColor value)
        {
            WarningColor = value;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="ErrorColor"/> property value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole SetErrorColor(ConsoleColor value)
        {
            ErrorColor = value;
            return this;
        }

        /// <summary>
        /// Sets the <see cref="SuccessColor"/> property value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        public SmartConsole SetSuccessColor(ConsoleColor value)
        {
            SuccessColor = value;
            return this;
        }

        /// <summary>
        /// Sets validation messages for type categories.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns></returns>
        public SmartConsole SetValidationMessage(ValidationMessages value)
        {
            ValidationMessages = value;
            return this;
        }

        #endregion

        #region helpers

        /// <summary>
        /// Collects the user input and makes sure that it is valid according
        /// to the specified validator, if any.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <param name="message">The prompt message, which is also the history dictionary's key.</param>
        /// <param name="historyLabel">An alternative message to use for history replay.</param>
        /// <param name="store">true to store the message into the prompt history, otherwise false.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="promptId">A unique identifier of the prompt.</param>
        /// <param name="converter">A function that converts the user input to the specified type <typeparamref name="T"/>.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        protected virtual SmartConsole GetInput<T>(string message, string historyLabel = null, bool store = false,
                                                    string validationMessage = null, Func<T, bool> validator = null,
                                                    string promptId = null,
                                                    Func<string, IFormatProvider, T> converter = null)
        {
            var response = GetValidInput(message, validationMessage, validator, converter);

            LastPrompt = null;

            if (!string.IsNullOrWhiteSpace(message))
            {
                if (_storePrompts.HasValue)
                    store = true;

                if (!_history.ContainsKey(message))
                {
                    if (store)
                        LastPrompt = StorePrompt(message, historyLabel, response, promptId);
                }
                else if (!EqualityComparer<T>.Default.Equals(response, default))
                {
                    // update the prompt if the user provided a valid response
                    LastPrompt = _history[message];
                    LastPrompt.Response = response;
                }
                else if (store)
                    LastPrompt = StorePrompt(message, historyLabel, response, promptId);
            }

            if (LastPrompt == null)
                LastPrompt = new Prompt(message, response, promptId);

            return this;
        }

        /// <summary>
        /// Collects the user input and makes sure that it is valid according
        /// to the specified validator, if any.
        /// </summary>
        /// <typeparam name="T">The conversion type.</typeparam>
        /// <param name="message">The prompt message, which is also the history dictionary's key.</param>
        /// <param name="validationMessage">A message to display if the user enters an invalid response.</param>
        /// <param name="validator">A function that further restricts or validates user input.</param>
        /// <param name="converter">A function that converts the user input to the specified type <typeparamref name="T"/>.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        protected virtual T GetValidInput<T>(string message, string validationMessage, Func<T, bool> validator, Func<string, IFormatProvider, T> converter)
        {
            var (integral, floatingPoint) = typeof(T).GetNumberCategory();
            var cult = Culture;
            var response = _ReadLine();

            if (string.IsNullOrWhiteSpace(response) && 
                !string.IsNullOrWhiteSpace(message) && 
                _history.ContainsKey(message))
            {
                return (T)_history[message].Response;
            }
            
            var hasConstraint = !string.IsNullOrWhiteSpace(validationMessage);
            var hasValidator = validator != null;
            var hasConverter = converter != null;

            if (!hasConstraint && validator != null)
                validationMessage = ValidationMessages.GetDefaultValidationMessage<T>(ValidationMessages);

            if (!Formatters.TryGetValue(typeof(T), out var provider) && cult != null)
                provider = cult;

            T result;

            while (_InputIsNotvalid())
            {
                if (hasConstraint) WriteError(validationMessage);

                response = _ReadLine();
            }

            return result;

            string _ReadLine()
            {
                if (CancelRequested) throw new OperationCanceledException();

                return integral || floatingPoint
                    ? SmartConsoleExtensions.ReadNumber(floatingPoint, cult)
                    : _instream.ReadLine();
            }

            bool _InputIsNotvalid() => !_TryConvert() ||
                (hasConstraint && string.IsNullOrWhiteSpace(response)) ||
                (hasValidator && !validator(result));
        
            bool _TryConvert()
            {
                if (hasConverter)
                {
                    result = converter.Invoke(response, provider);
                    return true;
                }
                return response.TryConvert(provider, out result);
            }
        }

        /// <summary>
        /// Stores the response into the prompt history.
        /// </summary>
        /// <param name="message">The prompt message.</param>
        /// <param name="historyLabel">An alternative message to use for history replay.</param>
        /// <param name="response">The prompt response to store.</param>
        /// <param name="promptId">A unique identifier of the prompt.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        protected Prompt StorePrompt(string message, string historyLabel, object response, string promptId = null)
        {
            var prompt = new Prompt(historyLabel ?? message, response, promptId);
            _history[message] = prompt;
            return prompt;
        }

        /// <summary>
        /// Writes the response of the prompt identified by <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The prompt message.</param>
        /// <returns>A reference to the current <see cref="SmartConsole" /> instance.</returns>
        protected SmartConsole RecallResponse(string message)
        {
            if (_history.ContainsKey(message))
                _outstream.Write($"[{_history[message].Response}] ");
            return this;
        }

        /// <summary>
        /// Adds the specified action to the collection of handlers that listen
        /// for the <see cref="Console.CancelKeyPress"/> event, which is 
        /// automatically added if none exists. You must explicitly unsubscribe
        /// from the <see cref="Console.CancelKeyPress"/> event to stop 
        /// receiving notifications.
        /// </summary>
        /// <param name="action">The action to add to the collection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        protected virtual void AddCancelAction(Action<SmartConsole, ConsoleCancelEventArgs> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (_cancelActions == null)
            {
                lock (this)
                {
                    if (_cancelActions == null)
                    {
                        Console.CancelKeyPress += OnCancelKeyPress;
                        _cancelActions = new HashSet<Action<SmartConsole, ConsoleCancelEventArgs>>();
                    }
                }
            }

            _cancelActions.Add(action);
        }

        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            CancelRequested = true;

            // simulate a multi-cast delegate to invoke the list of actions
            foreach (var action in _cancelActions)
            {
                action.Invoke(this, e);
            }
        }

        private static bool YesDefault(string input, params string[] defaultResponses)
        {
            if (defaultResponses == null || defaultResponses.Length == 0)
                defaultResponses = DefaultYesResponses;

            foreach (var answer in defaultResponses)
            {
                if (string.IsNullOrWhiteSpace(input) || 
                    string.Equals(input, answer, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool NoDefault(string input, params string[] defaultResponses)
        {
            if (defaultResponses == null || defaultResponses.Length == 0)
                defaultResponses = DefaultNoResponses;

            // if input contains any of the default responses, then the 
            // answer is 'No', which resolves as a 'true' outcome
            return defaultResponses.Any(answer => 
                string.IsNullOrWhiteSpace(input) || 
                string.Equals(input, answer, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}
