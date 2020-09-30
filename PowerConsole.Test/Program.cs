using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PowerConsole.Test
{
    class Program
    {
        const BindingFlags PUBLIC_STATIC = BindingFlags.Public | BindingFlags.Static;

        static void Main()
        {
            // get the default console
            var console = SmartConsole.Default.SetTitle("Power Console Demo")
                .SetForegroundColor(ConsoleColor.Green)
                .WriteLines("\tWelcome to the Power Console Demo!\n", 
                    $"\tProject:\t {nameof(PowerConsole)}", 
                    "\tVersion:\t 1.0.0", 
                    "\tDescription:\t Makes strongly-typed user input collection and validation through ",
                    "\t\t\t a console easier. This is a Console on steroids.",
                    "\tAuthor:\t\t Abdourahamane Kaba",
                    "\tLicense:\t MIT",
                    "\tCopyright:\t (c) 2020 Karfamsoft\n")
                .RestoreForegroundColor()

                // SetTimeout is called only once after the provided delay and
                // is automatically removed by the TimerManager class
                .SetTimeout(e =>
                {
                    // this action is called back after 10 seconds; the name
                    // of the time out is useful should we want to clear it
                    // before this action gets executed
                    e.Console.Write("\n\t").WriteError("A timeout occured!");
                    
                    // the next statement will make the current instance of 
                    // SmartConsole throw an exception on the next prompt attempt
                    // sender.CancelRequested = true;

                }, millisecondsDelay: 10000d, name: "SampleTimeout")

                .SetInterval(e =>
                {
                    if (e.Ticks == 1)
                    {
                        // write a new line
                        e.Console.WriteInfo($"\n\tInterval tick: {e.Ticks}");
                    }
                    else
                    {
                        // overwrite the previous output (write on the same line)
                        e.Console.WriteInfo($"\r\tInterval tick: {e.Ticks}");
                    
                        if (e.Ticks > 4)
                        {
                            // instructs the timer manager to clean it up
                            e.DisposeTimer();

                            // we could remove the previous timeout:
                            // e.Console.ClearTimeout("SampleTimeout");
                        }
                    }
                }, millisecondsInterval: 1000, "EverySecond")

                // we can add as many timers as we want (or the computer's resources permit)
                .SetInterval(e =>
                {
                    if (e.Ticks == 5)
                    {
                        e.DisposeTimer();
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine($"Second timer tick: {e.Ticks}");
                    }
                }, 2000)

                // this event handler intercepts the 'CTRL+C' and 'CTRL+Break'
                // key combinations; normally, this abruptly terminates the 
                // application but with this handler you are given a chance
                // to clean up resources
                .OnCancel((sender, e) =>
                {
                    sender.WriteWarning("\n\tTerminating the application...\n");

                    // e.Cancel == true leaves the app running, otherwise it 
                    // terminates immediately;
                    e.Cancel = true;

                    // do whatever clean up is required by the app...
                    
                    // and then explicitly quit with Environment.Exit(int);
                    // if you don't quit explicitly, an OperationCanceledException
                    // is thrown on the next prompt attempt
                    // Environment.Exit(1);
                })
                .Catch((sender, e) =>
                {
                    sender.Write("\n\t")
                        .WriteError(e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString()))
                        .WriteLine();

                    Environment.Exit(1);
                });

            if (!console.PromptNo("\tWould you like to define a specific culture for this session? (yes/No) "))
            {
                // try to set user-defined culture
                console.Write("\n\tEnter a culture name to use, like ")
                    .WriteList(ConsoleColor.Blue, ", ", "en", "en-us", "fr", "fr-ca", "de").WriteLine(", etc.")
                    .WriteLine("\tLeave empty if you wish to use your computer's current culture.\n")
                    .Write("\tCulture name: ")

                    // try to set the culture and eventually catch an instance of
                    // CultureNotFoundException; other error types will be rethrown
                    .TrySetResponse<CultureNotFoundException>(
                        culture => console.SetCulture(culture),
                        error => console.WriteError(error));
            }

            if (console.PromptYes("\n\tWould you like to run a specific demo? (Y/n) "))
            {
                // dynamically discover available method; all static public methods in
                // the Demos and Calculator classes are considered as demo methods
                var methods1 = typeof(Demos).GetMethods(PUBLIC_STATIC);
                var methods2 = typeof(Calculator).GetMethods(PUBLIC_STATIC);
                var m1Length = methods1.Length;
                var methodCount = m1Length + methods2.Length;
                var runDemos = true;

                while (runDemos)
                {
                    var index = 1;
                    try
                    {
                        runDemos = console.WriteLine()
                            .WriteLines(methods1.Select(m => $"\t{index++}: {m.Name}"))
                            .WriteLines(methods2.Select(m => $"\t{index++}: {m.Name}"))
                            .Write("\t").RepeatLine('-', 45)
                            .WriteLine("\t0: QUIT APPLICATION")
                            .SetResponse<int>($"\n\tSelect a demo [1 - {methodCount}]: ", input => index = input,
                                validator: input => input >= 0 && input <= methodCount,
                                validationMessage: "\tInvalid demo number. Try again: ")
                            .ContinueWhen(index > 0)
                            .Then(() =>
                            {
                                _ = index <= m1Length ?
                                    methods1[index - 1].Invoke(null, null) :
                                    methods2[index - m1Length - 1].Invoke(null, null);
                            })
                            .PromptYes("\n\tRun another demo? (Y/n) ");
                    }
                    catch (ContinuationException)
                    {
                        console.SetForegroundColor(ConsoleColor.Yellow);
                        if (console.PromptYes("\tQuit application? (Y/n) "))
                            break;
                        console.RestoreForegroundColor();
                    }
                }
            }
            else
            {
                // PromptNo: the default response is no, which resolves as false
                if (!console.PromptNo("\n\tRun user info collection demo? (y/N) "))
                    Demos.CollectUserInfo();

                // PromptYes: the default response is yes, which resolves as true
                if (console.PromptYes("\tRun password reader demo? (Yes/no) "))
                    Demos.ReadPassword();

                if (console.PromptYes("\tRun FizzBuzz demo? (Y/n) "))
                    Demos.FizzBuzz();

                if (console.PromptYes("\tRun Mortgate Calculator demo? (Y/n) "))
                    Demos.CalculateMortgage();

                if (console.PromptYes("\tRun simple calculator demo? (Y/n) "))
                    Demos.SimpleCalculator();
            }

            console.WriteLine().RestoreForegroundColor();
        }
    }
}
