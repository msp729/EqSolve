using System;
using System.Collections.Generic;
using System.Linq;
using EqSolve.Numbers;
using EqSolve.Terms.Meta;

namespace EqSolve.Terms.Standard
{
    public readonly struct PowerLaw<N> : Term<N> where N : INumber<N>
    {
        public Number<N> Coefficient { get; }
        public Number<N> Power { get; }

        public PowerLaw(Number<N> coefficient, Number<N> power)
        {
            Coefficient = coefficient;
            Power = power;
        }

        public bool IsConstant()
        {
            return Power == Power.FromInt(0) || Coefficient == Coefficient.FromInt(1);
        }

        public Number<N> Constant()
        {
            if (IsConstant()) return Coefficient;
            throw new IllegalStateException("Constant() called on non-constant term.");
        }

        public Term<N> Derivative()
        {
            return new PowerLaw<N>(
                Coefficient * Power,
                Power - Power.FromInt(1)
            ); //d/dx ax^b = ab*x^(b-1)
        }

        public Func<Number<N>, Number<N>> Function()
        {
            Number<N> coefficient = Coefficient, power = Power;
            return n => coefficient * n.Pow(power);
        }

        public bool CanSimplify(ComplexTerm<N> container)
        {
            if (!container.IsOn(this))
                throw new IllegalStateException("CanSimplify() called on external term. This should never happen.");

            switch (container)
            {
                case Sum<N> s:
                {
                    var p = Power;
                    return s.Terms.Count(t => t is PowerLaw<N> pl && pl.Power == p) >= 2;
                }

                case Product<N> p:
                {
                    return p.Terms.Count(t => t is PowerLaw<N>) >= 2;
                }

                case Quotient<N> { Numerator: PowerLaw<N>, Denominator: PowerLaw<N> }:
                    return true;

                case Chain<N> c:
                {
                    if (Equals(c.Outer))
                        return c.Inner is PowerLaw<N> or Chain<N> {Outer: PowerLaw<N>}; // a(cx^d)^b=a*c^b*x^(db)
                    if (Equals(c.Inner))
                        return c.Outer is Logarithm<N> or PowerLaw<N>; // lol
                    return false;
                }
                default: return false;
            }
        }

        public Term<N> Simplified(ComplexTerm<N> container)
        {
            if (!container.IsOn(this))
                throw new IllegalStateException("CanSimplify() called on external term. This should never happen.");
            switch (container)
            {
                case Sum<N> sum:
                    var power = Power;
                    if (sum.Terms.Count(t => t is PowerLaw<N> pl && pl.Power == power) < 2)
                        return container;
                    var toAdd = sum.Terms.Where(t => t is PowerLaw<N> pl && pl.Power == power).Cast<PowerLaw<N>>();
                    var summedCoefficient = toAdd.Select(t => t.Coefficient).Aggregate((a, b) => a + b);
                    var notSummed = sum.Terms.Where(t => t is not PowerLaw<N> pl || pl.Power != power);
                    return new Sum<N>(notSummed.Append(new PowerLaw<N>(summedCoefficient, power)).ToArray());
                case Product<N> product:
                    var toMultiply = product.Terms.Where(t => t is PowerLaw<N>).Cast<PowerLaw<N>>();
                    var unmultiplied = product.Terms.Where(t => t is not PowerLaw<N>);
                    var (newCoefficient, newPower) = toMultiply.Select(t => (t.Coefficient, t.Power))
                        .Aggregate((a, b) => (a.Coefficient * b.Coefficient, a.Power + b.Power));
                    return new Product<N>(unmultiplied.Append(new PowerLaw<N>(newCoefficient, newPower)).ToArray());
                case Quotient<N> { Numerator: PowerLaw<N> numerator, Denominator: PowerLaw<N> denominator }:
                    return new PowerLaw<N>(numerator.Coefficient / denominator.Coefficient,
                        numerator.Power - denominator.Power);
                case Chain<N> chain:
                    return SimplifyChain(chain);
                default: return container;
            }
        }

        private static Term<N> SimplifyChain(Chain<N> chain)
        {
            var inner = chain.Inner;
            Term<N> @internal = null;
            if (inner is Chain<N> {Outer: { } o, Inner: { } i }) (inner, @internal) = (o, i); // un-nest internal chains
            Term<N> @out;
            switch (inner, chain.Outer)
            {
                case (PowerLaw<N> {Coefficient: var c1, Power: var p1}, PowerLaw<N> {Coefficient: var c2, Power: var p2}
                    ):
                    @out = new PowerLaw<N>(c1.Pow(p2) * c2, p1 * p2);
                    break;
                case (PowerLaw<N> pl, Logarithm<N> log):
                    @out = new Chain<N>(log.Multiply(pl.Power),
                        new PowerLaw<N>(pl.Coefficient, pl.Coefficient.FromInt(1)));
                    break;
                default:
                    @out = chain;
                    break;
            }

            return @internal != null ? new Chain<N>(@out, @internal) : @out;
        }

        Number<N> Term<N>.Coefficient()
        {
            return Coefficient;
        }

        public Term<N> Multiply(int n)
        {
            return new PowerLaw<N>(Coefficient * Coefficient.FromInt(n), Power);
        }

        public Term<N> Multiply(Number<N> n)
        {
            return new PowerLaw<N>(n * Coefficient, Power);
        }
    }
}