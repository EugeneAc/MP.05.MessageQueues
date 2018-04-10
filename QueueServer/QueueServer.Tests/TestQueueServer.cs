using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueServer.Tests
{
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    class TestQueueServer
    {
        private QueueServer _server;
        private QueueClient _client;

        [TestInitialize]
        public void Init()
        {

            NamespaceManager namespaceManager = NamespaceManager.Create();
            var queueName = "testqueue";
            if (namespaceManager.QueueExists(queueName))
            {
                namespaceManager.DeleteQueue(queueName);
            }

            _server = new QueueServer(queueName);
            namespaceManager.CreateQueue(new QueueDescription(queueName) { EnablePartitioning = false });
            _client = QueueClient.Create(queueName, ReceiveMode.ReceiveAndDelete);
        }
    }
}
