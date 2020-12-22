using Cmb.SkyApm.Common;
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
        private readonly ISegmentContextFactory _segmentContextFactory;
        private readonly IUniqueIdGenerator _uniqueIdGenerator;
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        private readonly ILocalSegmentContextAccessor _localSegmentContextAccessor;
        private readonly IExitSegmentContextAccessor _exitSegmentContextAccessor;

        public CmbCarrierPropagator(IEnumerable<ICarrierFormatter> carrierFormatters,
            ISegmentContextFactory segmentContextFactory,
            IUniqueIdGenerator uniqueIdGenerator,
            IEntrySegmentContextAccessor entrySegmentContextAccessor,
            ILocalSegmentContextAccessor localSegmentContextAccessor,
            IExitSegmentContextAccessor exitSegmentContextAccessor)
        {
            _carrierFormatters = carrierFormatters;
            _segmentContextFactory = segmentContextFactory;
            _uniqueIdGenerator = uniqueIdGenerator;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
            _localSegmentContextAccessor = localSegmentContextAccessor;
            _exitSegmentContextAccessor = exitSegmentContextAccessor;
        }

        public void Inject(SegmentContext segmentContext, ICarrierHeaderCollection headerCollection)
        {
            var carrier = OriginInject(segmentContext, headerCollection);
            var cmbCarrier = carrier.ToCmbCarrier();

            cmbCarrier.CmbParentSpanId = GetParentSegmentContext(SpanType.Exit)?.SegmentId ?? "0";
            cmbCarrier.CmbSpanId = HashHelpers.GetHashString(carrier.ParentSegmentId + 1);

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

                cmbCarrier.CmbParentSpanId = !string.IsNullOrEmpty(cmbCarrier.CmbParentSpanId) ? cmbCarrier.CmbParentSpanId : "0";
                cmbCarrier.CmbSpanId = !string.IsNullOrEmpty(cmbCarrier.CmbSpanId) ? cmbCarrier.CmbSpanId : HashHelpers.GetHashString(carrier.ParentSegmentId + 1);
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

        private SegmentContext GetParentSegmentContext(SpanType spanType)
        {
            switch (spanType)
            {
                case SpanType.Entry:
                    return null;
                case SpanType.Local:
                    return _entrySegmentContextAccessor.Context;
                case SpanType.Exit:
                    return _localSegmentContextAccessor.Context ?? _entrySegmentContextAccessor.Context;
                default:
                    throw new ArgumentOutOfRangeException(nameof(spanType), spanType, "Invalid SpanType.");
            }
        }
    }
}
