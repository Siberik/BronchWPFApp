using FellowOakDicom;
using FellowOakDicom.Imaging;
using HelixToolkit.Wpf;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BronchWPFApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupViewport();
        }

        private void SetupViewport()
        {
            // Используем DefaultLights для автоматического добавления света
            viewPort.Children.Add(new DefaultLights());

            // Настроим камеру
            PerspectiveCamera camera = new PerspectiveCamera
            {
                Position = new Point3D(0, 0, 5),
                LookDirection = new Vector3D(0, 0, -1),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 60
            };
            viewPort.Camera = camera;
        }

        private void LoadDicomFiles(string folderPath)
        {
            var dicomFiles = Directory.GetFiles(folderPath, "*.dcm", SearchOption.AllDirectories);

            if (dicomFiles.Any())
            {
                viewPort.Children.Clear();

                var modelGroup = new Model3DGroup();

                foreach (var filePath in dicomFiles)
                {
                    var dicomFile = DicomFile.Open(filePath);
                    var image = new DicomImage(dicomFile.Dataset);

                    var meshBuilder = new MeshBuilder();
                    meshBuilder.AddBox(new Point3D(0, 0, 0), image.Width, image.Height, 1);

                    var material = new DiffuseMaterial(Brushes.Blue); // Замените на свой материал

                    var geometryModel3D = new GeometryModel3D
                    {
                        Geometry = meshBuilder.ToMesh(),
                        Material = material
                    };

                    modelGroup.Children.Add(geometryModel3D);
                }

                var modelVisual3D = new ModelVisual3D { Content = modelGroup };
                viewPort.Children.Add(modelVisual3D);
            }
            else
            {
                MessageBox.Show("В выбранной папке нет файлов DICOM.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = dialog.SelectedPath;
                LoadDicomFiles(folderPath);
            }
        }
    }
}
