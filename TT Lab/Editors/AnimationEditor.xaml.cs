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
using TT_Lab.AssetData.Code;
using TT_Lab.Assets.Code;
using TT_Lab.ViewModels;

namespace TT_Lab.Editors
{
    /// <summary>
    /// Interaction logic for AnimationEditor.xaml
    /// </summary>
    public partial class AnimationEditor : BaseEditor<Animation>
    {
        public AnimationEditor()
        {
            InitializeComponent();
        }

        public AnimationEditor(Animation animation) : base(animation)
        {
            InitializeComponent();
            BitfieldBox.Text = "0x"+ animation.GetData<AnimationData>().Bitfield.ToString("X");
        }
    }
}