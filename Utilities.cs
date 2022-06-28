using System.Linq;
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
            return (a, a.IsConstant(), b, b.IsConstant()) switch
            {
                (_, true, _, true) =>
                    new ConstantValue<N>(a.Constant() * b.Constant()) // if this breaks it's someone else's fault
                ,
                (_, true, _, false) => b.Multiply(a.Constant()),
                (_, false, _, true) => a.Multiply(b.Constant()),
                (Product<N> p1, _, Product<N> p2, _) => new Product<N>(p1.Terms.Concat(p2.Terms).ToArray()),
                (Product<N> p, _, _, _) => new Product<N>(p.Terms.Concat(new[] {b}).ToArray()),
                (_, _, Product<N> p, _) => new Product<N>(p.Terms.Concat(new[] {a}).ToArray()),
                _ => new Product<N>(a, b)
            };
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