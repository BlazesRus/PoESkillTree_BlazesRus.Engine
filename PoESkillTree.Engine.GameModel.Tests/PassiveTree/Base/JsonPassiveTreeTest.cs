using NUnit.Framework;

namespace PoESkillTree.Engine.GameModel.PassiveTree.Base
{
    [TestFixture]
    public class JsonPassiveTreeTest
    {
        [TestCase(10f, 10f, 10f, 10f)]
        [TestCase(10f, 10f, 10f, -10f)]
        [TestCase(10f, 10f, -10f, 10f)]
        [TestCase(10f, -10f, 10f, 10f)]
        [TestCase(-10f, 10f, 10f, 10f)]
        [TestCase(-10f, 10f, 10f, -10f)]
        [TestCase(-10f, 10f, -10f, 10f)]
        [TestCase(-10f, -10f, 10f, 10f)]
        [TestCase(-10f, -10f, 10f, -10f)]
        [TestCase(-10f, -10f, -10f, 10f)]
        [TestCase(-10f, -10f, -10f, -10f)]
        public void JsonPassiveTree_Bounds(float minX, float minY, float maxX, float maxY)
        {
            var tree = new JsonPassiveTree
            {
                MinX = minX,
                MinY = minY,
                MaxX = maxX,
                MaxY = maxY
            };

            Assert.AreEqual(tree.MinX, tree.Bounds.Left, $"{nameof(tree.MinX)} doesn't equal {nameof(tree.Bounds)}.{nameof(tree.Bounds.Left)}");
            Assert.AreEqual(tree.MinY, tree.Bounds.Top, $"{nameof(tree.MinY)} doesn't equal {nameof(tree.Bounds)}.{nameof(tree.Bounds.Top)}");
            Assert.AreEqual(tree.MaxX, tree.Bounds.Right, $"{nameof(tree.MaxX)} doesn't equal {nameof(tree.Bounds)}.{nameof(tree.Bounds.Right)}");
            Assert.AreEqual(tree.MaxY, tree.Bounds.Bottom, $"{nameof(tree.MaxY)} doesn't equal {nameof(tree.Bounds)}.{nameof(tree.Bounds.Bottom)}");
        }

        [TestCase("test")]
        [TestCase("test/")]
        [TestCase("/test/")]
        [TestCase("http://web.poecdn.com/image/")]
        public void JsonPassiveTree_ImageRoot(string imageRoot)
        {
            var tree = new JsonPassiveTree
            {
                ImageRoot = imageRoot
            };

            Assert.IsTrue(tree.ImageRoot.StartsWith('/'));
            Assert.IsTrue(tree.ImageRoot.EndsWith('/'));
            Assert.IsFalse(tree.ImageRoot.Contains(tree.WebCDN.AbsoluteUri));
        }

        [TestCase(new[] { 1f, 2f, 3f, 4f }, ExpectedResult = 4f)]
        [TestCase(new[] { 1f, 2f, 3f }, ExpectedResult = 3f)]
        [TestCase(new[] { 1f, 2f }, ExpectedResult = 2f)]
        [TestCase(new[] { 1f }, ExpectedResult = 1f)]
        [TestCase(new float[0], ExpectedResult = 1f)]
        public float JsonPassiveTree_MaxImageZoomLevel(float[] imageZoomLevels)
        {
            var tree = new JsonPassiveTree
            {
                ImageZoomLevels = imageZoomLevels
            };

            return tree.MaxImageZoomLevel;
        }
    }
}
