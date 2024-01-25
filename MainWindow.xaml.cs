using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using FellowOakDicom.Imaging;
using FellowOakDicom;

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
            // Добавьте настройку камеры, света или другие параметры отображения, если необходимо
        }

        private void LoadDicomFiles(string folderPath)
        {
            var dicomFiles = Directory.GetFiles(folderPath, "*.dcm", SearchOption.AllDirectories);

            if (dicomFiles.Any())
            {
                viewPort.Children.Clear();

                foreach (var filePath in dicomFiles)
                {
                    var dicomImage = DicomFile.Open(filePath).Dataset;
                    var model = CreateModelFromImage(dicomImage);
                    viewPort.Children.Add(model);
                }
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

        private ModelVisual3D CreateModelFromImage(DicomDataset dicomImage)
        {
            var meshBuilder = new MeshBuilder();

            // Ваш код для создания геометрии MeshBuilder на основе dicomImage

            var geometry = meshBuilder.ToMesh();
            var material = new DiffuseMaterial(Brushes.Blue); // Замените на свой материал

            var model = new GeometryModel3D
            {
                Geometry = geometry,
                Material = material
            };

            return new ModelVisual3D { Content = model };
        }


    }
}
