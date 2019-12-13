﻿using System;
using System.IO;

namespace MSharp.Framework.Services
{
    /// <summary>
    /// This class provides a unique file path in a temporary folder (i.e. in the application temp folder
    /// in the system by default and can be provided in Config of the application through a setting with key "Application.TemporaryFilesPath")
    /// After this instance is disposed any possibly created file in the path will be deleted physically.
    /// 
    /// If this class fails to dispose an application event will be added to the projects database.
    /// </summary>
    public class TemporaryFilePath : IDisposable
    {
        static string TemporaryFileFolder = GetTemporaryFileFolder();

        /// <summary>
        /// Creates a new instance of temporary file. The file will have "dat" extension by default.
        /// </summary>
        public TemporaryFilePath() : this("dat") { }

        /// <summary>
        /// Creates a new instance of temporary file.
        /// with the given extension. Extension can either have "." or not
        /// </summary>
        public TemporaryFilePath(string extension)
        {
            ID = Guid.NewGuid();
            Extension = extension;
        }

        /// <summary>
        /// Finds a proper folder path for temporary files
        /// </summary>
        static string GetTemporaryFileFolder()
        {
            var relativePath = Config.Get<string>("Application.TemporaryFilesPath", "");

            if (relativePath.HasValue())
                return AppDomain.CurrentDomain.BaseDirectory + relativePath.Replace("/", "\\").Replace("\\\\", "\\").Trim('\\');

            return Path.GetTempPath();
        }

        Guid ID { get; set; }

        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets the FilePath of this TemporaryFile.
        /// </summary>
        public string FilePath
        {
            get
            {
                var folder = TemporaryFileFolder;
                if (Directory.Exists(folder) == false)
                    Directory.CreateDirectory(folder);

                var filename = ID.ToString() + "." + Extension.Trim('.');
                return folder + "\\" + filename;
            }
        }

        /// <summary>
        /// Disposes this instance of temporary file and deletes the file if provided
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (FilePath.AsFile().Exists()) File.Delete(FilePath);
            }
            catch (Exception ex)
            {
                Log.Error("Can not dispose temporary file.", ex);
            }
        }
    }
}