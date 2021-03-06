using PoESkillTree.Engine.Computation.Common.Builders.Skills;
using PoESkillTree.Engine.GameModel.Skills;

namespace PoESkillTree.Engine.Computation.Builders.Skills
{
    public class KeywordBuilders : IKeywordBuilders
    {
        public IKeywordBuilder Attack { get; } = From(Keyword.Attack);
        public IKeywordBuilder Spell { get; } = From(Keyword.Spell);
        public IKeywordBuilder Projectile { get; } = From(Keyword.Projectile);
        public IKeywordBuilder AreaOfEffect { get; } = From(Keyword.AreaOfEffect);
        public IKeywordBuilder Melee { get; } = From(Keyword.Melee);
        public IKeywordBuilder Totem { get; } = From(Keyword.Totem);
        public IKeywordBuilder Curse { get; } = From(Keyword.Curse);
        public IKeywordBuilder Trap { get; } = From(Keyword.Trap);
        public IKeywordBuilder Movement { get; } = From(Keyword.Movement);
        public IKeywordBuilder Mine { get; } = From(Keyword.Mine);
        public IKeywordBuilder Vaal { get; } = From(Keyword.Vaal);
        public IKeywordBuilder Aura { get; } = From(Keyword.Aura);
        public IKeywordBuilder Golem { get; } = From(Keyword.Golem);
        public IKeywordBuilder Minion { get; } = From(Keyword.Minion);
        public IKeywordBuilder Warcry { get; } = From(Keyword.Warcry);
        public IKeywordBuilder Herald { get; } = From(Keyword.Herald);
        public IKeywordBuilder Offering { get; } = From(Keyword.Offering);
        public IKeywordBuilder CounterAttack { get; } = From(Keyword.CounterAttack);
        public IKeywordBuilder Bow { get; } = From(Keyword.Bow);
        public IKeywordBuilder Brand { get; } = From(Keyword.Brand);
        public IKeywordBuilder Banner { get; } = From(Keyword.Banner);
        public IKeywordBuilder Triggered { get; } = From(Keyword.Triggered);
        IKeywordBuilder IKeywordBuilders.From(Keyword keyword) => From(keyword);

        private static IKeywordBuilder From(Keyword keyword) => new KeywordBuilder(keyword);
    }

    public class KeywordBuilder : ConstantBuilder<IKeywordBuilder, Keyword>, IKeywordBuilder
    {
        public KeywordBuilder(Keyword keyword) : base(keyword)
        {
        }
    }
}