using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System.Collections.Generic;
using System.Linq;

namespace Cmb.SkyApm.Tracing
{
    public class CmbCarrierPropagator : CarrierPropagator
    {
        private readonly IEnumerable<ICarrierFormatter> _carrierFormatters;
        private readonly ISegmentContextFactory _segmentContextFactory;

        public CmbCarrierPropagator(IEnumerable<ICarrierFormatter> carrierFormatters,
            ISegmentContextFactory segmentContextFactory) : base(carrierFormatters, segmentContextFactory) { }

        public new void Inject(SegmentContext segmentContext, ICarrierHeaderCollection headerCollection)
        {
            base.Inject(segmentContext, headerCollection);

            headerCollection.Add("1", "a");
            headerCollection.Add("2", "b");
            headerCollection.Add("3", "c");
        }

        public new ICmbCarrier Extract(ICarrierHeaderCollection headerCollection)
        {
            var cmbCarrier = base.Extract(headerCollection).ToCmbCarrier();

            if (cmbCarrier.HasValue)
            {
                foreach (var header in headerCollection.Where(k => k.Key.StartsWith("X-B3-")))
                {

                }
            }

            return cmbCarrier;
        }
    }
}
