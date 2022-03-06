using System;
using EqSolve.Numbers;

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
            switch (container.GetType().Name)
            {
                default: return false; // TODO: power law simplification logic
            }
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