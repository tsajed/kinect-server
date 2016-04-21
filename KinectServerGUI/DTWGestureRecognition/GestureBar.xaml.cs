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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace HCI2012
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AnimationBar : Window
    {
        public AnimationBar()
        {
            InitializeComponent();
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight - 100;
            Top = 110;
            Left = 0;

            SkeletonWindow skeleton = new SkeletonWindow(this);
            VoiceCommandWindow voiceCommand = new VoiceCommandWindow();


            skeleton.Show();
            voiceCommand.Show();
           //MouseEventArgs mouse = new MouseEventArgs(Mouse.PrimaryDevice, 0);
           //mouse.RoutedEvent = Mouse.MouseEnterEvent;
           //button3.RaiseEvent(mouse);
        }

        void ButtonTest(object sender, RoutedEventArgs e)
        {
            //WindowVoice voice = new WindowVoice();
            this.Close();
            //voice.Show();
        }
        public void ReturnScreen()
        {
            this.Show();
        }
    }
}
