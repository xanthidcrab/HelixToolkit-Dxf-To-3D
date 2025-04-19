using _3DExample.Helpers;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace _3DExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            string[] strings = Directory.GetFiles(DxfHelper.DxfPath, "*.dxf");
            foreach (var item in strings)
            {
                dxfListBox.Items.Add(item);
            }
            helixViewport3D.MouseMove += (s, e) =>
            {
                Point mousePosition = e.GetPosition(helixViewport3D);
                var hits = Viewport3DHelper.FindHits(helixViewport3D.Viewport, mousePosition);

                if (hits.Count > 0)
                {
                    var hit = hits[0];
                    var hitPoint = hit.Position;
                    MousePositionLabel.Text = $"X: {hitPoint.X:F2}, Y: {hitPoint.Y:F2}, Z: {hitPoint.Z:F2}";

                    // Bu noktayı bir etikette, tooltip’te ya da 3D göstericiyle görselleştirebilirsin
                    Console.WriteLine($"Mouse üzerinde: X={hitPoint.X:F2}, Y={hitPoint.Y:F2}, Z={hitPoint.Z:F2}");
                }
               
             
            };

        }

        private void LoadDxf_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void DxfListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            DxfHelper.ReadDxfFile(helixViewport3D, dxfListBox.SelectedItem.ToString());
            DxfHelper.AddLightingAndEnvironment(helixViewport3D);
        }
    }
}
