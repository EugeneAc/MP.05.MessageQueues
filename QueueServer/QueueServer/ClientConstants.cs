using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueServer
{
        static class ClientConstants
        {
            public static string StatusQueueName { get; set; } = "StatusQueue";

            public static string FileQueuName { get; set; } = "FileQueue";

            public static string SettingTopicName { get; set; } = "SettingsTopic";

            public static string NewConfigurationMessage { get; set; } = "New Configuration";

            public static string UpdateStatusMessage { get; set; } = "Update Status";

            public static string TimeoutSectionName { get; set; } = "Timeout";

    }
}
