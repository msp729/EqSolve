using System;
using EqSolve.Numbers;

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
            switch (container.GetType().Name)
            {
                default: return false; // TODO: logarithm simplification logic
            }
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