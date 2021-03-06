using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Core.Events;
using PoESkillTree.Engine.Computation.Core.NodeCollections;

namespace PoESkillTree.Engine.Computation.Core.Graphs
{
    [TestFixture]
    public class StatRegistryTest
    {
        [Test]
        public void SutIsDeterminesNodeRemoval()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IDeterminesNodeRemoval>(sut);
        }

        [Test]
        public void AddAddsCorrectNodeToNodeCollection()
        {
            var stat = new StatStub { ExplicitRegistrationType = Registered };
            var coreNode = Mock.Of<ICalculationNode>();
            var nodeRepository = Mock.Of<INodeRepository>(r => r.GetNode(stat, NodeType.Total, Path) == coreNode);
            var nodeCollection = CreateNodeCollection();
            var sut = CreateSut(nodeCollection, nodeRepository);

            sut.Add(stat);

            CollectionAssert.IsNotEmpty(nodeCollection);
            CollectionAssert.DoesNotContain(nodeCollection, coreNode);
        }

        [Test]
        public void RemoveRemovesCorrectNodeFromNodeCollection()
        {
            var stat = new StatStub { ExplicitRegistrationType = Registered };
            var coreNode = Mock.Of<ICalculationNode>();
            var nodeRepository = Mock.Of<INodeRepository>(r => r.GetNode(stat, NodeType.Total, Path) == coreNode);
            var nodeCollection = CreateNodeCollection();
            var sut = CreateSut(nodeCollection, nodeRepository);
            sut.Add(stat);

            sut.Remove(stat);

            CollectionAssert.IsEmpty(nodeCollection);
        }

        [Test]
        public void AddDoesNotAddIfStatIsNotRegisteredExplicitly()
        {
            var stat = new StatStub();
            var nodeCollection = CreateNodeCollection();
            var sut = CreateSut(nodeCollection);

            sut.Add(stat);

            CollectionAssert.IsEmpty(nodeCollection);
        }

        [Test]
        public void RemoveDoesNothingIfStatWasNotAdded()
        {
            var stat = new StatStub { ExplicitRegistrationType = Registered };
            var sut = CreateSut();

            sut.Remove(stat);
        }

        [TestCase(1, ExpectedResult = false)]
        [TestCase(0, ExpectedResult = true)]
        public bool CanBeRemovedWithCountsSubscribersReturnsCorrectResult(int subscriberCount)
        {
            var node = Mock.Of<ICountsSubsribers>(c => c.SubscriberCount == subscriberCount);
            var sut = CreateSut();

            return sut.CanBeRemoved(node);
        }

        [TestCase(1, ExpectedResult = false)]
        [TestCase(0, ExpectedResult = true)]
        public bool CanBeRemovedWithUnknownNodeReturnsCorrectResult(int subscriberCount)
        {
            var node =
                Mock.Of<IBufferingEventViewProvider<ICalculationNode>>(c => c.SubscriberCount == subscriberCount);
            var sut = CreateSut();

            return sut.CanBeRemoved(node);
        }

        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = false)]
        public bool CanBeRemovedWithKnownNodeReturnsCorrectResult(int subscriberCount)
        {
            var stat = new StatStub { ExplicitRegistrationType = Registered };
            var coreNode = Mock.Of<ICalculationNode>();
            var nodeRepository = Mock.Of<INodeRepository>(r => r.GetNode(stat, NodeType.Total, Path) == coreNode);
            var sut = CreateSut(nodeRepository: nodeRepository);
            sut.Add(stat);
            var node = Mock.Of<IBufferingEventViewProvider<ICalculationNode>>(c =>
                c.SubscriberCount == subscriberCount && c.BufferingView == coreNode);

            return sut.CanBeRemoved(node);
        }

        [TestCase(1, ExpectedResult = false)]
        public bool CanBeRemovedWithRemovedNodeReturnsCorrectResult(int subscriberCount)
        {
            var stat = new StatStub { ExplicitRegistrationType = Registered };
            var coreNode = Mock.Of<ICalculationNode>();
            var nodeRepository = Mock.Of<INodeRepository>(r => r.GetNode(stat, NodeType.Total, Path) == coreNode);
            var sut = CreateSut(nodeRepository: nodeRepository);
            sut.Add(stat);
            sut.Remove(stat);
            var node = Mock.Of<IBufferingEventViewProvider<ICalculationNode>>(c =>
                c.SubscriberCount == subscriberCount && c.BufferingView == coreNode);

            return sut.CanBeRemoved(node);
        }

        [Test]
        public void RemoveDisposesWrappingNode()
        {
            var stat = new StatStub { ExplicitRegistrationType = Registered };
            var nodeMock = new Mock<ICalculationNode>();
            var nodeCollection = CreateNodeCollection();
            var nodeRepository = Mock.Of<INodeRepository>(r => r.GetNode(stat, NodeType.Total, Path) == nodeMock.Object);
            var sut = CreateSut(nodeCollection, nodeRepository);
            sut.Add(stat);
            var wrappingNode = nodeCollection.Single().node;

            sut.Remove(stat);
            
            wrappingNode.AssertValueChangedWillNotBeInvoked();
            nodeMock.RaiseValueChanged();
        }

        private static StatRegistry CreateSut(
            NodeCollection<IStat>? nodeCollection = null,
            INodeRepository? nodeRepository = null)
        {
            return new StatRegistry(nodeCollection ?? CreateNodeCollection())
            {
                NodeRepository = nodeRepository
            };
        }

        private static readonly PathDefinition Path = PathDefinition.MainPath;

        private static readonly ExplicitRegistrationType Registered = ExplicitRegistrationTypes.UserSpecifiedValue(0);

        private static NodeCollection<IStat> CreateNodeCollection()
            => new NodeCollection<IStat>(new EventBuffer());
    }
}