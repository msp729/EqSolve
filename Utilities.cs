using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using EqSolve.Numbers;
using EqSolve.Terms;
using EqSolve.Terms.Meta;
using EqSolve.Terms.Standard;

namespace EqSolve
{
    public static class Utilities
    {
        public static Term<N> SimpleMultiply<N>(Term<N> a, Term<N> b) where N : INumber<N>
        {
            switch (a, a.IsConstant(), b, b.IsConstant())
            {
                case (_, true, _, true):
                    return new PowerLaw<N>(a.Constant() * b.Constant(), a.Constant().FromInt(0)); // if this breaks it's someone else's fault
                case (_, true, _, false):
                    return b.Multiply(a.Constant());
                case (_, false, _, true):
                    return a.Multiply(b.Constant());
                case (Product<N> p1, _, Product<N> p2, _):
                    return new Product<N>(p1.Terms.Concat(p2.Terms).ToArray());
                case (Product<N> p, _, _, _):
                    return new Product<N>(p.Terms.Concat(new[] { b }).ToArray());
                case (_, _, Product<N> p, _):
                    return new Product<N>(p.Terms.Concat(new[] { a }).ToArray());
                default:
                    return new Product<N>(a, b);
            }
        }

        public static Quotient<N> SimplifyQuotient<N>(Quotient<N> quotient) where N : INumber<N>
        {
            return (quotient.Numerator, quotient.Denominator) switch
            {
                (Quotient<N> numerator, Quotient<N> denominator) => new Quotient<N>(
                    SimpleMultiply(numerator.Numerator, denominator.Denominator),
                    SimpleMultiply(numerator.Denominator, denominator.Numerator)),
                (Quotient<N> numerator, { } denominator) => new Quotient<N>(
                    numerator.Numerator,
                    SimpleMultiply(denominator, numerator.Denominator)),
                ({ } numerator, Quotient<N> denominator) => new Quotient<N>(
                    SimpleMultiply(numerator, denominator.Denominator),
                    denominator.Numerator),
                _ => quotient
            };
        }
    }
}