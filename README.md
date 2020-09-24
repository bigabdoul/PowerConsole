# PowerConsole

Makes strongly-typed user input collection and validation through the Console
easier, and adds more useful utility methods to it.

The `SmartConsole` class is the blueprint that provides the core functions.

## Getting started

```C#
using System;

namespace PowerConsole.Test
{
    class Program
    {
        static void Main()
        {
            var console = SmartConsole.Default
                .SetForegroundColor(ConsoleColor.Green)
                .WriteLine("Welcome to the Power Console Demo!\n")
                .WriteLine($"Project:\t {nameof(PowerConsole)}")
                .WriteLine("Version:\t 1.0.0")
                .WriteLine("Description:\t Makes strongly-typed user input collection and validation through ")
                .WriteLine("\t\t a console easier. This is a Console on steroids.")
                .WriteLine("Author:\t\t Abdourahamane Kaba")
                .WriteLine("License:\t MIT")
                .WriteLine("Copyright:\t (c) 2020 Karfamsoft\n")
                .RestoreForegroundColor();

            if (!console.PromptNo("Would you like to define a specific culture for this session? (yes/No) "))
            {
                // try to set user-defined culture
                try
                {
                    console.Write("\nEnter a culture name to use, like ")
                        .WriteList(ConsoleColor.Blue, ", ", "en", "en-us", "fr", "fr-ca", "de").WriteLine(", etc.")
                        .WriteLine("Leave empty if you wish to use your computer's current culture.\n")
                        .Write("Culture name: ")
                        .SetResponse(culture => console.SetCulture(culture));
                }
                catch (System.Globalization.CultureNotFoundException ex)
                {
                    console.WriteError(ex);
                }
            }
        }
    }
}
```