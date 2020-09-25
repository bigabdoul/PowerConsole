﻿using System;
using System.IO;

namespace PowerConsole.Test
{
    internal static class Demos
    {
        static readonly SmartConsole MyConsole = SmartConsole.Default;

        internal static void ReadPassword()
        {
            try
            {
                var mode = 0;
                var validationError = "Unknown mode! Please enter 1 or 2 to continue, or 0 to quit: ";

                void _Setter(int input) => mode = input;
                static bool _Validator(int input) => input > -1 && input < 3;

                var username = MyConsole.WriteInfo("\nHi there! This is the password reader demo.\n\n")
                    .WriteLine("Input modes:")
                    .WriteLine("  1: Secured")
                    .WriteLine("  2: Obscured")
                    .WriteLine("  0: Quit")
                    .Write("\nChoose a mode: ")
                    .SetResponse<int>(_Setter, _Validator, validationError)
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

            Goodbye();
        }

        internal static void FizzBuzz()
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

        internal static void CalculateMortgage()
        {
            MyConsole.WriteInfo("\nWelcome to Mortgage Calculator!\n\n");

            var principal = MyConsole.GetResponse<decimal>("Principal: ",
                validationMessage: "Enter a number between 1000 and 1,000,000: ",
                validator: input => input >= 1000M && input <= 1000000M);

            var numPayments = MyConsole.GetResponse<short>("Number of payments: ",
                "Please enter a whole number between 1 and 360: ",
                input => input >= 1 && input <= 360);

            var rate = MyConsole.GetResponse<float>("Annual interest rate: ",
                "The interest rate must be > 0 and <= 30.",
                input => input > 0F && input <= 30F);

            var mortgage = MortgageCalculator.Calculate(principal, numPayments, rate);

            MyConsole
                .Write($"The monthly down payment is: ")
                .WriteInfo($"{mortgage:C}\n\n");

            Goodbye();
        }

        internal static void CollectUserInfo()
        {
            MyConsole.WriteInfo("\nWelcome to the user info collection demo!\n");

            // by simply providing a validation message, we force 
            // the input not to be empty or white space only (and to
            // be of the appropriate type if different from string)
            var nameValidationMessage = "Your full name is required: ";

            bool validateAge(int input) => input >= 5 && input <= 100;
            var ageErrorMessage = "Age (in years) must be a whole number from 5 to 100: ";

            // notice the 'promptId' parameter: they'll allow us 
            // strongly-typed object instantiation and property mapping
            while
            (
                MyConsole.Store() // forces all subsequent prompts to be stored
                    .Prompt("\nEnter your full name: ", "Full Name:", validationMessage: nameValidationMessage, promptId: nameof(UserInfo.FullName))
                    .Prompt<int>("How old are you? ", "Plain Age:", validationMessage: ageErrorMessage, validator: validateAge, promptId: nameof(UserInfo.Age))
                    .Prompt("In which country were you born? ", "Birth Country:", promptId: nameof(UserInfo.BirthCountry))
                    .Prompt("What's your preferred color? ", "Preferred Color:", promptId: nameof(UserInfo.PreferredColor))
                    .WriteLine()
                    .WriteLine("Here's what you've entered: ")
                    .WriteLine()
                    .Recall(prefix: "> ")
                    .WriteLine()
                    .Store(false) // stops storing prompts

                    // give the user an opportunity to review and correct their inputs
                    .PromptYes("Is that correct? (Y/n) ") == false
            )
            {
                // nothing else required within this while loop
            }

            MyConsole.WriteInfo("Thank you for providing your details.\n");

            if (!MyConsole.PromptNo("Do you wish to save them now? (y/N) "))
            {
                SaveUserDetails();
            }

            Goodbye();
        }

        static void SaveUserDetails()
        {
            // now process the collected data; obviously, you'll have to 
            // save it to a useful and secure store somehow...
            MyConsole
                .WriteLine()
                .Repeat('-', 70)
                .WriteWarning("CAUTION: EXISTING FILES WILL BE OVERRIDDEN WITHOUT FURTHER NOTICE!\n")
                .Repeat('-', 70)
                .WriteLine();

            // but for the sake of simplicity, we'll save them to disk
            var filename = MyConsole.GetResponse("File name (full path); leave empty to use current directory: ");

            string path;

            if (string.IsNullOrWhiteSpace(filename))
            {
                path = Path.Combine(Environment.CurrentDirectory, $"PromptConsoleLog");
                filename = $"{path}-UserInfo.txt";
            }
            else
            {
                path = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));
            }

            var historyFileName = $"{path}-History.txt";

            // create a UserInfo object from the previously-collected
            // AND identified prompts (created with the 'promptId' parameter)
            var user = MyConsole.CreateObject<UserInfo>();

            File.WriteAllText(filename, user.ToString());

            MyConsole
                // we can also save all prompts with their respective responses to a file
                .WriteFile(historyFileName)
                .WriteLine($"\nYour details have been saved to:\n{filename}\n\nand:\n{historyFileName}\n");
        }

        private static void Goodbye(string message = null)
        {
            // say goodbye and clean up
            MyConsole.WriteSuccess(message ?? "Goodbye!\n\n")
                .Repeat(15, "{0}{1}", "o-", "=-")
                .WriteLine("o\n")

                // Removes all prompts from the prompt history;
                // Does NOT clear the console buffer and corresponding 
                // console window of display information.
                .Clear();
        }
    }
}
