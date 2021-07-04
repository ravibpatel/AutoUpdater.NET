using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AutoUpdaterDotNET
{
    /// <summary>
    /// Provides a mechanism for storing AutoUpdater state between sessions on a Json formatted file.
    /// </summary>
    public class JsonFilePersistenceProvider : IPersistenceProvider
    {
        /// <summary>
        /// Path for the Json formatted file.
        /// </summary>
        private string FileName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private PersistedValues PersistedValues { get; set; }

        /// <summary>
        /// Initializes a new instance of the JsonFilePersistenceProvider class.
        /// </summary>
        /// <remarks>The path for the Json formatted file must be specified using the FileName property.</remarks>
        public JsonFilePersistenceProvider(string jsonPath)
        {
            FileName = jsonPath;
            ReadFile();
        }

        /// <summary>
        /// Stores applied modifications into the Json formatted file specified in the FileName property.
        /// </summary>
        private void Save()
        {
            string json;

            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(PersistedValues.GetType());
                serializer.WriteObject(stream, PersistedValues);

                using (StreamReader reader = new StreamReader(stream))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    json = reader.ReadToEnd();
                }
            }

            File.WriteAllText(FileName, json);
        }

        /// <summary>
        /// Reads a Json formatted file and returns an initialized instance of the class PersistedValues.
        /// </summary>
        /// <remarks>The function creates a new instance, initialized with default parameters, in case the file does not exist.</remarks>
        private void ReadFile()
        {
            PersistedValues jsonFile = null;

            if (File.Exists(FileName))
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(FileName))))
                {
                    DataContractJsonSerializer serializer =
                        new DataContractJsonSerializer(typeof(PersistedValues));
                    jsonFile = (PersistedValues) serializer.ReadObject(stream);
                }
            }

            PersistedValues = jsonFile ?? new PersistedValues();
        }

        /// <inheritdoc />
        public Version GetSkippedVersion()
        {
            return PersistedValues.SkippedVersion;
        }

        /// <inheritdoc />
        public DateTime? GetRemindLater()
        {
            return PersistedValues.RemindLaterAt;
        }

        /// <inheritdoc />
        public void SetSkippedVersion(Version version)
        {
            PersistedValues.SkippedVersion = version;
            Save();
        }

        /// <inheritdoc />
        public void SetRemindLater(DateTime? remindLaterAt)
        {
            PersistedValues.RemindLaterAt = remindLaterAt;
            Save();
        }
    }

    /// <summary>
    /// Provides way to serialize and deserialize AutoUpdater persisted values. 
    /// </summary>
    [DataContract]
    public class PersistedValues
    {
        /// <summary>
        /// Application version to be skipped.
        /// </summary>
        /// <remarks>The SetSkippedVersion function sets this property and calls the Save() method for making changes permanent.</remarks>
        [DataMember]
        public Version SkippedVersion { get; set; }

        /// <summary>
        /// Date and time at which the user must be given again the possibility to upgrade the application.
        /// </summary>
        [DataMember]
        public DateTime? RemindLaterAt { get; set; }
    }
}