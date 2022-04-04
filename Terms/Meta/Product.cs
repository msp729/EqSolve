using System;
using System.Linq;
using EqSolve.Numbers;

namespace EqSolve.Terms.Meta
{
    public readonly struct Product<N> : ComplexTerm<N> where N : INumber<N>
    {
        public readonly Term<N>[] Terms;

        public Product(params Term<N>[] terms)
        {
            Terms = terms;
        }

        public Term<N> Derivative()
        {
            if (Terms.Length == 1)
                return Terms[0].Derivative();
            if (Terms.Length == 2)
                return new Sum<N>(
                    new Product<N>(Terms[0].Derivative(), Terms[1]),
                    new Product<N>(Terms[0], Terms[1].Derivative())
                );
            Product<N> left = new(Terms[..(Terms.Length / 2)]),
                right = new(Terms[(Terms.Length / 2)..Terms.Length]);
            return new Product<N>(left, right).Derivative();
        }

        public Func<Number<N>, Number<N>> Function()
        {
            Term<N>[] terms = Terms;
            return number => terms.Select(t => t.Function())
                .Select(f => f(number))
                .Aggregate((a, b) => a * b);
        }

        public bool IsConstant()
        {
            return Terms.All(b => b.IsConstant());
        }

        Term<N> Term<N>.Multiply(Number<N> n)
        {
            return new Product<N>(new[]{Terms[0].Multiply(n)}.Concat(Terms[1..]).ToArray());
        }

        public bool CanSimplify(ComplexTerm<N> container)
        {
            if (!container.IsOn(this))
                throw new IllegalStateException("CanSimplify() called on external term. This should never happen.");
            switch (container)
            {
                case Product<N> or Quotient<N>:
                    return true;
                default:
                    return false; // TODO: i should probably add more logic for product simplification
            }
        }

        public Number<N> Constant()
        {
            return Terms.Select(t => t.Constant())
                .Aggregate((a, b) => a * b);
        }

        public Number<N> Coefficient()
        {
            return Terms[0].Coefficient().FromInt(1); // lmao
        }

        public Term<N> Multiply(int n)
        {
            return new Product<N>(Terms[1..].Concat(new[]{Terms[0].Multiply(n)}).ToArray());
        }

        public bool IsOn(Term<N> that)
        {
            return Terms.Any(that.Equals);
        }
    }
}