using System;

namespace net.jommy.RuuviCore.Common
{
    /// <summary>
    /// The HTTP listening settings.
    /// </summary>
    [Serializable]
    public class ListeningSettings
    {
        public bool HttpEnabled { get; set; }
        public bool UseHttps { get; set; }
        public int ListeningPort { get; set; }
        public string CertificateFile { get; set; }
        public string CertificateKey { get; set; }
    }
}