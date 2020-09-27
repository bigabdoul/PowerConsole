using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PowerConsole.Test
{
    class Program
    {
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
                .RestoreForegroundColor();

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
                var methods1 = typeof(Demos).GetMethods(BindingFlags.Public | BindingFlags.Static);
                var methods2 = typeof(Calculator).GetMethods(BindingFlags.Public | BindingFlags.Static);
                var m1Length = methods1.Length;
                var m2Length = methods2.Length;
                var methodCount = m1Length + m2Length;
                var runDemos = true;

                while (runDemos)
                {
                    var index = 1;
                    runDemos = console.WriteLine()
                        .WriteLines(methods1.Select(m => $"\t{index++}: {m.Name}"))
                        .WriteLines(methods2.Select(m => $"\t{index++}: {m.Name}"))
                        .SetResponse<int>($"\n\tSelect a demo [1 - {methodCount}]: ", input => index = input,
                            validator: input => input > 0 && input <= methodCount,
                            validationMessage: "\tInvalid demo number. Try again: ")
                        .Then(() =>
                        {
                            _ = index <= m1Length ?
                                methods1[index - 1].Invoke(null, null) :
                                methods2[index - m1Length - 1].Invoke(null, null);
                        })
                        .PromptYes("\n\n\tRun another demo? (Y/n) ");
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
        }
    }
}
