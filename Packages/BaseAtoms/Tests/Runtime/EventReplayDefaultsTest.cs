using System.Collections.Generic;
using NUnit.Framework;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Tests
{
    public class EventReplayDefaultsTest
    {
        private IntEvent _event;

        [SetUp]
        public void SetUp() => _event = ScriptableObject.CreateInstance<IntEvent>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_event);

        [Test]
        public void NewEventDoesNotBufferValues()
        {
            _event.Raise(7);
            Assert.AreEqual(0, _event.ReplayBufferSize);
            Assert.IsEmpty(_event.ReplayBuffer);
        }

        [Test]
        public void RegisterSkipsReplayButReceivesFutureValues()
        {
            _event.ReplayBufferSize = 1;
            _event.Raise(7);
            var values = new List<int>();
            _event.Register(values.Add);
            Assert.IsEmpty(values);
            _event.Raise(8);
            CollectionAssert.AreEqual(new[] { 8 }, values);
        }

        [Test]
        public void RegisterCanExplicitlyReplay()
        {
            _event.ReplayBufferSize = 1;
            _event.Raise(7);
            var values = new List<int>();
            _event.Register(values.Add, replayEventsBuffer: true);
            CollectionAssert.AreEqual(new[] { 7 }, values);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ListenerReplaysOnlyWhenRequested(bool replay)
        {
            _event.ReplayBufferSize = 1;
            _event.Raise(7);
            var listener = new Listener();
            if (replay)
                _event.RegisterListener(listener, replayEventsBuffer: true);
            else
                _event.RegisterListener(listener);
            CollectionAssert.AreEqual(replay ? new[] { 7 } : new int[0], listener.Values);
            _event.Raise(8);
            Assert.AreEqual(8, listener.Values[listener.Values.Count - 1]);
        }

        private sealed class Listener : IAtomListener<int>
        {
            public readonly List<int> Values = new List<int>();
            public void OnEventRaised(int item) => Values.Add(item);
        }
    }
}
