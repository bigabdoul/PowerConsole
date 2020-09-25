# PowerConsole

Makes strongly-typed user input collection and validation through the Console
easier, and adds more useful utility methods to it.

## Introduction

Creating console applications has never been easier and more fun than with the
fresh new **PowerConsole** project. And let me say this straight away: No, console
apps are not a thing of the past! In fact, they're becoming more and more 
popular thanks to command line tools like dotnet, git, npm (backed by Node.js),
and so on.

## What is PowerConsole?

PowerConsole is a .NET Standard project that makes strongly-typed user input
collection and validation through a console easier. Through the **SmartConsole** 
class, it enhances the traditional system's Console by encapsulating complex 
and redundant processing logic, and defining a bunch of utility functions. 
Check it out on Github at https://github.com/bigabdoul/PowerConsole or quickly 
create a new .NET Core Console application and install the package from NuGet:

`Install-Package PowerConsole -Version 1.0.0`



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

## Background

I initially started this project as a Java package using JDK 14 but quickly 
realized that this language has very limited support for **TRUE** generic 
programming. So I copy-pasted my code into a new .NET Standard project, started
refactoring it there, and eventually ended up adding more and more functions 
to the class named SmartConsole.

## Why PowerConsole?

Almost every beginner tutorial for any server-side programming language makes 
use of the console to write out on a standard input/output screen the famous 
"Hello World!". Why? Because console applications are a great choice for 
learners: they're easy to create and fast to execute. However, when a console 
app requires user interaction such as prompting for their name and password, 
or collecting and validating their inputs against a predefined set of rules, 
then developing efficiently a console app becomes a real pain in the back.

"Great choice for learners" doesn't mean that it's not meant for experienced
developers. In fact, developing advanced console applications are reserved for 
those who actually know what they're doing. So it makes perfectly sense to have
a tool that allows them to be more productive.

Developing efficient console applications is a daunting task. This is mostly 
due to the fact that the Console class in .NET's System namespace is a static 
class by design and as such does not retain state, thus making it difficult to 
collect a set of related data. That's where SmartConsole steps in: it 
encapsulates methods and data required to build an interactive console 
application that seamlessly enforces complex business logic.

## Use cases

Let's quickly walk through two use cases to illustrate what problems 
**PowerConsole** solves and what extra values it adds.

### Use case #1: Yes or No?

This is a typical prompt in console apps: a question that requires a logical 
answer (yes or no)  which resolves as a boolean (true or false) value.

For instance: `Would you like to define a specific culture for this session? (yes/No)`

The above prompt expects a yes or a No response. If the response is empty
(when the user presses the Enter key without typing anything) then the answer 
is considered a "No". Why? Because the first letter of the "No" word is capitalized.
The user can enter either:

- a case-insensitive n (or N, it doesn't matter),
- a case-insensitive no (or No or NO, it makes no difference),
- or just press the Enter key.

Any response different from all of the above is considered a Yes.

How would one collect such an input? In C# it would ressemble a variation of 
the following:

```C#
Console.WriteLine("Would you like to define a specific culture for this session? (yes/No) ");

string answer = Console.ReadLine();

if (string.IsNullOrWhiteSpace(answer) ||
    string.Equals(answer, "n", StringComparison.OrdinalIgoreCase) ||
    string.Equals(answer, "no", StringComparison.OrdinalIgoreCase))
{
    // do stuff when the answer is no...
}
else
{
    // do other stuff when the answer is affirmative...
}
```

With *SmartConsole* we can write:

```C#
if (SmartConsole.Default.PromptNo("Would you like to define a specific culture for this session? (yes/No) "))
{
    // do stuff when the answer is negative...
}
else
{
    // do other stuff when the answer is affirmative...
}
```

As we can see, the amount of code required to do the same thing is less than 
what we were used to write: asking a question and getting the response is a 
one-liner.

### Use case #2: Type conversion with graceful error handling

Knowing that `Console.ReadLine()` (not surprisingly) returns a string (a series
of Unicode characters), it becomes obvious that we as developers must convert 
the line read into the desired type that a given piece of code expects. For 
instance, a mortgage calculator requires a few variables such as the principal 
**P** (or amount borrowed), the number of monthly payments **n**, and the annual 
interest rate **R**. The monthly down payment **M** is determined by the formula:

```C#
var r = R / 100 / 12;
var M = P * r * Math.Pow(1 + r, n) / (Math.Pow(1 + r, n) - 1);
```

Although this formula looks sophisticated it's not what we want to focus on 
here. What's more important for us to know are the business requirements:

  - The principal P must be a decimal number at least 1,000.00 (one thousand) 
    and not exceed 1,000,000.00 (one million).
  - The number of monthly payments n must be a whole number greater than zero
    and not exceed 360 (30 years).
  - The annual interest rate R, expressed as a percentage, must be a 
    single-precision floating point number greater than zero and not exceed 30.

With these constraints clearly defined, let's take a look at how we can use 
*SmartConsole* to solve this problem. Open your preferred code editor, create a
new .NET Core Console project named *MortgageCalculatorApp*, and a file named 
*MortgageCalculator.cs* with the following content:

```C#
using System;
using PowerConsole;

namespace MortgageCalculatorApp
{
    public static class MortgageCalculator
    {
        // for the sake of reusability, let's gather all inputs from within this class
        internal static readonly SmartConsole MyConsole = SmartConsole.Default;

        public static void GetInputAndCalculate()
        {
            MyConsole.WriteInfo("Welcome to Mortgage Calculator!\n\n");

            var principal = MyConsole.GetResponse<decimal>("Principal: ",
                validationMessage: "Enter a number between 1000 and 1,000,000: ",
                validator: input => input >= 1000M && input <= 1000000M);

            var numPayments = MyConsole.GetResponse<short>("Number of payments: ",
                "Please enter a whole number between 1 and 360: ",
                input => input >= 1 && input <= 360);

            var rate = MyConsole.GetResponse<float>("Annual interest rate: ",
                "The interest rate must be > 0 and <= 30.",
                input => input > 0F && input <= 30F);

            var mortgage = Calculate(principal, numPayments, rate);

            MyConsole
                .Write($"The monthly down payment is: ")
                .WriteInfo($"{mortgage:C}\n");
        }

        public static decimal Calculate(decimal principal, short numberOfPayments, float annualInterestRate)
        {
            byte PERCENT = 100;
            byte MONTHS_IN_YEAR = 12;
            float r = annualInterestRate / PERCENT / MONTHS_IN_YEAR;
            double factor = Math.Pow(1 + r, numberOfPayments);
             
            return principal * new decimal(r * factor / (factor - 1));
        }
    }
}
```

Change (or create) the *Program.cs* file in such a way to ressemble the 
following:

```C#
namespace MortgageCalculatorApp
{
    class Program
    {
        static void Main()
        {
            MortgageCalculator.GetInputAndCalculate();
        }
    }
}
```

That's it! Really, that's all! Everything from user input collection, casting 
(type conversion), and validation is handled internally. Every time the user 
enters an invalid entry, an appropriate error message is displayed and she/he 
will be given another opportunity to enter an acceptable value.
