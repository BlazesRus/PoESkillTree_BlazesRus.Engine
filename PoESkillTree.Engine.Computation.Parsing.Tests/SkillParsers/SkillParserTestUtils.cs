using System.Collections.Generic;
using System.Linq;
using Moq;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;

namespace PoESkillTree.Engine.Computation.Parsing.SkillParsers
{
    public static class SkillParserTestUtils
    {
        public static SkillDefinition CreateActive(
            string id, ActiveSkillDefinition activeSkill, IReadOnlyDictionary<int, SkillLevelDefinition> levels)
            => SkillDefinition.CreateActive(id, 0, "", null, new[] { "" }, null, activeSkill, levels);

        public static SkillDefinition CreateSupport(
            string id, SupportSkillDefinition supportSkill, IReadOnlyDictionary<int, SkillLevelDefinition> levels)
            => SkillDefinition.CreateSupport(id, 0, "", null, new[] { "" }, null, supportSkill, levels);

        public static ActiveSkillDefinition CreateActiveSkillDefinition(
            string displayName, IEnumerable<string> activeSkillTypes, IReadOnlyList<Keyword> keywords,
            bool providesBuff = false, IReadOnlyList<ItemClass>? weaponRestrictions = null)
            => CreateActiveSkillDefinition(displayName, null, activeSkillTypes, keywords,
                providesBuff: providesBuff, weaponRestrictions: weaponRestrictions);

        public static SkillLevelDefinition CreateLevelDefinition(
            double? damageEffectiveness = null, double? damageMultiplier = null, double? criticalStrikeChance = null,
            int? attackSpeedMultiplier = null,
            int? manaCost = null, double? manaMultiplier = null, int? manaCostOverride = null, int? cooldown = null, bool canBypassCooldown = false,
            int requiredLevel = 0, int requiredDexterity = 0, int requiredIntelligence = 0, int requiredStrength = 0,
            IReadOnlyList<UntranslatedStat>? qualityStats = null, IReadOnlyList<UntranslatedStat>? stats = null,
            IReadOnlyList<IReadOnlyList<UntranslatedStat>>? additionalStatsPerPart = null,
            IReadOnlyList<BuffStat>? qualityBuffStats = null, IReadOnlyList<BuffStat>? buffStats = null,
            IReadOnlyList<UntranslatedStat>? qualityPassiveStats = null,
            IReadOnlyList<UntranslatedStat>? passiveStats = null,
            SkillTooltipDefinition? tooltip = null)
            => new SkillLevelDefinition(damageEffectiveness, damageMultiplier, criticalStrikeChance,
                attackSpeedMultiplier, manaCost,
                manaMultiplier, manaCostOverride, cooldown, canBypassCooldown, requiredLevel, requiredDexterity, requiredIntelligence,
                requiredStrength, qualityStats ?? new UntranslatedStat[0], stats ?? new UntranslatedStat[0],
                additionalStatsPerPart ?? new[] { new UntranslatedStat[0]}, qualityBuffStats ?? new BuffStat[0],
                buffStats ?? new BuffStat[0], qualityPassiveStats ?? new UntranslatedStat[0],
                passiveStats ?? new UntranslatedStat[0], tooltip!);

        public static ActiveSkillDefinition CreateActiveSkillDefinition(
            string displayName, int? castTime = null, IEnumerable<string>? activeSkillTypes = null,
            IReadOnlyList<Keyword>? keywords = null, IReadOnlyList<IReadOnlyList<Keyword>>? keywordsPerPart = null,
            bool providesBuff = false, double? totemLifeMultiplier = null,
            IReadOnlyList<ItemClass>? weaponRestrictions = null)
        {
            keywords ??= new Keyword[0];
            return new ActiveSkillDefinition(displayName, castTime ?? 0, activeSkillTypes ?? new string[0],
                new string[0], keywords, keywordsPerPart ?? new[] { keywords }, providesBuff, totemLifeMultiplier,
                weaponRestrictions ?? new ItemClass[0]);
        }

        public static SupportSkillDefinition CreateSupportSkillDefinition(
            IEnumerable<string>? allowedActiveSkillTypes = null, IEnumerable<string>? addedActiveSkillTypes = null,
            IReadOnlyList<Keyword>? addedKeywords = null)
            => new SupportSkillDefinition(false,
                allowedActiveSkillTypes ?? new string[0], new string[0], 
                addedActiveSkillTypes ?? new string[0], addedKeywords ?? new Keyword[0]);

        public static IValueCalculationContext MockValueCalculationContextForMainSkill(
            Skill skill, params (string stat, double? value)[] nodeValues)
            => MockValueCalculationContext(skill, true, false, nodeValues);

        public static IValueCalculationContext MockValueCalculationContextForActiveSkill(
            Skill skill, params (string stat, double? value)[] nodeValues)
            => MockValueCalculationContext(skill, false, true, nodeValues);

        public static IValueCalculationContext MockValueCalculationContextForInactiveSkill(
            Skill skill, params (string stat, double? value)[] nodeValues)
            => MockValueCalculationContext(skill, false, false, nodeValues);

        public static IValueCalculationContext MockValueCalculationContext(
            Skill skill, bool isMainSkill, bool isActiveSkill)
            => MockValueCalculationContext(skill, isMainSkill, isActiveSkill, new (string, double?)[0]);

        public static IValueCalculationContext MockValueCalculationContext(
            Skill skill, bool isMainSkill, bool isActiveSkill, params (string stat, double? value)[] nodeValues)
            => MockValueCalculationContext(skill, isMainSkill, isActiveSkill,
                nodeValues.Select(t => (t.stat, default(Entity), t.value)).ToArray());

        public static IValueCalculationContext MockValueCalculationContext(
            Skill skill, bool isMainSkill, bool isActiveSkill,
            params (string stat, Entity entity, double? value)[] nodeValues)
        {
            var contextMock = new Mock<IValueCalculationContext>();
            var mainSkillItemSlotStat = new Stat("MainSkill.ItemSlot");
            var mainSkillItemSlot = isMainSkill ? skill.ItemSlot : ItemSlot.Unequipable;
            contextMock.Setup(c => c.GetValue(mainSkillItemSlotStat, NodeType.Total, PathDefinition.MainPath))
                .Returns((NodeValue?) (double) mainSkillItemSlot);
            var mainSkillSocketIndexStat = new Stat("MainSkillSocketIndex");
            contextMock.Setup(c => c.GetValue(mainSkillSocketIndexStat, NodeType.Total, PathDefinition.MainPath))
                .Returns((NodeValue?) skill.SocketIndex);
            var mainSkillSkillIndexStat = new Stat("MainSkillSkillIndex");
            contextMock.Setup(c => c.GetValue(mainSkillSkillIndexStat, NodeType.Total, PathDefinition.MainPath))
                .Returns((NodeValue?) skill.SkillIndex);
            var activeSkillItemSlotStat = new Stat($"{skill.Id}.ActiveSkillItemSlot");
            var activeSkillItemSlot = isActiveSkill ? skill.ItemSlot : ItemSlot.Unequipable;
            contextMock.Setup(c => c.GetValue(activeSkillItemSlotStat, NodeType.Total, PathDefinition.MainPath))
                .Returns((NodeValue?) (double) activeSkillItemSlot);
            var activeSkillSocketIndexStat = new Stat($"{skill.Id}.ActiveSkillSocketIndex");
            var activeSkillSocketIndex = isActiveSkill ? skill.SocketIndex : -1;
            contextMock.Setup(c => c.GetValue(activeSkillSocketIndexStat, NodeType.Total, PathDefinition.MainPath))
                .Returns((NodeValue?) activeSkillSocketIndex);
            foreach (var (statIdentity, entity, value) in nodeValues)
            {
                var stat = new Stat(statIdentity, entity);
                contextMock.Setup(c => c.GetValue(stat, NodeType.Total, PathDefinition.MainPath))
                    .Returns((NodeValue?) value);
            }
            return contextMock.Object;
        }

        public static UntranslatedStatParserParameter EmptyParserParameter(ModifierSource.Local.Skill source)
            => new UntranslatedStatParserParameter(source, new UntranslatedStat[0]);
    }
}