using System;
using System.Linq;
using EqSolve.Numbers;
using EqSolve.Terms.Meta;

namespace EqSolve.Terms.Standard
{
    public readonly struct Logarithm<N> : Term<N> where N : INumber<N>
    {
        public Number<N> Base { get; }
        public Number<N> Coefficient { get; }

        public Logarithm(Number<N> @base, Number<N> coefficient)
        {
            Base = @base;
            Coefficient = coefficient;
        }

        public Term<N> Derivative()
        {
            Func<int, Number<N>> conv = Base.FromInt;
            return new PowerLaw<N>(conv(1) / Base.Log(), conv(-1));
        }

        public bool IsConstant()
        {
            return Coefficient == Base.FromInt(0);
        }

        public Number<N> Constant()
        {
            if (IsConstant()) return Base.FromInt(0);
            throw new IllegalStateException("Constant() called on non-constant term.");
        }

        public Func<Number<N>, Number<N>> Function()
        {
            Number<N> b = Base, c = Coefficient;
            if (IsConstant())
                return _ => c;
            return n => c * n.Log(b);
        }

        public bool CanSimplify(ComplexTerm<N> container)
        {
            if (!container.IsOn(this))
                throw new IllegalStateException("CanSimplify() called on external term. This should never happen.");
            switch (container)
            {
                case Chain<N> c when Equals(c.Outer):
                    return c.Inner is Exponential<N>;
                case Chain<N> c when Equals(c.Inner):
                    return c.Outer is Exponential<N>;
                case Sum<N> s:
                    var @base = Base;
                    return s.Terms.Count(t => t is Logarithm<N> l && l.Base == @base) >= 2;
                default:
                    return false;
            }
        }

        public Term<N> Simplified(ComplexTerm<N> container)
        {
            switch (container)
            {
                case Chain<N> chain:
                    return SimplifyChain(chain);
                case Sum<N> sum:
                    var @base = Base;
                    var toAdd = sum.Terms.Where(t => t is Logarithm<N> l && l.Base == @base).Cast<Logarithm<N>>();
                    if (toAdd.Count() <= 1) return sum;
                    var newCoefficient = toAdd.Select(l => l.Coefficient).Aggregate((a, b) => a + b);
                    var theRest = sum.Terms.Where(t => t is not Logarithm<N> l || l.Base != @base);
                    return new Sum<N>(theRest.Append(new Logarithm<N>(@base, newCoefficient)).ToArray());
                default:
                    return container;
            }
        }

        internal static Term<N> SimplifyChain(Chain<N> chain)
        {
            var inner = chain.Inner;
            Term<N> @internal = null, @out;
            if (inner is Chain<N> {Outer: { } o,Inner: { } i}) (inner, @internal) = (o, i); // un-nest internal chains
            @out = (inner, chain.Outer) switch
            {
                (Logarithm<N> {Base: { } b, Coefficient: { } c1}, Exponential<N> {Base: { } e, Coefficient: { } c2}) =>
                    new PowerLaw<N>(c2, c1 * e.Log(b)),
                (Exponential<N> {Base: { } e, Coefficient: { } c1}, Logarithm<N> {Base: { } b, Coefficient: { } c2}) =>
                    new Sum<N>(new ConstantValue<N>(c1.Log(b)), new PowerLaw<N>(e.Log(b), b.FromInt(1))).Multiply(c2),
                _ => chain
            };
            return @internal != null ? new Chain<N>(@out, @internal) : @out;
        }

        Number<N> Term<N>.Coefficient()
        {
            return Coefficient;
        }

        public Term<N> Multiply(int n)
        {
            return new Logarithm<N>(Base, Coefficient * Base.FromInt(n));
        }

        public Term<N> Multiply(Number<N> n)
        {
            return new Logarithm<N>(Base, Coefficient * n);
        }
    }
}