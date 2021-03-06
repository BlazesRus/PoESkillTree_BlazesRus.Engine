using Moq;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders.Forms;
using PoESkillTree.Engine.Computation.Common.Builders.Values;

namespace PoESkillTree.Engine.Computation.Builders.Forms
{
    [TestFixture]
    public class FormBuildersTest
    {
        [Test]
        public void SutIsFormBuilders()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IFormBuilders>(sut);
        }

        [TestCase(nameof(IFormBuilders.BaseSet), Form.BaseSet)]
        [TestCase(nameof(IFormBuilders.BaseAdd), Form.BaseAdd)]
        [TestCase(nameof(IFormBuilders.BaseSubtract), Form.BaseAdd)]
        [TestCase(nameof(IFormBuilders.PercentIncrease), Form.Increase)]
        [TestCase(nameof(IFormBuilders.PercentReduce), Form.Increase)]
        [TestCase(nameof(IFormBuilders.PercentMore), Form.More)]
        [TestCase(nameof(IFormBuilders.PercentLess), Form.More)]
        [TestCase(nameof(IFormBuilders.TotalOverride), Form.TotalOverride)]
        public void PropertyBuildReturnsCorrectForm(string propertyName, Form expected)
        {
            var sut = CreateSut();

            var actual = GetProperty(sut, propertyName).Build().form;

            Assert.AreEqual(expected, actual);
        }

        [TestCase(nameof(IFormBuilders.BaseSet))]
        [TestCase(nameof(IFormBuilders.BaseAdd))]
        [TestCase(nameof(IFormBuilders.PercentIncrease))]
        public void PropertyBuildReturnsIdentityConverter(string propertyName)
        {
            var sut = CreateSut();

            var converter = GetProperty(sut, propertyName).Build().valueConverter;

            var expected = Mock.Of<IValueBuilder>();
            var actual = converter(expected);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(nameof(IFormBuilders.BaseSubtract))]
        [TestCase(nameof(IFormBuilders.PercentReduce))]
        [TestCase(nameof(IFormBuilders.PercentLess))]
        public void PropertytBuildReturnsNegatingConverter(string propertyName)
        {
            var sut = CreateSut();

            var converter = GetProperty(sut, propertyName).Build().valueConverter;

            var expected = Mock.Of<IValueBuilder>();
            var inputBuilder = Mock.Of<IValueBuilder>(v => v.Multiply(v.Create(-1)) == expected);
            var actual = converter(inputBuilder);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void BaseSetResolveReturnsSelf()
        {
            var sut = CreateSut();

            var expected = sut.BaseSet;
            var actual = expected.Resolve(null!);

            Assert.AreEqual(expected, actual);
        }

        private static FormBuilders CreateSut() => new FormBuilders();

        private static IFormBuilder GetProperty(IFormBuilders sut, string property)
            => (IFormBuilder) sut.GetType().GetProperty(property)!.GetValue(sut)!;
    }
}