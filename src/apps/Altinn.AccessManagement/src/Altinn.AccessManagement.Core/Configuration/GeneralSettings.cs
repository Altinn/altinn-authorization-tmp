namespace Altinn.AccessManagement.Core.Configuration
{
    /// <summary>
    /// General configuration settings
    /// </summary>
    public class GeneralSettings
    {
        /// <summary>
        /// Gets or sets the host name.
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Gets or sets the CPU load loop count.
        /// </summary>
        public long CpuLoadLoopCount { get; set; } = 100_000_000;
    }
}
