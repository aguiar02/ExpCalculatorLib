using System.Reflection;
using ExpCalculatorLib.Exceptions;

namespace ExpCalculatorLib
{
    public class MethodInvoker
    {
        public MethodInfo Method { get; private set; }
        public object TargetObject { get; private set; }

        public string Description { get; set; }

        public string Hint { get; set; }

        public MethodInvoker(MethodInfo methodInfo, object target)
        {
            this.TargetObject = target;
            this.Method = methodInfo;
        }

        public MethodInvoker(MethodInfo methodInfo, string description, string hint)
        {
            if (!methodInfo.IsStatic)
                throw new ExpressionException("O método deve ser estático.");
            this.Method = methodInfo;
            this.Description = description;
            this.Hint = hint;
        }

        public object Invoke(ParsingContext context, params object[] parametros)
        {
            if (Method.IsGenericMethodDefinition)
                return context.GetResolvedMethodInfo(Method).Invoke(TargetObject, parametros);
            return Method.Invoke(TargetObject, parametros);
        }
    }
}
