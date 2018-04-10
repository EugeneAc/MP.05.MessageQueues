namespace QueueServer
{
    using System.Configuration;

    public class ServiceConfigurations : ConfigurationSection
    {
        [ConfigurationProperty("Timeout")]
        public TimeoutTime TimeoutTime
        {
            get { return (TimeoutTime)this["Timeout"]; }
        }
    }

    public class TimeoutTime : ConfigurationElement
    {
        [ConfigurationProperty("duration")]
        public int Duration
        {
            get { return (int)this["duration"]; }
        }
    }
}
