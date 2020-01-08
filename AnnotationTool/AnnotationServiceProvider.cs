using AnnotationTool;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnotationTool
{
    class AnnotationServiceProvider : IDisposable
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

        public void Dispose()
        {
            _annotationDbModel.Dispose();
        }

        private readonly IList<AnnotationRecord> _annotationRecords;
        private readonly string _name;
        private readonly string _sequencePath;
        private readonly AnnotationRecordCachePersistentProvider _annotationDbModel;
        private bool _matlabIndexing;

        public void SetMatlabIndexing(bool value)
        {
            if (_matlabIndexing != value)
            {
                _matlabIndexing = value;
                GenerateMatlabFile(_annotationRecords);
            }
        }

        public AnnotationServiceProvider(string sequencePath, bool matlabIndexing)
        {
            _matlabIndexing = matlabIndexing;
            _sequencePath = sequencePath;
            _name = Path.GetFileName(sequencePath);
            if (_name != null && _name.Length == 0)
                throw new ArgumentException("无效的序列路径");

            var imageFileNames = GetImageFileNames();

            string txtFilePath = Path.Combine(sequencePath, "res.txt");
            List<int[]> initialRecords = null;
            if (File.Exists(txtFilePath))
            {
                initialRecords = LoadInitialRecordFromTxt(txtFilePath);
                if (initialRecords.Count >= imageFileNames.Count)
                    throw new ApplicationException($"序列损坏");
            }

            IList<AnnotationRecord> records = null;

            string matFilePath = Path.Combine(sequencePath, "res.mat");
            if (File.Exists(matFilePath))
            {
                records = Load(Path.Combine(sequencePath, "res.mat"), imageFileNames, out bool requireUpdate);

                if (initialRecords != null)
                {
                    for (int i = 0; i < initialRecords.Count; ++i)
                    {
                        int X = initialRecords[i][0];
                        int Y = initialRecords[i][1];
                        int W = initialRecords[i][2];
                        int H = initialRecords[i][3];

                        if (records[i].X != X)
                        {
                            requireUpdate = true;
                            records[i].X = X;
                        }
                        if (records[i].Y != Y)
                        {
                            requireUpdate = true;
                            records[i].Y = Y;
                        }
                        if (records[i].W != W)
                        {
                            requireUpdate = true;
                            records[i].W = W;
                        }
                        if (records[i].H != H)
                        {
                            requireUpdate = true;
                            records[i].H = H;
                        }
                    }
                }
                if (requireUpdate)
                {
                    GenerateMatlabFile(records);
                }
            }
            else
            {
                if (initialRecords == null)
                    throw new ApplicationException("不存在 res.mat 或 res.txt");

                records = new List<AnnotationRecord>();

                for (int i = 0; i < initialRecords.Count; ++i)
                {
                    int X = initialRecords[i][0];
                    int Y = initialRecords[i][1];
                    int W = initialRecords[i][2];
                    int H = initialRecords[i][3];

                    records.Add(new AnnotationRecord
                    {
                        X = X,
                        Y = Y,
                        W = W,
                        H = H,
                        IsLabeled = true,
                        IsFullyOccluded = false,
                        IsOutOfView = false,
                        Path = imageFileNames[i]
                    });
                }

                for (int i = initialRecords.Count; i < imageFileNames.Count; ++i)
                {
                    records.Add(new AnnotationRecord
                    {
                        X = 0,
                        Y = 0,
                        W = 0,
                        H = 0,
                        IsLabeled = false,
                        IsFullyOccluded = false,
                        IsOutOfView = false,
                        Path = imageFileNames[i]
                    });
                }

                GenerateMatlabFile(records);
            }

            _annotationDbModel = new AnnotationRecordCachePersistentProvider(Path.Combine(_sequencePath, "cache.db"));
            InitializeCache(records);

            _annotationRecords = records;
        }

        private List<int[]> LoadInitialRecordFromTxt(string txtFilePath)
        {
            List<int[]> boundingBoxes = new List<int[]>();
            using (StreamReader sr = new StreamReader(txtFilePath))
            {
                while (true)
                {
                    string line = sr.ReadLine();
                    if (line == null)
                        break;

                    string[] words = line.Split(',');
                    if (words.Length != 4)
                        throw new ApplicationException($"{txtFilePath} 无效");
                    int[] boundingBox = new int[4];
                    for (int i = 0; i < 4; ++i)
                    {
                        boundingBox[i] = checked((int)UInt32.Parse(words[i]));
                    }

                    if (_matlabIndexing)
                    {
                        if (boundingBox[0] > 0) boundingBox[0] -= 1;
                        if (boundingBox[1] > 0) boundingBox[1] -= 1;
                    }

                    boundingBoxes.Add(boundingBox);
                }
            }

            return boundingBoxes;
        }

        private void GenerateMatlabFile(IList<AnnotationRecord> records)
        {
            using (AnnotationRecordOperator annotationRecordOperator
                = new AnnotationRecordOperator(Path.Combine(_sequencePath, "res.mat"), DesiredAccess.Write, CreationDisposition.CreateAlways))
            {
                int count = records.Count;
                annotationRecordOperator.Resize(Convert.ToUInt64(count));

                for (int index = 0; index < count; ++index)
                {
                    var record = records[index];

                    if (_matlabIndexing)
                        annotationRecordOperator.Update(Convert.ToUInt32(index), index + 1, record.IsLabeled, record.X + 1, record.Y + 1, record.W, record.H, record.IsFullyOccluded, record.IsOutOfView, record.Path);
                    else
                    {
                        annotationRecordOperator.Update(Convert.ToUInt32(index), index + 1, record.IsLabeled, record.X, record.Y, record.W, record.H, record.IsFullyOccluded, record.IsOutOfView, record.Path);
                    }
                }
            }
        }

        private void InitializeCache(IList<AnnotationRecord> records)
        {
            if (_annotationDbModel.GetSize() != records.Count)
            {
                _annotationDbModel.Resize(0);
                for (int i = 0; i < records.Count; ++i)
                {
                    var annotation = records[i];
                    _annotationDbModel.Push(annotation);
                }
            }
            else
            {
                for (int i = 1; i < records.Count; ++i)
                {
                    records[i]=_annotationDbModel.Get(i);
                }
            }
        }

        private IList<string> GetImageFileNames()
        {
            var directoryInfo = new DirectoryInfo(_sequencePath);
            var imageFileInfos = directoryInfo.EnumerateFiles("*.jpg", SearchOption.TopDirectoryOnly).ToArray();

            if (!imageFileInfos.Any())
                throw new ApplicationException("文件夹下没有有效的图片文件");

            IList<string> imageFiles = new List<string>(imageFileInfos.Count());

            int count = 1;

            foreach (var imageFileInfo in imageFileInfos)
            {
                string expectedFileName = $"{count:D8}.jpg";

                if (imageFileInfo.Name != expectedFileName)
                    throw new ApplicationException($"缺少图片 {expectedFileName}");

                imageFiles.Add(expectedFileName);

                count++;
            }

            return imageFiles;
        }

        private IList<AnnotationRecord> Load(string matPath, IList<string> imageFiles, out bool requireUpdate)
        {
            IList<AnnotationRecord> records = new List<AnnotationRecord>();
            requireUpdate = false;

            using (AnnotationRecordOperator annotationRecordOperator
                = new AnnotationRecordOperator(matPath, DesiredAccess.Read, CreationDisposition.OpenAlways))
            {
                var numberOfRecords = annotationRecordOperator.GetNumberOfRecords();
                ulong index = 0;
                bool abandonAnnotationRecord = false;
                foreach (var imageFile in imageFiles)
                {
                    int id = Convert.ToInt32(index + 1), x = 0, y = 0, w = 0, h = 0;
                    bool isLabeled = false, isOccluded = false, isOutOfView = false;
                    string path = "";
                    if (index == numberOfRecords) abandonAnnotationRecord = true;
                    if (!abandonAnnotationRecord)
                    {
                        if (!annotationRecordOperator.Get(index, out id, out isLabeled, out x, out y,
                            out w, out h, out isOccluded, out isOutOfView, out path))
                        {
                            if (index == 0)
                                throw new ApplicationException("res.mat 无效");
                            else
                            {
                                abandonAnnotationRecord = true;
                            }
                        }
                    }

                    if (path != imageFile)
                    {
                        path = imageFile;
                        requireUpdate = true;
                    }

                    if (_matlabIndexing)
                    {
                        x -= 1;
                        y -= 1;
                        if (x < 0) x = 0;
                        if (y < 0) y = 0;
                    }

                    records.Add(new AnnotationRecord()
                    {
                        X = x,
                        Y = y,
                        W = w,
                        H = h,
                        IsLabeled = isLabeled,
                        IsFullyOccluded = isOccluded,
                        IsOutOfView = isOutOfView,
                        Path = path
                    });

                    index++;
                }

                if (abandonAnnotationRecord)
                    requireUpdate = true;

                if (imageFiles.Count != Convert.ToInt32(numberOfRecords))
                    requireUpdate = true;
            }

            bool needFix = false;

            foreach (var annotationRecord in records)
            {
                if (annotationRecord.X < 0 || annotationRecord.Y < 0)
                {
                    needFix = true;
                    break;
                }
            }

            if (needFix)
            {
                int ind = 0;
                foreach (var annotationRecord in records)
                {
                    if (ind == 0)
                    {
                        ++ind;
                        continue;
                    }

                    if (annotationRecord.X < -1)
                    {
                        annotationRecord.X = 0;
                    }
                    else
                    {
                        annotationRecord.X++;
                    }

                    if (annotationRecord.Y < -1)
                    {
                        annotationRecord.Y = 0;
                    }
                    else
                    {
                        annotationRecord.Y++;
                    }

                    ++ind;
                }
            }

            if (needFix)
                requireUpdate = true;

            return records;
        }

        public void Import(string matPath)
        {
            var imageFileNames = GetImageFileNames();

            var records = Load(matPath, imageFileNames, out bool requireUpdate);

            _pendingUpdates.Clear();

            for (int index = 1; index < records.Count; index++)
            {
                _annotationRecords[index] = records[index];
            }

            GenerateMatlabFile(_annotationRecords);
            InitializeCache(_annotationRecords);
        }

        public void Update(int index, AnnotationRecord record)
        {
            _annotationRecords[index].Assign(record);
        }

        private readonly ISet<int> _pendingUpdates = new HashSet<int>();

        public void Submit(int index)
        {
            var annotation = _annotationRecords[index];

            _pendingUpdates.Add(index);

            _annotationDbModel.Set(index, annotation.IsLabeled, annotation.X, annotation.Y, annotation.W, annotation.H,
                annotation.IsFullyOccluded, annotation.IsOutOfView, annotation.Path);
        }

        public void UpdateMatlabFile()
        {
            if (_pendingUpdates.Count == 0)
                return;

            using (AnnotationRecordOperator annotationRecordOperator =
                new AnnotationRecordOperator(Path.Combine(_sequencePath, "res.mat"), DesiredAccess.Write,
                    CreationDisposition.OpenAlways))
            {
                foreach (var index in _pendingUpdates)
                {
                    var record = _annotationRecords[index];

                    if (_matlabIndexing)
                        annotationRecordOperator.Update(Convert.ToUInt32(index), index + 1, record.IsLabeled, record.X + 1, record.Y + 1, record.W, record.H, record.IsFullyOccluded, record.IsOutOfView, record.Path);
                    else
                    {
                        annotationRecordOperator.Update(Convert.ToUInt32(index), index + 1, record.IsLabeled, record.X, record.Y, record.W, record.H, record.IsFullyOccluded, record.IsOutOfView, record.Path);
                    }
                }
            }

            _pendingUpdates.Clear();
        }

        public int GetNumberOfRecords()
        {
            return _annotationRecords.Count;
        }

        public AnnotationRecord Get(int index)
        {
            return _annotationRecords[index].Clone();
        }

        public string GetName()
        {
            return _name;
        }

        public string GetImagePath(int index)
        {
            return Path.Combine(_sequencePath, _annotationRecords[index].Path);
        }
    }
}
