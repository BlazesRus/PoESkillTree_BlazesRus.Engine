using System.Collections.Generic;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Modifiers;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Data;
using PoESkillTree.Engine.Computation.Data.Base;
using PoESkillTree.Engine.Computation.Data.Collections;

namespace PoESkillTree.Engine.Computation.Data
{
    /// <inheritdoc />
    /// <summary>
    /// <see cref="IStatMatchers"/> implementation matching stat parts specifying damage stats.
    /// <para>These matchers are referenceable and don't reference any non-<see cref="IReferencedMatchers"/> 
    /// themselves.</para>
    /// </summary>
    public class DamageStatMatchers : StatMatchersBase
    {
        private readonly IModifierBuilder _modifierBuilder;

        public DamageStatMatchers(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
        }

        public override IReadOnlyList<string> ReferenceNames { get; } = new[] { "StatMatchers" };

        // Kind of counter-intuitive, but "with attack skills" on a stat line refers to the dealt damage being attack
        // damage or ailment damage from that attack. On the other hand, IDamageRelatedStatBuilder.WithSkills
        // differentiates from ailment damage and restricts to not apply to ailment damage.
        // E.g. "attack damage" does not include ailment damage and corresponds to WithSkills(DamageSource.Attack).
        // "damage with attack skills" does include ailment damage and corresponds to With(DamageSource.Attack).
        protected override IReadOnlyList<MatcherData> CreateCollection() =>
            new StatMatcherCollection<IDamageRelatedStatBuilder>(_modifierBuilder)
            {
                // unspecific
                { "damage", Damage },
                { "^deals damage", Damage },
                { "global damage", Damage },
                // by source
                { "attack damage", Damage.WithSkills(DamageSource.Attack) },
                { "spell damage", Damage.WithSkills(DamageSource.Spell) },
                { "damage over time", Damage.With(DamageSource.OverTime) },
                // by type
                { "({DamageTypeMatchers}) damage", Reference.AsDamageType.Damage },
                { "global ({DamageTypeMatchers}) damage", Reference.AsDamageType.Damage },
                { "damage of a random element", RandomElement.Damage },
                // by keyword
                { "({KeywordMatchers}) damage", Damage.With(Reference.AsKeyword) },
                { "trap and mine damage", Damage, Or(With(Keyword.Trap), With(Keyword.Mine)) },
                { "projectiles deal damage", Damage.With(Keyword.Projectile) },
                // by skill vs. ailment
                { "damage with hits and ailments", Damage.WithHitsAndAilments },
                { "damage with hits and ({AilmentMatchers})", Damage.WithHits, Damage.With(Reference.AsAilment) },
                { "(?<!no )damage (with|from) hits", Damage.WithHits },
                { "damage with ailments", Damage.WithAilments },
                { "damage with ailments from attack skills", Damage.WithAilments.With(DamageSource.Attack) },
                { "attack skills deal damage with ailments", Damage.WithAilments.With(DamageSource.Attack) },
                { "damage with ({AilmentMatchers})", Damage.With(Reference.AsAilment) },
                {
                    "damage with ({AilmentMatchers}) and ({AilmentMatchers})",
                    Damage.With(References[0].AsAilment), Damage.With(References[1].AsAilment)
                },
                // by source and type
                { "attack physical damage", Physical.Damage.WithSkills(DamageSource.Attack) },
                {
                    "({DamageTypeMatchers}) damage to attacks",
                    Reference.AsDamageType.Damage.WithSkills(DamageSource.Attack)
                },
                {
                    "({DamageTypeMatchers}) attack damage",
                    Reference.AsDamageType.Damage.WithSkills(DamageSource.Attack)
                },
                {
                    "({DamageTypeMatchers}) damage with attack skills",
                    Reference.AsDamageType.Damage.With(DamageSource.Attack)
                },
                {
                    "({DamageTypeMatchers}) spell damage",
                    Reference.AsDamageType.Damage.WithSkills(DamageSource.Spell)
                },
                {
                    "spell ({DamageTypeMatchers}) damage",
                    Reference.AsDamageType.Damage.WithSkills(DamageSource.Spell)
                },
                {
                    "({DamageTypeMatchers}) damage over time",
                    Reference.AsDamageType.Damage.With(DamageSource.OverTime)
                },
                { "burning damage", Fire.Damage.WithSkills(DamageSource.OverTime), Fire.Damage.With(Ailment.Ignite) },
                // other combinations
                { "(?<!no )({DamageTypeMatchers}) damage (with|from) hits", Reference.AsDamageType.Damage.WithHits },
                {
                    "damage of types matching supported skill gem's tags",
                    (Lightning.Damage, With(Lightning)), (Cold.Damage, With(Cold)),
                    (Fire.Damage, With(Fire)), (Chaos.Damage, With(Chaos))
                },
                // specific attack damage
                { "melee physical damage", Physical.Damage.With(Keyword.Melee) },
                { "physical melee damage", Physical.Damage.With(Keyword.Melee) },
                { "physical weapon damage", Physical.Damage.WithSkills(DamageSource.Attack), MainHand.HasItem },
                {
                    "unarmed physical damage",
                    Physical.Damage.WithSkills(DamageSource.Attack).With(Keyword.Melee), Not(MainHand.HasItem)
                },
                { "projectile attack damage", Damage.WithSkills(DamageSource.Attack).With(Keyword.Projectile) },
                { "projectile damage from attack hits", Damage.WithSkills(DamageSource.Attack).With(Keyword.Projectile) },
                { "projectiles deal damage with hits and ailments", Damage.WithHitsAndAilments.With(Keyword.Projectile) },
                {
                    "physical projectile attack damage",
                    Physical.Damage.WithSkills(DamageSource.Attack).With(Keyword.Projectile)
                },
                { "melee area damage", Physical.Damage.With(Keyword.Melee).With(Keyword.AreaOfEffect) },
                { "main hand attack damage", Damage.WithSkills.With(AttackDamageHand.MainHand) },
                // other entities
                { "minion damage", Damage.For(Entity.Minion) },
                { "golem damage", Damage.For(Entity.Minion).With(Keyword.Golem) },
                {
                    "minion and totem elemental damage",
                    Elemental.Damage.For(Entity.Minion), Elemental.Damage.With(Keyword.Totem)
                },
            }; //add
    }
}