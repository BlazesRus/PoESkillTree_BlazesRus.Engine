using System.Collections.Generic;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils.Extensions;

namespace PoESkillTree.Engine.Computation.Builders.Buffs
{
    internal class BuffRestrictions
    {
        private readonly IReadOnlyList<Keyword> _restrictedToKeywords;
        private readonly IReadOnlyList<Keyword> _excludedKeywords;

        public BuffRestrictions(IReadOnlyList<Keyword> restrictedToKeywords, IReadOnlyList<Keyword> excludedKeywords)
        {
            _restrictedToKeywords = restrictedToKeywords;
            _excludedKeywords = excludedKeywords;
        }

        public bool AllowsBuff(BuffBuilderWithKeywords buff) =>
            buff.Keywords.ContainsAll(_restrictedToKeywords) && !_excludedKeywords.ContainsAny(buff.Keywords);
    }
}