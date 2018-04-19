namespace QueueServer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    public class AzureQueueClient
    {
        private SubscriptionClient _subscibtionClient;

        private int _sequence;

        public AzureQueueClient(string clientUniqueName)
        {
            NamespaceManager namespaceManager = NamespaceManager.Create();
            var topicName = ClientConstants.SettingTopicName;
            try
            {
                if (namespaceManager.TopicExists(topicName))
                {
                    if (namespaceManager.SubscriptionExists(topicName, clientUniqueName))
                    {
                        namespaceManager.DeleteSubscription(topicName, clientUniqueName);
                    }

                    namespaceManager.CreateSubscription(topicName, clientUniqueName);
                }
            }
            catch (Exception e)
            {
                // ignored
            }
            _subscibtionClient = SubscriptionClient.Create(topicName, clientUniqueName, ReceiveMode.ReceiveAndDelete);
        }

        public bool SendFile(string filePath)
        {
            return SendFile(ClientConstants.FileQueuName, filePath, 256000);
        }

        public bool SendFile(string queueName, string filePath, int chunkSize)
        {
            try
            {
                var client = QueueClient.Create(queueName);
                var length = new System.IO.FileInfo(filePath).Length;
                var sendchunk = new byte[chunkSize];
                using (FileStream fs = File.Open(filePath, FileMode.Open))
                {
                    for (int i = 0; i <= length / chunkSize; i++)
                    {
                        var partCount = new KeyValuePair<string, object>("PartCount", length / chunkSize);
                        var part = new KeyValuePair<string, object>("Part", i);
                        var sequenceNumber = new KeyValuePair<string, object>("Sequence", _sequence);
                        sendchunk = new BinaryReader(fs).ReadBytes(chunkSize);
                        client.Send(new BrokeredMessage(sendchunk)
                        {
                            ContentType = "application/octet-stream",
                            Properties =
                                {
                                    partCount, part, sequenceNumber
                                }
                        });
                       Array.Clear(sendchunk,0,chunkSize);
                    }
                }

                _sequence++;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool SendStatusMessage(string message)
        {
            return SendStatusMessage(ClientConstants.StatusQueueName, message);
        }

        public bool SendStatusMessage(string statusQueueName, string message)
        {
            try
            {
                var client = QueueClient.Create(statusQueueName);
                client.Send(new BrokeredMessage(message));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public Dictionary<string, int> ReceiveNewSettings()
        {
            var settinsDict = new Dictionary<string, int>();
            var message = _subscibtionClient.Receive(new TimeSpan(1));
            if (message != null)
            {
                var body = message.GetBody<string>();
                if (body == ClientConstants.NewConfigurationMessage)
                {
                    var timeoutSetting = (int)message.Properties[ClientConstants.TimeoutSectionName];
                    settinsDict.Add(ClientConstants.TimeoutSectionName, timeoutSetting);
                }
                if (body == ClientConstants.UpdateStatusMessage)
                {
                    settinsDict.Add("StatusUpdate", 1);
                }
            }

            return settinsDict;
        }
    }
}


