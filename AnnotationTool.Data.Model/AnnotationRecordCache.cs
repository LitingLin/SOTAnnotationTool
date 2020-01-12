using System;
using System.Collections.Generic;
using System.Text;

namespace AnnotationTool.Data.Model
{
    public class AnnotationRecordCache
    {
        public bool IsLabeled;
        public int X;
        public int Y;
        public int W;
        public int H;
        public bool IsFullyOccluded;
        public bool IsOutOfView;

        public AnnotationRecordCache(bool isLabeled, int x, int y, int w, int h, bool isFullyOccluded, bool isOutOfView)
        {
            IsLabeled = isLabeled;

            X = x;
            Y = y;
            W = w;
            H = h;

            IsFullyOccluded = isFullyOccluded;
            IsOutOfView = isOutOfView;
        }

        public AnnotationRecordCache Clone()
        {
            return (AnnotationRecordCache) MemberwiseClone();
        }
    }
}
