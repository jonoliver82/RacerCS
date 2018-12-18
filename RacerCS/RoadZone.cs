using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacerCS
{
    public class RoadZone
    {
        private int _segmentCount;
        private List<RoadSegment> _segments;

        public RoadZone(int segments)
        {
            _segmentCount = segments;
            _segments = new List<RoadSegment>(segments);
        }

        public double StartHeight { get; private set; }
        public double EndHeight { get; private set; }
        public HeightState HeightTransition { get; private set; }
        public double HeightStep { get; private set; }

        public double StartCurve { get; private set; }
        public double EndCurve { get; private set; }
        public CurveState CurveTransition { get; private set; }
        public double CurveStep { get; private set; }

        public List<RoadSegment> Segments => _segments;

        public void AddCurve(CurveState transition, double start, double end)
        {
            CurveTransition = transition;
            StartCurve = start;
            EndCurve = end;
            CurveStep = (EndCurve - StartCurve) / _segmentCount;
        }

        public void AddHeight(HeightState transition, double start, double end)
        {
            HeightTransition = transition;
            StartHeight = start;
            EndHeight = end;
            HeightStep = (EndHeight - StartHeight) / _segmentCount;
        }

        public void AddSegment(RoadSegment segment)
        {
            _segments.Add(segment);
        }
    }
}
