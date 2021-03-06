﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TT_Lab.AssetData;
using TT_Lab.AssetData.Graphics;
using TT_Lab.Editors.Graphics;
using TT_Lab.ViewModels;
using TT_Lab.ViewModels.Graphics;
using Twinsanity.TwinsanityInterchange.Implementations.PS2.Items.Graphics;

namespace TT_Lab.Assets.Graphics
{
    public class Material : SerializableAsset
    {

        public Material(UInt32 id, String name, PS2AnyMaterial material) : base(id, name)
        {
            assetData = new MaterialData(material);
        }

        public Material()
        {
        }

        public override Byte[] ToFormat()
        {
            throw new NotImplementedException();
        }

        public override void ToRaw(Byte[] data)
        {
            throw new NotImplementedException();
        }

        public override Type GetEditorType()
        {
            return typeof(MaterialEditor);
        }

        public override AbstractAssetData GetData()
        {
            if (!IsLoaded || assetData.Disposed)
            {
                assetData = new MaterialData();
                assetData.Load(System.IO.Path.Combine("assets", SavePath, Data));
                IsLoaded = true;
            }
            return assetData;
        }

        public override AssetViewModel GetViewModel(AssetViewModel parent = null)
        {
            if (viewModel == null)
            {
                viewModel = new MaterialViewModel(UUID, parent);
            }
            return viewModel;
        }
    }
}
