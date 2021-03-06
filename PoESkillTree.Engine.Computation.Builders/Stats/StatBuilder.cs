using System;
using System.Collections.Generic;
using PoESkillTree.Engine.Computation.Builders.Conditions;
using PoESkillTree.Engine.Computation.Builders.Values;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Conditions;
using PoESkillTree.Engine.Computation.Common.Builders.Entities;
using PoESkillTree.Engine.Computation.Common.Builders.Resolving;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.Computation.Common.Parsing;
using PoESkillTree.Engine.GameModel.Items;

namespace PoESkillTree.Engine.Computation.Builders.Stats
{
    public class StatBuilder : IStatBuilder
    {
        protected IStatFactory StatFactory { get; }
        protected ICoreStatBuilder CoreStatBuilder { get; }

        public StatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder)
        {
            StatFactory = statFactory;
            CoreStatBuilder = coreStatBuilder;
        }

        protected virtual IStatBuilder With(ICoreStatBuilder coreStatBuilder) =>
            WithUntyped(coreStatBuilder);

        private IStatBuilder WithUntyped(ICoreStatBuilder coreStatBuilder) =>
            new StatBuilder(StatFactory, coreStatBuilder);

        protected IStatBuilder WithStatConverter(Func<IStat, IStat> statConverter)
            => WithStatConverter((_, s) => statConverter(s));

        protected virtual IStatBuilder WithStatConverter(Func<ModifierSource, IStat, IStat> statConverter)
            => With(new StatBuilderWithStatConverter(CoreStatBuilder, statConverter));

        public virtual IStatBuilder Resolve(ResolveContext context) => With(CoreStatBuilder.Resolve(context));

        public IStatBuilder Minimum => WithStatConverter(GetMinimumWithNullCheck);

        private static IStat GetMinimumWithNullCheck(IStat stat)
        {
            var minimum = stat.Minimum;
            if (minimum is null)
                throw new InvalidOperationException($"{stat} has no minimum");
            return minimum;
        }

        public IStatBuilder Maximum => WithStatConverter(GetMaximumWithNullCheck);

        private static IStat GetMaximumWithNullCheck(IStat stat)
        {
            var maximum = stat.Maximum;
            if (maximum is null)
                throw new InvalidOperationException($"{stat} has no maximum");
            return maximum;
        }

        public ValueBuilder Value => ValueFor(NodeType.Total);

        public ValueBuilder ValueFor(NodeType nodeType, ModifierSource? modifierSource = null)
            => new ValueBuilder(new ValueBuilderImpl(
                ps => BuildValue(nodeType, modifierSource ?? new ModifierSource.Global(), ps),
                c => Resolve(c).ValueFor(nodeType, modifierSource)));

        private IValue BuildValue(NodeType nodeType, ModifierSource modifierSource, BuildParameters parameters)
        {
            IStat? firstStat = null;
            foreach (var (stats, _, _) in Build(parameters))
            {
                foreach (var stat in stats)
                {
                    if (firstStat is null)
                        firstStat = stat;
                    else
                        throw CreateException($"This builder built to at least {firstStat} and {stat}.");
                }
            }
            if (firstStat is null)
                throw CreateException("This builder built to zero stats.");

            return new StatValue(firstStat, nodeType, modifierSource);

            ParseException CreateException(string messageSuffix)
                => new ParseException("Can only access the value of stat builders that represent a single stat. "
                                      + messageSuffix);
        }

        public IConditionBuilder IsSet =>
            ValueConditionBuilder.Create(Value, v => v.HasValue, v => v + ".IsSet");

        public IConditionBuilder IsTrue =>
            ValueConditionBuilder.Create(Value, v => v.IsTrue(), v => v + ".IsTrue");

        public IStatBuilder ConvertTo(IStatBuilder stat) =>
            WithUntyped(new ConversionStatBuilder(StatFactory.ConvertTo,
                new StatBuilderAdapter(this), new StatBuilderAdapter(stat)));

        public IStatBuilder GainAs(IStatBuilder stat) =>
            WithUntyped(new ConversionStatBuilder(StatFactory.GainAs,
                new StatBuilderAdapter(this), new StatBuilderAdapter(stat)));

        public IStatBuilder ChanceToDouble => WithStatConverter(StatFactory.ChanceToDouble);

        public IStatBuilder AsItemProperty
            => WithStatConverter((m, s) => StatFactory.ItemProperty(s, GetItemSlot(m)));

        private static ItemSlot GetItemSlot(ModifierSource modifierSource)
        {
            if (modifierSource is ModifierSource.Local.Item itemSource)
                return itemSource.Slot;
            throw new ParseException(
                "IStatBuilder.AsItemProperty can only be used with a source of type ModifierSource.Local.Item");
        }

        public IStatBuilder AsItemPropertyForSlot(ItemSlot slot)
            => WithStatConverter((_, s) => StatFactory.ItemProperty(s, slot));

        public IStatBuilder AsPassiveNodeProperty
            => WithStatConverter((m, s) => PassiveNodeProperty(s, GetPassiveNodeId(m)));

        public IStatBuilder AsPassiveNodeBaseProperty
            => WithStatConverter((m, s) => PassiveNodeBaseProperty(s, GetPassiveNodeId(m)));

        private static ushort GetPassiveNodeId(ModifierSource modifierSource)
        {
            ModifierSource? nullableSource = modifierSource;
            if (nullableSource is ModifierSource.Global globalSource)
                nullableSource = globalSource.LocalSource;
            if (nullableSource is ModifierSource.Local.PassiveNode nodeSource)
                return nodeSource.NodeId;
            throw new ParseException(
                "IStatBuilder.AsPassiveNodeProperty can only be used with a source of type ModifierSource.Local.PassiveNode");
        }

        public IStatBuilder AsPassiveNodePropertyFor(ushort nodeId)
            => WithStatConverter((_, s) => PassiveNodeProperty(s, nodeId));

        private IStat PassiveNodeProperty(IStat source, ushort nodeId)
            => StatFactory.FromIdentity(nodeId + "." + source.Identity, source.Entity, source.DataType);

        private IStat PassiveNodeBaseProperty(IStat source, ushort nodeId)
            => StatFactory.FromIdentity(nodeId + "." + source.Identity + ".Base", source.Entity, source.DataType);

        public IStatBuilder For(IEntityBuilder entity) => With(CoreStatBuilder.WithEntity(entity));

        public IStatBuilder WithCondition(IConditionBuilder condition) =>
            WithUntyped(new StatBuilderAdapter(this, condition));

        public IStatBuilder CombineWith(IStatBuilder other) =>
            WithUntyped(new CompositeCoreStatBuilder(new StatBuilderAdapter(this), new StatBuilderAdapter(other)));

        public IStatBuilder Concat(IStatBuilder other) =>
            WithUntyped(new ConcatCompositeCoreStatBuilder(new StatBuilderAdapter(this), new StatBuilderAdapter(other)));

        public virtual IEnumerable<StatBuilderResult> Build(BuildParameters parameters) =>
            CoreStatBuilder.Build(parameters);
    }
}