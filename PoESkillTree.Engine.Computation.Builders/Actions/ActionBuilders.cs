using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PoESkillTree.Engine.Computation.Builders.Entities;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Actions;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Entities;
using PoESkillTree.Engine.Computation.Common.Builders.Skills;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.Computation.Common.Parsing;

namespace PoESkillTree.Engine.Computation.Builders.Actions
{
    public class ActionBuilders : IActionBuilders
    {
        private readonly IStatFactory _statFactory;
        private readonly IEntityBuilder _entity = new ModifierSourceEntityBuilder();

        public ActionBuilders(IStatFactory statFactory)
        {
            _statFactory = statFactory;
        }

        public IActionBuilder Kill => Create();
        public IBlockActionBuilder Block => new BlockActionBuilder(_statFactory, _entity);
        public IActionBuilder Hit => Create();

        public IActionBuilder HitWith(IDamageTypeBuilder damageType)
        {
            var stringBuilder = CoreBuilder.Create<IKeywordBuilder, string>(damageType, BuildHitWithIdentity);
            return new ActionBuilder(_statFactory, stringBuilder, _entity);
        }

        private static string BuildHitWithIdentity(BuildParameters parameters, IKeywordBuilder builder)
        {
            var damageTypes = ((IDamageTypeBuilder) builder).BuildDamageTypes(parameters);
            if (damageTypes.Count != 1)
                throw new ParseException(
                    $"IDamageTypeBuilders passed to {nameof(HitWith)} must build to exactly one damage type." +
                    $" {string.Join(",", damageTypes)} given");
            return $"Hit.{damageTypes.Single()}";
        }

        public IActionBuilder HitWith(AttackDamageHand hand) =>
            new ActionBuilder(_statFactory, CoreBuilder.Create($"Hit.{hand}"), _entity);

        public IActionBuilder SavageHit => Create();
        public ICriticalStrikeActionBuilder CriticalStrike => new CriticalStrikeActionBuilder(_statFactory, _entity);
        public IActionBuilder NonCriticalStrike => Create();
        public IActionBuilder Shatter => Create();
        public IActionBuilder ConsumeCorpse => Create();
        public IActionBuilder TakeDamage => Create();
        public IActionBuilder Die => Create();
        public IActionBuilder Focus => Create();

        public IActionBuilder SpendMana(IValueBuilder amount)
        {
            var stringBuilder = CoreBuilder.Create(amount, BuildSpendManaIdentity);
            return new ActionBuilder(_statFactory, stringBuilder, _entity);
        }

        private static string BuildSpendManaIdentity(BuildParameters parameters, IValueBuilder builder) =>
            $"Spend{builder.Build(parameters).Calculate(new ThrowingContext())}Mana";

        public IActionBuilder EveryXSeconds(IValueBuilder interval)
        {
            var stringBuilder = CoreBuilder.Create(interval, BuildEveryXSecondsIdentity);
            return new ActionBuilder(_statFactory, stringBuilder, _entity);
        }

        private static string BuildEveryXSecondsIdentity(BuildParameters parameters, IValueBuilder builder)
            => $"Every{builder.Build(parameters).Calculate(new ThrowingContext())}Seconds";

        public IActionBuilder Unique(string description) => Create(description);

        private IActionBuilder Create([CallerMemberName] string identity = "") =>
            new ActionBuilder(_statFactory, CoreBuilder.Create(identity), _entity);

        private class ThrowingContext : IValueCalculationContext
        {
            public PathDefinition CurrentPath => throw CreateException();

            public IReadOnlyCollection<PathDefinition> GetPaths(IStat stat) =>
                throw CreateException();

            public NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path) =>
                throw CreateException();

            public List<NodeValue?> GetValues(Form form, IEnumerable<(IStat stat, PathDefinition path)> paths) =>
                throw CreateException();

            private static ParseException CreateException() =>
                new ParseException(
                    $"Value builders passed to {nameof(SpendMana)} must not use the IValueCalculationContext");
        }
    }
}