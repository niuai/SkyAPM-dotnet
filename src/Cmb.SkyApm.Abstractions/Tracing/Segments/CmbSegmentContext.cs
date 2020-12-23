using SkyApm.Tracing.Segments;

namespace Cmb.SkyApm.Tracing.Segments
{
    public class CmbSegmentContext : SegmentContext
    {
        public string BusinessId { get; set; }

        public string CmbParentSpanId { get; set; }

        public string CmbSpanId { get; set; }

        public string CmbTimeStamp { get; set; }

        public string CmbSampled { get; set; }

        public string CmbDebug { get; set; }

        public CmbSegmentContext(string traceId, string segmentId, bool sampled, string serviceId, string serviceInstanceId,
            string operationName, SpanType spanType) : base(traceId, segmentId, sampled, serviceId, serviceInstanceId, operationName, spanType)
        {

        }
    }
}
