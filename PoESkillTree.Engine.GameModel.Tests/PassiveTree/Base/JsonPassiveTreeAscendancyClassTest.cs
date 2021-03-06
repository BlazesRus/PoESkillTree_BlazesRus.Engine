using Newtonsoft.Json;
using NUnit.Framework;
using System.Drawing;

namespace PoESkillTree.Engine.GameModel.PassiveTree.Base
{
    public class JsonPassiveTreeAscendancyClassTest
    {
        [TestCase(25f, 25f, 50f, 50f)]
        [TestCase(0.25f, 0.50f, 150f, 50f)]
        [TestCase(0.50f, 0.25f, 100f, 50f)]
        public void JsonAscendancyClassOption_FlavourTextBounds(float x, float y, float width, float height)
        {
            var ascendancy = JsonConvert.DeserializeObject<JsonPassiveTreeAscendancyClass>($"{{\"flavourTextRect\": {{ \"x\": {x},\"y\": {y},\"width\": {width},\"height\": {height} }}}}");

            Assert.AreEqual(x, ascendancy.FlavourTextBounds.X);
            Assert.AreEqual(y, ascendancy.FlavourTextBounds.Y);
            Assert.AreEqual(width, ascendancy.FlavourTextBounds.Width);
            Assert.AreEqual(height, ascendancy.FlavourTextBounds.Height);
        }

        [TestCase(25f, 25f, 50f, 50f)]
        [TestCase(0.25f, 0.50f, 150f, 50f)]
        [TestCase(0.50f, 0.25f, 100f, 50f)]
        public void JsonAscendancyClassOption_FlavourTextBounds_Old(float x, float y, float width, float height)
        {
            var ascendancy = JsonConvert.DeserializeObject<JsonPassiveTreeAscendancyClass>($"{{\"flavourTextRect\": \"{x},{y},{width},{height}\" }}");

            Assert.AreEqual(x, ascendancy.FlavourTextBounds.X);
            Assert.AreEqual(y, ascendancy.FlavourTextBounds.Y);
            Assert.AreEqual(width, ascendancy.FlavourTextBounds.Width);
            Assert.AreEqual(height, ascendancy.FlavourTextBounds.Height);
        }

        [Test]
        public void JsonAscendancyClassOption_FlavourTextBounds_Empty()
        {
            var ascendancy = JsonConvert.DeserializeObject<JsonPassiveTreeAscendancyClass>($"{{ \"flavourTextRect\": {{ }} }}");

            Assert.AreEqual(RectangleF.Empty, ascendancy.FlavourTextBounds);
        }

        [TestCase("50,50,50,50,50")]
        [TestCase("50,50,50")]
        [TestCase("50,50")]
        [TestCase("50")]
        [TestCase("")]
        public void JsonAscendancyClassOption_FlavourTextBounds_Empty_Old(string boundsString)
        {
            var ascendancy = JsonConvert.DeserializeObject<JsonPassiveTreeAscendancyClass>($"{{\"flavourTextRect\": \"{boundsString}\" }}");

            Assert.AreEqual(RectangleF.Empty, ascendancy.FlavourTextBounds);
        }

        [TestCase("128,128,128")]
        [TestCase("128128")]
        [TestCase("128")]
        public void JsonAscendancyClassOption_FlavourTextColour_IsNotEmpty(string colourString)
        {
            var ascendancy = JsonConvert.DeserializeObject<JsonPassiveTreeAscendancyClass>($"{{\"flavourTextColour\": \"{colourString}\" }}");

            Assert.AreNotEqual(Color.Empty, ascendancy.FlavourTextColour);
        }

        [TestCase(128, 128, 128)]
        [TestCase(128, 128, 128)]
        [TestCase(128, 128, 128)]
        public void JsonAscendancyClassOption_FlavourTextColour_Old(byte red, byte green, byte blue)
        {
            var ascendancy = JsonConvert.DeserializeObject<JsonPassiveTreeAscendancyClass>($"{{\"flavourTextColour\": \"{red},{green},{blue}\" }}");

            Assert.IsNotNull(ascendancy.FlavourTextColour);
            Assert.AreEqual(red, ascendancy.FlavourTextColour.R);
            Assert.AreEqual(green, ascendancy.FlavourTextColour.G);
            Assert.AreEqual(blue, ascendancy.FlavourTextColour.B);
        }

        [TestCase("128,128,128,128")]
        [TestCase("128,128")]
        [TestCase("")]
        public void JsonAscendancyClassOption_FlavourTextColour_IsEmpty(string colourString)
        {
            var ascendancy = JsonConvert.DeserializeObject<JsonPassiveTreeAscendancyClass>($"{{\"flavourTextColour\": \"{colourString}\" }}");

            Assert.AreEqual(Color.Empty, ascendancy.FlavourTextColour);
        }
    }
}
