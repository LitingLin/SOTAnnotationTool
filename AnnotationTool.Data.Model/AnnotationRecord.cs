using System;

namespace AnnotationTool.Data.Model
{
    [Serializable]
    public class AnnotationRecord
    {
        public bool IsLabeled;
        public int X;
        public int Y;
        public int W;
        public int H;
        public bool IsFullyOccluded;
        public bool IsOutOfView;
        public string Path;

        public AnnotationRecord Clone()
        {
            return (AnnotationRecord)MemberwiseClone();
        }

        public AnnotationRecord(bool isLabeled, int x, int y, int w, int h, bool isFullyOccluded, bool isOutOfView, string path)
        {
            IsLabeled = isLabeled;

            X = x;
            Y = y;
            W = w;
            H = h;

            IsFullyOccluded = isFullyOccluded;
            IsOutOfView = isOutOfView;

            Path = path;
        }
    }

}
