using MediaFireDownloader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MediaFireDownloader
{
    internal sealed class RecursiveFileReader
    {
        private readonly Mediafire _mediafire;
        private readonly int _chunkSize;
        private readonly Queue<EntryWrapper<FolderEntry>> _folders = new Queue<EntryWrapper<FolderEntry>>();
        private int _chunk = 1;
        private string _destination;
        private int _index = 0;
        private ChunkedResult<FileEntry> _result;

        public RecursiveFileReader(Mediafire mediafire, FolderEntry folder, string destination, int chunkSize)
        {
            _mediafire = mediafire;
            _chunkSize = chunkSize;
            _folders.Enqueue(new EntryWrapper<FolderEntry>(destination, folder));
        }

        public async Task<EntryWrapper<FileEntry>> ReadNextFile()
        {
            if (_result == null && !await TryMoveToNextChunk())
                return null;

            if (_result.Result.Length == _index)
            {
                if (_result.HasNextChunk)
                {
                    if (!await TryMoveToNextChunk())
                        return null;
                }
                else
                {
                    return null;
                }
            }

            return new EntryWrapper<FileEntry>(_destination, _result.Result[_index++]);
        }

        private async Task<bool> TryMoveToNextChunk()
        {
            var folder = _folders.Peek();

            _destination = Path.Combine(folder.Destination, folder.Entry.Name);
            _index = 0;
            _result = await _mediafire.GetFiles(folder.Entry, _chunk++, _chunkSize);

            return _result.Result.Length > 0;
        }

        public async Task<EntryWrapper<FolderEntry>> ReadNextFolder()
        {
            var folder = _folders.Dequeue();
            var destination = Path.Combine(folder.Destination, folder.Entry.Name);
            var chunk = 1;
            ChunkedResult<FolderEntry> result;

            do
            {
                result = await _mediafire.GetFolders(folder.Entry, chunk, _chunkSize);

                foreach (var item in result.Result)
                    _folders.Enqueue(new EntryWrapper<FolderEntry>(destination, item));
            }
            while (result.HasNextChunk);

            if (_folders.Count == 0)
                return null;

            _chunk = 1;
            _result = null;

            return _folders.Peek();
        }
    }
}
