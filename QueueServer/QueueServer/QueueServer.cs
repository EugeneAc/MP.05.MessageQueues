namespace QueueServer
{
    using System;
    using System.Collections.Generic;
	using System.Configuration;
	using System.IO;
	using System.Threading.Tasks;

	using Microsoft.ServiceBus;
	using Microsoft.ServiceBus.Messaging;

	public class QueueServer
	{
		private string _saveFilePath;
		private Task _receiveFileTask;
		private Task _receiveStatusTask;

		private string _fileQueueName;
		private string _statusQueueName;

		private string _topicName;

	    private TopicClient _topicClient;


		public QueueServer(string saveFilePath)
			: this(saveFilePath, ServerConstants.FileQueuName, ServerConstants.StatusQueueName)
		{
		}

		public QueueServer(string saveFilePath, string fileQueueName, string statusQueueName)
		{
			_fileQueueName = fileQueueName;
			_statusQueueName = statusQueueName;
			_saveFilePath = saveFilePath;
		    
            CreateQueue(_fileQueueName);
			CreateQueue(_statusQueueName);

			NamespaceManager namespaceManager = NamespaceManager.Create();
			_topicName = ServerConstants.SettingTopicName;


			if (!namespaceManager.TopicExists(_topicName))
			{
			    namespaceManager.CreateTopic(new TopicDescription(_topicName) { EnablePartitioning = false });
            }

		    _topicClient = TopicClient.Create(_topicName);
            FileSystemWatcher watcher = new FileSystemWatcher();
			watcher.Path = Directory.GetCurrentDirectory();
		    watcher.Filter = ServerConstants.ConfigFileName;
			watcher.NotifyFilter = NotifyFilters.LastWrite;
			watcher.Changed += this.OnChanged;
			watcher.EnableRaisingEvents = true;
		}

		public void SendBroadcastMessage(string message, Dictionary<string, object> parameters)
		{
			var messageToSend = new BrokeredMessage(message);
			if (parameters != null)
			{
				foreach (var p in parameters)
				{
					messageToSend.Properties.Add(p);
				}
			}

			_topicClient.Send(messageToSend);
		}

	    private void OnChanged(object sender, FileSystemEventArgs e)
	    {
	        ConfigurationManager.RefreshSection(ServerConstants.ConfigSectionName);
	        var config = (ServiceConfigurations)ConfigurationManager.GetSection(ServerConstants.ConfigSectionName);
	        if (config == null)
	        {
	            return;
	        }

	        var timeout = config.TimeoutTime.Duration;
	        this.SendBroadcastMessage(ServerConstants.NewConfigurationMessage, new Dictionary<string, object>() { { ServerConstants.TimeoutSectionName, timeout } });
	    }

        private void ReceiveStatusMessage()
		{
			var client = QueueClient.Create(_statusQueueName, ReceiveMode.ReceiveAndDelete);
			while (true)
			{
			    var msg = client.Receive(new TimeSpan(1000));
				if (msg != null)
				{
			        var result = msg.GetBody<string>();
			        File.AppendAllText(ServerConstants.StatusMessgaesFileName, DateTime.Now.ToString("HH:mm:ss") + " - " + result + Environment.NewLine);
			    }
			}
		}

		public void Start()
		{
			_receiveFileTask = Task.Factory.StartNew(ReceiveFileFromServiceBus);
			_receiveStatusTask = Task.Factory.StartNew(ReceiveStatusMessage);
		}

		private void CreateQueue(string queueName)
		{
			NamespaceManager namespaceManager = NamespaceManager.Create();
			if (!namespaceManager.QueueExists(queueName))
			{
			    namespaceManager.CreateQueue(new QueueDescription(queueName) { EnablePartitioning = false });
            }
		}

		private void ReceiveFileFromServiceBus()
		{
			var client = QueueClient.Create(_fileQueueName, ReceiveMode.ReceiveAndDelete);

			while (true)
			{
				var firstmessage = client.Receive();
				if (firstmessage != null)
				{
					var partCount = (long)firstmessage.Properties["PartCount"];
					var partNumber = (int)firstmessage.Properties["Part"];
					var sequenceNumber = (int)firstmessage.Properties["Sequence"];
					var bytes = new List<byte>();
					bytes.AddRange(firstmessage.GetBody<byte[]>());
					while (partNumber < partCount)
					{
						var b = client.Receive();
						bytes.AddRange(b.GetBody<byte[]>());
						partNumber = (int)b.Properties["Part"];
					}

					File.WriteAllBytes(Path.Combine(_saveFilePath, @"AzureDocument" + sequenceNumber + ".pdf"), bytes.ToArray());
				}
			}
		}
	}
}
