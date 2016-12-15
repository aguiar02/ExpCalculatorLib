using System;
using System.Collections.Generic;
using System.Linq;
using ExpCalculatorLib.Expression;

namespace ExpCalculatorLib
{
    public static class Functions
    {
        public static T Se<T>(bool condicao, Expression<T> exp1, Expression<T> exp2)
        {
            if (condicao)
                return exp1.Eval();
            else
                return exp2.Eval();
        }

        public static DateTime Hoje()
        {
            return DateTime.Today;
        }

        public static bool Nao(bool condicao)
        {
            return !condicao;
        }

        public static bool ENulo(object valor)
        {
            return valor == null;
        }

        public static T SeErro<T>(Expression<T> expressaoOk, Expression<T> expressaoSeErro)
        {
            try
            {
                T eval = expressaoOk.Eval();
                if (typeof(T) == typeof(double))
                {
                    double valor = Convert.ToDouble(eval);
                    if (double.IsInfinity(valor) || double.IsNaN(valor))
                        return expressaoSeErro.Eval();
                }
                return eval;
            }
            catch
            {
                return expressaoSeErro.Eval();
            }
        }

        public static int Conta<T>(IEnumerable<T> lista)
        {
            return lista.Count();
        }

        public static IEnumerable<T> Filtra<T>(IEnumerable<T> lista, LambdaExpressionFunc<T, bool> filtro)
        {
            return lista.Where(i => filtro.EvalLambda(i));
        }

        public static double Soma<T>(IEnumerable<T> lista, LambdaExpressionFunc<T, double> selector)
        {
            return lista.Sum(i => selector.EvalLambda(i));
        }

        public static double Media<T>(IEnumerable<T> lista, LambdaExpressionFunc<T, double> selector)
        {
            return lista.Average(i => selector.EvalLambda(i));
        }

        public static IEnumerable<TOut> Seleciona<T, TOut>(IEnumerable<T> lista, LambdaExpressionFunc<T, TOut> selector)
        {
            return lista.Select(i => selector.EvalLambda(i));
        }

        public static double Somase<T>(IEnumerable<T> lista, LambdaExpressionFunc<T, bool> filtro, LambdaExpressionFunc<T, double> selector)
        {
            return lista.Where(i => filtro.EvalLambda(i)).Sum(i => selector.EvalLambda(i));
        }

        public static int? Dia(DateTime? data)
        {
            return data != null ? data.Value.Day : (int?)null;
        }

        public static int? Mes(DateTime? data)
        {
            return data != null ? data.Value.Month : (int?)null;
        }

        public static int? Ano(DateTime? data)
        {
            return data != null ? data.Value.Year : (int?)null;
        }

        public static DateTime Data(double dia, double mes, double ano)
        {
            return new DateTime((int)ano, (int)mes, (int)dia);
        }

        public static int DiasCorridos(DateTime? dataInicial, DateTime? dataFinal)
        {
            if (dataInicial == null || dataFinal == null)
                return 0;
            return (dataFinal.Value - dataInicial.Value).Days;
        }

        public static bool Contem(string str, string substr)
        {
            return str.Contains(substr);
        }
    }
}
