using System;

namespace AnnotationTool.Data.Model
{
    public class AnnotationRecord
    {
        public bool IsLabeled = false;
        public int X = 0;
        public int Y = 0;
        public int W = 0;
        public int H = 0;
        public bool IsFullyOccluded = false;
        public bool IsOutOfView = false;
        public string Path = "";

        public AnnotationRecord Clone()
        {
            return new AnnotationRecord { X = X, Y = Y, W = W, H = H, IsOutOfView = IsOutOfView, IsLabeled = IsLabeled, IsFullyOccluded = IsFullyOccluded, Path = Path };
        }

        public void Assign(AnnotationRecord annotationRecord)
        {
            X = annotationRecord.X;
            Y = annotationRecord.Y;
            W = annotationRecord.W;
            H = annotationRecord.H;

            IsOutOfView = annotationRecord.IsOutOfView;
            IsLabeled = annotationRecord.IsLabeled;
            IsFullyOccluded = annotationRecord.IsFullyOccluded;

            Path = annotationRecord.Path;
        }
    }

}
