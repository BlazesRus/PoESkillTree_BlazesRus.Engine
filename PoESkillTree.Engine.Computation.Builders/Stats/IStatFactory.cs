using System;
using System.Collections.Generic;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Effects;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;

namespace PoESkillTree.Engine.Computation.Builders.Stats
{
    public interface IStatFactory
    {
        IStat FromIdentity(string identity, Entity entity, Type dataType,
            ExplicitRegistrationType? explicitRegistrationType = null);

        IStat CopyWithSuffix(IStat stat, string identitySuffix, Type dataType,
            ExplicitRegistrationType? explicitRegistrationType = null);

        IStat ChanceToDouble(IStat stat);

        IEnumerable<IStat> ConvertTo(IStat sourceStat, IEnumerable<IStat> targetStats);
        IEnumerable<IStat> GainAs(IStat sourceStat, IEnumerable<IStat> targetStats);
        IStat ConvertTo(IStat source, IStat target);
        IStat GainAs(IStat source, IStat target);
        IStat Conversion(IStat source);
        IStat SkillConversion(IStat source);

        IStat Regen(Entity entity, Pool pool);
        IStat RegenTargetPool(Entity entity, Pool regenPool);

        IStat MainSkillId(Entity entity);
        IStat MainSkillItemSlot(Entity entity);
        IStat MainSkillHasKeyword(Entity entity, Keyword keyword);
        IStat MainSkillPartHasKeyword(Entity entity, Keyword keyword);
        IStat MainSkillPartCastRateHasKeyword(Entity entity, Keyword keyword);
        IStat MainSkillPartDamageHasKeyword(Entity entity, Keyword keyword, DamageSource damageSource);
        IStat MainSkillPartAilmentDamageHasKeyword(Entity entity, Keyword keyword);

        IStat ActiveSkillItemSlot(Entity entity, string skillId);
        IStat ActiveSkillSocketIndex(Entity entity, string skillId);
        IStat SkillReservation(Entity entity, string skillId);

        IStat BuffEffect(Entity source, Entity target, string buffIdentity);        
        IStat BuffIsActive(Entity target, string buffIdentity);
        IStat BuffSourceIs(Entity source, Entity target, string buffIdentity);

        IStat Damage(Entity entity, DamageType damageType);
        IStat ConcretizeDamage(IStat stat, IDamageSpecification damageSpecification);
        IStat ApplyModifiersToSkillDamage(IStat stat, DamageSource damageSource, Form form);
        IStat ApplyModifiersToAilmentDamage(IStat stat, Form form);
        IStat DamageTaken(IStat damage);
        IStat AilmentDealtDamageType(Entity entity, Ailment ailment);
        IStat DamageBaseAddEffectiveness(Entity entity);
        IStat DamageBaseSetEffectiveness(Entity entity);
        IStat Exposure(Entity entity, DamageType damageType);

        IStat StatIsAffectedByModifiersToOtherStat(IStat stat, IStat otherStat, Form form);
        IStat Requirement(IStat stat);
        IStat ItemProperty(IStat stat, ItemSlot slot);
    }
}