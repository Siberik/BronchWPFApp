using FellowOakDicom;
using HelixToolkit.Wpf;
using System;
using System.IO;
using System.Linq;
using System.Windows;
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
            // Добавьте настройку камеры, света или другие параметры отображения, если необходимо
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
                    var dicomImage = dicomFile.Dataset;

                    var modelVisual3D = CreateModelFromImage(dicomImage);
                    viewPort.Children.Add(modelVisual3D);
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

      private bool successMessageShown = false;

private ModelVisual3D CreateModelFromImage(DicomDataset dicomImage)
{
    try
    {
        var meshBuilder = new MeshBuilder();

        // Ваш код для создания геометрии MeshBuilder на основе dicomImage
        // Например, добавьте слои изображения в meshBuilder

        var geometry = meshBuilder.ToMesh();
        var material = new DiffuseMaterial(Brushes.Blue); // Замените на свой материал

        var model = new GeometryModel3D
        {
            Geometry = geometry,
            Material = material
        };

        if (!successMessageShown)
        {
            MessageBox.Show($"DICOM-файл успешно обработан: {dicomImage.GetValues<string>(DicomTag.PatientName)}");
            successMessageShown = true;
        }

        return new ModelVisual3D { Content = model };
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Ошибка при обработке DICOM-файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        return null;
    }
}






    }
}
