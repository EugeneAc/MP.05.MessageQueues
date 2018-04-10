namespace QueueServer.Tests
{
    using System.IO;
    using System.Net;
    using System.Threading;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class TestServer
	{
		private QueueServer _server;
		private string _statusQueueName;

		[TestInitialize]
		public void Init()
		{
			var queueName = "testqueue";
			_statusQueueName = "testStatusQueue";
            _server = new QueueServer(Directory.GetCurrentDirectory(), queueName, _statusQueueName);
		    _server.Start();
        }

		[TestMethod]
		public void TestSendReceiveMessage()
		{
            if (File.Exists("StatusMessages.txt"))
            { 
                File.Delete("StatusMessages.txt");
            }

            var client = new AzureQueueClient("TestClient");
			client.SendStatusMessage(_statusQueueName, "TestStatusMessage");
		    Thread.Sleep(5000);
            Assert.IsTrue(File.Exists("StatusMessages.txt"));
			var text = File.ReadAllText(@"StatusMessages.txt");
			Assert.IsTrue(text.Contains("TestStatusMessage"));
        }

	    [TestMethod]
	    public void TestSendReceiveFile()
	    {
	        using (var client = new WebClient())
	        {
	            client.DownloadFile("http://www.pdf995.com/samples/pdf.pdf", "TestFile.pdf");
	        }

	        var queueclient = new AzureQueueClient("TestClient");
            queueclient.SendFile("testqueue", "TestFile.pdf", 256000);
            Thread.Sleep(10000);
	        Assert.IsTrue(File.Exists("TestFile.pdf"));
        }

	    [TestMethod]
	    public void TestSendReceiveBroadcast()
	    {
			var queueclient = new AzureQueueClient("TestClient1");
            var queueclient2 = new AzureQueueClient("TestClient2");

            string readText = File.ReadAllText(@"Service.config");
	        var newtext = readText.Replace(@"<Timeout duration=""1000"" />", @"<Timeout duration=""10000"" />");
	        File.WriteAllText("Service.config", newtext);
			Thread.Sleep(1000);
			
	        var s = queueclient.ReceiveNewSettings();
	        var setting = s["Timeout"];
            Assert.IsTrue(setting == 10000);
            
	        var s2 = queueclient2.ReceiveNewSettings();
	        var setting2 = s2["Timeout"];
			Assert.IsTrue(setting2 == 10000);
		}
    }
}
