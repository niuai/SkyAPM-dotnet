using SkyApm.Common;

namespace Cmb.SkyApm.Tracing
{
    public class NullableCmbCarrier : ICmbCarrier
    {
        public static NullableCmbCarrier Instance { get; } = new NullableCmbCarrier();

        #region NullableCarrier

        public bool HasValue { get; } = false;

        public bool? Sampled { get; }

        public string TraceId { get; }

        public string ParentSegmentId { get; }

        public int ParentSpanId { get; }

        public string ParentServiceInstanceId { get; }

        public string EntryServiceInstanceId { get; }

        public StringOrIntValue NetworkAddress { get; }

        public StringOrIntValue EntryEndpoint { get; }

        public StringOrIntValue ParentEndpoint { get; }

        public string ParentServiceId { get; }

        #endregion

        public string BusinessId { get; set; }
    }
}
