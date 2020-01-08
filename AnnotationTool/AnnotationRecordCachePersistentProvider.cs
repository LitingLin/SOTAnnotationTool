using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LevelDB;


namespace AnnotationTool
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
                _db = provider._db;
                _size = provider._size;
                _writeOptions = provider._writeOptions;
                _batch = new WriteBatch();
            }

            public void Push(AnnotationServiceProvider.AnnotationRecord record)
            {
                _batch.Put(Utilities.Serialization.Serialize(_size), Utilities.Serialization.Serialize(record));
                ++_size;
            }

            public void Set(int index, AnnotationServiceProvider.AnnotationRecord record)
            {
                if (index >= _size)
                    throw new IndexOutOfRangeException();
                _batch.Put(Utilities.Serialization.Serialize(index), Utilities.Serialization.Serialize(record));
            }

            public void Pop()
            {
                if (_size == 0)
                    throw new IndexOutOfRangeException();

                _batch.Delete(Utilities.Serialization.Serialize(_size));
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

        public void Dispose()
        {
            _db.Close();
        }

        public int GetSize()
        {
            return _size;
        }

        public void Push(AnnotationServiceProvider.AnnotationRecord record)
        {
            _db.Put(Utilities.Serialization.Serialize(_size), Utilities.Serialization.Serialize(record), _writeOptions);
            ++_size;
        }

        public AnnotationServiceProvider.AnnotationRecord Get(int size)
        {
            return Utilities.Serialization.Deserialize<AnnotationServiceProvider.AnnotationRecord>(_db.Get(Utilities.Serialization.Serialize(size)));
        }

        public void Set(int index, AnnotationServiceProvider.AnnotationRecord record)
        {
            if (index >= _size)
                throw new IndexOutOfRangeException();
            _db.Put(Utilities.Serialization.Serialize(index), Utilities.Serialization.Serialize(record), _writeOptions);
        }

        public void Pop()
        {
            if(_size==0)
                throw new IndexOutOfRangeException();

            _db.Delete(Utilities.Serialization.Serialize(_size), _writeOptions);
            --_size;
        }
        public void Resize(int size)
        {
            _db.Close();

            File.Delete(_path);

            _db = new DB(new Options { CreateIfMissing = true }, _path);
            var record = Utilities.Serialization.Serialize(new AnnotationServiceProvider.AnnotationRecord());

            using (var batch = new WriteBatch())
            {
                for (int i = 0; i < size; ++i)
                {
                    batch.Put(Utilities.Serialization.Serialize(i), record);
                }
                
                _db.Write(batch, _writeOptions);
            }

            _size = size;
        }
    }
}
