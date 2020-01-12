using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnnotationTool.Data.Model;
using AnnotationTool.NativeInteropServices;
using AnnotationTool.Resources;

namespace AnnotationTool.Data
{
    class AnnotationRecordDataAccessor : IDisposable
    {
        public void Dispose()
        {
            _cachePersistentProvider.Dispose();
        }

        private readonly IList<AnnotationRecord> _annotationRecords;
        private readonly string _name;
        private readonly string _sequencePath;
        private readonly AnnotationRecordCachePersistentProvider _cachePersistentProvider;

        private static List<int[]> LoadInitialRecordsFromTxt(string txtFilePath)
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
                        throw new Exception($"{txtFilePath} 无效");
                    int[] boundingBox = new int[4];
                    for (int i = 0; i < 4; ++i)
                    {
                        boundingBox[i] = checked((int)UInt32.Parse(words[i]));
                    }

                    if (boundingBox[0] > 0) boundingBox[0] -= 1;
                    if (boundingBox[1] > 0) boundingBox[1] -= 1;

                    boundingBoxes.Add(boundingBox);
                }
            }

            return boundingBoxes;
        }

        private static IList<string> GetImageFileList(string sequencePath)
        {
            var directoryInfo = new DirectoryInfo(sequencePath);
            var imageFileInfos = directoryInfo.EnumerateFiles("*.jpg", SearchOption.TopDirectoryOnly).ToArray();

            if (!imageFileInfos.Any())
                throw new Exception(MultiLanguageResources.NoValidImageFilesInDirectory);

            IList<string> imageFiles = new List<string>(imageFileInfos.Count());

            int count = 1;

            foreach (var imageFileInfo in imageFileInfos)
            {
                string expectedFileName = $"{count:D8}.jpg";

                if (imageFileInfo.Name != expectedFileName)
                    throw new Exception(string.Format(MultiLanguageResources.ImageFileMissing, expectedFileName));

                imageFiles.Add(expectedFileName);

                count++;
            }

            return imageFiles;
        }

        private static (IList<AnnotationRecord>, IList<bool>) LoadRecordsFromMatFile(string matPath)
        {
            IList<AnnotationRecord> recordList = new List<AnnotationRecord>();
            IList<bool> recordValidityList = new List<bool>();

            try
            {
                using (AnnotationRecordOperator annotationRecordOperator
                    = new AnnotationRecordOperator(matPath, AnnotationRecordOperator.DesiredAccess.Read,
                        AnnotationRecordOperator.CreationDisposition.OpenAlways))
                {
                    var numberOfRecordsULong = annotationRecordOperator.Length();
                    if (numberOfRecordsULong >= int.MaxValue) throw new Exception("Too many records");
                    int numberOfRecords = Convert.ToInt32(numberOfRecordsULong);

                    for (int index = 0; index < numberOfRecords; ++index)
                    {
                        AnnotationRecord record = null;
                        bool isRecordValid;
                        try
                        {
                            (record, isRecordValid) = annotationRecordOperator.Get(Convert.ToUInt64(index));
                        }
                        catch (Exception)
                        {
                            isRecordValid = false;
                        }

                        if (record == null) record = new AnnotationRecord(false, 0, 0, 0, 0, false, false, null);

                        recordList.Add(record);
                        recordValidityList.Add(isRecordValid);
                    }
                }
            }
            catch (Exception)
            {
                return (recordList, recordValidityList);
            }

            return (recordList, recordValidityList);
        }

        private static (AnnotationRecordCachePersistentProvider, IList<AnnotationRecordCache>) InitializeAnnotationCache(string cachePath)
        {
            var cachePersistentProvider = new AnnotationRecordCachePersistentProvider(cachePath);
            return (cachePersistentProvider, cachePersistentProvider.AsList());
        }

        private static bool isAnnotationRecordIdentityToCache(AnnotationRecord left, AnnotationRecordCache right)
        {
            return !((left.IsLabeled != right.IsLabeled) || (left.X != right.X) || (left.Y != right.Y) ||
                           (left.W != right.W) || (left.H != right.H) || (left.IsFullyOccluded != right.IsFullyOccluded) ||
                           (left.IsOutOfView != right.IsOutOfView));
        }

        private static void apply

        private static (IList<AnnotationRecord>, IDictionary<int, AnnotationRecordCache>, bool)
            MergeMatFileRecordsAndCacheRecords(IList<AnnotationRecord> matFileRecords, IList<bool> matFileValidity,
                IList<AnnotationRecordCache> cacheRecords, IList<string> imageFileNameList)
        {
            bool matFileUpdateRequired = false;

            IList<AnnotationRecord> mergeResults = new List<AnnotationRecord>(imageFileNameList.Count);
            IDictionary<int, AnnotationRecordCache> cacheUpdateList = new Dictionary<int, AnnotationRecordCache>();

            int maxRecordsLength = imageFileNameList.Count;

            if (matFileRecords.Count != maxRecordsLength)
                matFileUpdateRequired = true;

            // Trust chain: cache > mat file

            int firstPhaseLength = Math.Min(Math.Min(maxRecordsLength, matFileRecords.Count), cacheRecords.Count);
            for (int index = 0; index < firstPhaseLength; ++index)
            {
                AnnotationRecord mergedRecord = matFileRecords[index].Clone();

                var cacheResult = cacheRecords[index];
                if (isAnnotationRecordIdentityToCache(mergedRecord, cacheResult))
                {

                }
            }
        }

        void init(string sequencePath)
        {
            string initialRecordPath = Path.Combine(sequencePath, "res.txt");
            string matFilePath = Path.Combine(sequencePath, "res.mat");
            string cacheFilePath = Path.Combine(sequencePath, "cache.db");

            IList<int[]> initialRecords = null;
            var imageFileList = GetImageFileList(sequencePath);
            if (File.Exists(initialRecordPath))
                initialRecords = LoadInitialRecordsFromTxt(initialRecordPath);

            
            var (matRecords, isMatRecordValid) = LoadRecordsFromMatFile(matFilePath);
            var (cachePersistentProvider, cacheRecords) = InitializeAnnotationCache(cacheFilePath);



            _cachePersistentProvider = cachePersistentProvider;
        }

        public AnnotationRecordDataAccessor(string sequencePath)
        {
            _sequencePath = sequencePath;
            _name = Path.GetFileName(sequencePath);
            if (_name != null && _name.Length == 0)
                throw new ArgumentException("无效的路径");

            var imageFileNames = GetImageFileNames();

            string txtFilePath = Path.Combine(sequencePath, "res.txt");
            List<int[]> initialRecords = null;
            if (File.Exists(txtFilePath))
            {
                initialRecords = LoadInitialRecordsFromTxt(txtFilePath);
                if (initialRecords.Count >= imageFileNames.Count)
                    throw new Exception("无效的路径");
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
                    throw new Exception("不存在 res.mat 或 res.txt");

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

            _cachePersistentProvider = new AnnotationRecordCachePersistentProvider(Path.Combine(_sequencePath, "cache.db"));
            InitializeCache(records);

            _annotationRecords = records;
        }


        private void GenerateMatlabFile(IList<AnnotationRecord> records)
        {
            using (AnnotationRecordOperator annotationRecordOperator
                = new AnnotationRecordOperator(Path.Combine(_sequencePath, "res.mat"), AnnotationRecordOperator.DesiredAccess.Write, AnnotationRecordOperator.CreationDisposition.CreateAlways))
            {
                int count = records.Count;
                annotationRecordOperator.Resize(Convert.ToUInt64(count));

                for (int index = 0; index < count; ++index)
                {
                    var record = records[index];

                    annotationRecordOperator.Set(Convert.ToUInt64(index), record);
                }
            }
        }

        private void InitializeCache(IList<AnnotationRecord> records)
        {
            if (_cachePersistentProvider.GetSize() != records.Count)
            {
                _cachePersistentProvider.Resize(0);
                for (int i = 0; i < records.Count; ++i)
                {
                    var annotation = records[i];
                    _cachePersistentProvider.Push(annotation);
                }
            }
            else
            {
                for (int i = 1; i < records.Count; ++i)
                {
                    records[i] = _cachePersistentProvider.Get(i);
                }
            }
        }

        private (IList<AnnotationRecord>, bool) LoadRecordsFromMatFile1(string matPath, IList<string> imageFiles)
        {
            IList<AnnotationRecord> records = new List<AnnotationRecord>();
            bool isRecordValid = true;

            using (AnnotationRecordOperator annotationRecordOperator
                = new AnnotationRecordOperator(matPath, AnnotationRecordOperator.DesiredAccess.Read, AnnotationRecordOperator.CreationDisposition.OpenAlways))
            {
                var numberOfRecordsULong = annotationRecordOperator.Length();
                if (numberOfRecordsULong >= int.MaxValue) throw new Exception("Too many records");
                int numberOfRecords = Convert.ToInt32(numberOfRecordsULong);
                if (numberOfRecords != imageFiles.Count) isRecordValid = false;

                int index = 0;
                foreach (var imageFile in imageFiles)
                {
                    AnnotationRecord record = null;
                    if (index < numberOfRecords)
                    {
                        try
                        {
                            record = annotationRecordOperator.Get(Convert.ToUInt64(index));
                        }
                        catch (Exception)
                        {
                            if (index == 0)
                                throw new Exception(string.Format(MultiLanguageResources.InvalidFile, "res.mat"));
                            else
                                isRecordValid = false;
                        }
                    }

                    if (record == null) record = new AnnotationRecord();

                    if (record.Path != imageFile)
                    {
                        record.Path = imageFile;
                        isRecordValid = false;
                    }

                    records.Add(record);

                    index++;
                }
            }

            return (records, isRecordValid);
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

            _cachePersistentProvider.Set(index, annotation.IsLabeled, annotation.X, annotation.Y, annotation.W, annotation.H,
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
