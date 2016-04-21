using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Speech.Recognition;

namespace HCI2012
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class VoiceCommandWindow : Window
    {
        static List<string> grammar;
        public VoiceCommandWindow()
        {
            InitializeComponent();
            this.Top = 0;
            this.Left = 0;
            Topmost = true;

            grammar = SkeletonWindow.voiceCommandList;
            listBoxVoiceCommand.ItemsSource = grammar;
        }

    }
}
