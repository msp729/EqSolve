using System;
using System.Linq;
using EqSolve.Numbers;
using EqSolve.Terms.Meta;

namespace EqSolve.Terms.Standard
{
    public readonly struct Exponential<N> : Term<N> where N : INumber<N>
    {
        public Number<N> Base { get; }
        public Number<N> Coefficient { get; }

        public Exponential(Number<N> @base, Number<N> coefficient)
        {
            Base = @base;
            Coefficient = coefficient;
        }

        public Term<N> Derivative()
        {
            return new Exponential<N>(Base, Coefficient / Base.Log());
            // d/dx a*b^x=a/ln(b)*b^x
        }

        public Func<Number<N>, Number<N>> Function()
        {
            Number<N> @base = Base, coefficient = Coefficient, zero = Base.FromInt(0);
            if (Base == Base.FromInt(1))
                return _ => coefficient;
            if (Base == zero || Coefficient == zero)
                return _ => zero;

            return n => coefficient * @base.Pow(n);
        }

        public bool IsConstant()
        {
            var zero = Base.FromInt(0);
            return Base == Base.FromInt(1) || Base == zero || Coefficient == zero;
        }

        public Number<N> Constant()
        {
            var zero = Base.FromInt(0);
            if (Base == Base.FromInt(1))
                return Coefficient;
            if (Base == zero || Coefficient == zero)
                return zero;
            throw new IllegalStateException("Constant() called on non-constant term.");
        }

        Number<N> Term<N>.Coefficient()
        {
            return Coefficient;
        }

        public Term<N> Multiply(int n)
        {
            return new Exponential<N>(Base, Coefficient * Base.FromInt(n));
        }

        public Term<N> Multiply(Number<N> n)
        {
            return new Exponential<N>(Base, Coefficient * n);
        }

        public bool CanSimplify(ComplexTerm<N> container)
        {
            if (!container.IsOn(this))
                throw new IllegalStateException("CanSimplify() called on external term. This should never happen.");
            switch (container)
            {
                case Product<N> p:
                    return p.Terms.Count(t => t is Exponential<N>) > 1;
                case Quotient<N> q:
                    return q.Numerator is Exponential<N> && q.Denominator is Exponential<N>;
                case Sum<N> s:
                    var b = Base;
                    return s.Terms.Count(t => t is Exponential<N> e && e.Base == b) > 1;
                case Chain<N> c:
                    if (c.Inner.Equals(this)) return c.Outer is Logarithm<N>;
                    return c.Inner is Logarithm<N>;
                default: return false; // TODO: probably add more exponent simplification logic
            }
        }
        
        public Term<N> Simplified(ComplexTerm<N> container)
        {
            switch (container)
            {
                case Product<N> {Terms: { } terms}:
                    var exponents = terms.Where(t => t is Exponential<N>).Cast<Exponential<N>>()
                        .Select(e => e.Base).Aggregate((a, b) => a * b);
                    var baseTerms = terms.Where(t => t is not Exponential<N>);
                    if (baseTerms.Any())
                        return new Product<N>(baseTerms.Append(new Exponential<N>(exponents, Base.FromInt(1))).ToArray());
                    return new Exponential<N>(exponents, Base.FromInt(1));
                case Quotient<N> {Numerator: Exponential<N> {Base: { } b1, Coefficient: { } c1},
                    Denominator: Exponential<N> {Base: { } b2, Coefficient: { } c2}}:
                    return new Exponential<N>(b1 / b2, c1 / c2);
                case Chain<N> c:
                    return Logarithm<N>.SimplifyChain(c);
                default:
                    return container;
            }
        }
    }
}
