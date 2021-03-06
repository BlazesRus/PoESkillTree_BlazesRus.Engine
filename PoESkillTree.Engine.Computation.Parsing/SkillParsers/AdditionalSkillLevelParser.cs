using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Engine.Computation.Common.Builders.Skills;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.GameModel.Skills;

namespace PoESkillTree.Engine.Computation.Parsing.SkillParsers
{
    public class AdditionalSkillLevelParser : AdditionalSkillStatParserBase
    {
        private readonly IGemStatBuilders _gemStatBuilders;
        private readonly IGemTagBuilders _gemTagBuilders;
        private readonly IValueBuilders _valueBuilders;
        private readonly IMetaStatBuilders _metaStatBuilders;
        private readonly ISkillBuilders _skillBuilders;

        public AdditionalSkillLevelParser(
            SkillDefinitions skillDefinitions, IGemStatBuilders gemStatBuilders, IGemTagBuilders gemTagBuilders, IValueBuilders valueBuilders,
            IMetaStatBuilders metaStatBuilders, ISkillBuilders skillBuilders)
            : base(skillDefinitions)
        {
            _gemStatBuilders = gemStatBuilders;
            _gemTagBuilders = gemTagBuilders;
            _valueBuilders = valueBuilders;
            _metaStatBuilders = metaStatBuilders;
            _skillBuilders = skillBuilders;
        }

        protected override IReadOnlyDictionary<Skill, ValueBuilder> Parse(Skill activeSkill, IReadOnlyList<Skill> supportingSkills)
        {
            var dict = supportingSkills.Select(skill => (skill, ParseSupport(skill))).ToDictionary();
            dict[activeSkill] = ParseActive(activeSkill, dict);
            return dict;
        }

        protected override IStatBuilder GetAdditionalStatBuilder(Skill skill) =>
            _gemStatBuilders.AdditionalLevels(skill);

        private ValueBuilder ParseSupport(Skill supportingSkill)
        {
            var value = new ValueBuilder(_valueBuilders.Create(0));
            if (supportingSkill.Gem is null)
                return value;

            value += _gemStatBuilders.AdditionalLevelsForModifierSourceItemSlot().Value
                     + _gemStatBuilders.AdditionalLevels(_skillBuilders.FromId(supportingSkill.Id)).Value;
            
            var baseItem = GetBaseItem(supportingSkill);
            if (baseItem is null)
                return value;

            foreach (var gemTagBuilder in GetGemTagBuilders(baseItem))
            {
                value += _gemStatBuilders.AdditionalLevelsForModifierSourceItemSlot(gemTagBuilder).Value;
            }
            return value;
        }

        private ValueBuilder ParseActive(Skill activeSkill, IReadOnlyDictionary<Skill, ValueBuilder> supportingSkills)
        {
            var value = new ValueBuilder(_valueBuilders.Create(0));
            if (activeSkill.Gem is null)
                return value;

            value += _gemStatBuilders.AdditionalLevelsForModifierSourceItemSlot().Value
                     + _gemStatBuilders.AdditionalActiveLevelsForModifierSourceItemSlot().Value
                     + _gemStatBuilders.AdditionalLevels(_skillBuilders.FromId(activeSkill.Id)).Value;

            var baseItem = GetBaseItem(activeSkill);
            if (baseItem is null)
                return value;

            value += GetAdditionalValueFromSupportingSkills(supportingSkills, baseItem);

            var isSpell = baseItem.GemTags.Contains("spell");
            foreach (var gemTagBuilder in GetGemTagBuilders(baseItem))
            {
                value += _gemStatBuilders.AdditionalActiveLevels(gemTagBuilder).Value
                         + _gemStatBuilders.AdditionalLevelsForModifierSourceItemSlot(gemTagBuilder).Value;
                if (isSpell)
                {
                    value += _gemStatBuilders.AdditionalActiveSpellLevels(gemTagBuilder).Value;
                }
            }
            return value;
        }

        private ValueBuilder GetAdditionalValueFromSupportingSkills(
            IReadOnlyDictionary<Skill, ValueBuilder> supportingSkills, SkillBaseItemDefinition baseItem)
        {
            var valueBuilder = new ValueBuilder(_valueBuilders.Create(0));
            foreach (var (supportingSkill, supportValueBuilder) in supportingSkills)
            {
                valueBuilder += _valueBuilders.If(_metaStatBuilders.SkillIsEnabled(supportingSkill).IsTrue)
                    .Then(supportValueBuilder.Select(d => SelectActiveAdditionalLevels(supportingSkill, (int) d),
                        v => $"SelectActiveAdditionalLevels({supportingSkill.Id}, {supportingSkill.Level}, {v})"))
                    .Else(0);
            }

            return valueBuilder;

            int SelectActiveAdditionalLevels(Skill supportingSkill, int supportAdditionalLevels)
            {
                var value = 0;
                foreach (var untranslatedStat in GetLevelStats(supportingSkill, supportAdditionalLevels))
                {
                    var match = SkillStatIds.SupportedSkillGemLevelRegex.Match(untranslatedStat.StatId);
                    var tag = match.Groups[1].Value;
                    if (tag == "active" || baseItem.GemTags.Contains(tag))
                    {
                        value += untranslatedStat.Value;
                    }
                }

                return value;
            }
        }

        private SkillBaseItemDefinition? GetBaseItem(Skill skill) =>
            GetSkillDefinition(skill.Gem!.SkillId).BaseItem;

        private IEnumerable<IGemTagBuilder> GetGemTagBuilders(SkillBaseItemDefinition baseItem) =>
            baseItem.GemTags.Select(_gemTagBuilders.From);
    }
}