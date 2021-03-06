using System;
using System.Collections.Generic;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Modifiers;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.Computation.Common.Data;
using PoESkillTree.Engine.Computation.Data.Base;
using PoESkillTree.Engine.Computation.Data.Collections;
using PoESkillTree.Engine.GameModel.Items;
using static PoESkillTree.Engine.Computation.Common.Builders.Values.ValueBuilderUtils;

namespace PoESkillTree.Engine.Computation.Data
{
    /// <inheritdoc />
    /// <summary>
    /// <see cref="IStatMatchers"/> implementation matching stat parts specifying converters to the modifier's
    /// main value (at the moment, these are all multipliers).
    /// </summary>
    public class ValueConversionMatchers : StatMatchersBase
    {
        private readonly IModifierBuilder _modifierBuilder;

        public ValueConversionMatchers(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
        }

        protected override IReadOnlyList<MatcherData> CreateCollection() =>
            new ValueConversionMatcherCollection(_modifierBuilder)
            {
                // action
                { "for each enemy you've killed recently", Kill.CountRecently },
                { "per enemy killed recently, up to #%", CappedMultiplier(Kill.CountRecently, Value) },
                {
                    "per enemy killed by you or your totems recently",
                    Kill.CountRecently + Kill.By(Entity.Totem).CountRecently
                },
                {
                    "for each enemy you or your minions have killed recently, up to #%( per second)?",
                    CappedMultiplier(Kill.CountRecently + Kill.By(Entity.Minion).CountRecently, Value)
                },
                { "for each hit you've blocked recently", Block.CountRecently },
                { "for each corpse consumed recently", Action.ConsumeCorpse.CountRecently },
                { "for each corpse consumed recently, up to a maximum of #%", CappedMultiplier(Action.ConsumeCorpse.CountRecently, Value) },
                // equipment
                { "for each magic item( you have)? equipped", Equipment.Count(e => e.Has(FrameType.Magic)) },
                // stats
                { "per # accuracy rating", PerStat(Stat.Accuracy.With(AttackDamageHand.MainHand)) },
                { "per #%? ({StatMatchers})(?! leech)", PerStat(stat: Reference.AsStat, divideBy: Value) },
                { "per # ({StatMatchers}), up to #%", CappedMultiplier((Reference.AsStat.Value / Values[0]).Floor(), Values[1]) },
                { "per # ({StatMatchers}), up to a maximum of #%", CappedMultiplier((Reference.AsStat.Value / Values[0]).Floor(), Values[1]) },
                { "per # ({StatMatchers}) ceiled", PerStatCeiled(stat: Reference.AsStat, divideBy: Value) },
                { "per ({StatMatchers})(?! leech)", PerStat(stat: Reference.AsStat) },
                { "per ({StatMatchers}), up to #%", CappedMultiplier(Reference.AsStat.Value, Value) },
                {
                    "per # ({StatMatchers}) on helmet",
                    PerStat(Reference.AsStat.ValueFor(NodeType.Base, new ModifierSource.Local.Item(ItemSlot.Helm)),
                        divideBy: Value)
                },
                {
                    "per # ({StatMatchers}) on body armour",
                    PerStat(
                        Reference.AsStat.ValueFor(NodeType.Base, new ModifierSource.Local.Item(ItemSlot.BodyArmour)),
                        divideBy: Value)
                },
                {
                    "per # ({StatMatchers}) on shield",
                    PerStat(
                        Reference.AsStat.ValueFor(NodeType.Base, new ModifierSource.Local.Item(ItemSlot.OffHand)),
                        divideBy: Value)
                },
                {
                    "per # ({StatMatchers}) or ({StatMatchers}) on shield",
                    PerStat(
                        References[0].AsStat.ValueFor(NodeType.Base, new ModifierSource.Local.Item(ItemSlot.OffHand))
                        + References[1].AsStat.ValueFor(NodeType.Base, new ModifierSource.Local.Item(ItemSlot.OffHand)),
                        divideBy: Value)
                },
                { "per grand spectrum", PerStat(stat: Stat.GrandSpectrumJewelsSocketed) },
                { "per level", PerStat(Stat.Level) },
                { "per (stage|fuse charge|explosive arrow on target|energy)", PerStat(Stat.SkillStage) },
                { "for each (stage|blade)", PerStat(Stat.SkillStage) },
                { @"per (stage|explosive arrow on target), up to \+#", CappedMultiplier(Stat.SkillStage.Value, Value) },
                { "runes (have|deal) (?<inner>.*) for each time they have been improved", Stat.SkillStage.Value, "${inner}" },
                { "per (stage|energy) after the first", PerStatAfterFirst(Stat.SkillStage) },
                {
                    "per ({ChargeTypeMatchers}) removed",
                    Reference.AsChargeType.Amount.Value - Reference.AsChargeType.Amount.Minimum.Value
                },
                {
                    "when placed, (?<inner>.*) per stage",
                    Stat.BannerStage.Value, Flag.IsBannerPlanted, "${inner}"
                },
                { "per nearby enemy", OpponentsOfSelf.CountNearby },
                { "per one hundred nearby enemies", OpponentsOfSelf.CountNearby / 100 },
                { @"per nearby enemy, up to \+#%?", CappedMultiplier(OpponentsOfSelf.CountNearby, Value) },
                { "per # unreserved maximum mana", PerStat(Mana.Value - Mana.Reservation.Value, divideBy: Value) },
                {
                    "per # unreserved maximum mana, up to #%",
                    CappedMultiplier(((Mana.Value - Mana.Reservation.Value) / Values[0]).Floor(), Values[1])
                },
                {
                    "per # mana spent recently, up to #%",
                    CappedMultiplier(Action.SpendMana(Values[0]).CountRecently, Values[1])
                },
                {
                    "per # additional melee( strike)? range",
                    PerStat(Stat.Range.With(AttackDamageHand.MainHand).ValueFor(NodeType.BaseAdd), Value)
                },
                { "per projectile", PerStat(Projectile.Count) },
                { "per curse applied", PerStat(Stat.CursesLinkedToBane) },
                {
                    "per # power,? up to a maximum of #%",
                    CappedMultiplier((Stat.Warcry.LastPower / Values[0]).Floor(), Values[1])
                },
                // buffs
                { "per buff on you", Buffs(targets: Self).Count() },
                { "per curse on you", Buffs(targets: Self).With(Keyword.Curse).Count() },
                { "for each herald affecting you", Buffs(targets: Self).With(Keyword.Herald).Count() },
                { "for each herald affecting you, up to #%", CappedMultiplier(Buffs(targets: Self).With(Keyword.Herald).Count(), Value) },
                { "per curse on enemy", Buffs(targets: OpponentsOfSelf).With(Keyword.Curse).Count() },
                { "for each curse on that enemy,", Buffs(targets: OpponentsOfSelf).With(Keyword.Curse).Count() },
                { "for each impale on enemy", Buff.Impale.StackCount.For(MainOpponentOfSelf).Value },
                { "each ({BuffMatchers}) applies", Reference.AsBuff.StackCount.For(MainOpponentOfSelf).Value },
                // ailments
                { "for each poison on the enemy", Ailment.Poison.InstancesOn(MainOpponentOfSelf).Value },
                { "per poison on enemy", Ailment.Poison.InstancesOn(MainOpponentOfSelf).Value },
                { "per poison on you", Ailment.Poison.InstancesOn(Self).Value },
                { "per poison(?= affecting enemies)", Ailment.Poison.InstancesOn(MainOpponentOfSelf).Value },
                {
                    @"per poison affecting enemy, up to \+#%",
                    CappedMultiplier(Ailment.Poison.InstancesOn(MainOpponentOfSelf).Value, Value)
                },
                {
                    "for each poison on the enemy, up to #",
                    CappedMultiplier(Ailment.Poison.InstancesOn(MainOpponentOfSelf).Value, Value)
                },
                { "per elemental ailment on the enemy", Ailment.Elemental.Count(b => b.IsOn(MainOpponentOfSelf)) },
                // skills
                { "for each zombie you own", Skills.RaiseZombie.Instances.Value },
                { "for each raised zombie", Skills.RaiseZombie.Instances.Value },
                { "per summoned sentinel of purity", Skills.FromId("HeraldOfPurity").Instances.Value },
                { "for each summoned golem", Golems.CombinedInstances.Value },
                { "per summoned golem", Golems.CombinedInstances.Value },
                { "for each golem you have summoned", Golems.CombinedInstances.Value },
                { "for each type of golem you have summoned", Golems.CombinedInstances.Value },
                {
                    "per minion, up to #%",
                    CappedMultiplier(Skills[Keyword.Minion].CombinedInstances.Value, Value)
                },
                {
                    "per brand, up to a maximum of #%",
                    CappedMultiplier(Skills[Keyword.Brand].CombinedInstances.Value, Value)
                },
                {
                    "for each skill you've used Recently, up to #%",
                    CappedMultiplier(AllSkills.Cast.CountRecently, Value)
                },
                { "for each time you've warcried recently", Skills[Keyword.Warcry].Cast.CountRecently },
                // traps, mines, totems
                { "for each trap", Traps.CombinedInstances.Value },
                { "for each mine", Mines.CombinedInstances.Value },
                { "for each trap and mine you have", Traps.CombinedInstances.Value + Mines.CombinedInstances.Value },
                { "(per|for each)( summoned)? totem", Totems.CombinedInstances.Value },
                {
                    "each mine( from supported skills)? applies (?<inner>.*) to( hits against)? enemies near it, up to( a maximum of)? #%",
                    CappedMultiplier(MineAura(), Value),
                    "${inner}"
                },
                {
                    "each mine (?<inner>.*) to( hits against)? enemies near it, up to( a maximum of)? # to #",
                    CappedMultiplier(MineAura(), ValueFactory.FromMinAndMax(Values[0], Values[1])),
                    "${inner}"
                },
                // jewels
                {
                    "(per|for every) # ({AttributeStatMatchers}) (allocated|(from|on) allocated passives) in radius",
                    PerStat(PassiveTree.AllocatedInModifierSourceJewelRadius(Reference.AsStat), Value)
                },
                {
                    "(per|for every) # ({AttributeStatMatchers}) (from|on) unallocated passives in radius",
                    PerStat(PassiveTree.UnallocatedInModifierSourceJewelRadius(Reference.AsStat), Value)
                },
                {
                    "passive skills in radius also grant:",
                    PassiveTree.AllocatedNodeCountInModifierSourceJewelRadius
                },
                {
                    "per notable allocated in radius",
                    PassiveTree.AllocatedNotableCountInModifierSourceJewelRadius
                },
                // unique
                {
                    "for each poison you have inflicted recently",
                    Stat.UniqueAmount("# of Poisons inflicted Recently")
                },
                {
                    "for each remaining chain",
                    AtLeastZero(Projectile.ChainCount.Value - Stat.UniqueAmount("Projectile.ChainedCount"))
                },
                { "per chain", Stat.UniqueAmount("Projectile.ChainedCount") },
                { "for each time they have chained", Stat.UniqueAmount("Projectile.ChainedCount") },
                { "for each enemy pierced", Stat.UniqueAmount("Projectile.PiercedCount") },
                {
                    "for each (of your mines|mine) detonated recently, up to #%( per second)?",
                    CappedMultiplier(Stat.UniqueAmount("# of Mines Detonated Recently"), Value)
                },
                {
                    "for each (of your traps|trap) triggered recently, up to #%( per second)?",
                    CappedMultiplier(Stat.UniqueAmount("# of Traps Triggered Recently"), Value)
                },
                {
                    "for each time you've blocked in the past 10 seconds",
                    Stat.UniqueAmount("# of times blocked in the past 10 seconds")
                },
                {
                    "for each endurance charge lost recently, up to #%",
                    CappedMultiplier(Stat.UniqueAmount("# of Endurance Charges lost recently"), Value)
                },
                {
                    "for each nearby corpse, (?<inner>.*) up to #%? per second",
                    CappedMultiplier(Stat.UniqueAmount("# of nearby Corpses"), Value), "${inner}"
                },
                {
                    "for each prior mine in detonation sequence",
                    CappedMultiplier(
                        Stat.UniqueAmount("# of prior mines in detonation sequence"),
                        Mines.CombinedInstances.Maximum.Value)
                },
                {
                    "for every # prior mines in detonation sequence",
                    CappedMultiplier(
                        (Stat.UniqueAmount("# of prior mines in detonation sequence") / Value).Floor(),
                        Mines.CombinedInstances.Maximum.Value)
                },
                {
                    "while stationary, gain (?<inner>#% .*) every second, up to a maximum of #%",
                    CappedMultiplier(Stat.UniqueAmount("# of seconds you've been stationary for"), Values[1] / Values[0]), "${inner}"
                },
                {
                    "per affected ally, to a maximum of #%",
                    CappedMultiplier(Stat.UniqueAmount("Warcry.AffectedAllies"), Value)
                },
                {
                    "per previous attack exerted by this warcry",
                    Stat.UniqueAmount("Warcry.AttacksExertedPreviously")
                },
            }; // add

        private ValueBuilder MineAura()
            => Mines.CombinedInstances.Value * Buff.GenericMine.EffectOn(MainOpponentOfSelf).Value;

        private Func<ValueBuilder, ValueBuilder> CappedMultiplier(ValueBuilder multiplier, IValueBuilder maximum)
            => v => ValueFactory.Minimum(v * multiplier, maximum);

        private ValueBuilder AtLeastZero(ValueBuilder value)
            => ValueFactory.Maximum(value, 0);
    }
}