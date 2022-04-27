using EqSolve.Numbers;

namespace EqSolve.Terms
{
    public interface ComplexTerm<N> : Term<N> where N : INumber<N>
    {
        public bool IsOn(Term<N> that);
        public bool CanBeSimplified();
        public Term<N> Simplified();
    }
}