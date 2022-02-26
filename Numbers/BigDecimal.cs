using System;
using System.Numerics;

namespace EqSolve.Numbers
{
    public readonly struct BigDecimal : INumber<BigDecimal>
    {
        #region Fields
        
        public static int
            maxPrecision = 50,
            defaultPrecision = 20,
            expPrecisionScaling = 3,
            expPrecisionBase = 10;
        private static readonly BigInteger Ten = new(10);
        public BigInteger UnscaledValue { get; }
        public int Scale { get; }
        
        #endregion

        #region Initializers

        public BigDecimal(BigInteger value, int scale)
        {
            UnscaledValue = value;
            Scale = scale;
        }
        
        public BigDecimal(BigInteger value) : this(value * BigInteger.Pow(Ten, defaultPrecision), -defaultPrecision) {}

        public static implicit operator BigDecimal(BigInteger value)
        {
            return new BigDecimal(value);
        }
        
        public BigDecimal(int value) : this(value * BigInteger.Pow(Ten, defaultPrecision), -defaultPrecision) {}

        public static implicit operator BigDecimal(int value)
        {
            return new BigDecimal(value);
        }

        public static implicit operator BigDecimal(long value)
        {
            return new BigDecimal(value);
        }

        #endregion

        #region Operations

        #region Arithmetic

        public BigDecimal Add(BigDecimal that)
        {
            return (Scale - that.Scale) switch
            {
                > 0 => new BigDecimal(
                    UnscaledValue * (int) Math.Pow(10, Scale - that.Scale) + that.UnscaledValue,
                    that.Scale
                ),
                < 0 => new BigDecimal(
                    UnscaledValue + that.UnscaledValue * (int) Math.Pow(10, that.Scale - Scale),
                    Scale
                ),
                0 => new BigDecimal(
                    UnscaledValue + that.UnscaledValue,
                    Scale
                )
            };
        }
        
        public BigDecimal Negate()
        {
            return new BigDecimal(-UnscaledValue, Scale);
        }

        public BigDecimal Sub(BigDecimal that)
        {
            return Add(that.Negate());
        }

        public BigDecimal Abs()
        {
            return new(BigInteger.Abs(UnscaledValue), Scale);
        }

        #endregion

        #region Geometric

        public BigDecimal Mul(BigDecimal that)
        {
            return new BigDecimal(
                UnscaledValue * that.UnscaledValue,
                Scale + that.Scale
            ).Normalize();
        }
        
        public BigDecimal Div(BigDecimal that)
        {
            var output = ScaledTo(that.Scale-maxPrecision);
            return new BigDecimal(
                output.UnscaledValue / that.UnscaledValue,
                -maxPrecision
            ).Normalize();
        }

        #endregion

        #region Exponential

        #region Exponents

        public BigDecimal Exp()
        {
            BigDecimal total = new(BigInteger.Pow(Ten, maxPrecision), -maxPrecision), term = 1;

            for (long i = 1;
                 term != 0
                 && i < expPrecisionBase
                 + expPrecisionScaling * UnscaledValue.GetBitLength();
                 // Use more terms for bigger numbers. This improves accuracy.
                 i++)
            {
                term = term.Mul(this).Div(i);
                total = total.Add(term);
            }

            return total;
        }

        public BigDecimal Expm1()
        {
            BigDecimal total = new(0, -maxPrecision), term = 1;

            for (long i = 1;
                 term != 0
                 && i < expPrecisionBase
                 + expPrecisionScaling * UnscaledValue.GetBitLength();
                 // Use more terms for bigger numbers. This improves accuracy.
                 i++)
            {
                term = term.Mul(this).Div(i);
                total = total.Add(term);
            }

            return total;
        }

        public BigDecimal Pow(BigDecimal exp)
        {
            return Log().Mul(exp).Exp();
        }

        #endregion

        #region Logarithms

        /// <summary>
        /// Approximates the natural logarithm.
        /// </summary>
        /// <returns>An approximation of ln(this).
        /// Shouldn't be off by too much, but also shouldn't be treated as precise.</returns>
        public BigDecimal LnApprox()
        {
            // ln 2 ~= 277/400 ~= 7/10
            // ln 10 ~= 921/400 ~= 7/3
            // 277 and 921. anagrams. weird.
            BigDecimal unscaledLog = UnscaledValue.GetBitLength();
            unscaledLog = unscaledLog.Mul(410).Div(291);
            BigDecimal scaleLog = Scale;
            scaleLog = scaleLog.Mul(410).Div(921);
            return unscaledLog.Add(scaleLog); // note that the actual value would be closer to 400, but 420 is funnier
            // also Newton's method works much better with overestimates
        }

        public BigDecimal Log()
        {
            BigDecimal n = this;
            BigDecimal x = LnApprox().Add(1); // computationally inexpensive, uses simple approximations of ln(10) and of ln(2)
            Func<BigDecimal, BigDecimal> f = @decimal => @decimal.Exp().Sub(n);
            Func<BigDecimal, BigDecimal> deriv = @decimal => @decimal.Exp();
            Func<BigDecimal, BigDecimal> change = @decimal => f(@decimal).Div(deriv(@decimal));
            // newton's method setup: establish f and f', get x0.
            // then, calculate.
            for (int i = 0; i < expPrecisionBase + UnscaledValue.GetBitLength() + Math.Abs(Scale); i++)
            {
                x = x.Sub(change(x)); // it shouldn't be necessary to iterate too much.
                // iterating further w/ larger scale & bit length compensates for how shitty LnApprox is.
            }
            // halley's method could be used here. that said, i prefer newton's method.

            return x;
        }

        public BigDecimal Logp1()
        {
            BigDecimal n = this;
            BigDecimal x = LnApprox(); // computationally inexpensive, uses simple approximations of ln(10) and of ln(2)
            Func<BigDecimal, BigDecimal> f = @decimal => @decimal.Sub(1).Exp().Sub(n);
            Func<BigDecimal, BigDecimal> deriv = @decimal => @decimal.Sub(1).Exp();
            Func<BigDecimal, BigDecimal> change = @decimal => f(@decimal).Div(deriv(@decimal));
            // newton's method setup: establish f and f', get x0.
            // then, calculate.
            for (int i = 0; i < expPrecisionBase + UnscaledValue.GetBitLength() + Math.Abs(Scale); i++)
            {
                x = x.Sub(change(x)); // it shouldn't be necessary to iterate too much.
                // iterating further w/ larger scale & bit length compensates for how shitty LnApprox is.
            }

            return x;
        }

        public BigDecimal Log(BigDecimal @base)
        {
            return Log().Div(@base.Log());
        }

        #endregion
        
        #endregion

        public BigDecimal ScaledTo(int scale)
        {
            return new BigDecimal(
                UnscaledValue * BigInteger.Pow(Ten, Scale - scale),
                scale
            );
        }
        
        public BigDecimal Normalize()
        {
            BigInteger outUnscaledValue = UnscaledValue;
            int outScale = Scale;
            while (outUnscaledValue % Ten == 0)
            {
                outUnscaledValue /= 10;
                outScale++;
            }

            if (outScale < -maxPrecision)
                outUnscaledValue /= BigInteger.Pow(10, -maxPrecision - outScale);

            return new BigDecimal(outUnscaledValue, outScale);
        }

        #endregion

        #region Comparisons

        public static bool operator !=(BigDecimal left, BigDecimal right)
        {
            if (left.Scale == right.Scale) return left.UnscaledValue != right.UnscaledValue;
            if (left.UnscaledValue == right.UnscaledValue) return left.Scale != right.Scale;
            if (left.Scale > right.Scale) left = left.ScaledTo(right.Scale);
            else right = right.ScaledTo(left.Scale);
            return left.UnscaledValue != right.UnscaledValue;
        }

        public static bool operator ==(BigDecimal left, BigDecimal right)
        {
            if (left.Scale == right.Scale) return left.UnscaledValue == right.UnscaledValue;
            if (left.UnscaledValue == right.UnscaledValue) return left.Scale == right.Scale;
            if (left.Scale > right.Scale) left = left.ScaledTo(right.Scale);
            else right = right.ScaledTo(left.Scale);
            return left.UnscaledValue == right.UnscaledValue;
        }
        
        public int CompareTo(BigDecimal other)
        {
            if (Scale == other.Scale)
                return UnscaledValue.CompareTo(other.UnscaledValue);
            if (UnscaledValue == other.UnscaledValue)
                return Scale.CompareTo(other.Scale);
            if (Scale > other.Scale)
                return ScaledTo(other.Scale).UnscaledValue.CompareTo(other.UnscaledValue);
            return other.ScaledTo(Scale).UnscaledValue.CompareTo(UnscaledValue);
        }

        public bool Equals(BigDecimal other)
        {
            BigDecimal self = this;
            if (self.Scale == other.Scale) return self.UnscaledValue == other.UnscaledValue;
            if (self.UnscaledValue == other.UnscaledValue) return self.Scale == other.Scale;
            if (self.Scale > other.Scale) self = self.ScaledTo(other.Scale);
            else other = other.ScaledTo(self.Scale);
            return self.UnscaledValue == other.UnscaledValue;
        }

        public override bool Equals(object that)
        {
            if (that is BigDecimal bd) return bd == this;
            return false; // fuck every other numeric type
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UnscaledValue.GetHashCode(), Scale.GetHashCode());
        }

        public static bool operator >(BigDecimal left, BigDecimal right)
        {
            return left.CompareTo(right) == 1;
        }

        public static bool operator <(BigDecimal left, BigDecimal right)
        {
            return right.CompareTo(left) == 1;
        }

        #endregion

        #region Stringification

        public new string ToString()
        {
            BigDecimal normalized = Normalize();
            BigInteger poweredScale = BigInteger.Pow(Ten, -normalized.Scale);
            return Scale switch
            {
                0 => normalized.UnscaledValue.ToString(),
                > 0 => normalized.UnscaledValue.ToString() + '0' * Scale,
                < 0 => normalized.UnscaledValue / poweredScale + "." + normalized.UnscaledValue % poweredScale
            };
        }

        public static implicit operator string(BigDecimal value)
        {
            return value.ToString();
        }

        #endregion
    }
}