using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.Utils;

namespace PoESkillTree.Engine.GameModel.Skills
{
    public class SkillDefinition : IDefinition<string>
    {
        private SkillDefinition(
            string id, int numericId, bool isSupport, string statTranslationFile, string? secondarySkillId, IReadOnlyList<string> partNames,
            SkillBaseItemDefinition? baseItem, ActiveSkillDefinition? activeSkill, SupportSkillDefinition? supportSkill,
            IReadOnlyDictionary<int, SkillLevelDefinition> levels)
            => (Id, NumericId, IsSupport, PartNames, BaseItem, ActiveSkill, SupportSkill, Levels, StatTranslationFile, SecondarySkillId) =
                (id, numericId, isSupport, partNames, baseItem, activeSkill!, supportSkill!, levels, statTranslationFile, secondarySkillId);

        public static SkillDefinition CreateActive(
            string id, int numericId, string statTranslationFile, string? secondarySkillId, IReadOnlyList<string> partNames,
            SkillBaseItemDefinition? baseItem, ActiveSkillDefinition activeSkill,
            IReadOnlyDictionary<int, SkillLevelDefinition> levels)
            => new SkillDefinition(id, numericId, false, statTranslationFile, secondarySkillId, partNames, baseItem,
                activeSkill, null, levels);

        public static SkillDefinition CreateSupport(
            string id, int numericId, string statTranslationFile, string? secondarySkillId, IReadOnlyList<string> partNames,
            SkillBaseItemDefinition? baseItem, SupportSkillDefinition supportSkill,
            IReadOnlyDictionary<int, SkillLevelDefinition> levels)
            => new SkillDefinition(id, numericId, true, statTranslationFile, secondarySkillId, partNames, baseItem,
                null, supportSkill, levels);

        public string Id { get; }
        public int NumericId { get; }
        public bool IsSupport { get; }
        public string StatTranslationFile { get; }
        public string? SecondarySkillId { get; }

        public IReadOnlyList<string> PartNames { get; }

        public SkillBaseItemDefinition? BaseItem { get; }

        public ActiveSkillDefinition ActiveSkill { get; }
        public SupportSkillDefinition SupportSkill { get; }

        public IReadOnlyDictionary<int, SkillLevelDefinition> Levels { get; }

        public string? DisplayName => IsSupport
            ? BaseItem?.DisplayName
            : ActiveSkill.DisplayName;
    }

    public class SkillBaseItemDefinition
    {
        public SkillBaseItemDefinition(
            string displayName, string metadataId, ReleaseState releaseState, IReadOnlyCollection<string> gemTags)
            => (DisplayName, MetadataId, ReleaseState, GemTags) = (displayName, metadataId, releaseState, gemTags);

        public string DisplayName { get; }
        public string MetadataId { get; }
        public ReleaseState ReleaseState { get; }
        public IReadOnlyCollection<string> GemTags { get; }
    }

    public class ActiveSkillDefinition
    {
        public ActiveSkillDefinition(
            string displayName, int castTime,
            IEnumerable<string> activeSkillTypes, IEnumerable<string> minionActiveSkillTypes,
            IReadOnlyList<Keyword> keywords, IReadOnlyList<IReadOnlyList<Keyword>> keywordsPerPart, bool providesBuff,
            double? totemLifeMultiplier, IReadOnlyList<ItemClass> weaponRestrictions)
            => (DisplayName, CastTime, ActiveSkillTypes, MinionActiveSkillTypes, Keywords, KeywordsPerPart,
                    ProvidesBuff, TotemLifeMultiplier, WeaponRestrictions) =
                (displayName, castTime, activeSkillTypes, minionActiveSkillTypes, keywords, keywordsPerPart,
                    providesBuff, totemLifeMultiplier, weaponRestrictions);

        public string DisplayName { get; }
        public int CastTime { get; }
        public IEnumerable<string> ActiveSkillTypes { get; }
        public IEnumerable<string> MinionActiveSkillTypes { get; }
        public IReadOnlyList<Keyword> Keywords { get; }
        public IReadOnlyList<IReadOnlyList<Keyword>> KeywordsPerPart { get; }
        public bool ProvidesBuff { get; }
        public double? TotemLifeMultiplier { get; }
        public IReadOnlyList<ItemClass> WeaponRestrictions { get; }
    }

    public class SupportSkillDefinition
    {
        public SupportSkillDefinition(
            bool supportsGemsOnly, IEnumerable<string> allowedActiveSkillTypes,
            IEnumerable<string> excludedActiveSkillTypes, IEnumerable<string> addedActiveSkillTypes,
            IReadOnlyList<Keyword> addedKeywords)
            => (SupportsGemsOnly, AllowedActiveSkillTypes, ExcludedActiveSkillTypes, AddedActiveSkillTypes,
                    AddedKeywords) =
                (supportsGemsOnly, allowedActiveSkillTypes, excludedActiveSkillTypes, addedActiveSkillTypes,
                    addedKeywords);

        public bool SupportsGemsOnly { get; }

        public IEnumerable<string> AllowedActiveSkillTypes { get; }
        public IEnumerable<string> ExcludedActiveSkillTypes { get; }
        public IEnumerable<string> AddedActiveSkillTypes { get; }
        public IReadOnlyList<Keyword> AddedKeywords { get; }
    }

    public class SkillLevelDefinition
    {
        public SkillLevelDefinition(
            double? damageEffectiveness, double? damageMultiplier, double? criticalStrikeChance,
            int? attackSpeedMultiplier,
            int? manaCost, double? manaMultiplier, int? manaCostOverride, int? cooldown, bool canBypassCooldown,
            int requiredLevel, int requiredDexterity, int requiredIntelligence, int requiredStrength,
            IReadOnlyList<UntranslatedStat> qualityStats, IReadOnlyList<UntranslatedStat> stats,
            IReadOnlyList<IReadOnlyList<UntranslatedStat>> additionalStatsPerPart,
            IReadOnlyList<BuffStat> qualityBuffStats, IReadOnlyList<BuffStat> buffStats,
            IReadOnlyList<UntranslatedStat> qualityPassiveStats, IReadOnlyList<UntranslatedStat> passiveStats,
            SkillTooltipDefinition tooltip)
        {
            DamageEffectiveness = damageEffectiveness;
            DamageMultiplier = damageMultiplier;
            CriticalStrikeChance = criticalStrikeChance;
            AttackSpeedMultiplier = attackSpeedMultiplier;
            ManaCost = manaCost;
            ManaMultiplier = manaMultiplier;
            ManaCostOverride = manaCostOverride;
            Cooldown = cooldown;
            CanBypassCooldown = canBypassCooldown;
            Requirements = new Requirements(requiredLevel, requiredDexterity, requiredIntelligence, requiredStrength);
            QualityStats = qualityStats;
            Stats = stats;
            AdditionalStatsPerPart = additionalStatsPerPart;
            QualityBuffStats = qualityBuffStats;
            BuffStats = buffStats;
            QualityPassiveStats = qualityPassiveStats;
            PassiveStats = passiveStats;
            Tooltip = tooltip;
        }

        public double? DamageEffectiveness { get; }
        public double? DamageMultiplier { get; }
        public double? CriticalStrikeChance { get; }
        public int? AttackSpeedMultiplier { get; }

        public int? ManaCost { get; }
        public double? ManaMultiplier { get; }
        public int? ManaCostOverride { get; }
        public int? Cooldown { get; }
        public bool CanBypassCooldown { get; }
        
        public Requirements Requirements { get; }

        // Stats that apply when the skill is the main skill
        public IReadOnlyList<UntranslatedStat> QualityStats { get; }
        public IReadOnlyList<UntranslatedStat> Stats { get; }
        public IReadOnlyList<IReadOnlyList<UntranslatedStat>> AdditionalStatsPerPart { get; }

        // Stats that apply as part of the skill's buff
        public IReadOnlyList<BuffStat> QualityBuffStats { get; }
        public IReadOnlyList<BuffStat> BuffStats { get; }

        // Stats that always apply when the skill is socketed (these usually modify the skill's buff)
        public IReadOnlyList<UntranslatedStat> QualityPassiveStats { get; }
        public IReadOnlyList<UntranslatedStat> PassiveStats { get; }

        public SkillTooltipDefinition Tooltip { get; }
    }

    public class BuffStat
    {
        private readonly Func<Entity, IEnumerable<Entity>> _affectedEntitiesForSource;

        public BuffStat(UntranslatedStat stat, Func<Entity, IEnumerable<Entity>> affectedEntitiesForSource) =>
            (Stat, _affectedEntitiesForSource) = (stat, affectedEntitiesForSource);

        public UntranslatedStat Stat { get; }
        public IEnumerable<Entity> GetAffectedEntities(Entity sourceEntity) => _affectedEntitiesForSource(sourceEntity);
    }

    public class SkillTooltipDefinition
    {
        public SkillTooltipDefinition(
            string name, IReadOnlyList<TranslatedStat> properties, IReadOnlyList<TranslatedStat> requirements,
            IReadOnlyList<string> description,
            IReadOnlyList<TranslatedStat> qualityStats, IReadOnlyList<TranslatedStat> stats)
        {
            Name = name;
            Properties = properties;
            Requirements = requirements;
            Description = description;
            QualityStats = qualityStats;
            Stats = stats;
        }

        public string Name { get; }
        public IReadOnlyList<TranslatedStat> Properties { get; }
        public IReadOnlyList<TranslatedStat> Requirements { get; }
        public IReadOnlyList<string> Description { get; }
        public IReadOnlyList<TranslatedStat> QualityStats { get; }
        public IReadOnlyList<TranslatedStat> Stats { get; }
    }

    public class TranslatedStat : ValueObject
    {
        public TranslatedStat(string formatText, params double[] values)
            => (FormatText, Values) = (formatText, values);

        public string FormatText { get; }
        public IReadOnlyList<double> Values { get; }

        public override string ToString()
            => string.Format(FormatText, Values.Cast<object>().ToArray());

        protected override object ToTuple() => (FormatText, WithSequenceEquality(Values));
    }
}