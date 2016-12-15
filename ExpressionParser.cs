using System;
using System.Collections.Generic;
using System.Reflection;
using ExpCalculatorLib.Exceptions;
using ExpCalculatorLib.Expression;
using ExpCalculatorLib.Tokenizer;

namespace ExpCalculatorLib
{
    public class ExpressionParser
    {
        private Stack<IToken> tokenStack = new Stack<IToken>();

        private TokenParser tokenParser;

        public ParsingContext ParsingContext { get; private set; }

        public static ExpressionParser CreateParser()
        {
            ExpressionParser result = new ExpressionParser();

            MethodInfo trunc = typeof(Math).GetMethod("Truncate", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(double) }, null);
            result.ParsingContext.Functions.Add("arredondar", new MethodInvoker(trunc, "Calcula a parte inteira do valor especificado", "arredondar(numero)"));

            MethodInfo se = typeof(Functions).GetMethod("Se", BindingFlags.Static | BindingFlags.Public);
            result.ParsingContext.Functions.Add("se",
                new MethodInvoker(
                    se,
                    "Verifica se a condição especificada no 1º argumento é verdadeira e retorna o 2º argumento ser for verdadeira, caso contrário retorna o 3º argumento",
                    "se(condição, valor_se_verdadeiro, valor_se_falso)"
                    ));

            MethodInfo hoje = typeof(Functions).GetMethod("Hoje", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
            result.ParsingContext.Functions.Add("hoje", new MethodInvoker(hoje, "Retorna a data atual do sistema", "hoje()"));

            MethodInfo nao = typeof(Functions).GetMethod("Nao", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(bool) }, null);
            result.ParsingContext.Functions.Add("nao", new MethodInvoker(nao, "Retorna a negação da condição especificada", "nao(condicao)"));

            MethodInfo enulo = typeof(Functions).GetMethod("ENulo", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(object) }, null);
            result.ParsingContext.Functions.Add("enulo", new MethodInvoker(enulo, "Verifica se o valor especificado é nulo", "enulo(valor)"));

            MethodInfo seerro = typeof(Functions).GetMethod("SeErro", BindingFlags.Static | BindingFlags.Public);
            result.ParsingContext.Functions.Add("seerro", new MethodInvoker(seerro, "Retorna a expressão especificada em valor. Caso a expressão resulte em erro, retorna valor_se_erro.", "seerro(valor, valor_se_erro)"));

            MethodInfo filtra = typeof(Functions).GetMethod("Filtra", BindingFlags.Static | BindingFlags.Public);
            result.ParsingContext.Functions.Add("filtra", new MethodInvoker(filtra, "Retorna uma lista com os itens que satisfazem a função de predicado", "filtra(lista, predicado)"));

            MethodInfo conta = typeof(Functions).GetMethod("Conta", BindingFlags.Static | BindingFlags.Public);
            result.ParsingContext.Functions.Add("conta", new MethodInvoker(conta, "Retorna a quantidade de elementos de uma lista", "conta(lista)"));

            MethodInfo soma = typeof(Functions).GetMethod("Soma", BindingFlags.Static | BindingFlags.Public);
            result.ParsingContext.Functions.Add("soma", new MethodInvoker(soma, 
                "Calcula a soma dos itens obtidos através da função de transformação aplicada a cada item", "soma(lista, transformacao)"));

            MethodInfo media = typeof(Functions).GetMethod("Media", BindingFlags.Static | BindingFlags.Public);
            result.ParsingContext.Functions.Add("media", new MethodInvoker(media, 
                "Calcula a soma dos itens obtidos através da função de transformação aplicada a cada item", "media(lista, transformacao)"));

            MethodInfo seleciona = typeof(Functions).GetMethod("Seleciona", BindingFlags.Static | BindingFlags.Public);
            result.ParsingContext.Functions.Add("seleciona", new MethodInvoker(seleciona, 
                "Retorna uma lista cujos elementos são obtidos através da função de transformação aplicada a cada item", "seleciona(lista, transformacao)"));

            MethodInfo somase = typeof(Functions).GetMethod("Somase", BindingFlags.Static | BindingFlags.Public);
            result.ParsingContext.Functions.Add("somase",
                new MethodInvoker(
                    somase,
                    "Calcula a soma dos itens obtidos através da função de transformação aplicada a cada item e que satisfazem a função de predicado",
                    "somase(lista, predicado, transformacao)"
                    ));

            MethodInfo dia = typeof(Functions).GetMethod("Dia", BindingFlags.Static | BindingFlags.Public);
            result.ParsingContext.Functions.Add("dia", new MethodInvoker(dia, "Retorna o dia do mês de uma data", "dia(data)"));

            MethodInfo mes = typeof(Functions).GetMethod("Mes", BindingFlags.Static | BindingFlags.Public);
            result.ParsingContext.Functions.Add("mes", new MethodInvoker(mes, "Retorna o mês de uma data", "mes(data)"));

            MethodInfo ano = typeof(Functions).GetMethod("Ano", BindingFlags.Static | BindingFlags.Public);
            result.ParsingContext.Functions.Add("ano", new MethodInvoker(ano, "Retorna o ano de uma data", "ano(data)"));

            MethodInfo data = typeof(Functions).GetMethod("Data", BindingFlags.Static | BindingFlags.Public);
            result.ParsingContext.Functions.Add("data", new MethodInvoker(data, "Retorna uma data com o dia, mes e ano informados", "data(dia, mes, ano)"));

            MethodInfo diasCorridos = typeof(Functions).GetMethod("DiasCorridos", BindingFlags.Static | BindingFlags.Public);
            result.ParsingContext.Functions.Add("diasCorridos", new MethodInvoker(diasCorridos, "Retorna a quantidade de dias corridos entre a data inicial e a data final. Se uma das datas for nula, retorna 0.", "diasCorridos(data_inicial, data_final)"));

            MethodInfo contem = typeof(Functions).GetMethod("Contem", BindingFlags.Static | BindingFlags.Public);
            result.ParsingContext.Functions.Add("contem", new MethodInvoker(contem, "Verifica se um texto contém um subtexto", "contem(texto, subtexto)"));


            return result;
        }

        private ExpressionParser()
        {
            ParsingContext = new ParsingContext();
        }

        public IEnumerable<string> Keywords
        {
            get
            {
                return TokenParser.Keywords;
            }
        }

        public IExpression Parse(string expValue)
        {
            tokenParser = new TokenParser(expValue);
            IToken token = tokenParser.GetNextToken();

            IExpression expression = ParseExpression(ref token);
            if (!(token is EndToken))
                throw new SyntaxErrorException("operador esperado.");
            return expression;
        }

        private IExpression ParseExpression(ref IToken token)
        {
            return ParseLogicalOrExpression(ref token);
        }

        private IExpression ParseLogicalOrExpression(ref IToken token)
        {
            IExpression exp = ParseLogicalAndExpression(ref token);
            if (token is OuToken)
            {
                token = tokenParser.GetNextToken();
                return new Expression.OrExpression(exp, ParseLogicalOrExpression(ref token));
            }
            else
                return exp;

        }

        private IExpression ParseLogicalAndExpression(ref IToken token)
        {
            IExpression exp = ParseEqualityExpression(ref token);
            if (token is EToken)
            {
                token = tokenParser.GetNextToken();
                return new Expression.AndExpression(exp, ParseLogicalAndExpression(ref token));
            }
            else
                return exp;
        }

        private IExpression ParseEqualityExpression(ref IToken token)
        {
            IExpression exp = ParseRelationalExpression(ref token);
            try
            {
                if (token is EqualsToken)
                {
                    token = tokenParser.GetNextToken();
                    return new EqualsExpression(exp, ParseEqualityExpression(ref token));
                }
                else if (token is DifferentToken)
                {
                    token = tokenParser.GetNextToken();
                    return new Expression.DifferentExpresstion(exp, ParseEqualityExpression(ref token));
                }
                else
                    return exp;
            }
            catch (SyntaxErrorException e)
            {
                throw new SyntaxErrorException(e.Message, e, exp, exp);
            }
        }

        private IExpression ParseRelationalExpression(ref IToken token)
        {
            IExpression exp = ParseAdditiveExpression(ref token);
            if (token is GreaterThanToken)
            {
                token = tokenParser.GetNextToken();
                return new Expression.GreaterThanExpression(exp, ParseRelationalExpression(ref token));
            }
            else if (token is GreaterThanOrEqualToken)
            {
                token = tokenParser.GetNextToken();
                return new Expression.GreaterThanOrEqualExpression(exp, ParseRelationalExpression(ref token));
            }
            else if (token is LessThanToken)
            {
                token = tokenParser.GetNextToken();
                return new Expression.LessThanExpression(exp, ParseRelationalExpression(ref token));
            }
            else if (token is LessThanOrEqualToken)
            {
                token = tokenParser.GetNextToken();
                return new Expression.LessThanOrEqualExpression(exp, ParseRelationalExpression(ref token));
            }
            else
                return exp;
        }

        private IExpression ParseAdditiveExpression(ref IToken token)
        {
            IExpression leftExpression = ParseMultiplicExpression(ref token);
            while (token is PlusToken || token is MinusToken)
            {
                if (token is PlusToken)
                {
                    token = tokenParser.GetNextToken();
                    leftExpression = new Expression.PlusExpression(leftExpression, ParseMultiplicExpression(ref token));
                }
                else
                {
                    token = tokenParser.GetNextToken();
                    leftExpression = new Expression.MinusExpression(leftExpression, ParseMultiplicExpression(ref token));
                }
            }
            return leftExpression;
        }

        private IExpression ParseMultiplicExpression(ref IToken token)
        {
            IExpression leftExpression = ParseExponentExpression(ref token);
            while (token is MultiplyToken || token is DivideToken || token is ModToken)
            {
                if (token is MultiplyToken)
                {
                    token = tokenParser.GetNextToken();
                    leftExpression = new Expression.MultiplyExpression(leftExpression, ParseExponentExpression(ref token));
                }
                else if (token is DivideToken)
                {
                    token = tokenParser.GetNextToken();
                    leftExpression = new Expression.DivideExpression(leftExpression, ParseExponentExpression(ref token));
                }
                else
                {
                    token = tokenParser.GetNextToken();
                    leftExpression = new Expression.ModExpression(leftExpression, ParseExponentExpression(ref token));
                }
            }
            return leftExpression;
        }

        private IExpression ParseExponentExpression(ref IToken token)
        {
            IExpression leftExpression = ParseUnaryExpression(ref token);
            while (token is ExponentationToken)
            {
                token = tokenParser.GetNextToken();
                leftExpression = new Expression.ExponentiationExpression(leftExpression, ParseUnaryExpression(ref token));
            }
            return leftExpression;
        }

        private IExpression ParseUnaryExpression(ref IToken token)
        {
            IExpression unaryExpression = ParseLiteralExpression(ref token);

            if (unaryExpression != null)
            {
                token = tokenParser.GetNextToken();
            }
            else if (token is MinusToken)
            {
                token = tokenParser.GetNextToken();
                return new UnaryMinusExpression(ParseUnaryExpression(ref token));
            }
            else if (token is IdentifierToken)
            {
                IdentifierToken idToken = token as IdentifierToken;
                token = tokenParser.GetNextToken();
                if (token is OpenParenthesisToken)
                {
                    try
                    {
                        ParameterExpression[] args = ParseArgumentList(ref token);
                        unaryExpression = new FunctionExpression(idToken, args);
                    }
                    catch (SyntaxErrorException e)
                    {
                        throw new SyntaxErrorException(e.Message, e, e.Expression, new FunctionExpression(idToken, e.Args));
                    }

                }
                else if (token is LambdaInvokeToken)
                {
                    try
                    {
                        token = tokenParser.GetNextToken();
                        IExpression rightExpression = ParseExpression(ref token);
                        unaryExpression = new LambdaExpression(idToken, rightExpression);
                    }
                    catch (SyntaxErrorException e)
                    {
                        throw new SyntaxErrorException(e.Message, e, e.Expression, new LambdaExpression(idToken, e.RootExpression));
                    }
                }
                else
                    unaryExpression = new IdentifierExpression(idToken);
            }
            else if (token is OpenParenthesisToken)
            {
                token = tokenParser.GetNextToken();
                IExpression complexExpression = ParseLogicalOrExpression(ref token);
                if (!(token is CloseParenthesisToken))
                    throw new SyntaxErrorException("')' experado.");
                token = tokenParser.GetNextToken();
                unaryExpression = complexExpression;
            }
            else
                throw new SyntaxErrorException("expressão esperada.");

            //decide se é uma propery access expression
            while (token is DotToken)
            {
                token = tokenParser.GetNextToken();
                if (!(token is IdentifierToken))
                {
                    token = tokenParser.GetNextToken();
                    throw new SyntaxErrorException("identificador esperado", unaryExpression, unaryExpression);
                }
                    
                IdentifierToken id = token as IdentifierToken;
                unaryExpression = new PropertyAccessExpression(unaryExpression, id.IdentifierName);

                token = tokenParser.GetNextToken();
            }

            return unaryExpression;
        }

        private ParameterExpression[] ParseArgumentList(ref IToken token)
        {
            token = tokenParser.GetNextToken();
            List<ParameterExpression> result = new List<ParameterExpression>();
            if (token is CloseParenthesisToken)
            {
                token = tokenParser.GetNextToken();
                return result.ToArray();
            }

            do
            {
                try
                {
                    IExpression arg = ParseLogicalOrExpression(ref token);
                    result.Add(new ParameterExpression(arg));
                }
                catch (SyntaxErrorException e)
                {
                    result.Add(new ParameterExpression(e.RootExpression));
                    throw new SyntaxErrorException(e.Message, e, e.Expression) { Args = result.ToArray() };
                }
            } while (token is CommaToken && (token = tokenParser.GetNextToken()) == token);
            if (!(token is CloseParenthesisToken))
                throw new SyntaxErrorException("')' esperado.", result[result.Count - 1].InnerExpression) { Args = result.ToArray() };
            token = tokenParser.GetNextToken();
            return result.ToArray();
        }

        private IExpression ParseLiteralExpression(ref IToken token)
        {
            if (token is NumberLiteralToken)
            {
                NumberLiteralToken numberLiteralToken = token as NumberLiteralToken;
                return new NumericLiteralExpression(numberLiteralToken.Value);
            }
            else if (token is StringLiteralToken)
            {
                StringLiteralToken stringLiteralToken = token as StringLiteralToken;
                return new StringLiteralExpression(stringLiteralToken.Value);
            }
            else if (token is BooleanLiteralToken)
            {
                BooleanLiteralToken booleanLiteralToken = token as BooleanLiteralToken;
                return new BooleanLiteralExpression(booleanLiteralToken.Value);
            }
            return null;
        }
    }
}
