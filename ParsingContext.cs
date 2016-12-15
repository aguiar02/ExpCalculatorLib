using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExpCalculatorLib
{
    public class ParsingContext
    {
        public bool IsPrivateContext { get; set; }

        public ParsingContext()
        {
            this.Parameters = new Dictionary<string, Parameter>();
            this.Functions = new Dictionary<string, MethodInvoker>();
            this.GenericArgs = null;
        }

        public ParsingContext(ParsingContext context)
            : this(context, false)
        {
        }

        public ParsingContext(ParsingContext context, bool herdaGenericArgs)
            : this()
        {
            foreach (var kvp in context.Parameters)
                Parameters.Add(kvp.Key, kvp.Value);
            foreach (var kvp in context.Functions)
                Functions.Add(kvp.Key, kvp.Value);

            if (!herdaGenericArgs || context.GenericArgs == null)
                this.GenericArgs = new Dictionary<string, Type>();
            else
                this.GenericArgs = context.GenericArgs;
            this.IsPrivateContext = context.IsPrivateContext;
        }

        public Dictionary<string, Parameter> Parameters { get; private set; }
        public Dictionary<string, MethodInvoker> Functions { get; private set; }
        internal Dictionary<string, Type> GenericArgs { get; private set; }

        public void ResolveType(Type genericType, Type actualType)
        {

            if (genericType.IsGenericParameter)
                GenericArgs[genericType.Name] = actualType;

            if (genericType.IsGenericType)
            {
                if (genericType.GetGenericTypeDefinition().Equals(typeof(Expression.Expression<>)))
                {
                    ResolveType(genericType.GetGenericArguments()[0], actualType);
                    return;
                }

                Type[] genArgs = genericType.GetGenericArguments();
                Type[] actArgs = actualType.GetGenericArguments();

                if (genArgs.Length != actArgs.Length)
                    return;
                for (int i = 0; i < genArgs.Length; i++)
                {
                    ResolveType(genArgs[i], actArgs[i]);
                }
            }
        }

        public Type GetResolvedType(Type genericType)
        {
            if (genericType.IsGenericParameter)
                return GenericArgs[genericType.Name];

            if (genericType.IsGenericType)
            {
                Type[] genArgs = genericType.GetGenericArguments();
                Type[] actArgs = new Type[genArgs.Length];
                for (int i = 0; i < genArgs.Length; i++)
                {
                    actArgs[i] = GetResolvedType(genArgs[i]);
                }
                return genericType.GetGenericTypeDefinition().MakeGenericType(actArgs.ToArray());
            }
            else
                return genericType;
        }

        internal MethodInfo GetResolvedMethodInfo(MethodInfo method)
        {
            return method.MakeGenericMethod(method.GetGenericArguments().Select(gma => this.GetResolvedType(gma)).ToArray());
        }
    }
}
