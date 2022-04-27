using System;
using System.Collections.Generic;
using System.Linq;
using EqSolve.Numbers;
using EqSolve.Terms.Standard;

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
                case Product<N>:
                    return true;
                case Quotient<N> q:
                    return (q.Numerator, q.Denominator) switch
                    {
                        (Product<N> numerator, Product<N> denominator) => numerator.Terms.Any(t => denominator.IsOn(t)),
                        (Product<N> p, Chain<N> c) => p.IsOn(c.Outer is PowerLaw<N> ? c.Inner : c),
                        (Chain<N> c, Product<N> p) => p.IsOn(c.Outer is PowerLaw<N> ? c.Inner : c),
                        _ => false
                    };
                case Chain<N>:
                    return false; // i would like to not think about this.
                case Sum<N>:
                    return false; // i doubt there are any significant simplifications to be made here.
                default:
                    return false; // TODO: i should probably add more logic for product simplification
            }
        }

        /// <summary>It simplifies the container.</summary>
        /// <remarks>This is an especially long and complicated function, one which should probably be split into multiple functions. </remarks>
        public Term<N> Simplified(ComplexTerm<N> container)
        {
            if (!container.IsOn(this))
                throw new IllegalStateException("Simplified(ComplexTerm) called on external term. This should never happen.");
            switch (container)
            {
                case Product<N> p:
                    return new Product<N>(p.Terms.SelectMany(t => t switch
                    {
                        Product<N> pr => pr.Terms,
                        _ => new []{t}
                    }).ToArray());
                case Quotient<N> q:
                    switch (q.Numerator, q.Denominator)
                    {
                        case (Product<N> p1, Product<N> p2):
                            Dictionary<Term<N>, int> shared;
                            int sharedCount;
                            (int, int) newLengths;
                            shared = new Dictionary<Term<N>, int>(p1.Terms.Intersect(p2.Terms)
                                .Select(t => (t, Math.Min(p1.Terms.Count(t.Equals), p2.Terms.Count(t.Equals))))
                                .Select(t => new KeyValuePair<Term<N>, int>(t.Item1, t.Item2)));
                            sharedCount = shared.Values.Sum();
                            if (sharedCount == 0) return container;
                            newLengths = (p1.Terms.Length - sharedCount, p2.Terms.Length - sharedCount);
                            switch (newLengths.Item2)
                            {
                                case > 1:
                                {
                                    List<Term<N>> newNumerator, newDenominator;
                                    int i;
                                    newNumerator = new List<Term<N>>(p1.Terms);
                                    newDenominator = new List<Term<N>>(p2.Terms);
                                    foreach (var (term, count) in shared)
                                    {
                                        for (i = 0; i < count; i++)
                                        {
                                            newNumerator.Remove(term);
                                            newDenominator.Remove(term);
                                        }
                                    }

                                    return new Quotient<N>(
                                        new Product<N>(newNumerator.ToArray()),
                                        new Product<N>(newDenominator.ToArray())
                                    );
                                }
                                case 1:
                                {
                                    List<Term<N>> newNumerator, denominator;
                                    newNumerator = new List<Term<N>>(p1.Terms);
                                    denominator = new List<Term<N>>(p2.Terms);
                                    int i;
                                    foreach (var (term, count) in shared)
                                    {
                                        for (i = 0; i < count; i++)
                                        {
                                            newNumerator.Remove(term);
                                            denominator.Remove(term);
                                        }
                                    }

                                    return new Quotient<N>(
                                        new Product<N>(newNumerator.ToArray()),
                                        denominator[0]
                                    );
                                }
                                case 0:
                                {
                                    List<Term<N>> newNumerator;
                                    newNumerator = new List<Term<N>>(p1.Terms);
                                    int i;
                                    foreach (var (term, count) in shared)
                                    {
                                        for (i = 0; i < count; i++)
                                        {
                                            newNumerator.Remove(term);
                                        }
                                    }

                                    return new Product<N>(newNumerator.ToArray());
                                }
                                default:
                                    throw new IllegalStateException(
                                        "An impossible value was calculated in Product.Simplified(ComplexTerm<N>)."
                                        + " This is literally impossible without integer overflow."
                                    );
                            }
                        default: return container;
                    }
                default: return container;
            }
        }

        public Term<N> Simplified()
        {
            var @this = this;
            return Terms.Any(t => t.CanSimplify(@this)) ? Terms.First(t => t.CanSimplify(@this)).Simplified(@this) : @this;
        }

        public bool CanBeSimplified()
        {
            var @this = this;
            return Terms.Any(t => t.CanSimplify(@this));
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