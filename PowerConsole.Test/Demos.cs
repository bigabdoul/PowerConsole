﻿using System;

namespace PowerConsole.Test
{
    internal static class Demos
    {
        static readonly SmartConsole MyConsole = SmartConsole.Default;

        public static void RunTimers()
        {
            // CAUTION: SmartConsole is not thread safe!
            // Spawn multiple timers carefully when accessing
            // simultaneously members of the SmartConsole class.
            ConsoleColor.Blue.Write("\nWelcome to the Timers demo!\n")

                // SetTimeout is called only once after the provided delay and
                // is automatically removed by the TimerManager class
                .SetTimeout(e =>
                {
                    // this action is called back after 5.5 seconds; the name
                    // of the time out is useful should we want to clear it
                    // before this action gets executed
                    e.Console.Write("\n").WriteError("First timer: Time out occured after 5.5 seconds! " +
                        "Timer has been automatically disposed.\n");

                    // the next statement will make the current instance of 
                    // SmartConsole throw an exception on the next prompt attempt
                    // e.Console.CancelRequested = true;

                    // use 5500 or any other value not multiple of 1000 to 
                    // reduce write collision risk with the next timer
                }, millisecondsDelay: 5500, name: "SampleTimeout")

                .SetInterval(e =>
                {
                    if (e.Ticks == 1)
                    {
                        e.Console.WriteLine();
                    }

                    e.Console
                    .Write($"\rSecond timer tick: ", System.ConsoleColor.White)
                    .WriteInfo(e.TicksToSecondsElapsed());

                    if (e.Ticks > 4)
                    {
                        // we could remove the previous timeout:
                        // e.Console.ClearTimeout("SampleTimeout");
                    }

                }, millisecondsInterval: 1000)

                // we can add as many timers as we want (or the computer's resources permit)
                .SetInterval(e =>
                {
                    if (e.Ticks == 1 || e.Ticks == 3) // 1.5 or 4.5 seconds to avoid write collision
                    {
                        e.Console.WriteSuccess($"\nThird timer is {(e.Ticks == 1 ? "" : "still ")}active...\n");
                    }
                    else if (e.Ticks == 5)
                    {
                        e.Console.WriteWarning("\nThird timer is disposing...\n");

                        // doesn't dispose the timer
                        // e.Timer.Stop();

                        // clean up if we no longer need it
                        e.DisposeTimer();

                        ConsoleColor.DarkGreen.Write("Third timer was disposed.\n");
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine($"Third timer tick: {e.Ticks}");
                    }
                }, 1500)
                .Prompt("\nPress Enter to stop the timers: ")
                
                // makes sure that any remaining timer is disposed off
                .ClearTimers()

                .WriteSuccess("Timers cleared!\n");
        }

        public static void ReadPassword()
        {
            try
            {
                var mode = 0;
                var validationError = "Unknown mode! Please enter 1 or 2 to continue, or 0 to quit: ";

                void _Setter(int input) => mode = input;
                static bool _Validator(int input) => input > -1 && input < 3;

                var username = ConsoleColor.Blue.Write("\nHi there! This is the password reader demo.\n\n")
                    .WriteLine("Input modes:")
                    .WriteLine("  1: Secured")
                    .WriteLine("  2: Obscured")
                    .WriteLine("  0: Quit")
                    .Write("\nChoose a mode: ")
                    .SetResponse<int>(_Setter, _Validator, validationError)

                    // throws ContinuationException if condition is not met
                    .ContinueWhen(mode > 0)

                    // this gets executed only if the above condition (mode > 0) is true
                    .GetResponse("\nUser name: ");

                var pwd = mode == 1 ?
                        MyConsole.GetSecureInput("Secured Password: ") :
                        MyConsole.GetMaskedInput("Obscured Password: ");

                MyConsole.WriteLine()
                    .WriteLine($"\nYou have entered user name={username} and password={pwd}\n");
            }
            catch (ContinuationException)
            {
            }

            MyConsole.GoodBye();
        }

        public static void FizzBuzz()
        {
            ConsoleColor.Blue.Write("\nWelcome to FizzBuzz!\nTo quit the loop enter 0.\n\n");

            double? number;
            const string validationMessage = "Only numbers, please! ";

            while ((number = MyConsole.GetResponse<double?>("Enter a number: ", validationMessage)) != 0)
            {
                // Writes out:
                //  "FizzBuzz" if the input is divisible both by 3 and 5
                //  "Buzz" if divisible by 3 only
                //  "Fizz" if divisible by 5 only
                if (number == null) continue;

                if ((number % 3 == 0) && (number % 5 == 0))
                {
                    MyConsole.WriteLine("FizzBuzz");
                }
                else if (number % 3 == 0)
                {
                    MyConsole.WriteLine("Buzz");
                }
                else if (number % 5 == 0)
                {
                    MyConsole.WriteLine("Fizz");
                }
                else
                {
                    MyConsole.WriteLine(number);
                }
            }

            MyConsole.GoodBye("\nThank you for playing FizzBuzz. Goodbye!\n\n");
        }

        public static void CollectUserInfo() => UserInfoCollector.CollectUserInfo().GoodBye();

        public static void CalculateMortgage() => MortgageCalculator.CalculateMortgage().GoodBye();

        internal static void SimpleCalculator() => Calculator.Calculate().GoodBye();

        private static void GoodBye(this SmartConsole console, string message = null)
        {
            if (console == null)
                console = MyConsole;

            // say goodbye and clean up
            console.WriteSuccess(message ?? "Goodbye!\n\n")
                .Repeat(15, "{0}{1}", "o-", "=-")
                .WriteLine("o\n")

                // Removes all prompts from the prompt history;
                // Does NOT clear the console buffer and corresponding 
                // console window of display information.
                .Clear();
        }
    }
}
