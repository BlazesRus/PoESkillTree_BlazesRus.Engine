using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EnumsNET;
using PoESkillTree.Engine.Computation.Builders.Behaviors;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Effects;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;

namespace PoESkillTree.Engine.Computation.Builders.Stats
{
    public class StatFactory : IStatFactory
    {
        private readonly ConcurrentDictionary<(string, Entity), IStat> _cache =
            new ConcurrentDictionary<(string, Entity), IStat>();

        private readonly BehaviorFactory _behaviorFactory;

        public StatFactory()
        {
            _behaviorFactory = new BehaviorFactory(this);
        }

        public IStat FromIdentity(string identity, Entity entity, Type dataType,
            ExplicitRegistrationType? explicitRegistrationType = null) =>
            GetOrAdd(identity, entity, dataType, explicitRegistrationType);

        public IStat CopyWithSuffix(IStat stat, string identitySuffix, Type dataType,
            ExplicitRegistrationType? explicitRegistrationType = null) =>
            CopyWithSuffix(stat, identitySuffix, dataType, null, explicitRegistrationType);

        public IStat ChanceToDouble(IStat stat) =>
            CopyWithSuffix(stat, nameof(ChanceToDouble), typeof(uint));

        public IEnumerable<IStat> ConvertTo(IStat source, IEnumerable<IStat> targets)
        {
            foreach (var target in targets)
            {
                yield return ConvertTo(source, target);
            }
            yield return Conversion(source);
            yield return SkillConversion(source);
        }

        public IEnumerable<IStat> GainAs(IStat source, IEnumerable<IStat> targets)
        {
            foreach (var target in targets)
            {
                yield return GainAs(source, target);
            }
        }

        public IStat ConvertTo(IStat source, IStat target) =>
            CopyWithSuffix(source, $"{nameof(ConvertTo)}({target.Identity})", typeof(uint),
                () => _behaviorFactory.ConvertTo(source, target));

        public IStat GainAs(IStat source, IStat target) =>
            CopyWithSuffix(source, $"{nameof(GainAs)}({target.Identity})", typeof(uint),
                () => _behaviorFactory.GainAs(source, target));

        public IStat Conversion(IStat source) =>
            CopyWithSuffix(source, "Conversion", typeof(uint));

        public IStat SkillConversion(IStat source) =>
            CopyWithSuffix(source, "SkillConversion", typeof(uint),
                () => _behaviorFactory.SkillConversion(source));

        public IStat Regen(Entity entity, Pool pool) =>
            GetOrAdd($"{pool}.Regen", entity, typeof(double), behaviors: () => _behaviorFactory.Regen(pool, entity));

        public IStat RegenTargetPool(Entity entity, Pool regenPool) =>
            GetOrAdd($"{regenPool}.Regen.TargetPool", entity, typeof(Pool));

        public IStat MainSkillId(Entity entity) =>
            GetOrAdd("MainSkill.Id", entity, typeof(int));

        public IStat MainSkillItemSlot(Entity entity) =>
            GetOrAdd("MainSkill.ItemSlot", entity, typeof(ItemSlot));

        public IStat MainSkillHasKeyword(Entity entity, Keyword keyword) =>
            GetOrAdd($"MainSkill.Has.{keyword}", entity, typeof(bool));

        public IStat MainSkillPartHasKeyword(Entity entity, Keyword keyword) =>
            GetOrAdd($"MainSkillPart.Has.{keyword}", entity, typeof(bool));

        public IStat MainSkillPartCastRateHasKeyword(Entity entity, Keyword keyword) =>
            GetOrAdd($"MainSkillPart.CastRate.Has.{keyword}", entity, typeof(bool));

        public IStat MainSkillPartDamageHasKeyword(Entity entity, Keyword keyword, DamageSource damageSource) =>
            GetOrAdd($"MainSkillPart.Damage.{damageSource}.Has.{keyword}", entity, typeof(bool));

        public IStat MainSkillPartAilmentDamageHasKeyword(Entity entity, Keyword keyword) =>
            GetOrAdd($"MainSkillPart.Damage.Ailment.Has.{keyword}", entity, typeof(bool));

        public IStat ActiveSkillItemSlot(Entity entity, string skillId)
            => GetOrAdd($"{skillId}.ActiveSkillItemSlot", entity, typeof(ItemSlot),
                behaviors: () => _behaviorFactory.ActiveSkillItemSlot(entity, skillId));

        public IStat ActiveSkillSocketIndex(Entity entity, string skillId)
            => GetOrAdd($"{skillId}.ActiveSkillSocketIndex", entity, typeof(uint),
                behaviors: () => _behaviorFactory.ActiveSkillSocketIndex(entity, skillId));

        public IStat SkillReservation(Entity entity, string skillId) =>
            GetOrAdd($"{skillId}.Reservation", entity, typeof(uint), rounding: RoundingBehaviors.Ceiling);

        public IStat BuffEffect(Entity source, Entity target, string buffIdentity) =>
            GetOrAdd($"{buffIdentity}.EffectOn({target.GetName()})", source, typeof(double));

        public IStat BuffIsActive(Entity target, string buffIdentity) =>
            GetOrAdd($"{buffIdentity}.BuffActive", target, typeof(bool));

        public IStat BuffSourceIs(Entity source, Entity target, string buffIdentity) =>
            GetOrAdd(buffIdentity + ".BuffSourceIs(" + source.GetName() + ")", target, typeof(bool));

        public IStat Damage(Entity entity, DamageType damageType) =>
            GetOrAdd(damageType.GetName() + ".Damage", entity, typeof(int));

        public IStat ConcretizeDamage(IStat stat, IDamageSpecification damageSpecification) =>
            CopyWithSuffix(stat, damageSpecification.StatIdentitySuffix, stat.DataType,
                () => _behaviorFactory.ConcretizeDamage(stat, damageSpecification));

        public IStat ApplyModifiersToSkillDamage(IStat stat, DamageSource damageSource, Form form) =>
            CopyWithSuffix(stat,
                "ApplyModifiersToSkills(" + damageSource.GetName() + " for form " + form.GetName() + ")",
                typeof(bool));

        public IStat ApplyModifiersToAilmentDamage(IStat stat, Form form) =>
            CopyWithSuffix(stat, "ApplyModifiersToAilments(for form " + form.GetName() + ")", typeof(bool));

        public IStat DamageTaken(IStat damage) =>
            CopyWithSuffix(damage, "Taken", typeof(double));

        public IStat AilmentDealtDamageType(Entity entity, Ailment ailment) =>
            GetOrAdd($"{ailment}.DamageType", entity, typeof(DamageType));

        public IStat DamageBaseAddEffectiveness(Entity entity) =>
            GetOrAdd("DamageBaseAddEffectiveness", entity, typeof(double));

        public IStat DamageBaseSetEffectiveness(Entity entity) =>
            GetOrAdd("DamageBaseSetEffectiveness", entity, typeof(double));

        public IStat Exposure(Entity entity, DamageType damageType)
            => GetOrAdd(damageType + ".Exposure", entity, typeof(int),
                behaviors: () => _behaviorFactory.Exposure(entity, damageType));

        public IStat StatIsAffectedByModifiersToOtherStat(IStat stat, IStat otherStat, Form form)
            => GetOrAdd($"ModifiersTo({otherStat}).Affect({stat}).ForForm({form})", stat.Entity, typeof(bool),
                behaviors: () => _behaviorFactory.StatIsAffectedByModifiersToOtherStat(stat, otherStat, form));

        public IStat Requirement(IStat stat)
            => CopyWithSuffix(stat, "Required", stat.DataType, () => _behaviorFactory.Requirement(stat));

        public IStat ItemProperty(IStat stat, ItemSlot slot)
            => GetOrAdd(slot.GetName() + "." + stat.Identity, stat.Entity, stat.DataType,
                behaviors: () => _behaviorFactory.ItemProperty(stat, slot));

        private IStat CopyWithSuffix(IStat source, string identitySuffix, Type dataType,
            Func<IReadOnlyList<Behavior>>? behaviors, ExplicitRegistrationType? explicitRegistrationType = null,
            Func<NodeValue?, NodeValue?>? rounding = null)
        {
            return GetOrAdd(source.Identity + "." + identitySuffix, source.Entity,
                dataType, explicitRegistrationType, behaviors, rounding);
        }

        private IStat GetOrAdd(string identity, Entity entity, Type dataType,
            ExplicitRegistrationType? explicitRegistrationType = null, Func<IReadOnlyList<Behavior>>? behaviors = null,
            Func<NodeValue?, NodeValue?>? rounding = null)
        {
            // Func<IReadOnlyList<Behavior>> for performance reasons: Only retrieve behaviors if necessary.
            return _cache.GetOrAdd((identity, entity), _ =>
                new Stat(identity, entity, dataType, explicitRegistrationType, behaviors?.Invoke(), rounding));
        }
    }
}