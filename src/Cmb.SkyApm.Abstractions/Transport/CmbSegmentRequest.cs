using SkyApm.Transport;

namespace Cmb.SkyApm.Transport
{
    public class CmbSegmentRequest : SegmentRequest
    {
        public CmbSegmentObjectRequest CmbSegment { get; set; }
    }

    public class CmbSegmentObjectRequest : SegmentObjectRequest
    {
        public string BusinessId { get; set; }

        public string CmbParentSpanId { get; set; }

        public string CmbSpanId { get; set; }

        public string CmbTimeStamp { get; set; }

        public string CmbSampled { get; set; }

        public string CmbDebug { get; set; }
    }
}
