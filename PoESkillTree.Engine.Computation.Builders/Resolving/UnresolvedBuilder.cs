using System;
using System.Collections.Generic;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Builders.Values;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Entities;
using PoESkillTree.Engine.Computation.Common.Builders.Resolving;
using PoESkillTree.Engine.Computation.Common.Builders.Skills;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.Computation.Common.Parsing;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils;

namespace PoESkillTree.Engine.Computation.Builders.Resolving
{
    public class UnresolvedBuilder<TResolve, TBuild> : IResolvable<TResolve>
        where TResolve : class
    {
        protected string Description { get; }
        protected Func<ResolveContext, TResolve> Resolver { get; }

        public UnresolvedBuilder(string description, Func<ResolveContext, TResolve> resolver)
        {
            Description = description;
            Resolver = resolver;
        }

        public TResolve Resolve(ResolveContext context) =>
            Resolver(context);

        public TBuild Build(BuildParameters parameters) => 
            throw new UnresolvedException(Description);

        public override string ToString() => Description;
    }

    public class UnresolvedException : ParseException
    {
        public UnresolvedException(string message) 
            : base("Builder must be resolved before being built, " + message)
        {
        }
    }

    internal class UnresolvedKeywordBuilder : UnresolvedBuilder<IKeywordBuilder, Keyword>, IKeywordBuilder
    {
        public UnresolvedKeywordBuilder(string description, Func<ResolveContext, IKeywordBuilder> resolver) 
            : base(description, resolver)
        {
        }
    }

    internal class UnresolvedGemTagBuilder : UnresolvedBuilder<IGemTagBuilder, string>, IGemTagBuilder
    {
        public UnresolvedGemTagBuilder(string description, Func<ResolveContext, IGemTagBuilder> resolver) : base(description, resolver)
        {
        }
    }

    public class UnresolvedValueBuilder : ValueBuilderImpl
    {
        private readonly string _description;

        public UnresolvedValueBuilder(string description, Func<ResolveContext, IValueBuilder> resolver)
            : base(_ => throw new UnresolvedException(description), resolver)
        {
            _description = description;
        }

        public override string ToString() => _description;
    }

    public class UnresolvedCoreBuilder<TResult>
        : UnresolvedBuilder<ICoreBuilder<TResult>, TResult>, ICoreBuilder<TResult>
    {
        public UnresolvedCoreBuilder(string description, Func<ResolveContext, ICoreBuilder<TResult>> resolver)
            : base(description, resolver)
        {
        }
    }

    internal class UnresolvedCoreStatBuilder
        : UnresolvedBuilder<ICoreStatBuilder, IEnumerable<StatBuilderResult>>, ICoreStatBuilder
    {
        public UnresolvedCoreStatBuilder(string description, Func<ResolveContext, ICoreStatBuilder> resolver)
            : base(description, resolver)
        {
        }

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new UnresolvedCoreStatBuilder(Description, Resolver.AndThen(b => b.WithEntity(entityBuilder)));
    }
}