using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnnotationTool.Data.Model;
using AnnotationTool.NativeInteropServices;
using AnnotationTool.Resources;

namespace AnnotationTool.Data
{
    public class AnnotationRecordDataAccessor : IDisposable
    {
        public void Dispose()
        {
            _cachePersistentProvider.Dispose();
        }

        private readonly IList<AnnotationRecord> _annotationRecords;
        private readonly string _name;
        private readonly string _sequencePath;
        private readonly AnnotationRecordCachePersistentProvider _cachePersistentProvider;
        private readonly ISet<int> _pendingUpdates = new HashSet<int>();
        private bool _isFirstFrameAnnotationRecordLocked = true;


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
                        throw new Exception(string.Format(MultiLanguageResources.InvalidFile, txtFilePath));
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

        private static bool IsAnnotationRecordIdentityToCache(AnnotationRecord left, AnnotationRecordCache right)
        {
            return !((left.IsLabeled != right.IsLabeled) || (left.X != right.X) || (left.Y != right.Y) ||
                           (left.W != right.W) || (left.H != right.H) || (left.IsFullyOccluded != right.IsFullyOccluded) ||
                           (left.IsOutOfView != right.IsOutOfView));
        }

        private static AnnotationRecord ConvertAnnotationRecordCacheToRecord(AnnotationRecordCache cache)
        {
            return new AnnotationRecord(cache.IsLabeled, cache.X, cache.Y, cache.W, cache.H, cache.IsFullyOccluded, cache.IsOutOfView, null);
        }

        private static AnnotationRecordCache ConvertAnnotationRecordToCache(AnnotationRecord record)
        {
            return new AnnotationRecordCache(record.IsLabeled, record.X, record.Y, record.W, record.H, record.IsFullyOccluded, record.IsOutOfView);
        }

        private static bool IsEmptyRecord(AnnotationRecordCache cache)
        {
            return !cache.IsLabeled && cache.X == 0 && cache.Y == 0 && cache.W == 0 && cache.H == 0 &&
                   !cache.IsFullyOccluded && !cache.IsOutOfView;
        }

        private static bool IsEmptyRecord(AnnotationRecord record)
        {
            return !record.IsLabeled && record.X == 0 && record.Y == 0 && record.W == 0 && record.H == 0 &&
                   !record.IsFullyOccluded && !record.IsOutOfView;
        }

        private static AnnotationRecordCache CreateEmptyAnnotationRecordCache()
        {
            return new AnnotationRecordCache(false, 0, 0, 0, 0, false, false);
        }

        private static AnnotationRecord CreateEmptyAnnotationRecord()
        {
            return new AnnotationRecord(false, 0, 0, 0, 0, false, false, null);
        }

        private static (IList<AnnotationRecord>, IDictionary<int, AnnotationRecordCache>, bool)
            MergeMatFileRecordsAndCacheRecords(IList<AnnotationRecord> matFileRecords, IList<bool> matFileRecordValidity,
                IList<AnnotationRecordCache> cacheRecords, IList<string> imageFileNameList)
        {
            bool matFileUpdateRequired = false;

            IList<AnnotationRecord> mergeResults = new List<AnnotationRecord>(imageFileNameList.Count);
            IDictionary<int, AnnotationRecordCache> cacheUpdateToDoList = new Dictionary<int, AnnotationRecordCache>();

            int maxRecordsLength = imageFileNameList.Count;

            if (matFileRecords.Count != maxRecordsLength)
                matFileUpdateRequired = true;

            // Trust chain: cache > mat file

            int firstPhaseEndIndex = Math.Min(Math.Min(maxRecordsLength, matFileRecords.Count), cacheRecords.Count);
            for (int index = 0; index < firstPhaseEndIndex; ++index)
            {
                AnnotationRecord mergedRecord = matFileRecords[index];

                var cacheResult = cacheRecords[index];

                bool isAnnotationRecordIdentityToCache = IsAnnotationRecordIdentityToCache(mergedRecord, cacheResult);
                bool isEmptyRecord = IsEmptyRecord(cacheResult);
                if (!isAnnotationRecordIdentityToCache && !isEmptyRecord)
                {
                    mergedRecord = ConvertAnnotationRecordCacheToRecord(cacheResult);
                    matFileUpdateRequired = true;
                }
                else if (!isAnnotationRecordIdentityToCache && isEmptyRecord)
                {
                    mergedRecord = mergedRecord.Clone();
                    cacheUpdateToDoList.Add(index, ConvertAnnotationRecordToCache(mergedRecord));
                }
                else
                {
                    mergedRecord = mergedRecord.Clone();
                }

                string path = imageFileNameList[index];
                if (mergedRecord.Path != path)
                {
                    mergedRecord.Path = path;
                    matFileUpdateRequired = true;
                }

                if (!matFileRecordValidity[index])
                    matFileUpdateRequired = true;

                mergeResults.Add(mergedRecord);
            }

            int secondPhaseEndIndex = Math.Min(Math.Max(matFileRecords.Count, cacheRecords.Count), maxRecordsLength);
            bool secondPhaseDoWithMatFile = matFileRecords.Count > cacheRecords.Count;
            if (secondPhaseDoWithMatFile)
            {
                for (int index = firstPhaseEndIndex; index < secondPhaseEndIndex; ++index)
                {
                    AnnotationRecord mergedRecord = matFileRecords[index].Clone();
                    cacheUpdateToDoList.Add(index, ConvertAnnotationRecordToCache(mergedRecord));

                    string path = imageFileNameList[index];
                    if (mergedRecord.Path != path)
                    {
                        mergedRecord.Path = path;
                        matFileUpdateRequired = true;
                    }

                    if (!matFileRecordValidity[index])
                        matFileUpdateRequired = true;

                    mergeResults.Add(mergedRecord);
                }
            }
            else
            {
                for (int index = firstPhaseEndIndex; index < secondPhaseEndIndex; ++index)
                {
                    AnnotationRecord mergedRecord = ConvertAnnotationRecordCacheToRecord(cacheRecords[index]);
                    mergedRecord.Path = imageFileNameList[index];
                    matFileUpdateRequired = true;

                    mergeResults.Add(mergedRecord);
                }
            }

            int thirdPhaseEndIndex = maxRecordsLength;
            for (int index = secondPhaseEndIndex; index < thirdPhaseEndIndex; ++index)
            {
                AnnotationRecord mergedRecord = CreateEmptyAnnotationRecord();
                mergedRecord.Path = imageFileNameList[index];
                matFileUpdateRequired = true;
                cacheUpdateToDoList.Add(index, CreateEmptyAnnotationRecordCache());

                mergeResults.Add(mergedRecord);
            }

            return (mergeResults, cacheUpdateToDoList, matFileUpdateRequired);
        }

        private static bool MergeAnnotationRecordsWithInitialRecords(IList<AnnotationRecord> records,
            IList<int[]> initialRecords, int maxLength, IDictionary<int, AnnotationRecordCache> cacheUpdateToDoList)
        {
            bool isMatFileUpdateRequired = false;

            int endIndex = Math.Min(initialRecords.Count, maxLength);
            for (int index = 0; index < endIndex; ++index)
            {
                bool changesHappened = false;
                AnnotationRecord record = records[index];
                int[] initialRecord = initialRecords[index];
                if (record.X != initialRecord[0])
                {
                    record.X = initialRecord[0];
                    changesHappened = true;
                }
                if (record.Y != initialRecord[1])
                {
                    record.Y = initialRecord[1];
                    changesHappened = true;
                }
                if (record.W != initialRecord[2])
                {
                    record.W = initialRecord[2];
                    changesHappened = true;
                }
                if (record.H != initialRecord[3])
                {
                    record.H = initialRecord[3];
                    changesHappened = true;
                }

                if (!record.IsLabeled)
                {
                    record.IsLabeled = true;
                    changesHappened = true;
                }

                if (changesHappened)
                {
                    cacheUpdateToDoList[index] = ConvertAnnotationRecordToCache(record);
                    isMatFileUpdateRequired = true;
                }
            }

            return isMatFileUpdateRequired;
        }
        
        public AnnotationRecordDataAccessor(string sequencePath)
        {
            _sequencePath = sequencePath;
            _name = Path.GetFileName(sequencePath);
            if (_name != null && _name.Length == 0)
                throw new ArgumentException(string.Format(MultiLanguageResources.InvalidPath, sequencePath));

            string initialRecordPath = Path.Combine(sequencePath, "res.txt");
            string matFilePath = Path.Combine(sequencePath, "res.mat");
            string cacheFilePath = Path.Combine(sequencePath, "cache.db");

            bool existsInitialRecordFile = File.Exists(initialRecordPath);
            bool existsMatFile = File.Exists(matFilePath);

            var imageFileNameList = GetImageFileList(sequencePath);
            if (imageFileNameList.Count == 0)
                throw new Exception(MultiLanguageResources.NoValidImageFilesInDirectory);

            IList<int[]> initialRecords = null;
            IList<AnnotationRecord> matFileRecords = null;
            IList<bool> matFileRecordValidity = null;

            if (existsInitialRecordFile)
                initialRecords = LoadInitialRecordsFromTxt(initialRecordPath);
            if (existsMatFile)
                (matFileRecords, matFileRecordValidity) = LoadRecordsFromMatFile(matFilePath);
            var (cachePersistentProvider, cacheRecords) = InitializeAnnotationCache(cacheFilePath);

            if (matFileRecords == null)
            {
                matFileRecords = new List<AnnotationRecord>();
                matFileRecordValidity = new List<bool>();
            }

            var (mergedRecords, cacheUpdateToDoList, isMatFileUpdateRequired) = MergeMatFileRecordsAndCacheRecords(matFileRecords, matFileRecordValidity, cacheRecords, imageFileNameList);
            if (initialRecords != null)
            {
                isMatFileUpdateRequired |= MergeAnnotationRecordsWithInitialRecords(mergedRecords, initialRecords, imageFileNameList.Count,
                    cacheUpdateToDoList);
            }

            if (IsEmptyRecord(mergedRecords[0]))
                throw new Exception("No valid first frame annotation");

            if (isMatFileUpdateRequired)
                GenerateMatlabFile(mergedRecords, matFilePath);

            cachePersistentProvider.Resize(imageFileNameList.Count);
            foreach (var keyValuePair in cacheUpdateToDoList)
            {
                cachePersistentProvider.Set(keyValuePair.Key, keyValuePair.Value);
            }

            _cachePersistentProvider = cachePersistentProvider;
            _annotationRecords = mergedRecords;
        }


        private static void GenerateMatlabFile(IList<AnnotationRecord> records, string filePath)
        {
            using (AnnotationRecordOperator annotationRecordOperator
                = new AnnotationRecordOperator(filePath, AnnotationRecordOperator.DesiredAccess.Write, AnnotationRecordOperator.CreationDisposition.CreateAlways))
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

        private string GetMatFilePath()
        {
            return Path.Combine(_sequencePath, "res.mat");
        }

        private void BackupMatFile()
        {
            string backupFile = Path.Combine(_sequencePath, "res.mat.bak");
            if (File.Exists(backupFile))
                File.Delete(backupFile);
            File.Copy(GetMatFilePath(), backupFile);
        }

        public void Import(string matPath)
        {
            var imageFileNames = GetImageFileList(_sequencePath);

            var(records, _) = LoadRecordsFromMatFile(matPath);

            _pendingUpdates.Clear();

            int beginIndex = _isFirstFrameAnnotationRecordLocked ? 1 : 0;
            int endIndex = Math.Min(imageFileNames.Count, records.Count);
            for (int index = beginIndex; index < endIndex; index++)
            {
                var record = records[index];
                _annotationRecords[index] = record;
                _annotationRecords[index].Path = imageFileNames[index];

                _cachePersistentProvider.Set(index, ConvertAnnotationRecordToCache(record));
            }

            BackupMatFile();
            GenerateMatlabFile(_annotationRecords, GetMatFilePath());
        }

        public void Update(int index, AnnotationRecord record)
        {
            _annotationRecords[index] = record.Clone();
        }

        public void Submit(int index)
        {
            var annotation = _annotationRecords[index];

            _pendingUpdates.Add(index);

            _cachePersistentProvider.Set(index, ConvertAnnotationRecordToCache(annotation));
        }

        public void UpdateMatlabFile()
        {
            if (_pendingUpdates.Count == 0)
                return;

            using (AnnotationRecordOperator annotationRecordOperator =
                new AnnotationRecordOperator(GetMatFilePath(), AnnotationRecordOperator.DesiredAccess.Write,
                    AnnotationRecordOperator.CreationDisposition.OpenAlways))
            {
                foreach (var index in _pendingUpdates)
                {
                    var record = _annotationRecords[index];

                    annotationRecordOperator.Set(Convert.ToUInt32(index), record);
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
