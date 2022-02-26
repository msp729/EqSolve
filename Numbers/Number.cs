namespace EqSolve.Numbers
{
    public struct Number<T> : INumber<T> where T : INumber<T>
    {
        public readonly T Value;

        #region Initializers & Deinitializers
        
        public Number(T value)
        {
            Value = value;
        }

        public static implicit operator Number<T>(T value)
        {
            return new(value);
        }

        public static implicit operator T(Number<T> value)
        {
            return value.Value;
        }

        #endregion

        #region Delegating to Value

        #region Comparison
        #nullable enable

        public int CompareTo(T? other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(T? other)
        {
            return Value.Equals(other);
        }

        #nullable disable
        #endregion

        #region Arithmetic

        public T Add(T that)
        {
            return Value.Add(that);
        }

        public T Sub(T that)
        {
            return Value.Sub(that);
        }

        public T Negate()
        {
            return Value.Negate();
        }

        public T Abs()
        {
            return Value.Abs();
        }

        #endregion

        #region Geometric

        public T Mul(T that)
        {
            return Value.Mul(that);
        }

        public T Div(T that)
        {
            return Value.Div(that);
        }

        #endregion

        #region Exponential

        public T Pow(T that)
        {
            return Value.Pow(that);
        }

        public T Exp()
        {
            return Value.Exp();
        }

        public T Expm1()
        {
            return Value.Expm1();
        }

        public T Log(T that)
        {
            return Value.Log(that);
        }

        public T Log()
        {
            return Value.Log();
        }

        public T Logp1()
        {
            return Value.Logp1();
        }

        #endregion

        #endregion

        #region Operators

        #region Unary
        public static Number<T> operator +(Number<T> value)
        {
            return value;
        }

        public static Number<T> operator -(Number<T> value)
        {
            return value.Negate(); // implicit casts are a developer's best friend.
        }

        #endregion

        #region Binary

        public static Number<T> operator +(Number<T> left, Number<T> right)
        {
            return left.Add(right);
        }

        public static Number<T> operator -(Number<T> left, Number<T> right)
        {
            return left.Sub(right);
        }

        public static Number<T> operator *(Number<T> left, Number<T> right)
        {
            return left.Mul(right);
        }

        public static Number<T> operator /(Number<T> left, Number<T> right)
        {
            return left.Div(right);
        }

        #endregion

        #endregion
    }
}