﻿using SyncTrayzor.Utils;
using System;
using System.IO;

namespace SyncTrayzor.Services
{
    public interface IFilesystemProvider
    {
        bool FileExists(string path);
        bool DirectoryExists(string path);
        FileStream Open(string path, FileMode mode);
        FileStream Open(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);
        FileStream CreateAtomic(string path);
        FileStream OpenRead(string path);
        void Copy(string from, string to);
        void Move(string from, string to);
        void CreateDirectory(string path);
        void DeleteFile(string path);
        void DeleteDirectory(string path, bool recursive);
        void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc);
        DateTime GetLastAccessTimeUtc(string path);
        string[] GetFiles(string path);
        string ReadAllText(string path);
        string[] GetFiles(string path, string searchPattern, SearchOption searchOption);
    }

    public class FilesystemProvider : IFilesystemProvider
    {
        public bool FileExists(string path) => File.Exists(path);

        public bool DirectoryExists(string path) => Directory.Exists(path);

        public FileStream Open(string path, FileMode mode) => File.Open(path, mode);

        public FileStream Open(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare) => new FileStream(path, fileMode, fileAccess, fileShare);

        public FileStream CreateAtomic(string path) => new AtomicFileStream(path);

        public FileStream OpenRead(string path) => File.OpenRead(path);

        public void Copy(string from, string to) => File.Copy(from, to);

        public void Move(string from, string to) => File.Move(from, to);

        public void CreateDirectory(string path) => Directory.CreateDirectory(path);

        public void DeleteFile(string path) =>  File.Delete(path);

        public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);

        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc) => File.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

        public DateTime GetLastAccessTimeUtc(string path) => File.GetLastAccessTimeUtc(path);

        public string[] GetFiles(string path) => Directory.GetFiles(path);

        public string ReadAllText(string path) => File.ReadAllText(path);

        public string[] GetFiles(string path, string searchPattern, SearchOption searchOption) => Directory.GetFiles(path, searchPattern, searchOption);
    }
}
