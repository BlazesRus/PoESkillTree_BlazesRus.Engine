using System;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Engine.Computation.Builders
{
    public interface ICoreBuilder<out TResult> : IResolvable<ICoreBuilder<TResult>>
    {
        TResult Build(BuildParameters parameters);
    }

    // As opposed to the constructors they call, these methods allow generic type inference.
    public static class CoreBuilder
    {
        public static ICoreBuilder<TResult> Create<TResult>(TResult result) =>
            new ConstantCoreBuilder<TResult>(result);

        public static ICoreBuilder<TResult> Create<TResult>(Func<BuildParameters, TResult> build) =>
            new FunctionalCoreBuilder<TResult>(build);

        public static ICoreBuilder<TResult>
            Create<TParameter, TResult>(TParameter parameter, Func<BuildParameters, TParameter, TResult> build)
            where TParameter : class, IResolvable<TParameter>
        {
            return new ParametrisedCoreBuilder<TParameter, TResult>(parameter, build);
        }

        public static ICoreBuilder<TOut> UnaryOperation<TIn, TOut>(ICoreBuilder<TIn> operand, Func<TIn, TOut> @operator)
        {
            return new UnaryOperatorCoreBuilder<TIn, TOut>(operand, @operator);
        }

        public static ICoreBuilder<TResult> BinaryOperation<TResult>(
            ICoreBuilder<TResult> left, ICoreBuilder<TResult> right, Func<TResult, TResult, TResult> @operator)
        {
            return new BinaryOperatorCoreBuilder<TResult>(left, right, @operator);
        }

        public static ICoreBuilder<TResult> Proxy<TProxied, TResult>(
            TProxied proxiedBuilder, Func<BuildParameters, TProxied, TResult> build)
            where TProxied : class, IResolvable<TProxied>
        {
            return new ProxyCoreBuilder<TProxied, TResult>(proxiedBuilder, build);
        }

        public static ICoreBuilder<TResult> Proxy<TProxied, TResolve, TResult>(
            TProxied proxiedBuilder, Func<BuildParameters, TProxied, TResult> build)
            where TProxied : class, IResolvable<TResolve>, TResolve
            where TResolve : class
        {
            return new ProxyCoreBuilder<TProxied, TResolve, TResult>(proxiedBuilder, build);
        }
    }

    internal class ConstantCoreBuilder<TResult> : ICoreBuilder<TResult>
    {
        private readonly TResult _result;

        public ConstantCoreBuilder(TResult result) => _result = result;

        public ICoreBuilder<TResult> Resolve(ResolveContext context) => this;

        public TResult Build(BuildParameters parameters) => _result;
    }

    internal class FunctionalCoreBuilder<TResult> : ICoreBuilder<TResult>
    {
        private readonly Func<BuildParameters, TResult> _build;

        public FunctionalCoreBuilder(Func<BuildParameters, TResult> build) => _build = build;

        public ICoreBuilder<TResult> Resolve(ResolveContext context) => this;

        public TResult Build(BuildParameters parameters) => _build(parameters);
    }

    internal class ParametrisedCoreBuilder<TParameter, TResult> : ICoreBuilder<TResult>
        where TParameter : class, IResolvable<TParameter>
    {
        private readonly TParameter _parameter;
        private readonly Func<BuildParameters, TParameter, TResult> _build;

        public ParametrisedCoreBuilder(TParameter parameter, Func<BuildParameters, TParameter, TResult> build)
        {
            _parameter = parameter;
            _build = build;
        }

        public ICoreBuilder<TResult> Resolve(ResolveContext context) =>
            new ParametrisedCoreBuilder<TParameter, TResult>(_parameter.Resolve(context), _build);

        public TResult Build(BuildParameters parameters) => _build(parameters, _parameter);
    }

    internal class UnaryOperatorCoreBuilder<TIn, TOut> : ICoreBuilder<TOut>
    {
        private readonly ICoreBuilder<TIn> _operand;
        private readonly Func<TIn, TOut> _operator;

        public UnaryOperatorCoreBuilder(ICoreBuilder<TIn> operand, Func<TIn, TOut> @operator)
        {
            _operand = operand;
            _operator = @operator;
        }

        public ICoreBuilder<TOut> Resolve(ResolveContext context) =>
            new UnaryOperatorCoreBuilder<TIn, TOut>(_operand.Resolve(context), _operator);

        public TOut Build(BuildParameters parameters) =>
            _operator(_operand.Build(parameters));
    }

    internal class BinaryOperatorCoreBuilder<TResult> : ICoreBuilder<TResult>
    {
        private readonly ICoreBuilder<TResult> _left;
        private readonly ICoreBuilder<TResult> _right;
        private readonly Func<TResult, TResult, TResult> _operator;

        public BinaryOperatorCoreBuilder(
            ICoreBuilder<TResult> left, ICoreBuilder<TResult> right, Func<TResult, TResult, TResult> @operator)
        {
            _left = left;
            _right = right;
            _operator = @operator;
        }

        public ICoreBuilder<TResult> Resolve(ResolveContext context) =>
            new BinaryOperatorCoreBuilder<TResult>(_left.Resolve(context), _right.Resolve(context), _operator);

        public TResult Build(BuildParameters parameters) =>
            _operator(_left.Build(parameters), _right.Build(parameters));
    }

    internal class ProxyCoreBuilder<TProxied, TResult> : ICoreBuilder<TResult>
        where TProxied : class, IResolvable<TProxied>
    {
        private readonly TProxied _proxiedBuilder;
        private readonly Func<BuildParameters, TProxied, TResult> _build;

        public ProxyCoreBuilder(TProxied proxiedBuilder, Func<BuildParameters, TProxied, TResult> build)
        {
            _proxiedBuilder = proxiedBuilder;
            _build = build;
        }

        public ICoreBuilder<TResult> Resolve(ResolveContext context) =>
            new ProxyCoreBuilder<TProxied, TResult>(_proxiedBuilder.Resolve(context), _build);

        public TResult Build(BuildParameters parameters) => _build(parameters, _proxiedBuilder);
    }

    internal class ProxyCoreBuilder<TProxied, TResolve, TResult> : ICoreBuilder<TResult>
        where TProxied : class, IResolvable<TResolve>, TResolve
        where TResolve : class
    {
        private readonly TProxied _proxiedBuilder;
        private readonly Func<BuildParameters, TProxied, TResult> _build;

        public ProxyCoreBuilder(TProxied proxiedBuilder, Func<BuildParameters, TProxied, TResult> build)
        {
            _proxiedBuilder = proxiedBuilder;
            _build = build;
        }

        public ICoreBuilder<TResult> Resolve(ResolveContext context) =>
            new ProxyCoreBuilder<TProxied, TResolve, TResult>((TProxied) _proxiedBuilder.Resolve(context), _build);

        public TResult Build(BuildParameters parameters) => _build(parameters, _proxiedBuilder);
    }
}