using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Computation.Builders.Entities;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Entities;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Engine.Computation.Builders.Stats
{
    internal static class StatBuilderHelper
    {
        public static ICoreStatBuilder CreateStatBuilder(string identity, params Entity[] entities) =>
            CreateStatBuilder(identity, new EntityBuilder(entities));

        public static ICoreStatBuilder CreateStatBuilder(string identity, IEntityBuilder? entityBuilder = null)
        {
            IStat CreateStat(Entity entity) => new Stat(identity, entity);
            return new LeafCoreStatBuilder(CreateStat, entityBuilder);
        }

        public static ICoreStatBuilder CreateStatBuilder(IStat stat, IEntityBuilder? entityBuilder = null) =>
            new LeafCoreStatBuilder(_ => stat, entityBuilder);

        public static IStat BuildToSingleStat(this IStatBuilder @this,
            ModifierSource? modifierSource = null, Entity entity = default) =>
            @this.BuildToSingleResult(modifierSource, entity).Stats.Single();

        public static StatBuilderResult BuildToSingleResult(this IStatBuilder @this,
            ModifierSource? modifierSource = null, Entity entity = default) =>
            @this.Build(new BuildParameters(modifierSource!, entity, default)).Single();

        public static IReadOnlyList<IStat> BuildToStats(this IStatBuilder @this,
            ModifierSource? modifierSource = null, Entity entity = default) =>
            @this.Build(new BuildParameters(modifierSource!, entity, default)).SelectMany(r => r.Stats).ToList();
    }
}