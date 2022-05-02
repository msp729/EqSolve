using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using CommandLineParser.Arguments;
using CommandLineParser.Exceptions;
using CommandLineParser.Validation;
using EqSolve.Numbers;
using EqSolve.Terms;

namespace EqSolve
{
    class Program
    {
        [ArgumentGroupCertification("d,m,f", EArgumentGroupCondition.ExactlyOneUsed)]
        public class Args
        {
            [SwitchArgument('d', "double", true, Description = "Use double precision")]
            public bool Double;

            [ValueArgument(typeof(int), 'm', "decimal", DefaultValue = 20, Description = "Number of decimal places", ValueOptional = true)]
            public int Decimal;
            
            [SwitchArgument('f', "fraction", false, Description = "Store numbers as fractions (very slow, but exceptionally accurate)")]
            public bool Fraction;
            
            [EnumeratedValueArgument(typeof(string), 'm', "method", AllowedValues = "bisect;newton;halley", DefaultValue = "newton")]
            public string Method;
        }

        private static void Main(string[] args)
        {
            CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
            Args arguments = new Args();
            parser.ExtractArgumentAttributes(arguments);
            try
            {
                parser.ParseCommandLine(args);
                Run(arguments);
            }
            catch (CommandLineException e)
            {
                Console.WriteLine(e.Message);
                
                parser.ShowUsage();
            }
        }
    }
}