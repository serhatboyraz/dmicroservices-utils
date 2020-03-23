using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;

namespace DMicroservices.Utils.Logger
{
    public class ElasticLogger
    {
        private static Serilog.Core.Logger _log;

        #region Singleton Section
        private static readonly Lazy<ElasticLogger> _instance = new Lazy<ElasticLogger>(() => new ElasticLogger());
        public static ElasticLogger Instance => _instance.Value;

        public bool IsConfigured { get; set; } = false;
        private ElasticLogger()
        {
            bool environmentNotCorrect = false;
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ELASTIC_URI")))
            {
                Console.WriteLine("ELASTIC_URI is empty.");
                environmentNotCorrect = true;
            }

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LOG_INDEX_FORMAT")))
            {
                Console.WriteLine("LOG_INDEX_FORMAT is empty.");
                environmentNotCorrect = true;
            }

            if (!environmentNotCorrect)
                Configure();
        }


        #endregion

        public void Error(Exception ex, string messageTemplate)
        {
            if (IsConfigured)
                _log.Error(ex, messageTemplate);
        }

        public void Info(string messageTemplate)
        {
            if (IsConfigured)
                _log.Information(messageTemplate);
        }

        private void Configure()
        {
            _log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Elasticsearch(
                    new ElasticsearchSinkOptions(new Uri(Environment.GetEnvironmentVariable("ELASTIC_URI")))
                    {
                        AutoRegisterTemplate = true,
                        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                        TemplateName = "serilog-events-template",
                        IndexFormat =
                            Environment.GetEnvironmentVariable("LOG_INDEX_FORMAT") // ex.  "serilog-{0:yyyy.MM.dd}"
                    })
                .CreateLogger();

            IsConfigured = true;
        }
    }
}