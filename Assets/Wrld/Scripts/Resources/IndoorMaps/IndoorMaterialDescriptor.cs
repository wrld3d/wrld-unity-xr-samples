using System.Collections.Generic;
using UnityEngine;

namespace Wrld.Resources.IndoorMaps
{
    /// <summary>
    /// Contains a number of parameters for building indoor map materials. It is used by the IIndoorMapMaterialFactory.
    /// These are streamed from WRLD's service alongside map geometry.
    /// </summary>
    public class IndoorMaterialDescriptor
    {
        public IndoorMaterialDescriptor(
            string indoorMapName,
            string materialName,
            Dictionary<string, string> strings,
            Dictionary<string, Color> colors,
            Dictionary<string, float> scalars,
            Dictionary<string, bool> booleans
            )
        {
            IndoorMapName = indoorMapName;
            MaterialName = materialName;
            Strings = strings;
            Colors = colors;
            Scalars = scalars;
            Booleans = booleans;
        }

        public IndoorMaterialDescriptor(
            string indoorMapName,
            string materialName
            )
        {
            IndoorMapName = indoorMapName;
            MaterialName = materialName;
            Strings = new Dictionary<string, string>();
            Colors = new Dictionary<string, Color>();
            Scalars = new Dictionary<string, float>();
            Booleans = new Dictionary<string, bool>();
        }

        /// <summary>
        /// The name of the Indoor Map associated with this material.
        /// </summary>
        public string IndoorMapName { get; private set; }
        /// <summary>
        /// The name of this material.
        /// </summary>
        public string MaterialName { get; private set; }
        /// <summary>
        /// A dictionary of string-based parameters.
        /// </summary>
        public Dictionary<string, string> Strings { get; private set; }
        /// <summary>
        /// A dictionary of Unity Color parameters.
        /// </summary>
        public Dictionary<string, Color> Colors { get; private set; }
        /// <summary>
        /// A dictionary of floating-point scalar parameters.
        /// </summary>
        public Dictionary<string, float> Scalars { get; private set; }
        /// <summary>
        /// A dictionary of boolean parameters.
        /// </summary>
        public Dictionary<string, bool> Booleans { get; private set; }
    }
}