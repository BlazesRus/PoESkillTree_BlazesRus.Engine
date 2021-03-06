using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Forms;
using PoESkillTree.Engine.Utils;

namespace PoESkillTree.Engine.Computation.Builders.Forms
{
    public class FormBuilders : IFormBuilders
    {
        public IFormBuilder BaseSet { get; } = Create(Form.BaseSet);
        public IFormBuilder BaseAdd { get; } = Create(Form.BaseAdd);
        public IFormBuilder BaseSubtract { get; } = CreateNegating(Form.BaseAdd);
        public IFormBuilder PercentIncrease { get; } = Create(Form.Increase);
        public IFormBuilder PercentReduce { get; } = CreateNegating(Form.Increase);
        public IFormBuilder PercentMore { get; } = Create(Form.More);
        public IFormBuilder PercentLess { get; } = CreateNegating(Form.More);
        public IFormBuilder TotalOverride { get; } = Create(Form.TotalOverride);
        public IFormBuilder From(Form form) => Create(form);

        private static IFormBuilder Create(Form form) => new FormBuilder(form, Funcs.Identity);

        private static IFormBuilder CreateNegating(Form form) => new FormBuilder(form, v => v.Multiply(v.Create(-1)));


        private class FormBuilder : ConstantBuilder<IFormBuilder, (Form, ValueConverter)>, IFormBuilder
        {
            public FormBuilder(Form form, ValueConverter valueConverter)
                : base((form, valueConverter))
            {
            }
        }
    }
}