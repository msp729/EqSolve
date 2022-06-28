using System;
using System.Linq;
using System.Text.RegularExpressions;
using EqSolve.Numbers;
using EqSolve.Terms;
using EqSolve.Terms.Meta;
using EqSolve.Terms.Standard;

namespace EqSolve
{
    public class FunctionParser<N> where N : INumber<N>
    {
        public static readonly Regex
            Exponential = new(@"^(e|pi|(\d+\.?\d*))\^(x|\(.+\))$"),
            PowerLaw = new(@"^(x|\(.*\))(\^(e|pi|(\d+\.?\d*)))?$"),
            Logarithm = new(@"^(ln|log_(e|pi|\d+\.?\d*))\((.+|x)\)$"),
            Sum = new(@"^.+?(\s*\+\s*.+?)+$"),
            Product = new(@"^.+(\s*\*\s*.+)+$"),
            Const = new(@"^-?\d+\.?\d*$");

        public readonly Func<string, N> Converter;

        public FunctionParser(Func<string, N> converter)
        {
            Converter = converter;
        }

        #nullable enable
        public Term<N>? ParseFunction(string function)
        {
            if (Exponential.IsMatch(function)) return ParseExponential(function);
            if (PowerLaw.IsMatch(function)) return ParsePowerLaw(function);
            if (Logarithm.IsMatch(function)) return ParseLogarithm(function);
            if (Sum.IsMatch(function)) return ParseSum(function);
            if (Product.IsMatch(function)) return ParseProduct(function);
            if (Const.IsMatch(function)) return new ConstantValue<N>(Converter(function));
            return null;
        }
        #nullable disable

        public Term<N> ParseExponential(string function)
        {
            var strings = function.Split('^');
            var @base = strings[0];
            var power = strings[1..].Aggregate((a, b) => a + '^' + b);

            if (power[0] == '(' && power[^1] == ')') power = power[1..^1];
            var intern = ParseFunction(power);
            var exp = new Exponential<N>(Converter(@base switch
            {
                "e" => "2.7182818284590452353602874713526625",
                "pi" => "3.1415926535897932384626433",
                _ => @base
            }), Converter("1"));
            return intern == null ? exp : new Chain<N>(exp, intern);
        }

        public Term<N> ParsePowerLaw(string function)
        {
            if (function == "x") return new PowerLaw<N>(Converter("1"), Converter("1"));
            var strings = function.Split('^');
            var @base = strings[..^1].Aggregate((a, b) => a + '^' + b);
            var power = strings[^1];

            var pl = new PowerLaw<N>(Converter("1"), Converter(power switch
            {
                "pi" => "3.1415926535897932384626433",
                "e" => "2.7182818284590452353602874713526625",
                _ => @base
            }));
            if (@base[0] == '(' && @base[^1] == ')') @base = @base[1..^1];
            var intern = ParseFunction(@base);
            return intern == null ? pl : new Chain<N>(pl, intern);
        }

        public Term<N> ParseLogarithm(string function)
        {
            var strings = function.Split('(');
            string @base;
            if (strings[0] == "ln") @base = "2.718281828459";
            else @base = strings[0].Split('_')[1];
            string argument = strings[1..].Aggregate((a, b) => a + '(' + b)[..^1];

            if (argument[0] == '(' && argument[^1] == ')') argument = argument[1..^1];
            var intern = ParseFunction(argument);
            var log = new Logarithm<N>(Converter(@base), Converter("1"));
            return intern == null ? log : new Chain<N>(log, intern);
        }
        
        public Term<N> ParseSum(string function)
        {
            var terms = function.Split('+').ToList();
            for (var i = 0; i < terms.Count; i++)
                while (terms[i].Count('('.Equals) > terms[i].Count(')'.Equals))
                {
                    if (i == terms.Count - 1) throw new InterpreterError("Unmatched open paren.");
                    terms[i] += terms[i + 1];
                    terms.RemoveAt(i + 1);
                }

            return new Sum<N>(terms.Select(ParseFunction).ToArray());
        }
        
        public Term<N> ParseProduct(string function)
        {
            var terms = function.Split('*').ToList();
            for (var i = 0; i < terms.Count; i++)
                while (terms[i].Count('('.Equals) > terms[i].Count(')'.Equals))
                {
                    if (i == terms.Count - 1) throw new InterpreterError("Unmatched open paren.");
                    terms[i] += terms[i + 1];
                    terms.RemoveAt(i + 1);
                }

            return new Product<N>(terms.Select(ParseFunction).ToArray());
        }
    }
}