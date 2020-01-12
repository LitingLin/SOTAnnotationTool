using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LevelDB;
using AnnotationTool.Data.Model;

namespace AnnotationTool.Data
{
    class AnnotationRecordCachePersistentProvider : IDisposable
    {
        private string _path;
        private DB _db;
        private int _size;
        private WriteOptions _writeOptions;

        class Transaction : IDisposable
        {
            private AnnotationRecordCachePersistentProvider _provider;
            private WriteBatch _batch;
            private DB _db;
            private int _size;
            private WriteOptions _writeOptions;

            private Transaction(AnnotationRecordCachePersistentProvider provider)
            {
                _provider = provider;
                _db = provider._db;
                _size = provider._size;
                _writeOptions = provider._writeOptions;
                _batch = new WriteBatch();
            }

            public void Push(AnnotationRecordCache record)
            {
                _batch.Put(Serialization.Serialize(_size), Serialization.Serialize(record));
                ++_size;
            }

            public void Set(int index, AnnotationRecordCache record)
            {
                if (index >= _size)
                    throw new IndexOutOfRangeException();
                _batch.Put(Serialization.Serialize(index), Serialization.Serialize(record));
            }

            public void Pop()
            {
                if (_size == 0)
                    throw new IndexOutOfRangeException();

                _batch.Delete(Serialization.Serialize(_size - 1));
                --_size;
            }

            public void Dispose()
            {
                _db.Write(_batch, _writeOptions);
                _batch.Dispose();
                _provider._size = _size;
            }
        }

        public AnnotationRecordCachePersistentProvider(string path)
        {
            _path = path;
            
            _db = new DB(new Options { CreateIfMissing = true }, path);
            _writeOptions = new WriteOptions { Sync = true };

            _size = 0;
            foreach (var value in _db)
            {
                ++_size;
            }
        }

        public IList<AnnotationRecordCache> AsList()
        {
            IList<AnnotationRecordCache> records = new List<AnnotationRecordCache>(_size);
            for (int i = 0; i < _size; ++i)
            {
                records.Add(Get(i));
            }

            return records;
        }

        public void Dispose()
        {
            _db.Close();
        }

        public int GetSize()
        {
            return _size;
        }

        public void Push(AnnotationRecordCache record)
        {
            _db.Put(Serialization.Serialize(_size), Serialization.Serialize(record), _writeOptions);
            ++_size;
        }

        public AnnotationRecordCache Get(int index)
        {
            return Serialization.Deserialize<AnnotationRecordCache>(_db.Get(Serialization.Serialize(index)));
        }

        public void Set(int index, AnnotationRecordCache record)
        {
            if (index >= _size)
                throw new IndexOutOfRangeException();
            _db.Put(Serialization.Serialize(index), Serialization.Serialize(record), _writeOptions);
        }

        public void Pop()
        {
            if (_size == 0)
                throw new IndexOutOfRangeException();

            _db.Delete(Serialization.Serialize(_size - 1), _writeOptions);
            --_size;
        }
        public void Resize(int size)
        {
            if (size == _size)
                return;
            
            using (var batch = new WriteBatch())
            {
                if (size < _size)
                {
                    for (int index = size; index < _size; ++index)
                    {
                        batch.Delete(Serialization.Serialize(index));
                    }
                }
                else
                {
                    var emptyRecord = Serialization.Serialize(new AnnotationRecordCache(false, 0, 0, 0, 0, false, false));

                    for (int index = _size; index < size; ++index)
                    {
                        batch.Put(Serialization.Serialize(index), emptyRecord);
                    }
                }

                _db.Write(batch, _writeOptions);
                _size = size;
            }
        }
    }
}
