using System;

namespace EqSolve.Numbers
{
    public interface INumber<T> : IComparable<T>, IEquatable<T> where T : INumber<T>
    {
        public T FromInt(int value);
        public T Add(T that);
        public T Sub(T that);
        public T Negate();
        public T Abs();
        public T Mul(T that);
        public T Div(T that);
        public T Pow(T that);
        public T Exp();
        public T Expm1();
        public T Log(T that);
        public T Log();
        public T Logp1();
        public T Mod(T that);
        public T Gcd(T that);
    }
}