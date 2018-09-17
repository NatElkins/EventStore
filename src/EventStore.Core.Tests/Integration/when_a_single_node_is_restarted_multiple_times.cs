﻿using EventStore.Core.Bus;
using EventStore.Core.Messages;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EventStore.Core.Tests.Integration
{
    [TestFixture, Category("LongRunning")]
    public class when_a_single_node_is_restarted_multiple_times : specification_with_a_single_node
    {
        private List<Guid> _epochIds = new List<Guid>();
        private const int _numberOfNodeStarts = 5;
        private ManualResetEventSlim _event = new ManualResetEventSlim(false);

        protected override void BeforeNodeStarts()
        {
            _node.Node.MainBus.Subscribe(new AdHocHandler<SystemMessage.EpochWritten>(Handle));
            base.BeforeNodeStarts();
        }

        protected override void Given()
        {
            for (int i = 0; i < _numberOfNodeStarts - 1; i++)
            {
                _event.Wait(TimeSpan.FromSeconds(5));
                ShutdownNode();
                _event.Reset();
                StartNode();
            }

            base.Given();
        }

        private void Handle(SystemMessage.EpochWritten msg)
        {
            _epochIds.Add(msg.Epoch.EpochId);
            _event.Set();
        }

        [Test]
        public void should_be_a_different_epoch_for_every_startup()
        {
            Assert.AreEqual(_numberOfNodeStarts, _epochIds.Distinct().Count());
        }
    }
}
