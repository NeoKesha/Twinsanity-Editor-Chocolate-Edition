﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using TT_Lab.AssetData;
using TT_Lab.ViewModels;

namespace TT_Lab.Assets
{
    [JsonObject(MemberSerialization.OptIn)]
    public interface IAsset
    {
        /// <summary>
        /// Asset's unique ID
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        Guid UUID { get; }

        /// <summary>
        /// Asset's string type
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        Type Type { get; }

        /// <summary>
        /// In-Game's ID number
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        UInt32 ID { get; set; }

        /// <summary>
        /// Asset's name
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        string Name { get; set; }

        /// <summary>
        /// Whether asset's data is in raw form
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        bool Raw { get; set; }

        /// <summary>
        /// Path to asset's data
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        string Data { get; set; }

        /// <summary>
        /// Name for the asset to display in project tree
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        string Alias { get; set; }

        /// <summary>
        /// Chunk path where this asset belongs to
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        string Chunk { get; }

        /// <summary>
        /// TT Lab specific data
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        Dictionary<String, Object> Parameters { get; }

        /// <summary>
        /// Order in the Project Tree
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        UInt32 Order { get; set; }

        /// <summary>
        /// For instances their Layout ID
        /// </summary>
        /// <remarks>Ranges from 0 to 7</remarks>
        [JsonProperty(Required = Required.AllowNull)]
        Int32? LayoutID { get; set; }

        /// <summary>
        /// Whether the data for this asset is currently in memory
        /// </summary>
        Boolean IsLoaded { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Asset's editor type</returns>
        Type GetEditorType();

        /// <summary>
        /// Loads in asset's data if it's not loaded
        /// </summary>
        /// <returns>Asset's data</returns>
        /// <typeparam name="T">Type of data</typeparam>
        AbstractAssetData GetData();

        /// <summary>
        /// Returns asset's viewmodel for editing
        /// </summary>
        AssetViewModel GetViewModel(AssetViewModel parent = null);

        /// <summary>
        /// Dump on disk in JSON format
        /// </summary>
        void Serialize();

        /// <summary>
        /// Convert JSON format to the project asset object
        /// </summary>
        void Deserialize(string json);

        /// <summary>
        /// Converts given data to raw game's data
        /// </summary>
        /// <param name="data">Data to convert</param>
        void ToRaw(Byte[] data);

        /// <summary>
        /// Converts raw data to a general format
        /// </summary>
        /// <returns>Data in the format</returns>
        Byte[] ToFormat();

        /// <summary>
        /// Finishes import on Project Creation stage
        /// </summary>
        void Import();
    }
}
