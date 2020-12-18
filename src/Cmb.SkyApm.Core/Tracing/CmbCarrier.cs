using Mapster;
using SkyApm.Common;
using SkyApm.Tracing;

namespace Cmb.SkyApm.Tracing
{
    public class CmbCarrier : ICmbCarrier
    {
        #region Carrier

        public bool HasValue { get; } = true;

        public bool? Sampled { get; set; }

        public string TraceId { get; }

        public string ParentSegmentId { get; }

        public int ParentSpanId { get; }

        public string ParentServiceId { get; }

        public string ParentServiceInstanceId { get; }

        public string EntryServiceInstanceId { get; }

        public StringOrIntValue NetworkAddress { get; set; }

        public StringOrIntValue EntryEndpoint { get; set; }

        public StringOrIntValue ParentEndpoint { get; set; }

        #endregion

        public string BusinessId { get; set; }
    }

    public static class CarrierExtensions
    {
        public static ICmbCarrier ToCmbCarrier(this ICarrier carrier)
        {
            if (!carrier.HasValue)
                return NullableCmbCarrier.Instance;

            return carrier.Adapt(new CmbCarrier());
        }
    }
}
