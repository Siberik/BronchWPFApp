using FellowOakDicom;
using HelixToolkit.Wpf;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Threading;
using SharpDX;

namespace BronchWPFApp
{
    public partial class MainWindow : Window
    {
        private bool successMessageShown = false;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            SetupViewport();

            // Инициализация таймера для очистки ресурсов каждые 10 секунд
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void SetupViewport()
        {
            // Добавьте настройку камеры, света или другие параметры отображения, если необходимо
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Очистка ресурсов
            viewPort.Children.Clear();
        }

        private void LoadDicomFiles(string folderPath)
        {
            // Очищаем вьюпорт перед загрузкой новых данных
            viewPort.Children.Clear();

            var dicomFiles = Directory.GetFiles(folderPath, "*.dcm", SearchOption.AllDirectories);

            if (dicomFiles.Any())
            {
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

        private ModelVisual3D CreateModelFromImage(DicomDataset dicomImage)
        {
            try
            {
                var modelGroup = new Model3DGroup();

                // Получаем данные изображения
                var rows = dicomImage.GetValue<int>(DicomTag.Rows, 0);
                var columns = dicomImage.GetValue<int>(DicomTag.Columns, 0);
                var pixelSpacingX = dicomImage.GetValue<double>(DicomTag.PixelSpacing, 0);
                var pixelSpacingY = dicomImage.GetValue<double>(DicomTag.PixelSpacing, 1);

                // Создаем единичный параллелепипед для представления DICOM-изображения
                double width = columns * pixelSpacingX;
                double height = rows * pixelSpacingY;
                double depth = ushort.MaxValue / 1000.0; // Значение, представляющее максимальное значение пикселя

                var meshBuilder = new MeshBuilder();
                meshBuilder.AddBox(new Rect3D(0, 0, 0, width, height, depth));

                // Создаем поверхность
                var surfaceModel = new GeometryModel3D
                {
                    Geometry = meshBuilder.ToMesh(),
                    Material = new DiffuseMaterial(Brushes.Blue)
                };

                modelGroup.Children.Add(surfaceModel);

                Console.WriteLine("Creating ModelVisual3D...");
                var modelVisual3D = new ModelVisual3D { Content = modelGroup };
                Console.WriteLine("ModelVisual3D created.");

                if (!successMessageShown)
                {
                    MessageBox.Show($"DICOM-файл успешно обработан: {dicomImage.GetSingleValue<string>(DicomTag.PatientName)}");
                    successMessageShown = true;
                }

                return modelVisual3D;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateModelFromImage: {ex}");
                MessageBox.Show($"Ошибка при обработке DICOM-файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
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
