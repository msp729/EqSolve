using System;
using EqSolve.Numbers;
using EqSolve.Terms.Meta;

namespace EqSolve.Terms.Standard
{
    public readonly struct Chain<N> : ComplexTerm<N> where N : INumber<N>
    {
        public readonly Term<N> Outer, Inner;

        public Chain(Term<N> outer, Term<N> inner)
        {
            Outer = outer;
            Inner = inner;
        }

        public bool IsConstant()
        {
            return Outer.IsConstant() || Inner.IsConstant();
        }

        public Number<N> Constant()
        {
            if (Outer.IsConstant()) return Outer.Constant();
            if (Inner.IsConstant()) return Outer.Function()(Inner.Constant());
            throw new IllegalStateException("Constant() called on non-constant term.");
        }

        public Number<N> Coefficient()
        {
            return Outer.Coefficient().FromInt(1);
        }

        public Term<N> Multiply(int n)
        {
            return new Chain<N>(Outer.Multiply(n), Inner);
        }

        public Term<N> Multiply(Number<N> n)
        {
            return new Chain<N>(Outer.Multiply(n), Inner);
        }

        public bool CanSimplify(ComplexTerm<N> container)
        {
            if (!container.IsOn(this))
                throw new IllegalStateException("CanSimplify() called on external term. This should never happen.");
            switch (container.GetType().Name)
            {
                default: return false; // TODO: chain simplification logic
            }
        }

        public Term<N> Derivative()
        {
            return new Product<N>(new Chain<N>(Outer.Derivative(), Inner), Inner.Derivative());
        }

        public Func<Number<N>, Number<N>> Function()
        {
            var outer = Outer.Function();
            var inner = Inner.Function();
            return n => outer(inner(n)); // :)
        }

        public bool IsOn(Term<N> that)
        {
            return that == Inner || that == Outer;
        }
    }
}