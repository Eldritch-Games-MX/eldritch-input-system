using NUnit.Framework;
using UnityEngine;

namespace EldritchGames.InputSystem.Tests
{
    public class PawnControllerTests
    {
        private GameObject _go;
        private TestPawnController _pawn;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject();
            _pawn = _go.AddComponent<TestPawnController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void TryGet_ReturnsTrue_WhenPawnImplementsCapability()
        {
            var found = _pawn.TryGet<ITestCapability>(out var cap);
            Assert.IsTrue(found);
            Assert.IsNotNull(cap);
            Assert.AreSame(_pawn, cap);
        }

        [Test]
        public void TryGet_ReturnsFalse_WhenPawnDoesNotImplementCapability()
        {
            var found = _pawn.TryGet<IUnimplementedCapability>(out var cap);
            Assert.IsFalse(found);
            Assert.IsNull(cap);
        }

        private interface ITestCapability { }
        private interface IUnimplementedCapability { }

        private class TestPawnController : PawnController, ITestCapability
        {
            public override void ExecuteCommand(ICommand command) { }
        }
    }
}
