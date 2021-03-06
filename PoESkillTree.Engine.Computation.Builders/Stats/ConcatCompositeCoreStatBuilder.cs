using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Entities;
using PoESkillTree.Engine.Computation.Common.Builders.Resolving;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;

namespace PoESkillTree.Engine.Computation.Builders.Stats
{
    public class ConcatCompositeCoreStatBuilder : ICoreStatBuilder
    {
        private readonly IReadOnlyList<ICoreStatBuilder> _items;

        public ConcatCompositeCoreStatBuilder(params ICoreStatBuilder[] items) =>
            _items = items;

        private ConcatCompositeCoreStatBuilder Select(Func<ICoreStatBuilder, ICoreStatBuilder> selector) =>
            new ConcatCompositeCoreStatBuilder(_items.Select(selector).ToArray());

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            Select(i => i.Resolve(context));

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            Select(i => i.WithEntity(entityBuilder));

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters) =>
            _items.SelectMany(b => b.Build(parameters));
    }
}