using System;

namespace PowerConsole.Test
{
    // let's make this class static because we only need one method
    static class MortgageCalculator
    {
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