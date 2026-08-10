using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace EldritchGames.InputSystem.Tests
{
    public class ActionMapInputContextTests
    {
        private InputActionMap _map;
        private TestActionMapContext _ctx;

        [SetUp]
        public void SetUp()
        {
            _map = new InputActionMap("Test");
            _map.AddAction("dummy", InputActionType.Button);
            _ctx = new TestActionMapContext(_map);
        }

        [Test]
        public void OnPush_EnablesActionMap()
        {
            _ctx.OnPush();
            Assert.IsTrue(_map.enabled);
        }

        [Test]
        public void OnPop_DisablesActionMap()
        {
            _map.Enable();
            _ctx.OnPop();
            Assert.IsFalse(_map.enabled);
        }

        [Test]
        public void OnResume_EnablesActionMap()
        {
            _ctx.OnResume();
            Assert.IsTrue(_map.enabled);
        }

        [Test]
        public void OnPause_DisablesActionMap()
        {
            _map.Enable();
            _ctx.OnPause();
            Assert.IsFalse(_map.enabled);
        }

        [Test]
        public void CollectCommands_DelegatesToSubclass()
        {
            _ctx.CollectCommands(new List<ICommand>());
            Assert.IsTrue(_ctx.CollectCommandsCalled);
        }

        private class TestActionMapContext : ActionMapInputContext
        {
            public bool CollectCommandsCalled { get; private set; }

            public TestActionMapContext(InputActionMap map) : base(map) { }

            public override void CollectCommands(IList<ICommand> buffer)
            {
                CollectCommandsCalled = true;
            }
        }
    }
}
