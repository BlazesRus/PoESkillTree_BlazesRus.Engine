using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Computation.Builders.Entities;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Entities;
using PoESkillTree.Engine.Computation.Common.Builders.Resolving;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.Utils;

namespace PoESkillTree.Engine.Computation.Builders.Stats
{
    public class LeafCoreStatBuilder : ICoreStatBuilder
    {
        private readonly Func<Entity, IStat> _statFactory;
        private readonly IEntityBuilder _entityBuilder;

        public LeafCoreStatBuilder(Func<Entity, IStat> statFactory, IEntityBuilder? entityBuilder = null)
        {
            _statFactory = statFactory;
            _entityBuilder = entityBuilder ?? new ModifierSourceEntityBuilder();
        }

        public static ICoreStatBuilder FromIdentity(
            IStatFactory statFactory, string identity, Type dataType,
            ExplicitRegistrationType? explicitRegistrationType = null) =>
            new LeafCoreStatBuilder(
                entity => statFactory.FromIdentity(identity, entity, dataType, explicitRegistrationType));

        public ICoreStatBuilder Resolve(ResolveContext context) => this;

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new LeafCoreStatBuilder(_statFactory, entityBuilder);

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters)
        {
            var stats = BuildStats(parameters);
            return new[] { new StatBuilderResult(stats, parameters.ModifierSource, Funcs.Identity) };
        }

        private IReadOnlyList<IStat> BuildStats(BuildParameters parameters)
        {
            var entities = _entityBuilder.Build(parameters.ModifierSourceEntity);
            return entities.Select(_statFactory).ToList();
        }
    }
}