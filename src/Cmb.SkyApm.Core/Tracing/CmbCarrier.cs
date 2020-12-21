using Mapster;
using SkyApm.Common;
using SkyApm.Tracing;
using System.ComponentModel;

namespace Cmb.SkyApm.Tracing
{
    public class CmbCarrier : ICmbCarrier
    {
        #region Carrier

        public bool HasValue { get; private set; } = true;

        public bool? Sampled { get; set; }

        [Description("X-B3-TraceId")]
        public string TraceId { get; private set; }

        public string ParentSegmentId { get; private set; }

        public int ParentSpanId { get; private set; }

        public string ParentServiceId { get; private set; }

        public string ParentServiceInstanceId { get; private set; }

        public string EntryServiceInstanceId { get; private set; }

        public StringOrIntValue NetworkAddress { get; set; }

        public StringOrIntValue EntryEndpoint { get; set; }

        public StringOrIntValue ParentEndpoint { get; set; }

        #endregion

        [Description("X-B3-BusinessId")]
        public string BusinessId { get; set; }

        //public 
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
