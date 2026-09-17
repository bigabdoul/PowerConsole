using System;

namespace PowerConsole
{
    /// <summary>
    /// Provides categorized values for <see cref="System.TypeCode"/> values.
    /// </summary>
    public enum TypeCategory
    {
        /// <summary>
        /// Boolean type code.
        /// </summary>
        Boolean,

        /// <summary>
        /// DateTime type code.
        /// </summary>
        DateTime,

        /// <summary>
        /// Integral number type codes, such as <see cref="byte"/>, 
        /// <see cref="short"/>, <see cref="int"/>, <see cref="long"/>, 
        /// and their respective unsigned equivalents.
        /// </summary>
        IntegralNumber,

        /// <summary>
        /// Floating point number type codes, such as <see cref="decimal"/>, 
        /// <see cref="double"/>, and <see cref="float"/>.
        /// </summary>
        FloatingPointNumber,

        /// <summary>
        /// Other type codes, such as <see cref="string"/>, <see cref="object"/>, 
        /// <see cref="DBNull"/>, and a null reference.
        /// </summary>
        Other
    }
}
