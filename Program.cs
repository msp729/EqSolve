using System;
using CommandLineParser.Arguments;
using CommandLineParser.Exceptions;
using CommandLineParser.Validation;
using EqSolve.Numbers;

namespace EqSolve
{
    class Program
    {
        [ArgumentGroupCertification("d,m,f", EArgumentGroupCondition.OneOreNoneUsed)]
        public class Args
        {
            [SwitchArgument('d', "double", true, Description = "Use double precision")]
            public bool Double;

            [ValueArgument(typeof(int), 'm', "decimal", DefaultValue = 20, Description = "Number of decimal places", ValueOptional = true)]
            public int Decimal;
            
            [SwitchArgument('f', "fraction", false, Description = "Store numbers as fractions (very slow, but exceptionally accurate)")]
            public bool Fraction;
            
            [EnumeratedValueArgument(typeof(string), 'M', "method", AllowedValues = "bisect;newton;halley", DefaultValue = "newton")]
            public string Method;
        }

        private static void Main(string[] args)
        {
            var parser = new CommandLineParser.CommandLineParser();
            var arguments = new Args();
            parser.ExtractArgumentAttributes(arguments);
            try
            {
                parser.ParseCommandLine(args);
                if (arguments.Double) Run(arguments, new FunctionParser<DoubleWrapper>(s => double.Parse(s)));
                else if (arguments.Fraction) Run(arguments, new FunctionParser<BigFraction>(s => new BigFraction(s)));
                //else RunDecimal(arguments);
                // i haven't implemented this yet, because it's going to be painful.
            }
            catch (CommandLineException e)
            {
                Console.WriteLine(e.Message);
                
                parser.ShowUsage();
            }
        }

        private static void Run<N>(Args arguments, FunctionParser<N> functionParser) where N : INumber<N>
        {
            Console.WriteLine("What function would you like to find the root of?");
            var fn = functionParser.ParseFunction(Console.ReadLine());
            if (fn == null) throw new("The input function is not valid.");
            if (arguments.Method == "newton")
            {
                Func<Number<N>, Number<N>> f = fn.Function(),
                    fprime = fn.Derivative().Function(),
                    delta = x => f(x) / fprime(x);
                Console.WriteLine("What is the initial guess?");
                Number<N> guess = functionParser.Converter(Console.ReadLine());
                Console.WriteLine("How many iterations would you like to perform?");
                var iterations = ParseInt();
                Console.WriteLine("How often should the current iteration be printed?\n"
                                  +" (0 for never, 1 for every iteration, 2 for every other iteration, etc.)");
                var printEvery = ParseInt();
                int i;
                for (i = 1; i <= iterations; i++)
                {
                    guess -= delta(guess);
                    if (printEvery != 0 && i % printEvery == 0)
                        Console.WriteLine($"Iteration {i}: {guess} (evaluates to {f(guess)})");
                }
            }
        }

        private static int ParseInt()
        {
            while (true)
            {
                try
                {
                    return int.Parse(Console.ReadLine());
                }
                catch (FormatException)
                {
                    Console.WriteLine("Please input an integer.");
                }
            }
        }
    }
}