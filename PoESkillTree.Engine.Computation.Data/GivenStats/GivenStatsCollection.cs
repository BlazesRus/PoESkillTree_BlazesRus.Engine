using System;
using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Modifiers;
using PoESkillTree.Engine.Computation.Common.Data;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Engine.Computation.Data.GivenStats
{
    public class GivenStatsCollection : IReadOnlyList<IGivenStats>
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly CharacterBaseStats _characterBaseStats;
        private readonly MonsterBaseStats _monsterBaseStats;
        private readonly Lazy<IReadOnlyList<IGivenStats>> _lazyCollection;

        public GivenStatsCollection(
            IBuilderFactories builderFactories,
            CharacterBaseStats characterBaseStats, MonsterBaseStats monsterBaseStats)
        {
            _builderFactories = builderFactories;
            _monsterBaseStats = monsterBaseStats;
            _characterBaseStats = characterBaseStats;
            _lazyCollection = new Lazy<IReadOnlyList<IGivenStats>>(() => CreateCollection(ModifierBuilder.Empty));
        }

        public IEnumerator<IGivenStats> GetEnumerator() => _lazyCollection.Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _lazyCollection.Value.Count;

        public IGivenStats this[int index] => _lazyCollection.Value[index];

        private IReadOnlyList<IGivenStats> CreateCollection(IModifierBuilder modifierBuilder)
            => new IGivenStats[]
            {
                new CommonGivenStats(_builderFactories, modifierBuilder),
                new CommonMechanics(_builderFactories, modifierBuilder),
                new EffectStats(_builderFactories, modifierBuilder),
                new MonsterGivenStats(_builderFactories, modifierBuilder),
                new AllyGivenStats(_builderFactories, modifierBuilder, _monsterBaseStats),
                new AllyMechanics(_builderFactories, modifierBuilder),
                new TotemGivenStats(_builderFactories, modifierBuilder),
                new EnemyGivenStats(_builderFactories, modifierBuilder, _monsterBaseStats),
                new EnemyMechanics(_builderFactories, modifierBuilder),
                new CharacterGivenStats(_builderFactories, modifierBuilder, _characterBaseStats),
                new CharacterMechanics(_builderFactories, modifierBuilder),
                new DamageMechanics(_builderFactories, modifierBuilder),
                new GameStateDependentMods(_builderFactories, modifierBuilder),
                new AdditionalSkillStats(_builderFactories, modifierBuilder),
                new SkillBaseRadii(_builderFactories, modifierBuilder),
            };
    }
}