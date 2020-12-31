using Cmb.SkyApm.Tracing.Segments;
using SkyApm.Tracing.Segments;
using SkyApm.Transport;
using System.Collections.Generic;

namespace Cmb.SkyApm.Transport
{
    public class CmbSegmentContextMapper : ISegmentContextMapper
    {
        public SegmentRequest Map(SegmentContext segmentContext)
        {
            var cmbSegmentContext = (CmbSegmentContext)segmentContext;
            var cmbSegmentRequest = OriginMap(cmbSegmentContext);

            cmbSegmentRequest.CmbSegment = new CmbSegmentObjectRequest
            {
                SegmentId = segmentContext.SegmentId,
                ServiceId = segmentContext.ServiceId,
                ServiceInstanceId = segmentContext.ServiceInstanceId,
                BusinessId = cmbSegmentContext.BusinessId,
                CmbParentSpanId = cmbSegmentContext.CmbParentSpanId,
                CmbSpanId = cmbSegmentContext.CmbSpanId,
                CmbTimeStamp = cmbSegmentContext.CmbTimeStamp,
                CmbSampled = cmbSegmentContext.CmbSampled,
                CmbDebug = cmbSegmentContext.CmbDebug,

                Spans = cmbSegmentRequest.Segment.Spans
            };

            return cmbSegmentRequest;
        }

        private CmbSegmentRequest OriginMap(CmbSegmentContext segmentContext)
        {
            var segmentRequest = new CmbSegmentRequest
            {
                TraceId = segmentContext.TraceId
            };
            var segmentObjectRequest = new SegmentObjectRequest
            {
                SegmentId = segmentContext.SegmentId,
                ServiceId = segmentContext.ServiceId,
                ServiceInstanceId = segmentContext.ServiceInstanceId
            };
            segmentRequest.Segment = segmentObjectRequest;
            var span = new SpanRequest
            {
                SpanId = segmentContext.Span.SpanId,
                ParentSpanId = segmentContext.Span.ParentSpanId,
                OperationName = segmentContext.Span.OperationName,
                StartTime = segmentContext.Span.StartTime,
                EndTime = segmentContext.Span.EndTime,
                SpanType = (int)segmentContext.Span.SpanType,
                SpanLayer = (int)segmentContext.Span.SpanLayer,
                IsError = segmentContext.Span.IsError,
                Peer = segmentContext.Span.Peer,
                Component = segmentContext.Span.Component
            };
            foreach (var reference in segmentContext.References)
                span.References.Add(new SegmentReferenceRequest
                {
                    TraceId = reference.TraceId,
                    ParentSegmentId = reference.ParentSegmentId,
                    ParentServiceId = reference.ParentServiceId,
                    ParentServiceInstanceId = reference.ParentServiceInstanceId,
                    ParentSpanId = reference.ParentSpanId,
                    ParentEndpointName = reference.ParentEndpoint,
                    EntryServiceInstanceId = reference.EntryServiceInstanceId,
                    EntryEndpointName = reference.EntryEndpoint,
                    NetworkAddress = reference.NetworkAddress,
                    RefType = (int)reference.Reference
                });

            foreach (var tag in segmentContext.Span.Tags)
                span.Tags.Add(new KeyValuePair<string, string>(tag.Key, tag.Value));

            foreach (var log in segmentContext.Span.Logs)
            {
                var logData = new LogDataRequest { Timestamp = log.Timestamp };
                foreach (var data in log.Data)
                    logData.Data.Add(new KeyValuePair<string, string>(data.Key, data.Value));
                span.Logs.Add(logData);
            }

            segmentObjectRequest.Spans.Add(span);
            return segmentRequest;
        }
    }
}
