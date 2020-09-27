using System;
using System.Linq;

namespace PowerConsole.Test
{
    class Calculator
    {
        private const string POSITIVE_REQUIRED = "Value must be greater than or equal to 0: ";
        private static readonly string[] Operators = new[] { "a", "s", "m", "d" };

        public static SmartConsole Calculate()
        {
            const int LINE_LEN = 30;
            const string NUMBER_ERROR = "This is not valid input. Please enter an integer value: ";
            const string ENTER_OR_QUIT = "Press Enter to continue, or any other non white space key and Enter to quit: ";

            // Display console window title as the Console Calculator Demo.
            var console = SmartConsole.Default.SetTitle("\nConsole Calculator Demo")
                .WriteInfo("\nWelcome to Console Calculator\n").Repeat('-', LINE_LEN);

            do
            {
                // Ask the user to type the first number.
                var num1 = console
                    .WriteLine() // Friendly linespacing.
                    .GetResponse<float>("Type a number, and then press Enter: ", NUMBER_ERROR);

                // Ask the user to type the second number.
                var num2 = console.GetResponse<float>("Type another number: ", NUMBER_ERROR);

                // Ask the user to choose an option.
                console.WriteLines("Choose an option from the following list:",
                    "\ta - Add",
                    "\ts - Subtract",
                    "\tm - Multiply",
                    "\td - Divide");

                try
                {
                    // This line is perfectly safe as long as the input validation is correct
                    var result = DoOperation(num1, num2, console.GetResponse("Your option? ", validator: input => _ValidateInput(input)));

                    console.WriteLine("Your result: {0:0.##}\n", result).RepeatLine('-', LINE_LEN);
                }
                catch (Exception ex)
                {
                    console.WriteError(ex);
                }

                // options and division validator
                bool _ValidateInput(string input)
                {
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        // first, validate the option
                        if (Operators.Contains(input, StringComparer.OrdinalIgnoreCase))
                        {
                            // option ok, now check if we can safely devide by the second number
                            if (num2 != 0F || !string.Equals(input, "d", StringComparison.OrdinalIgnoreCase))
                                return true;
                            else
                                console.WriteError("Cannot devide by 0: ");
                        }
                        else
                        {
                            console.WriteError("Choose a valid option: ");
                        }
                    }
                    return false;
                }
            } while (console.PromptYes(ENTER_OR_QUIT, defaultResponses: string.Empty));
            
            return SmartConsole.Default;
        }

        public static double SumOf2Numbers()
        {
            var result = 0d;
            SmartConsole.Default.WriteInfo("\nCalculate the sum of two numbers\n\n")
                .SetResponse<double>("Input number1: ", input => result = input)
                .SetResponse<double>("Input number2: ", input => result += input) // add previous result
                .WriteLine("\nResult: {0}", result);
            return result;
        }

        public static double RectangleArea()
        {
            var result = 0d;

            SmartConsole.Default.WriteInfo("\nCalculate the area of a rectangle\n\n")
                .SetResponse<double>("Please write the length of your rectangle: ", input => result = input, validator: IsPositive, POSITIVE_REQUIRED)
                .SetResponse<double>("Please write the width of your rectangle: ", input => result *= input, validator: IsPositive, POSITIVE_REQUIRED)
                .WriteLine("The area of rectangle : {0}", result);
            return result;
        }

        public static (double area, double perimeter) CircleAreaAndPerimeter()
        {
            double radius = 0d, perimeter = 0d, area = 0d;
            return SmartConsole.Default.WriteInfo("\nCalculate the area and perimeter of a circle\n\n")
                .SetResponse<double>("Please write the radius of your circle : ", input => radius = input, IsPositive, POSITIVE_REQUIRED)
                .Then(() => { perimeter = 2 * Math.PI * radius; area = Math.PI * Math.Pow(radius, 2); })
                .RepeatLine('=', 45)
                .WriteLines($"The perimeter of your circle is: {perimeter}", $"The area of your circle is: {area}")
                .Result(() => (area, perimeter));
        }

        public static void PrintPrimeNumbersInInterval()
        {
            int num1 = 0, num2 = 0, sayac = 0;
            SmartConsole.Default.WriteInfo("\nPrint prime number in a positive interval\n\n")
                .SetResponse<int>("Enter lower range: ", input => num1 = input, IsPositive, POSITIVE_REQUIRED)
                .SetResponse<int>("Enter upper range: ", input => num2 = input, validator: input => input > num1, "Upper range must be greater than lower value: ")
                .WriteLine("Prime numbers between {0} and {1} are: ", num1, num2)
                .Then(console =>
                {
                    // print prime numbers
                    for (int i = num1; i < num2; i++)
                    {
                        sayac = 0;
                        if (i > 1)
                        {
                            for (int j = 2; j < i; j++)
                            {
                                if (i % j == 0)
                                {
                                    sayac = 1;
                                    break;
                                }
                            }
                            if (sayac == 0)
                            {
                                console.Write($"{i}, ");
                            }
                        }
                    }
                }).WriteLine();
        }

        internal static double DoOperation(double num1, double num2, string op)
        {
            // Use a switch statement to do the math.
            switch (op)
            {
                case "a":
                    return num1 + num2;
                case "s":
                    return num1 - num2;
                case "m":
                    return num1 * num2;
                case "d":
                    return num1 / num2;
                default:
                    break;
            }
            return double.NaN;
        }

        internal static bool IsPositive(int input) => input >= 0;
        internal static bool IsPositive(double input) => input >= 0;
    }
}
