using System;
using EqSolve.Numbers;

namespace EqSolve.Terms
{
    public interface Term<N> where N : INumber<N>
    {
        public bool IsConstant();
        public Number<N> Constant();

        public Number<N> Coefficient();

        public Term<N> Multiply(int n);
        public Term<N> Multiply(Number<N> n);

        public bool CanSimplify(ComplexTerm<N> container);

        public Term<N> Derivative();
        public Func<Number<N>, Number<N>> Function();
    }
}