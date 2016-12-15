using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpCalculatorLib
{
    public class Parameter
    {
        private Parameter()
        {

        }

        public static Parameter NewParameter(Type type)
        {
            return new Parameter
            {
                ParameterType = type
            };
        }

        public static Parameter NewParameter(Type type, string description)
        {
            return new Parameter
            {
                ParameterType = type,
                Description = description
            };
        }

        public static Parameter NewParameter(Type type, string description, bool isPrivate)
        {
            return new Parameter
            {
                ParameterType = type,
                Description = description,
                IsPrivate = isPrivate
            };
        }

        public static Parameter NewParameter<T>(T value)
        {
            return new Parameter
            {
                ParameterType = typeof(T),
                ParameterValue = value,
            };
        }

        public static Parameter NewParameter<T>(T value, string description)
        {
            return new Parameter
            {
                ParameterType = typeof(T),
                ParameterValue = value,
                Description = description
            };
        }

        public static Parameter NewParameter<T>(T value, string description, bool isPrivate)
        {
            return new Parameter
            {
                ParameterType = typeof(T),
                ParameterValue = value,
                Description = description,
                IsPrivate = isPrivate
            };
        }

        public Type ParameterType { get; private set; }
        public object ParameterValue { get; set; }
        public bool IsPrivate { get; set; }

        public string Description { get; private set; }
    }
}
