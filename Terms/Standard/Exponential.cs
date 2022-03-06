using System;
using EqSolve.Numbers;

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
            switch (container.GetType().Name)
            {
                default: return false; // TODO: exponent simplification logic
            }
        }
    }
}