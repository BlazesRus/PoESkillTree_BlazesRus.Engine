using System.Linq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Core.Events;

namespace PoESkillTree.Engine.Computation.Core.NodeCollections
{
    [TestFixture]
    public class NodeCollectionTest
    {
        [Test]
        public void SutIsINodeCollection()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<INodeCollection<int>>(sut);
        }

        [Test]
        public void IsEmptyInitially()
        {
            var sut = CreateSut();

            CollectionAssert.IsEmpty(sut);
        }

        [Test]
        public void AddAddsNode()
        {
            var node = NodeHelper.MockNode();
            var sut = CreateSut();

            sut.Add(node, 0);

            CollectionAssert.AreEquivalent(new[] { (node, 0) }, sut);
        }

        [Test]
        public void RemoveRemovesNode()
        {
            var node = NodeHelper.MockNode();
            var sut = CreateSut();
            sut.Add(node, 0);

            sut.Remove(node, 0);

            CollectionAssert.IsEmpty(sut);
        }

        [Test]
        public void IsSet()
        {
            var node = NodeHelper.MockNode();
            var sut = CreateSut();
            sut.Add(node, 0);
            sut.Add(node, 0);

            CollectionAssert.AllItemsAreUnique(sut);
        }

        [Test]
        public void CountReturnsCorrectResult()
        {
            var sut = CreateSut();
            sut.Add(NodeHelper.MockNode(), 0);
            sut.Add(NodeHelper.MockNode(), 0);
            sut.Add(NodeHelper.MockNode(), 0);

            Assert.AreEqual(3, sut.Count);
        }

        [Test]
        public void AddRaisesCollectionChanged()
        {
            var node = NodeHelper.MockNode();
            var sut = CreateSut();
            var raised = false;
            sut.CollectionChanged += (sender, args) =>
            {
                Assert.AreSame(sender, sut);
                Assert.AreEqual(new[] { (node, 0) }, args.AddedItems);
                Assert.IsEmpty(args.RemovedItems);
                raised = true;
            };

            sut.Add(node, 0);

            Assert.IsTrue(raised);
        }

        [Test]
        public void RemoveRaisesCollectionChanged()
        {
            var node = NodeHelper.MockNode();
            var sut = CreateSut();
            sut.Add(node, 0);
            var raised = false;
            sut.CollectionChanged += (sender, args) =>
            {
                Assert.AreSame(sender, sut);
                Assert.IsEmpty(args.AddedItems);
                Assert.AreEqual(new[] { (node, 0) }, args.RemovedItems);
                raised = true;
            };

            sut.Remove(node, 0);

            Assert.IsTrue(raised);
        }

        [Test]
        public void RemoveDoesNotRaiseCollectionChangeIfNodeWasNotAdded()
        {
            var node = NodeHelper.MockNode();
            var sut = CreateSut();

            sut.CollectionChanged += (sender, args) => Assert.Fail();
            sut.Remove(node, 0);
        }

        [Test]
        public void NodePropertiesIsCorrect()
        {
            var removed = NodeHelper.MockNode();
            var expected = new[]
            {
                (NodeHelper.MockNode(), 0),
                (NodeHelper.MockNode(), 1),
                (NodeHelper.MockNode(), 2),
            };
            var sut = CreateSut();

            sut.Add(removed, -1);
            expected.ForEach(t => sut.Add(t.Item1, t.Item2));
            sut.Remove(removed, -1);

            CollectionAssert.AreEquivalent(expected, sut);
        }

        [TestCase(3, 1)]
        [TestCase(0, 0)]
        public void SubscriberCountReturnsCorrectResult(int changed, int untypedChanged)
        {
            var sut = CreateSut();
            Enumerable.Repeat(0, changed).ForEach(_ => sut.CollectionChanged += (sender, args) => { });
            Enumerable.Repeat(0, untypedChanged).ForEach(_ => sut.UntypedCollectionChanged += (sender, args) => { });

            var actual = sut.SubscriberCount;

            Assert.AreEqual(changed + untypedChanged, actual);
        }

        [Test]
        public void AddDoesNotRaiseCollectionChangedIfPreviouslyAdded()
        {
            var node = NodeHelper.MockNode();
            var sut = CreateSut();
            sut.Add(node, 0);

            sut.CollectionChanged += (sender, args) => Assert.Fail();
            sut.Add(node, 0);
        }

        private static NodeCollection<int> CreateSut()
            => new NodeCollection<int>(new EventBuffer());
    }
}