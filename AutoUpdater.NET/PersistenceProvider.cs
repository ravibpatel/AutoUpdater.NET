using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AutoUpdaterDotNET
{
    /// <summary>
    /// Provides a mechanism for storing AutoUpdater state between sessions.
    /// </summary>
    public interface IPersistenceProvider
    {
        /// <summary>
        /// Reads the flag indicating whether a specific version should be skipped or not.
        /// </summary>
        /// <param name="Skip">On return, this output parameter will be filled with the current state for Skip flag.</param>
        /// <param name="Version">On return, this output parameter will be filled with the current version code to be skipped. It should be ignored whenever the Skip flag is set to <code>false</code>.</param>
        /// <returns>Returns a value indicating whether the current state available in the storage is valid (<code>true</code>) or not (<code>false</code>).</returns>
        bool GetSkippedApplicationVersion( out bool Skip, out string Version );

        /// <summary>
        /// Reads the value containing the date and time at which the user must be given again the possibility to upgrade the application.
        /// </summary>
        /// <param name="RemaindLater">On return, this output parameter will be filled with the date and time at which the user must be given again the possibility to upgrade the application</param>
        /// <returns>Returns a value indicating whether the current state available in the storage is valid (<code>true</code>) or not (<code>false</code>).</returns>
        bool GetRemaindLater( out DateTime RemaindLater );

        /// <summary>
        /// Sets the values indicating the specific version that must be ignored by AutoUpdater.
        /// </summary>
        /// <param name="Skip">Flag indicating that a specific version must be ignored (<code>true</code>) or not (<code>false</code>).</param>
        /// <param name="Version">Version code for the specific version that must be ignored. This value is taken into account only when <paramref name="Skip"/> has value <code>true</code>.</param>
        void SetSkippedApplicationVersion( bool Skip, string Version );

        /// <summary>
        /// Sets the date and time at which the user must be given again the possibility to upgrade the application.
        /// </summary>
        /// <param name="RemaindLater">Date and time at which the user must be given again the possibility to upgrade the application.</param>
        void SetRemaindLater( DateTime RemaindLater );
    }


    /// <summary>
    /// Provides a mechanism for storing AutoUpdater state between sessions based on storing data on the Windows Registry.
    /// </summary>
    public class RegistryPersistenceProvider : IPersistenceProvider
    {
        /// <summary>
        /// Gets/sets the path for the Windows Registry key that will contain the data.
        /// </summary>
        public string RegistryLocation { get; private set; }


        /// <summary>
        /// Initializes a new instance of the RegistryPersistenceProvider class indicating the path for the Windows registry key to use for storing the data.
        /// </summary>
        /// <param name="RegistryLocation"></param>
        public RegistryPersistenceProvider( string RegistryLocation )
        {
            this.RegistryLocation = RegistryLocation;
        }


        /// <summary>
        /// Reads the flag indicating whether a specific version should be skipped or not.
        /// </summary>
        /// <param name="Skip">On return, this output parameter will be filled with the current state for Skip flag.</param>
        /// <param name="Version">On return, this output parameter will be filled with the current version code to be skipped. It should be ignored whenever the Skip flag is set to <code>false</code>.</param>
        /// <returns>Returns a value indicating whether the current state available in the storage is valid (<code>true</code>) or not (<code>false</code>).</returns>
        /// <remarks>This function does not create the registry key if it does not exist.</remarks>
        public bool GetSkippedApplicationVersion( out bool Skip, out string Version )
        {
            Skip = false;
            Version = null;

            using (RegistryKey UpdateKey = Registry.CurrentUser.OpenSubKey( RegistryLocation ))
            {
                if (UpdateKey == null)
                    return false;

                object SkipRegKey = UpdateKey.GetValue( "skip" );
                object VersionRegKey = UpdateKey.GetValue( "version" );

                if ((SkipRegKey == null) || (VersionRegKey == null))
                    return false;

                Skip = ("1".Equals( SkipRegKey.ToString() ));
                Version = VersionRegKey.ToString();

                return true;
            }
        }


        /// <summary>
        /// Reads the value containing the date and time at which the user must be given again the possibility to upgrade the application.
        /// </summary>
        /// <param name="RemaindLater">On return, this output parameter will be filled with the date and time at which the user must be given again the possibility to upgrade the application</param>
        /// <returns>Returns a value indicating whether the current state available in the storage is valid (<code>true</code>) or not (<code>false</code>).</returns>
        /// <remarks>This function does not create the registry key if it does not exist.</remarks>
        public bool GetRemaindLater( out DateTime RemaindLater )
        {
            RemaindLater = DateTime.MinValue;

            using (RegistryKey UpdateKey = Registry.CurrentUser.OpenSubKey( RegistryLocation ))
            {
                if (UpdateKey == null)
                    return false;

                object RemaindLaterRegKey = UpdateKey.GetValue( "remindlater" );

                if (RemaindLaterRegKey == null)
                    return false;

                RemaindLater = Convert.ToDateTime( RemaindLaterRegKey.ToString(), CultureInfo.CreateSpecificCulture( "en-US" ).DateTimeFormat );

                return true;
            }
        }


        /// <summary>
        /// Sets the values indicating the specific version that must be ignored by AutoUpdater.
        /// </summary>
        /// <param name="Skip">Flag indicating that a specific version must be ignored (<code>true</code>) or not (<code>false</code>).</param>
        /// <param name="Version">Version code for the specific version that must be ignored. This value is taken into account only when <paramref name="Skip"/> has value <code>true</code>.</param>
        public void SetSkippedApplicationVersion( bool Skip, string Version )
        {
            using (RegistryKey UpdateKeyWrite = Registry.CurrentUser.CreateSubKey( RegistryLocation ))
            {
                if (UpdateKeyWrite != null)
                {
                    UpdateKeyWrite.SetValue( "version", Version.ToString() );
                    UpdateKeyWrite.SetValue( "skip", Skip ? 1 : 0 );
                }
            }
        }


        /// <summary>
        /// Sets the date and time at which the user must be given again the possibility to upgrade the application.
        /// </summary>
        /// <param name="RemaindLater">Date and time at which the user must be given again the possibility to upgrade the application.</param>
        public void SetRemaindLater( DateTime RemaindLater )
        {
            using (RegistryKey UpdateKeyWrite = Registry.CurrentUser.CreateSubKey( RegistryLocation ))
            {
                if (UpdateKeyWrite != null)
                {
                    UpdateKeyWrite.SetValue( "remindlater", RemaindLater.ToString( CultureInfo.CreateSpecificCulture( "en-US" ).DateTimeFormat ) );
                }
            }
        }
    }


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
        public string RemaindLater { get; set; }


        /// <summary>
        /// Initializes a new instance of the JsonFilePersistenceProvider class.
        /// </summary>
        /// <remarks>The path for the Json formatted file must be specified using the FileName property.</remarks>
        public JsonFilePersistenceProvider()
        {}


        /// <summary>
        /// Stores applied modifications into the Json formatted file specified in the FileName property.
        /// </summary>
        private void Save()
        {
            string JSON;

            using (MemoryStream Stream = new MemoryStream())
            {
                DataContractJsonSerializer Serializer = new DataContractJsonSerializer( this.GetType() );
                Serializer.WriteObject( Stream, this );

                using (StreamReader Reader = new StreamReader( Stream ))
                {
                    Stream.Seek( 0, SeekOrigin.Begin );
                    JSON = Reader.ReadToEnd();
                }
            }

            File.WriteAllText( FileName, JSON );
        }


        /// <summary>
        /// Reads a Json formatted file and returns an initialized instance of the class JsonFilePersistenceProvider.
        /// </summary>
        /// <param name="FileName">Path for the Json formatted file.</param>
        /// <returns>Returns an initialized instance of the class JsonFilePersistenceProvider.</returns>
        /// <remarks>The function creates a new instance, initialized with default parameters, in case the file does not exist.</remarks>
        public static JsonFilePersistenceProvider FromFile( string FileName )
        {
            JsonFilePersistenceProvider JsonFile = null;

            if (File.Exists( FileName ))
            {
                using (MemoryStream Stream = new MemoryStream( Encoding.UTF8.GetBytes( File.ReadAllText(FileName) ) ))
                {
                    DataContractJsonSerializer Serializer = new DataContractJsonSerializer( typeof( JsonFilePersistenceProvider ) );
                    JsonFile = (JsonFilePersistenceProvider)Serializer.ReadObject( Stream );
                }
            }

            if (JsonFile == null)
                JsonFile = new JsonFilePersistenceProvider();

            JsonFile.FileName = FileName;

            return JsonFile;
        }


        /// <summary>
        /// Reads the flag indicating whether a specific version should be skipped or not.
        /// </summary>
        /// <param name="Skip">On return, this output parameter will be filled with the current state for Skip flag.</param>
        /// <param name="Version">On return, this output parameter will be filled with the current version code to be skipped. It should be ignored whenever the Skip flag is set to <code>false</code>.</param>
        /// <returns>Returns a value indicating whether the current state available in the storage is valid (<code>true</code>) or not (<code>false</code>).</returns>
        public bool GetSkippedApplicationVersion( out bool Skip, out string Version )
        {
            Skip = this.Skip;
            Version = this.SkippedApplicationVersion;

            return (Version != null);
        }


        /// <summary>
        /// Reads the value containing the date and time at which the user must be given again the possibility to upgrade the application.
        /// </summary>
        /// <param name="RemaindLater">On return, this output parameter will be filled with the date and time at which the user must be given again the possibility to upgrade the application</param>
        /// <returns>Returns a value indicating whether the current state available in the storage is valid (<code>true</code>) or not (<code>false</code>).</returns>
        public bool GetRemaindLater( out DateTime RemaindLater )
        {
            RemaindLater = DateTime.MinValue;

            if (!string.IsNullOrEmpty( this.RemaindLater))
            {
                RemaindLater = Convert.ToDateTime( this.RemaindLater.ToString(), CultureInfo.CreateSpecificCulture( "en-US" ).DateTimeFormat );
                return true;
            }

            return false;
        }


        /// <summary>
        /// Sets the values indicating the specific version that must be ignored by AutoUpdater.
        /// </summary>
        /// <param name="Skip">Flag indicating that a specific version must be ignored (<code>true</code>) or not (<code>false</code>).</param>
        /// <param name="Version">Version code for the specific version that must be ignored. This value is taken into account only when <paramref name="Skip"/> has value <code>true</code>.</param>
        public void SetSkippedApplicationVersion( bool Skip, string Version )
        {
            this.Skip = Skip;
            this.SkippedApplicationVersion = Version;
            Save();
        }


        /// <summary>
        /// Sets the date and time at which the user must be given again the possibility to upgrade the application.
        /// </summary>
        /// <param name="RemaindLater">Date and time at which the user must be given again the possibility to upgrade the application.</param>
        public void SetRemaindLater( DateTime RemaindLater )
        {
            this.RemaindLater = RemaindLater.ToString( CultureInfo.CreateSpecificCulture( "en-US" ).DateTimeFormat );

            Save();
        }

    }
}
