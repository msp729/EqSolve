using System;
using System.Linq;
using EqSolve.Numbers;

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
            return container switch
            {
                Sum<N> => true,
                _ => false
            };
        }
        
        public ComplexTerm<N> Simplified(ComplexTerm<N> container)
        {
            if (!container.IsOn(this)) return container;
            if (container is Sum<N> s) return new Sum<N>(Terms.Concat(s.Terms).ToArray());
            return container;
        }

        public bool CanBeSimplified()
        {
            var @this = this;
            return Terms.Any(t => t.CanSimplify(@this));
        }

        public ComplexTerm<N> Simplified()
        {
            var @this = this;
            return Terms.Any(t => t.CanSimplify(@this))
                ? Terms.First(t => t.CanSimplify(@this)).Simplified(this)
                : this;
        }

        public Term<N> Derivative()
        {
            return new Sum<N>(Terms.Select(t => t.Derivative()).ToArray());
        }

        public Func<Number<N>, Number<N>> Function()
        {
            var terms = Terms;
            return number => terms.Select(t => t.Function()) // just hope terms aren't too deeply nested
                .Select(f => f(number))
                .Aggregate((a, b) => a + b); // no sum function unless i cast to a primitive (i will not be doing that)
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