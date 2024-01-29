using FellowOakDicom;
using HelixToolkit.Wpf;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

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
                var pixels = dicomImage.GetValues<ushort>(DicomTag.PixelData);
                var rows = dicomImage.GetValue<int>(DicomTag.Rows, 0);
                var columns = dicomImage.GetValue<int>(DicomTag.Columns, 0);
                var pixelSpacingX = dicomImage.GetValue<double>(DicomTag.PixelSpacing, 0);
                var pixelSpacingY = dicomImage.GetValue<double>(DicomTag.PixelSpacing, 1);
                var sliceThickness = dicomImage.GetValue<double>(DicomTag.SliceThickness, 0);

                int groupSize = 5;

                for (int x = 0; x < columns; x += groupSize)
                {
                    for (int y = 0; y < rows; y += groupSize)
                    {
                        double px = x * pixelSpacingX;
                        double py = y * pixelSpacingY;
                        double pz = sliceThickness;

                        double averageHeight = 0;

                        for (int i = 0; i < groupSize && (x + i) < columns; i++)
                        {
                            for (int j = 0; j < groupSize && (y + j) < rows; j++)
                            {
                                averageHeight += pixels[(y + j) * columns + (x + i)] / 1000.0;
                            }
                        }

                        averageHeight /= groupSize * groupSize;

                        // Создаем куб
                        var cubeGeometry = new MeshGeometry3D
                        {
                            Positions = new Point3DCollection
                            {
                                new Point3D(px, py, pz),
                                new Point3D(px + groupSize * pixelSpacingX, py, pz),
                                new Point3D(px, py + groupSize * pixelSpacingY, pz),
                                new Point3D(px + groupSize * pixelSpacingX, py + groupSize * pixelSpacingY, pz)
                            },
                            TriangleIndices = new Int32Collection
                            {
                                0, 1, 2,
                                1, 3, 2
                            }
                        };

                        var cubeModel = new GeometryModel3D
                        {
                            Geometry = cubeGeometry,
                            Material = new DiffuseMaterial(Brushes.Blue)
                        };

                        modelGroup.Children.Add(cubeModel);
                    }
                }

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
