using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Computation.Common.Builders.Skills;
using PoESkillTree.Engine.Computation.Common.Data;
using PoESkillTree.Engine.Computation.Data.Base;
using PoESkillTree.Engine.GameModel.Skills;

namespace PoESkillTree.Engine.Computation.Data
{
    public class GemTagMatchers : ReferencedMatchersBase<IGemTagBuilder>
    {
        private static readonly string[] UntranslatedTagIdsToMatch = {"dexterity", "intelligence", "strength"};

        private readonly GemTags _gemTags;
        private readonly IGemTagBuilders _gemTagBuilders;

        public GemTagMatchers(GemTags gemTags, IGemTagBuilders gemTagBuilders)
        {
            _gemTags = gemTags;
            _gemTagBuilders = gemTagBuilders;
        }

        protected override IReadOnlyList<ReferencedMatcherData> CreateCollection()
        {
            return _gemTags.Tags
                .Select(t => t.Translation is null && UntranslatedTagIdsToMatch.Contains(t.InternalId)
                    ? new GemTag(t.InternalId, t.InternalId)
                    : t)
                .Where(t => t.Translation != null)
                .Select(t => new ReferencedMatcherData(t.Translation!, _gemTagBuilders.From(t.InternalId)))
                .ToList();
        }
    }
}