using Newtonsoft.Json;
using Oddmatics.PowerUser.Windows.QueryableStorage.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Oddmatics.PowerUser.Windows.QueryableStorage.Configuration
{
    /// <summary>
    /// Represents the configuration of the Queryable Storage service.
    /// </summary>
    internal sealed class ServiceConfiguration
    {
        /// <summary>
        /// The file name of the configuration document.
        /// </summary>
        private const string ConfigurationFileName = "queryfs.json";

        /// <summary>
        /// The full file path of the configuration document.
        /// </summary>
        private static readonly string ConfigurationFilePath = Path.Combine(
            Environment.CurrentDirectory +
            "\\" +
            ConfigurationFileName
            );


        /// <summary>
        /// The defined configurations.
        /// </summary>
        private Dictionary<string, string> Configurations;


        /// <summary>
        /// Initializes a new instance of the ServiceConfiguration class.
        /// </summary>
        public ServiceConfiguration()
        {
            if (!File.Exists(ConfigurationFilePath))
            {
                File.WriteAllText(
                    ConfigurationFilePath,
                    Resources.DefaultConfigurations
                    );
            }

            ReadConfigurationDocument(ConfigurationFilePath);
        }


        /// <summary>
        /// Gets or sets the configuration definition associated with the specified definition.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <returns>The configuration value.</returns>
        public string this[string definition]
        {
            get
            {
                return Configurations[definition.ToLower()];
            }

            set
            {
                Configurations[definition.ToLower()] = value;
            }
        }


        /// <summary>
        /// Determines whether the ServiceConfiguration contains the specified definition.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <returns>True if the definition is set.</returns>
        public bool ContainsDefinition(string definition)
        {
            return Configurations.ContainsKey(definition.ToLower());
        }


        /// <summary>
        /// Reads the configuration document.
        /// </summary>
        /// <param name="fullFilePath">The full file path to the configuration document.</param>
        private void ReadConfigurationDocument(string fullFilePath)
        {
            string fileContents = File.ReadAllText(fullFilePath);
            var rawConfigs = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContents);

            Configurations = new Dictionary<string, string>();

            foreach (var pair in rawConfigs)
            {
                Configurations[pair.Key.ToLower()] = pair.Value;
            }
        }
    }
}
