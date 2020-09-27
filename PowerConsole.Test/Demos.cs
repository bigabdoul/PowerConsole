namespace PowerConsole.Test
{
    internal static class Demos
    {
        static readonly SmartConsole MyConsole = SmartConsole.Default;

        public static void ReadPassword()
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

            MyConsole.GoodBye("\nThank you for playing FizzBuzz. Goodbye!\n\n");
        }

        public static void CollectUserInfo() => UserInfoCollector.Process().GoodBye();

        public static void CalculateMortgage() => MortgageCalculator.Process().GoodBye();

        internal static void SimpleCalculator() => Calculator.Process().GoodBye();

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
