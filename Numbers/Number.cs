namespace EqSolve.Numbers
{
    public readonly struct Number<T> : INumber<Number<T>> where T : INumber<T>
    {
        public readonly T Value;


        #region Convenience

        public static implicit operator T(Number<T> value)
        {
            return value.Value;
        }

        public static implicit operator Number<T>(T value)
        {
            return new(value);
        }
        
        public Number(T value)
        {
            Value = value;
        }

        #endregion

        #region Delegation

        public int CompareTo(Number<T> other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(Number<T> other)
        {
            return Value.Equals(other);
        }

        public Number<T> FromInt(int value)
        {
            return Value.FromInt(value);
        }

        public Number<T> Add(Number<T> that)
        {
            return Value.Add(that);
        }

        public Number<T> Sub(Number<T> that)
        {
            return Value.Sub(that);
        }

        public Number<T> Negate()
        {
            return Value.Negate();
        }

        public Number<T> Abs()
        {
            return Value.Abs();
        }

        public Number<T> Mul(Number<T> that)
        {
            return Value.Mul(that);
        }

        public Number<T> Div(Number<T> that)
        {
            return Value.Mul(that);
        }

        public Number<T> Pow(Number<T> that)
        {
            return Value.Pow(that);
        }

        public Number<T> Exp()
        {
            return Value.Exp();
        }

        public Number<T> Expm1()
        {
            return Value.Expm1();
        }

        public Number<T> Log(Number<T> that)
        {
            return Value.Log(that);
        }

        public Number<T> Log()
        {
            return Value.Log();
        }

        public Number<T> Logp1()
        {
            return Value.Logp1();
        }

        public Number<T> Mod(Number<T> that)
        {
            return Value.Mod(that);
        }

        public Number<T> Gcd(Number<T> that)
        {
            return Value.Gcd(that);
        }

        #endregion

        #region Operators

        public static Number<T> operator +(Number<T> value)
        {
            return value;
        }

        public static Number<T> operator -(Number<T> value)
        {
            return value.Negate();
        }

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

        public static bool operator ==(Number<T> left, Number<T> right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(Number<T> left, Number<T> right)
        {
            return !left.Equals(right);
        }
        
        public static bool operator >(Number<T> left, Number<T> right)
        {
            return left.CompareTo(right) == 1;
        }
        
        public static bool operator <(Number<T> left, Number<T> right)
        {
            return left.CompareTo(right) == -1;
        }

        #endregion
    }
}