using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AutoUpdaterDotNET
{
    /// <summary>
    /// Provides a mechanism for storing AutoUpdater state between sessions based on storing data on a Json formatted file.
    /// </summary>
    [DataContract]
    public class JsonFilePersistenceProvider : IPersistenceProvider
    {
        /// <summary>
        /// Path for the Json formatted file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Flag indicating whether the application version code specified in the property SkippedApplicationVersion should be skipped or not.
        /// </summary>
        /// <remarks>The SetSkippedApplicationVersion function sets this property and calls the Save() method for making changes permanent.</remarks>
        [DataMember]
        public bool Skip { get; set; }

        /// <summary>
        /// Application version code to be skipped.
        /// </summary>
        /// <remarks>The SetSkippedApplicationVersion function sets this property and calls the Save() method for making changes permanent.</remarks>
        [DataMember]
        public string SkippedApplicationVersion { get; set; }

        /// <summary>
        /// Date and time at which the user must be given again the possibility to upgrade the application
        /// </summary>
        [DataMember]
        public string RemindLater { get; set; }

        /// <summary>
        /// Initializes a new instance of the JsonFilePersistenceProvider class.
        /// </summary>
        /// <remarks>The path for the Json formatted file must be specified using the FileName property.</remarks>
        public JsonFilePersistenceProvider(string jsonPath)
        {
            FileName = jsonPath;
        }

        /// <summary>
        /// Stores applied modifications into the Json formatted file specified in the FileName property.
        /// </summary>
        private void Save()
        {
            string json;

            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(GetType());
                serializer.WriteObject(stream, this);

                using (StreamReader reader = new StreamReader(stream))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    json = reader.ReadToEnd();
                }
            }

            File.WriteAllText(FileName, json);
        }

        /// <summary>
        /// Reads a Json formatted file and returns an initialized instance of the class JsonFilePersistenceProvider.
        /// </summary>
        /// <param name="fileName">Path for the Json formatted file.</param>
        /// <returns>Returns an initialized instance of the class JsonFilePersistenceProvider.</returns>
        /// <remarks>The function creates a new instance, initialized with default parameters, in case the file does not exist.</remarks>
        public static JsonFilePersistenceProvider FromFile(string fileName)
        {
            JsonFilePersistenceProvider jsonFile = null;

            if (File.Exists(fileName))
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(fileName))))
                {
                    DataContractJsonSerializer serializer =
                        new DataContractJsonSerializer(typeof(JsonFilePersistenceProvider));
                    jsonFile = (JsonFilePersistenceProvider) serializer.ReadObject(stream);
                }
            }

            return jsonFile ?? new JsonFilePersistenceProvider(fileName);
        }

        /// <summary>
        /// Reads the flag indicating whether a specific version should be skipped or not.
        /// </summary>
        /// <param name="skip">On return, this output parameter will be filled with the current state for Skip flag.</param>
        /// <param name="version">On return, this output parameter will be filled with the current version code to be skipped. It should be ignored whenever the Skip flag is set to <code>false</code>.</param>
        /// <returns>Returns a value indicating whether the current state available in the storage is valid (<code>true</code>) or not (<code>false</code>).</returns>
        public bool GetSkippedApplicationVersion(out bool skip, out string version)
        {
            skip = Skip;
            version = SkippedApplicationVersion;

            return (version != null);
        }

        /// <summary>
        /// Reads the value containing the date and time at which the user must be given again the possibility to upgrade the application.
        /// </summary>
        /// <param name="remindLater">On return, this output parameter will be filled with the date and time at which the user must be given again the possibility to upgrade the application</param>
        /// <returns>Returns a value indicating whether the current state available in the storage is valid (<code>true</code>) or not (<code>false</code>).</returns>
        public bool GetRemindLater(out DateTime remindLater)
        {
            remindLater = DateTime.MinValue;

            if (!string.IsNullOrEmpty(RemindLater))
            {
                remindLater = Convert.ToDateTime(RemindLater,
                    CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the values indicating the specific version that must be ignored by AutoUpdater.
        /// </summary>
        /// <param name="skip">Flag indicating that a specific version must be ignored (<code>true</code>) or not (<code>false</code>).</param>
        /// <param name="version">Version code for the specific version that must be ignored. This value is taken into account only when <paramref name="skip"/> has value <code>true</code>.</param>
        public void SetSkippedApplicationVersion(bool skip, string version)
        {
            Skip = skip;
            SkippedApplicationVersion = version;
            Save();
        }

        /// <summary>
        /// Sets the date and time at which the user must be given again the possibility to upgrade the application.
        /// </summary>
        /// <param name="remindLater">Date and time at which the user must be given again the possibility to upgrade the application.</param>
        public void SetRemindLater(DateTime remindLater)
        {
            RemindLater = remindLater.ToString(CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat);

            Save();
        }
    }
}