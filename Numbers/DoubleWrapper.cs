using System;

namespace EqSolve.Numbers
{
    public readonly struct DoubleWrapper :INumber<DoubleWrapper>
    {
        public double Value { get; }

        public DoubleWrapper(double value)
        {
            Value = value;
        }

        public static implicit operator DoubleWrapper(double value)
        {
            return new DoubleWrapper(value);
        }

        public static implicit operator double(DoubleWrapper value)
        {
            return value.Value;
        }

        public DoubleWrapper FromInt(int value)
        {
            return value; // me when the implicit cast
        }

        public DoubleWrapper Negate()
        {
            return -(double) this; // interface provides a negation operator that relies on this method
            // the cast prevents recursion.
        }

        public DoubleWrapper Abs()
        {
            return Math.Abs(this);
        }

        public DoubleWrapper Add(DoubleWrapper that)
        {
            return Value + that;
        }

        public DoubleWrapper Sub(DoubleWrapper that)
        {
            return Value - that;
        }

        public DoubleWrapper Mul(DoubleWrapper that)
        {
            return Value * that;
        }

        public DoubleWrapper Div(DoubleWrapper that)
        {
            return Value / that;
        }

        public DoubleWrapper Exp()
        {
            return Math.Exp(this);
        }

        public DoubleWrapper Expm1()
        {
            return Exp() - 1;
        }

        public DoubleWrapper Pow(DoubleWrapper that)
        {
            return Math.Pow(Value, that);
        }

        public DoubleWrapper Log(DoubleWrapper that)
        {
            return Math.Log(Value, that);
        }

        public DoubleWrapper Log()
        {
            return Math.Log(Value);
        }

        public DoubleWrapper Logp1()
        {
            return Log() + 1;
        }

        public DoubleWrapper Mod(DoubleWrapper that)
        {
            return Value % that;
        }

        public DoubleWrapper Gcd(DoubleWrapper that)
        {
            return Gcd(this, that);
        }

        private static double Gcd(double a, double b)
        {
            if (a < b) return Gcd(b, a);
            if (b == 0) return b;
            return Gcd(b, a % b);
        }

        public int CompareTo(DoubleWrapper other)
        {
            return Sign(Value - other);
        }

        private static int Sign(double value)
        {
            return value switch
            {
                < 0 => -1,
                > 0 => 1,
                _ => 0
            };
        }

        public bool Equals(DoubleWrapper other)
        {
            return Value - other == 0;
        }
    }
}