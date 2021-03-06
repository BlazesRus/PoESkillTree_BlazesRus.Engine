using System;
using PoESkillTree.Engine.Computation.Builders.Conditions;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Conditions;
using PoESkillTree.Engine.Computation.Common.Builders.Resolving;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.Utils.Extensions;

namespace PoESkillTree.Engine.Computation.Builders.Values
{
    // ("Impl" suffix to avoid confusion with ValueBuilder in Common)
    public class ValueBuilderImpl : IValueBuilder
    {
        private readonly Func<BuildParameters, IValue> _buildValue;
        private readonly Func<ResolveContext, IValueBuilder> _resolve;

        public ValueBuilderImpl(double? value) : this(new Constant(value))
        {
        }

        public ValueBuilderImpl(IValue value) : this(_ => value)
        {
        }

        private ValueBuilderImpl(Func<BuildParameters, IValue> buildValue)
        {
            _buildValue = buildValue;
            _resolve = _ => this;
        }

        public ValueBuilderImpl(
            Func<BuildParameters, IValue> buildValue, Func<ResolveContext, Func<BuildParameters, IValue>> resolve)
            : this(buildValue, c => new ValueBuilderImpl(resolve(c)))
        {
        }

        public ValueBuilderImpl(Func<BuildParameters, IValue> buildValue, Func<ResolveContext, IValueBuilder> resolve)
        {
            _buildValue = buildValue;
            _resolve = resolve;
        }

        public IValueBuilder Resolve(ResolveContext context) => _resolve(context);

        public IValueBuilder MaximumOnly =>
            Create(this, o => o.Select(v => new NodeValue(0, v.Maximum)), o => o + ".MaximumOnly");

        public IValueBuilder Average =>
            Create(this, o => o.Select(v => new NodeValue((v.Minimum + v.Maximum) / 2)), o => o + ".Average");

        public IConditionBuilder Eq(IValueBuilder other) =>
            ValueConditionBuilder.Create(this, other, (left, right) => left == right, (l, r) =>  l + " == " + r);

        public IConditionBuilder GreaterThan(IValueBuilder other) =>
            ValueConditionBuilder.Create(this, other, 
                (left, right) => left.GetValueOrDefault() > right.GetValueOrDefault(), (l, r) => l + " > " + r);

        public IValueBuilder Add(IValueBuilder other) =>
            Create(this, other, (left, right) => left().SumWhereNotNull(right()), (l, r) => l + " + " + r);

        public IValueBuilder Multiply(IValueBuilder other) =>
            Create(this, other, CalculateMultiply, (l, r) => l + " * " + r);

        private static NodeValue? CalculateMultiply(Func<NodeValue?> leftFunc, Func<NodeValue?> rightFunc)
            => leftFunc() is NodeValue left ? left * rightFunc() : null;

        public IValueBuilder DivideBy(IValueBuilder divisor) =>
            Create(this, divisor, CalculateDivideBy, (l, r) => l + " / " + r);

        private static NodeValue? CalculateDivideBy(Func<NodeValue?> leftFunc, Func<NodeValue?> rightFunc)
            => leftFunc() is NodeValue left ? left / rightFunc() : null;

        public IValueBuilder If(IValue condition) =>
            Create(this, new ValueBuilderImpl(condition), CalculateIf, (l, r) => r + " ? " + l + " : null");

        private static NodeValue? CalculateIf(Func<NodeValue?> value, Func<NodeValue?> condition)
            => condition().IsTrue() ? value() : null;

        public IValueBuilder Select(Func<NodeValue, NodeValue> selector, Func<IValue, string> identity) =>
            Create(this, o => o.Select(selector), identity);

        public IValueBuilder Create(double value) => new ValueBuilderImpl(value);

        public IValue Build(BuildParameters parameters) => _buildValue(parameters);


        private static IValueBuilder Create(
            IValueBuilder operand,
            Func<NodeValue?, NodeValue?> calculate,
            Func<IValue, string> identity) =>
            new ValueBuilderImpl(
                ps => Build(ps, operand, calculate, identity),
                c => (ps => Build(ps, operand.Resolve(c), calculate, identity)));

        public static IValueBuilder Create(
            IValueBuilder left, IValueBuilder right,
            Func<Func<NodeValue?>, Func<NodeValue?>, NodeValue?> calculate,
            Func<IValue, IValue, string> identity) =>
            new ValueBuilderImpl(
                ps => Build(ps, left, right, calculate, identity),
                c => (ps => Build(ps, left.Resolve(c), right.Resolve(c), calculate, identity)));

        private static IValue Build(
            BuildParameters parameters,
            IValueBuilder operand,
            Func<NodeValue?, NodeValue?> calculate,
            Func<IValue, string> identity)
        {
            var builtOperand = operand.Build(parameters);
            return new FunctionalValue(c => calculate(builtOperand.Calculate(c)), identity(builtOperand));
        }

        private static IValue Build(
            BuildParameters parameters,
            IValueBuilder left, IValueBuilder right,
            Func<Func<NodeValue?>, Func<NodeValue?>, NodeValue?> calculate,
            Func<IValue, IValue, string> identity)
        {
            var l = left.Build(parameters);
            var r = right.Build(parameters);
            return new FunctionalValue(c => calculate(() => l.Calculate(c), () => r.Calculate(c)), identity(l, r));
        }
    }
}