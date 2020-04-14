using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoronaBuster;
using CoronaBuster.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusterHost.Services {
    public class PublicationService: IHostedService, IDisposable {
        public const string CACHE_PATH = "cache";
        public const string INTERIM_PATH = "interim";
        public const string PUBLIC_PATH = "wwwroot/public";

        private readonly ILogger<PublicationService> _logger;
        private Timer _timer;

        private int  _isBusy;
        CancellationToken _stoppingToken;

        public PublicationService(ILogger<PublicationService> logger) {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken) {
            _logger.LogInformation("Timed Hosted Service running.");
            _stoppingToken = stoppingToken;
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(10));
                //TimeSpan.FromMinutes(1200));

            return Task.CompletedTask;
        }

        private void DoWork(object state) {
            var old = Interlocked.CompareExchange(ref _isBusy, 1, 0);
            if (old != 0) return;

            var modifiedFiles = new HashSet<string>();
            try {
                Directory.CreateDirectory(INTERIM_PATH);
                Directory.CreateDirectory(PUBLIC_PATH);

                var publicationTime = Helpers.GetExactTime();
                var files = Directory.GetFiles(CACHE_PATH);


                foreach (var file in files) {
                    try {
                        _logger.LogInformation($"Publication: {file}");
                        using (var stream = File.OpenRead(file)) {

                            foreach (var r in ProtoBuf.Serializer.DeserializeItems<PublicRecordBase>(stream, ProtoBuf.PrefixStyle.Base128, 0)) {
                                var record = new PublicRecord(r, publicationTime);

                                // TODO: can be optimized by storing results in dictionary first
                                var outputPath = Path.Combine(INTERIM_PATH, record.Id.ToString());
                                using var output = new FileStream(outputPath, FileMode.Append | FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                                ProtoBuf.Serializer.SerializeWithLengthPrefix(output, record, ProtoBuf.PrefixStyle.Base128, 0);

                                modifiedFiles.Add(outputPath);
                            }
                        }

                        File.Delete(file);
                    } catch (Exception err) {
                        _logger.LogError(err, $"Error while processing {file}.");
                    }
                }
            } finally {

                foreach (var file in modifiedFiles) {
                    try {
                        File.Copy(file, file.Replace(INTERIM_PATH, PUBLIC_PATH), true);
                    } catch (Exception err) {
                        _logger.LogError(err, $"Error while copying interim file {file} to {PUBLIC_PATH}.");
                    }
                }

                _isBusy = 0;
            }
        }

        public Task StopAsync(CancellationToken stoppingToken) {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose() {
            _timer?.Dispose();
        }
    }
}
