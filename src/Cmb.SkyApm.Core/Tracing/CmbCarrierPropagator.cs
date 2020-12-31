using Cmb.SkyApm.Common;
using Cmb.SkyApm.Tracing.Segments;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Cmb.SkyApm.Tracing
{
    public class CmbCarrierPropagator : ICarrierPropagator
    {
        private readonly IEnumerable<ICarrierFormatter> _carrierFormatters;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;

        public CmbCarrierPropagator(IEnumerable<ICarrierFormatter> carrierFormatters,
            IEntrySegmentContextAccessor entrySegmentContextAccessor)
        {
            _carrierFormatters = carrierFormatters;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
        }

        public void Inject(SegmentContext segmentContext, ICarrierHeaderCollection headerCollection)
        {
            var cmbSegmentContext = (CmbSegmentContext)segmentContext;
            var carrier = OriginInject(segmentContext, headerCollection);
            var cmbCarrier = carrier.ToCmbCarrier();

            cmbCarrier.CmbParentSpanId = _entrySegmentContextAccessor.Context?.SegmentId ?? "0";
            cmbCarrier.CmbSpanId = HashHelpers.GetHashString(cmbCarrier.CmbParentSpanId + 1);
            cmbCarrier.CmbTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            cmbCarrier.BusinessId = cmbSegmentContext.BusinessId;
            cmbCarrier.CmbSampled = cmbSegmentContext.CmbSampled;
            cmbCarrier.CmbDebug = cmbSegmentContext.CmbDebug;

            foreach (var p in cmbCarrier.GetType().GetProperties().Where(cp => cp.GetCustomAttributes(typeof(DescriptionAttribute), true).Any()))
            {
                var key = ((DescriptionAttribute)p.GetCustomAttributes(typeof(DescriptionAttribute), true).First()).Description;
                var value = p.GetValue(cmbCarrier);

                headerCollection.Add(key, value?.ToString());
            }
        }

        public ICarrier Extract(ICarrierHeaderCollection headerCollection)
        {
            var carrier = OriginExtract(headerCollection);
            var cmbCarrier = carrier.ToCmbCarrier();

            if (headerCollection != null && headerCollection.Any(h => h.Key.StartsWith("X-B3-")))
            {
                if (!cmbCarrier.HasValue)
                    cmbCarrier = new CmbCarrier() { Sampled = true };

                foreach (var p in cmbCarrier.GetType().GetProperties().Where(cp => cp.GetCustomAttributes(typeof(DescriptionAttribute), true).Any()))
                {
                    var key = ((DescriptionAttribute)p.GetCustomAttributes(typeof(DescriptionAttribute), true).First()).Description;

                    foreach (var header in headerCollection)
                    {
                        if (!header.Key.Equals(key))
                            continue;

                        p.SetValue(cmbCarrier, header.Value);
                    }
                }

                if (string.IsNullOrEmpty(cmbCarrier.CmbParentSpanId))
                    cmbCarrier.CmbParentSpanId = "0";
                if (string.IsNullOrEmpty(cmbCarrier.CmbSpanId))
                    cmbCarrier.CmbSpanId = HashHelpers.GetHashString(carrier.ParentSegmentId + 1);
                if (string.IsNullOrEmpty(cmbCarrier.CmbSampled))
                    cmbCarrier.CmbSampled = "1";
                if (string.IsNullOrEmpty(cmbCarrier.CmbDebug))
                    cmbCarrier.CmbDebug = "0";
            }

            return cmbCarrier;
        }

        private ICarrier OriginInject(SegmentContext segmentContext, ICarrierHeaderCollection headerCollection)
        {
            var reference = segmentContext.References.FirstOrDefault();

            var carrier = new Carrier(segmentContext.TraceId, segmentContext.SegmentId, segmentContext.Span.SpanId,
                segmentContext.ServiceInstanceId, reference?.EntryServiceInstanceId ?? segmentContext.ServiceInstanceId,
                segmentContext.ServiceId)
            {
                NetworkAddress = segmentContext.Span.Peer,
                EntryEndpoint = reference?.EntryEndpoint ?? segmentContext.Span.OperationName,
                ParentEndpoint = segmentContext.Span.OperationName,
                Sampled = segmentContext.Sampled
            };

            foreach (var formatter in _carrierFormatters)
            {
                if (formatter.Enable)
                    headerCollection.Add(formatter.Key, formatter.Encode(carrier));
            }

            return carrier;
        }

        private ICarrier OriginExtract(ICarrierHeaderCollection headerCollection)
        {
            ICarrier carrier = NullableCarrier.Instance;
            if (headerCollection == null)
            {
                return carrier;
            }
            foreach (var formatter in _carrierFormatters.OrderByDescending(x => x.Key))
            {
                if (!formatter.Enable)
                {
                    continue;
                }

                foreach (var header in headerCollection)
                {
                    if (formatter.Key == header.Key)
                    {
                        carrier = formatter.Decode(header.Value);
                        if (carrier.HasValue)
                        {
                            if (formatter.Key.EndsWith("sw3") && carrier is Carrier c)
                            {
                                c.Sampled = true;
                            }

                            return carrier;
                        }
                    }
                }
            }

            return carrier;
        }
    }
}
