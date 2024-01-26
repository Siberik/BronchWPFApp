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
        private bool successMessageShown = false;

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
                    if (modelVisual3D != null)
                    {
                        viewPort.Children.Add(modelVisual3D);
                    }
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
            try
            {
                var meshBuilder = new MeshBuilder();

                // Получаем данные изображения
                var pixels = dicomImage.GetValues<ushort>(DicomTag.PixelData);
                if (pixels == null || pixels.Length == 0)
                {
                    Console.WriteLine("Пиксельные данные отсутствуют.");
                    return null;
                }

                var rows = dicomImage.GetValue<int>(DicomTag.Rows, 0);
                var columns = dicomImage.GetValue<int>(DicomTag.Columns, 0);
                var pixelSpacingX = dicomImage.GetValue<double>(DicomTag.PixelSpacing, 0);
                var pixelSpacingY = dicomImage.GetValue<double>(DicomTag.PixelSpacing, 1);
                var sliceThickness = dicomImage.GetValue<double>(DicomTag.SliceThickness, 0);

                // Log
                Console.WriteLine($"Rows: {rows}, Columns: {columns}");
                Console.WriteLine($"PixelSpacingX: {pixelSpacingX}, PixelSpacingY: {pixelSpacingY}");
                Console.WriteLine($"SliceThickness: {sliceThickness}");

                // Преобразуем пиксели в вершины MeshBuilder
                // Преобразуем пиксели в вершины MeshBuilder
                for (int x = 0; x < columns; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        // Рассчитываем координаты точек на основе пиксельных данных и их расположения
                        double px = x * pixelSpacingX;
                        double py = y * pixelSpacingY;
                        double pz = sliceThickness;

                        // Используем значения пикселей для определения высоты точек
                        double height = pixels[y * columns + x] / 1000.0; // Пример: масштабирование значений пикселей

                        // Добавляем вершину в MeshBuilder
                        meshBuilder.AddBox(new Point3D(px, py, pz), pixelSpacingX, pixelSpacingY, height);
                    }
                }


                // Log
                Console.WriteLine($"MeshBuilder vertices count: {meshBuilder.Positions.Count}");

                // Создаем Mesh
                var geometry = meshBuilder.ToMesh();
                var material = new DiffuseMaterial(Brushes.Blue); // Замените на свой материал

                var model = new GeometryModel3D
                {
                    Geometry = geometry,
                    Material = material
                };

                if (!successMessageShown)
                {
                    MessageBox.Show($"DICOM-файл успешно обработан: {dicomImage.GetSingleValue<string>(DicomTag.PatientName)}");
                    successMessageShown = true;
                }

                return new ModelVisual3D { Content = model };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateModelFromImage: {ex}");
                MessageBox.Show($"Ошибка при обработке DICOM-файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }



    }
}
