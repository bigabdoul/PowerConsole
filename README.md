# PowerConsole

Makes strongly-typed user input collection and validation through the Console
easier, and adds more useful utility methods to it.

The `SmartConsole` class is the blueprint that provides the core functionality.

## Getting started

```C#
using System;
using PowerConsole;

namespace PowerConsoleTest
{
    class Program
    {
        internal static readonly SmartConsole MyConsole = SmartConsole.Default;

        static void Main()
        {
            MyConsole.SetForegroundColor(ConsoleColor.Green)
                .WriteLine("Welcome to the Power Console Demo!\n")
                .WriteLine($"Project:\t {nameof(PowerConsole)}")
                .WriteLine("Version:\t 1.0.0")
                .WriteLine("Description:\t Makes strongly-typed user input collection and validation through ")
                .WriteLine("\t\t a console easier. This is a Console on steroids.")
                .WriteLine("Author:\t\t Abdourahamane Kaba")
                .WriteLine("License:\t MIT")
                .WriteLine("Copyright:\t (c) 2020 Karfamsoft\n")
                .RestoreForegroundColor();

            if (!MyConsole.PromptNo("Would you like to define a specific culture for this session? (yes/No) "))
            {
                // try to set user-defined culture
                try
                {
                    MyConsole.Write("\nEnter a culture name to use, like ")
                        .WriteList(ConsoleColor.Blue, ", ", "en", "en-us", "fr", "fr-ca", "de").WriteLine(", etc.")
                        .WriteLine("Leave empty if you wish to use your computer's current culture.\n")
                        .Write("Culture name: ")
                        .SetResponse(culture => MyConsole.SetCulture(culture));
                }
                catch (System.Globalization.CultureNotFoundException ex)
                {
                    MyConsole.WriteError(ex);
                }
            }

            PlayFizzBuzz();
        }

        private static void PlayFizzBuzz()
        {
            MyConsole.WriteInfo("\nWelcome to FizzBuzz!\nTo quit the loop enter 0.\n\n");

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

            Goodbye("\nThank you for playing FizzBuzz. Goodbye!\n\n");
        }

        
        private static void Goodbye(string message = null)
        {
            // say goodbye and clean up
            MyConsole.WriteSuccess(message ?? "Goodbye!\n\n")
                .Repeat(15, "{0}{1}", "o-", "=-")
                .WriteLine("o\n")
                .Clear();
        }
    }
}
```

