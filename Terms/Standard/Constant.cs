using System;
using System.Linq;
using EqSolve.Numbers;
using EqSolve.Terms.Meta;

namespace EqSolve.Terms.Standard
{
    public readonly struct ConstantValue<N> : Term<N> where N : INumber<N>
    {
        public Number<N> Value { get; }
        
        public ConstantValue(Number<N> value)
        {
            Value = value;
        }
        
        public bool IsConstant()
        {
            return true;
        }

        public Number<N> Constant()
        {
            return Value;
        }

        public Number<N> Coefficient()
        {
            return Value;
        }

        public Term<N> Multiply(int n)
        {
            return new ConstantValue<N>(Value * Value.FromInt(n));
        }

        public Term<N> Multiply(Number<N> n)
        {
            return new ConstantValue<N>(Value * n);
        }

        public bool CanSimplify(ComplexTerm<N> container)
        {
            return container switch
            {
                Product<N> => true,
                Sum<N> {Terms: { } t} => t.Count(n => n.IsConstant()) > 1,
                Chain<N> => true,
                Quotient<N> => true,
                _ => false
            };
        }

        public Term<N> Simplified(ComplexTerm<N> container)
        {
            switch (container)
            {
                case Product<N> {Terms: { } t}:
                    var constantFactor = t.Where(x => x.IsConstant())
                        .Select(x => x.Constant())
                        .Aggregate((a, b) => a * b);
                    var nonConstantFactor = t.Where(x => !x.IsConstant()).ToArray();
                    nonConstantFactor[0] = nonConstantFactor[0].Multiply(constantFactor);
                    return new Product<N>(nonConstantFactor);
                case Sum<N> {Terms: { } t}:
                    var constantTerm = t.Where(x => x.IsConstant())
                        .Select(x => x.Constant())
                        .Aggregate((a, b) => a + b);
                    return new Sum<N>(t.Where(x => !x.IsConstant()).Append(new ConstantValue<N>(constantTerm)).ToArray());
                case Chain<N> {Outer: ConstantValue<N> c}: return c;
                case Chain<N> {Outer: { } outer, Inner: ConstantValue<N> c}: return new ConstantValue<N>(outer.Function()(c.Value));
                case Quotient<N> {Numerator: ConstantValue<N> n, Denominator: ConstantValue<N> d}: return new ConstantValue<N>(n.Value / d.Value);
                default: return container;
            }
        }

        public Term<N> Derivative()
        {
            return new ConstantValue<N>(Value.FromInt(0));
        }

        public Func<Number<N>, Number<N>> Function()
        {
            var v = Value;
            return _ => v;
        }
    }
}