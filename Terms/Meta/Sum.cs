using System;
using System.Linq;
using EqSolve.Numbers;
using EqSolve;

namespace EqSolve.Terms.Meta
{
    public readonly struct Sum<N> : ComplexTerm<N> where N : INumber<N>
    {
        public readonly Term<N>[] Terms;
        
        public Sum(params Term<N>[] terms)
        {
            Terms = terms;
        }

        public bool IsConstant()
        {
            return Terms.All(t => t.IsConstant());
        }

        public Number<N> Constant()
        {
            return Terms.Select(t => t.Constant())
                .Aggregate((a, b) => a + b); // me when no .sum()
        }

        public bool CanSimplify(ComplexTerm<N> container)
        {
            if (!container.IsOn(this))
                throw new IllegalStateException("CanSimplify() called on external term. This should never happen.");
            switch (container.GetType().Name)
            {
                default: return false; // TODO: sum simplification logic
            }
        }

        public Term<N> Derivative()
        {
            return new Sum<N>(Terms.Select(t => t.Derivative()).ToArray());
        }

        public Func<Number<N>, Number<N>> Function()
        {
            Term<N>[] terms = Terms;
            return number => terms.Select(t => t.Function()) // just hope terms aren't too deeply nested
                .Select(f => f(number)) // wacky
                .Aggregate((a, b) => a + b); // sigma wackiness
        }

        public bool IsOn(Term<N> that)
        {
            return Terms.Any(that.Equals);
        }

        public Number<N> Coefficient()
        {
            return Terms[0].Coefficient().FromInt(1);
        }

        public Term<N> Multiply(int n)
        {
            return new Sum<N>(Terms.Select(t => t.Multiply(n)).ToArray());
        }

        public Term<N> Multiply(Number<N> n)
        {
            return new Sum<N>(Terms.Select(t => t.Multiply(n)).ToArray());
        }
    }
}