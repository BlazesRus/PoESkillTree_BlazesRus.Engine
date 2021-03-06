using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders.Effects;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;

namespace PoESkillTree.Engine.Computation.Builders.Effects
{
    public class AilmentBuilders : IAilmentBuilders
    {
        private readonly AilmentBuilderCollection _allAilments;

        public AilmentBuilders(IStatFactory statFactory)
        {
            _allAilments = new AilmentBuilderCollection(statFactory, Enums.GetValues<Ailment>().ToList());
            Elemental = new AilmentBuilderCollection(statFactory,
                new[] { Ailment.Ignite, Ailment.Shock, Ailment.Chill, Ailment.Freeze });

            ShockEffect = StatBuilderUtils.FromIdentity(statFactory, "Shock.Effect", typeof(double));
            IncreasedDamageTakenFromShocks = StatBuilderUtils.FromIdentity(statFactory,
                "Shock.IncreasedDamageTaken", typeof(uint), ExplicitRegistrationTypes.UserSpecifiedValue(15));
            ChillEffect = StatBuilderUtils.FromIdentity(statFactory, "Chill.Effect", typeof(double));
            ReducedActionSpeedFromChill = StatBuilderUtils.FromIdentity(statFactory,
                "Chill.ReducedActionSpeed", typeof(uint), ExplicitRegistrationTypes.UserSpecifiedValue(10));
        }

        public IAilmentBuilder Ignite => _allAilments[Ailment.Ignite];
        public IAilmentBuilder Shock => _allAilments[Ailment.Shock];
        public IAilmentBuilder Chill => _allAilments[Ailment.Chill];
        public IAilmentBuilder Freeze => _allAilments[Ailment.Freeze];
        public IAilmentBuilder Bleed => _allAilments[Ailment.Bleed];
        public IAilmentBuilder Poison => _allAilments[Ailment.Poison];
        public IAilmentBuilder From(Ailment ailment) => _allAilments[ailment];
        public IAilmentBuilderCollection Elemental { get; }
        public IAilmentBuilderCollection All => _allAilments;
        public IStatBuilder ShockEffect { get; }
        public IStatBuilder IncreasedDamageTakenFromShocks { get; }
        public IStatBuilder ChillEffect { get; }
        public IStatBuilder ReducedActionSpeedFromChill { get; }
    }

    internal class AilmentBuilderCollection
        : FixedBuilderCollection<Ailment, IAilmentBuilder>, IAilmentBuilderCollection
    {
        public AilmentBuilderCollection(IStatFactory statFactory, IReadOnlyList<Ailment> keys)
            : base(keys, a => new AilmentBuilder(statFactory, CoreBuilder.Create(a)))
        {
        }
    }
}