using SkyApm.Config;
using SkyApm.Logging;
using SkyApm.Transport;
using SkyApm.Transport.Grpc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cmb.SkyApm.Transport.Http
{
    public class CmbSegmentReporter : ISegmentReporter
    {
        private readonly ISegmentReporter _segmentReporterOrigin;
        private readonly GrpcConfig _grpcConfig;

        public CmbSegmentReporter(ConnectionManager connectionManager, IConfigAccessor configAccessor,
            ILoggerFactory loggerFactory)
        {
            _grpcConfig = configAccessor.Get<GrpcConfig>();
            _segmentReporterOrigin = new SegmentReporter(connectionManager, configAccessor, loggerFactory);
        }

        public async Task ReportAsync(IReadOnlyCollection<SegmentRequest> segmentRequests,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!string.IsNullOrEmpty(_grpcConfig.Servers))
                await _segmentReporterOrigin.ReportAsync(segmentRequests, cancellationToken);

            var t = segmentRequests;
        }
    }
}
