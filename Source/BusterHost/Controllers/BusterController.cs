using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoronaBuster.Models;
using CoronaBuster.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BusterHost.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class BusterController: ControllerBase {
        static Dictionary<uint, List<PublicRecord>> Records { get; } = new Dictionary<uint, List<PublicRecord>>();
        static List<PublicRecordBase> _cache = new List<PublicRecordBase>();
        static TimeSpan _lastPublication = TimeSpan.Zero;
        static object _lock = new object();

        private readonly ILogger<BusterController> _logger;

        public BusterController(ILogger<BusterController> logger) {
            _logger = logger;
        }

        [HttpGet("{id:int}/{ticksSince:long}")]
        public IEnumerable<PublicRecord> Get(int id, long ticksSince = 0) {
            CheckCache();

            _logger.LogInformation($"GET -> {id} -- {ticksSince}");
            if (!Records.TryGetValue((uint)id, out var records)) return Enumerable.Empty<PublicRecord>();
            var since = TimeSpan.FromTicks(ticksSince);
            _logger.LogInformation($"FOUND -> {id}, FILTER: {CoronaBuster.Helpers.EPOCH + since}");
            var result = records.Where(r => r.PublicationDate > since);
            _logger.LogInformation("RECORDS:\r\n" + string.Join(Environment.NewLine, result));
            return result;
        }

        [HttpPost]
        public void Post([FromBody]PublicRecordBase[] records) {
            _logger.LogInformation($"POST -> {records.Length} + {_cache.Count}");

            lock(_lock) _cache.AddRange(records);
        }

        // TODO: in production this should be done on a longer time scale for privacy, for instance every hour
        private static TimeSpan GetPublicationTime() => CoronaBuster.Helpers.GetApproximateMinute();


        private void CheckCache() {
            var pubTime = GetPublicationTime();
            if (_lastPublication < pubTime && _cache.Count > 0) {
                List<PublicRecordBase> tempCache;
                lock (_lock) {
                    if (_lastPublication >= pubTime) return; // already done on other thread after previous if
                    _lastPublication = pubTime;
                    tempCache = _cache;
                    _cache = new List<PublicRecordBase>();
                }
                ProcessCache(tempCache, _logger, pubTime);
            }
        }

        private static void ProcessCache(List<PublicRecordBase> tempCache, ILogger logger, TimeSpan pubDate) {
            logger.LogInformation($"PROCESSING CACHE -> {pubDate}");

            foreach (var record in tempCache.OrderBy(r => r.SharedSecret)) { // OrderBy so that the exact upload time cannot be guessed by sequence
                logger.LogInformation($"RECORD -> {record.Id}");
                if (!Records.TryGetValue(record.Id, out var list)) {
                    list = new List<PublicRecord>();
                    Records.Add(record.Id, list);
                }

                list.Add(new PublicRecord(record, pubDate));
            }

            logger.LogInformation($"CACHE PROCESSED -> {pubDate}");
        }
    }
}
