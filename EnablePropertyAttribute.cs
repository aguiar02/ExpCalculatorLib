using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpCalculatorLib
{
    /// <summary>
    /// Atributo usado para 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class EnablePropertyAttribute : Attribute
    {
    }
}
