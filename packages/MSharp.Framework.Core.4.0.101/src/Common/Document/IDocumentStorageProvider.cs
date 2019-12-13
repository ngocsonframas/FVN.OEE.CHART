﻿namespace MSharp.Framework
{
    public interface IDocumentStorageProvider
    {
        void Save(Document document);
        void Delete(Document document);
        byte[] Load(Document document);
        bool FileExists(Document document);
    }

    public interface IRemoteDocumentStorageProvider
    {
        bool CostsToCheckExistence();
    }
}

