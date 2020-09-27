using System;

namespace PowerConsole.Test
{
    // let's make this class static because we only need one method
    public static class MortgageCalculator
    {
        public static SmartConsole Process()
        {
            var console = SmartConsole.Default.SetTitle("Mortgage Calculator Demo")
                .WriteInfo("\nWelcome to Mortgage Calculator!\n\n");

            var principal = console.GetResponse<decimal>("Principal: ",
                validationMessage: "Enter a number between 1000 and 1,000,000: ",
                validator: input => input >= 1000M && input <= 1000000M);

            var numPayments = console.GetResponse<short>("Number of payments: ",
                "Please enter a whole number between 1 and 360: ",
                input => input >= 1 && input <= 360);

            var rate = console.GetResponse<float>("Annual interest rate: ",
                "The interest rate must be > 0 and <= 30: ",
                input => input > 0F && input <= 30F);

            var mortgage = Calculate(principal, numPayments, rate);

            console.Write($"The monthly down payment is: ").WriteInfo($"{mortgage:C}\n\n");
            return console;
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