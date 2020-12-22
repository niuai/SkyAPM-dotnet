﻿using Cmb.SkyApm.Common;
using SkyApm;
using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using System;
using System.Linq;

namespace Cmb.SkyApm.Tracing
{
    public class CmbSegmentContextFactory : ISegmentContextFactory
    {
        private readonly IEntrySegmentContextAccessor _entrySegmentContextAccessor;
        private readonly ILocalSegmentContextAccessor _localSegmentContextAccessor;
        private readonly IExitSegmentContextAccessor _exitSegmentContextAccessor;
        private readonly IRuntimeEnvironment _runtimeEnvironment;
        private readonly ISamplerChainBuilder _samplerChainBuilder;
        private readonly IUniqueIdGenerator _uniqueIdGenerator;
        private readonly InstrumentConfig _instrumentConfig;

        public CmbSegmentContextFactory(IRuntimeEnvironment runtimeEnvironment,
            ISamplerChainBuilder samplerChainBuilder,
            IUniqueIdGenerator uniqueIdGenerator,
            IEntrySegmentContextAccessor entrySegmentContextAccessor,
            ILocalSegmentContextAccessor localSegmentContextAccessor,
            IExitSegmentContextAccessor exitSegmentContextAccessor,
            IConfigAccessor configAccessor)
        {
            _runtimeEnvironment = runtimeEnvironment;
            _samplerChainBuilder = samplerChainBuilder;
            _uniqueIdGenerator = uniqueIdGenerator;
            _entrySegmentContextAccessor = entrySegmentContextAccessor;
            _localSegmentContextAccessor = localSegmentContextAccessor;
            _exitSegmentContextAccessor = exitSegmentContextAccessor;
            _instrumentConfig = configAccessor.Get<InstrumentConfig>();
        }

        public SegmentContext CreateEntrySegment(string operationName, ICarrier carrier)
        {
            var traceId = GetTraceId(carrier);
            var segmentId = GetSegmentId(carrier);
            var sampled = GetSampled(carrier, operationName);
            var segmentContext = new SegmentContext(traceId, segmentId, sampled,
                _instrumentConfig.ServiceName ?? _instrumentConfig.ApplicationCode,
                _instrumentConfig.ServiceInstanceName,
                operationName, SpanType.Entry);

            if (carrier.HasValue)
            {
                var segmentReference = new SegmentReference
                {
                    Reference = Reference.CrossProcess,
                    EntryEndpoint = carrier.EntryEndpoint,
                    NetworkAddress = carrier.NetworkAddress,
                    ParentEndpoint = carrier.ParentEndpoint,
                    ParentSpanId = carrier.ParentSpanId,
                    ParentSegmentId = carrier.ParentSegmentId,
                    EntryServiceInstanceId = carrier.EntryServiceInstanceId,
                    ParentServiceInstanceId = carrier.ParentServiceInstanceId,
                    TraceId = carrier.TraceId,
                    ParentServiceId = carrier.ParentServiceId,
                };
                segmentContext.References.Add(segmentReference);
            }

            _entrySegmentContextAccessor.Context = segmentContext;
            return segmentContext;
        }

        public SegmentContext CreateLocalSegment(string operationName)
        {
            var parentSegmentContext = GetParentSegmentContext(SpanType.Local);
            var traceId = GetTraceId(parentSegmentContext);
            var segmentId = GetSegmentId();
            var sampled = GetSampled(parentSegmentContext, operationName);
            var segmentContext = new SegmentContext(traceId, segmentId, sampled,
                _instrumentConfig.ServiceName ?? _instrumentConfig.ApplicationCode,
                _instrumentConfig.ServiceInstanceName,
                operationName, SpanType.Local);

            if (parentSegmentContext != null)
            {
                var parentReference = parentSegmentContext.References.FirstOrDefault();
                var reference = new SegmentReference
                {
                    Reference = Reference.CrossThread,
                    EntryEndpoint = parentReference?.EntryEndpoint ?? parentSegmentContext.Span.OperationName,
                    NetworkAddress = parentReference?.NetworkAddress ?? parentSegmentContext.Span.OperationName,
                    ParentEndpoint = parentSegmentContext.Span.OperationName,
                    ParentSpanId = parentSegmentContext.Span.SpanId,
                    ParentSegmentId = parentSegmentContext.SegmentId,
                    EntryServiceInstanceId =
                        parentReference?.EntryServiceInstanceId ?? parentSegmentContext.ServiceInstanceId,
                    ParentServiceInstanceId = parentSegmentContext.ServiceInstanceId,
                    ParentServiceId = parentSegmentContext.ServiceId,
                    TraceId = parentSegmentContext.TraceId
                };
                segmentContext.References.Add(reference);
            }

            _localSegmentContextAccessor.Context = segmentContext;
            return segmentContext;
        }

        public SegmentContext CreateExitSegment(string operationName, StringOrIntValue networkAddress)
        {
            var parentSegmentContext = GetParentSegmentContext(SpanType.Exit);
            var traceId = GetTraceId(parentSegmentContext);
            var segmentId = GetSegmentId(parentSegmentContext);
            var sampled = GetSampled(parentSegmentContext, operationName, networkAddress);
            var segmentContext = new SegmentContext(traceId, segmentId, sampled,
                _instrumentConfig.ServiceName ?? _instrumentConfig.ApplicationCode,
                _instrumentConfig.ServiceInstanceName,
                operationName, SpanType.Exit);

            if (parentSegmentContext != null)
            {
                var parentReference = parentSegmentContext.References.FirstOrDefault();
                var reference = new SegmentReference
                {
                    Reference = Reference.CrossThread,
                    EntryEndpoint = parentReference?.EntryEndpoint ?? parentSegmentContext.Span.OperationName,
                    NetworkAddress = parentReference?.NetworkAddress ?? parentSegmentContext.Span.OperationName,
                    ParentEndpoint = parentSegmentContext.Span.OperationName,
                    ParentSpanId = parentSegmentContext.Span.SpanId,
                    ParentSegmentId = parentSegmentContext.SegmentId,
                    EntryServiceInstanceId =
                        parentReference?.EntryServiceInstanceId ?? parentSegmentContext.ServiceInstanceId,
                    ParentServiceInstanceId = parentSegmentContext.ServiceInstanceId,
                    ParentServiceId = parentSegmentContext.ServiceId,
                    TraceId = parentSegmentContext.TraceId
                };
                segmentContext.References.Add(reference);
            }

            segmentContext.Span.Peer = networkAddress;
            _exitSegmentContextAccessor.Context = segmentContext;
            return segmentContext;
        }

        public void Release(SegmentContext segmentContext)
        {
            segmentContext.Span.Finish();
            switch (segmentContext.Span.SpanType)
            {
                case SpanType.Entry:
                    _entrySegmentContextAccessor.Context = null;
                    break;
                case SpanType.Local:
                    _localSegmentContextAccessor.Context = null;
                    break;
                case SpanType.Exit:
                    _exitSegmentContextAccessor.Context = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(SpanType), segmentContext.Span.SpanType, "Invalid SpanType.");
            }
        }

        private string GetTraceId(ICarrier carrier)
        {
            return carrier.HasValue ? carrier.TraceId : _uniqueIdGenerator.Generate();
        }

        private string GetTraceId(SegmentContext parentSegmentContext)
        {
            return parentSegmentContext?.TraceId ?? _uniqueIdGenerator.Generate();
        }

        private string GetSegmentId()
        {
            return HashHelpers.GetHashString(_uniqueIdGenerator.Generate());
        }

        private string GetSegmentId(ICarrier carrier)
        {
            string segmentId;

            if (carrier.HasValue && carrier is CmbCarrier c)
                segmentId = c.CmbSpanId;
            else
                segmentId = GetSegmentId();

            return segmentId;
        }

        private string GetSegmentId(SegmentContext parentSegmentContext)
        {
            return parentSegmentContext?.SegmentId;
        }

        private bool GetSampled(ICarrier carrier, string operationName)
        {
            if (carrier.HasValue && carrier.Sampled.HasValue)
            {
                return carrier.Sampled.Value;
            }

            SamplingContext samplingContext;
            if (carrier.HasValue)
            {
                samplingContext = new SamplingContext(operationName, carrier.NetworkAddress, carrier.EntryEndpoint,
                    carrier.ParentEndpoint);
            }
            else
            {
                samplingContext = new SamplingContext(operationName, default(StringOrIntValue), default(StringOrIntValue),
                    default(StringOrIntValue));
            }

            var sampler = _samplerChainBuilder.Build();
            return sampler(samplingContext);
        }

        private bool GetSampled(SegmentContext parentSegmentContext, string operationName,
            StringOrIntValue peer = default(StringOrIntValue))
        {
            if (parentSegmentContext != null) return parentSegmentContext.Sampled;
            var sampledContext = new SamplingContext(operationName, peer, new StringOrIntValue(operationName),
                default(StringOrIntValue));
            var sampler = _samplerChainBuilder.Build();
            return sampler(sampledContext);
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
