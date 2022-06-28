using System;
using System.Linq;
using System.Numerics;

namespace EqSolve.Numbers
{
    public readonly struct BigFraction : INumber<BigFraction>
    {
        public static BigFraction ExponentialEpsilon = new(1, BigInteger.Pow(10, 77));
        public BigInteger Numerator { get; }
        public BigInteger Denominator { get; }

        #region Initializers

        public BigFraction(BigInteger value)
        {
            Numerator = value;
            Denominator = 1;
        }

        public static implicit operator BigFraction(BigInteger value)
        {
            return new(value);
        }

        public BigFraction(string value)
        {
            switch (value.Count('.'.Equals))
            {
                case > 1: throw new ArgumentException("Too many decimal points in the string.");
                case 0: Numerator = BigInteger.Parse(value); Denominator = 1; break;
                case 1:
                    var split = value.Split('.');
                    Numerator = BigInteger.Parse(split[0] + split[1]);
                    Denominator = BigInteger.Pow(10, split[1].Length);
                    break;
                default: Numerator = Denominator = 1; break;
            }
        }

        public static implicit operator BigFraction(long value)
        {
            return new(value);
        }

        public BigFraction(BigInteger numerator, BigInteger denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public BigFraction FromInt(int value)
        {
            return value;
        }

        #endregion

        #region Operations

        public BigFraction Normalize()
        {
            BigInteger gcd = BigInteger.GreatestCommonDivisor(Numerator, Denominator);
            if (Denominator.Sign == -1 && gcd.Sign != -1) gcd = -gcd;
            return new(Numerator / gcd, Denominator / gcd);
        }

        #region Arithmetic

        public BigFraction Negate()
        {
            return new(-Numerator, Denominator);
        }

        public BigFraction Abs()
        {
            return new(BigInteger.Abs(Numerator), BigInteger.Abs(Denominator));
        }

        public BigFraction Add(BigFraction that)
        {
            if (Denominator == that.Denominator)
                return new BigFraction(Numerator + that.Numerator, Denominator).Normalize();
            return new BigFraction(
                Numerator * that.Denominator
                + that.Numerator * Denominator,
                Denominator * that.Denominator
            ).Normalize();
        }

        public BigFraction Sub(BigFraction that)
        {
            return Add(that.Negate());
        }

        public BigFraction Mod(BigFraction that)
        {
            return new BigFraction(
                (this.Numerator * that.Denominator) % (that.Numerator * this.Denominator),
                this.Denominator * that.Denominator
            ).Normalize();
        }

        public BigFraction Gcd(BigFraction that)
        {
            return Gcd(this, that);
        }

        private static BigFraction Gcd(BigFraction a, BigFraction b)
        {
            if (a < b) return Gcd(b, a);
            if (b.Equals(new BigFraction(0))) return a;
            return Gcd(b, a.Mod(b));
        }

        #endregion

        #region Geometric

        public BigFraction Reciprocal()
        {
            return new(Denominator, Numerator);
        }

        public BigFraction Mul(BigFraction that)
        {
            return new BigFraction(
                Numerator * that.Numerator,
                Denominator * that.Denominator
            ).Normalize();
        }

        public BigFraction Div(BigFraction that)
        {
            return Mul(that.Reciprocal());
        }

        public BigFraction Div(BigInteger that)
        {
            return new(Numerator, Denominator * that);
        }

        #endregion

        #region Exponential

        #region Powers

        public BigFraction Exp()
        {
            BigFraction total = 1, term = 1;
            BigInteger i = 1;
            do
            {
                term = term.Mul(this).Div(i);
                total = total.Add(term);
                i++;
            } while (term > ExponentialEpsilon);

            return total;
        }
        
        public BigFraction Expm1()
        {
            BigFraction total = 0, term = 1;
            BigInteger i = 1;
            do
            {
                term = term.Mul(this).Div(i);
                total = total.Add(term);
                i++;
            } while (term > ExponentialEpsilon);

            return total;
        }

        public BigFraction Pow(BigFraction that)
        {
            return Log().Mul(that).Exp();
        }

        #endregion

        #region Logarithms

        public BigFraction LnApprox()
        {
            BigInteger blog = Numerator.GetBitLength() - Denominator.GetBitLength();
            return blog * 7/10;
            // ln(2) is about 10/7
            // log2(x)/ln(2) = ln(x)
        }

        public BigFraction Log()
        {
            BigFraction n = this, x = LnApprox(), prev;
            Func<BigFraction, BigFraction> change = fraction => n.Negate().Div(fraction.Exp()).Add(1); // 1-(n/exp(x))
            do
            {
                prev = x;
                x = x.Sub(change(x));
            } while (prev.Sub(x).Abs() > ExponentialEpsilon);

            return x;
        }

        public BigFraction Log(BigFraction that)
        {
            return Log().Div(that.Log());
        }

        public BigFraction Logp1()
        {
            BigFraction n = this, x = LnApprox(), prev;
            Func<BigFraction, BigFraction> change = fraction => n.Negate().Div(fraction.Sub(1).Exp()).Add(1); // 1-(n/exp(x-1))
            do
            {
                prev = x;
                x = x.Sub(change(x));
            } while (prev.Sub(x).Abs() > ExponentialEpsilon);

            return x;
        }

        #endregion

        #endregion

        #endregion

        #region Comparisons

        public bool Equals(BigFraction other)
        {
            if (Denominator == other.Denominator) return Numerator == other.Numerator;
            return Numerator * other.Denominator == other.Numerator * Denominator;
        }

        public int CompareTo(BigFraction other)
        {
            if (Denominator == other.Denominator)
                return Numerator.CompareTo(other.Numerator);
            if (Numerator == other.Numerator)
                return -Denominator.CompareTo(other.Denominator);
            return (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);
        }

        #region operators

        public static bool operator >(BigFraction left, BigFraction right)
        {
            return left.CompareTo(right) == 1;
        }

        public static bool operator <(BigFraction left, BigFraction right)
        {
            return left.CompareTo(right) == -1;
        }

        #endregion

        #endregion

        public override string ToString()
        {
            return $"{Numerator}/{Denominator}";
        }
    }
}