using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EqSolve.Numbers;
using EqSolve.Terms.Standard;

namespace EqSolve.Terms.Meta
{
    public readonly struct Quotient<N> : ComplexTerm<N> where N:INumber<N>
    {
        public readonly Term<N> Numerator;
        public readonly Term<N> Denominator;

        public Quotient(Term<N> numerator, Term<N> denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public bool IsConstant()
        {
            return Numerator.IsConstant() && Denominator.IsConstant();
        }

        public Number<N> Constant()
        {
            return Numerator.Constant() / Denominator.Constant();
        }

        public bool CanSimplify(ComplexTerm<N> container)
        {
            if (!container.IsOn(this))
                throw new IllegalStateException("CanSimplify() called on external term. This should never happen.");
            switch (container)
            {
                case Product<N> p:
                    return p.Terms.Count(t => t is Quotient<N>) > 1;
                case Quotient<N>:
                    return true;
                default:
                    return false; // TODO: i should probably add more logic for quotient simplification
            }
        }

        public ComplexTerm<N> Simplified(ComplexTerm<N> container)
        {
            if (!container.IsOn(this))
                throw new IllegalStateException("Simplified() called on external term. This should never happen.");
            switch (container)
            {
                case Quotient<N> q:
                    return Utilities.SimplifyQuotient(q);
                case Product<N> p:
                    return new Quotient<N>(
                        new Product<N>(p.Terms.Select(t => t is Quotient<N> q ? q.Numerator : t).ToArray()),
                        new Product<N>(p.Terms.Where(t => t is Quotient<N>).Cast<Quotient<N>>()
                            .Select(t => t.Denominator).ToArray())
                    );
                default:
                    return container;
            }
        }

        public bool CanBeSimplified()
        {
            return Numerator.CanSimplify(this) || Denominator.CanSimplify(this);
        }

        public ComplexTerm<N> Simplified()
        {
            return (Numerator.CanSimplify(this), Denominator.CanSimplify(this)) switch
            {
                (true, _) => Numerator.Simplified(this),
                (_, true) => Denominator.Simplified(this),
                _ => this
            };
        }

        public Term<N> Derivative()
        {
            Func<int, Number<N>> conv = Numerator.Coefficient().FromInt;
            return new Quotient<N>(
                new Sum<N>(
                    new Product<N>(Numerator.Derivative(), Denominator),
                    new Product<N>(Denominator.Derivative(), Numerator).Multiply(-1)
                ),
                new Chain<N>(new PowerLaw<N>(conv(1), conv(2)), Denominator)
            );
        }

        public Func<Number<N>, Number<N>> Function()
        {
            var n = Numerator.Function();
            var d = Denominator.Function();
            return num => n(num) / d(num);
        }

        public bool IsOn(Term<N> that)
        {
            return that == Numerator || that == Denominator;
        }

        public Number<N> Coefficient()
        {
            return Numerator.Constant().FromInt(1);
        }

        public Term<N> Multiply(int n)
        {
            return new Quotient<N>(Numerator.Multiply(n), Denominator);
        }

        public Term<N> Multiply(Number<N> n)
        {
            return new Quotient<N>(Numerator.Multiply(n), Denominator);
        }
    }
}