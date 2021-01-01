﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TT_Lab.Rendering;
using TT_Lab.Rendering.Objects;
using TT_Lab.ViewModels;

namespace TT_Lab.Editors.Graphics
{
    /// <summary>
    /// Interaction logic for ModelEditor.xaml
    /// </summary>
    public partial class ModelEditor : BaseEditor
    {
        public ModelEditor()
        {
            InitializeComponent();
        }

        public ModelEditor(AssetViewModel model) : base(model)
        {
            InitializeComponent();
            SceneRenderer.RendererInit += SceneRenderer_RendererInit;
        }

        private void SceneRenderer_RendererInit(Object sender, EventArgs e)
        {
            SceneRenderer.Scene = new Scene((float)SceneRenderer.GLHost.ActualWidth, (float)SceneRenderer.GLHost.ActualHeight);
            SceneRenderer.Scene.SetCameraSpeed(0.2f);
            Model m = new Model((AssetData.Graphics.ModelData)DataContext);
            SceneRenderer.Scene.AddRender(m);
        }
    }
}
