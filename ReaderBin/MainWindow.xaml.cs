using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using File = System.IO.File;

namespace ReaderBin
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string pathToCurrentDirectory = Environment.CurrentDirectory;

        private BindingList<ListModel> dataD = new BindingList<ListModel>() { };
        private BindingList<ListModel> dataB = new BindingList<ListModel>() { };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void checkerFavoriteFiles()
        {
            string pathToLogs = pathToCurrentDirectory + "\\log.txt";
            if (!File.Exists(pathToLogs))
            { 
                try
                {
                    File.Create(pathToLogs);
                }
                catch
                {
                    MessageBox.Show($"Не удалось создать файл для записи логов! ({pathToLogs})");
                }
            }

            string pathToConfig = pathToCurrentDirectory + "\\config.cfg";
            if (!File.Exists(pathToConfig))
            {
                try
                {
                    File.Create(pathToConfig);
                    logsWriter($"Файл создан: {pathToConfig}");
                }
                catch
                {
                    MessageBox.Show($"Не удалось создать файл конфига! ({pathToConfig})");
                }
            }
        }

        private void TextBox_TextChangedB(object sender, TextChangedEventArgs e)
        {
            // Только цифры, 9 символов, увеличивать в ширину а не в высоту
            BindingList <ListModel> tmp = new BindingList<ListModel>();
            string input = inputFindB.Text;
            if(input == "") { 
                if(dataB.Count > 0) 
                    dgPassList.ItemsSource = dataB;  
                return; 
            }
            for(int i = 0; i < dataB.Count; i++)
            {  
                if(dataB[i].Card.Contains(input))
                {
                    tmp.Add(dataB[i]);
                }
            }
            dgPassList.ItemsSource = tmp;
        }

        private void TextBox_TextChangedD(object sender, TextChangedEventArgs e)
        {
            BindingList<ListModel> tmp = new BindingList<ListModel>();
            string input = inputFindD.Text;
            if (input == "")
            {
                if (dataD.Count > 0)
                    dgPassListTwo.ItemsSource = dataD;
                return;
            }
            for (int i = 0; i < dataD.Count; i++)
            {
                if (dataD[i].Card.Contains(input))
                {
                    tmp.Add(dataD[i]);
                }
            }
            dgPassListTwo.ItemsSource = tmp;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            checkerFavoriteFiles();
            mainWork();
        }

        private void mainWork()
        {
            string[] paths = readConfigFileAndReturnPathsToFiles();
            logsWriter("Start Program");

            if (paths != null)
            {
                if (paths.Count() == 1)
                {
                    dataB = readFile(paths[0]);
                    logsWriter($"ОШИБКА: найден только один файл: {paths[0]}");
                    MessageBox.Show("Найден только 1 файл");
                }
                if (paths.Count() == 2)
                {
                    fillListFirst(readFile(paths[0]));
                    fillListSecond(readFile(paths[1]));
                    logsWriter($"Файлы загружены успешно: 1 - {paths[0]}; 2 - {paths[1]}");
                }
            } else
            {
                logsWriter($"ОШИБКА: Файл с конфигом пуст!!!");
                MessageBox.Show("Путь до папки с файлами не задан!");
            }
        }

        private void fillListFirst(BindingList<ListModel> list)
        {
            dataB.Clear();
            dataB = list;
            dgPassList.ItemsSource = dataB;
        }

        private void fillListSecond(BindingList<ListModel> list)
        {
            dataD.Clear();
            dataD = list;
            dgPassListTwo.ItemsSource = dataD;
        }

        private String[] readConfigFileAndReturnPathsToFiles()
        {
            try
            {
                string pathToFileFirst = "";
                string pathToFileSecond = "";

                string filePathContent = "";

                try
                {
                    filePathContent = File.ReadAllText(pathToCurrentDirectory + "\\config.cfg");
                } catch
                {
                    MessageBox.Show($"Не удалось прочитать файл конфига: {pathToCurrentDirectory + "\\config.cfg"}");
                    logsWriter($"ОШИБКА: Не удалось прочитать файл конфига: {pathToCurrentDirectory}\\config.cfg");
                }
                string[] filesInPath = Directory.GetFiles(filePathContent);

                int counterB = 0;
                int counterD = 0;

                for(int i = 0; i < filesInPath.Length; i++)
                {
                    if(filesInPath[i].Contains("b.scn"))
                    {
                        pathToFileFirst = filesInPath[i];
                        string[] nameFile = filesInPath[i].Split('\\');
                        pathToFileB.Text = nameFile[nameFile.Length - 1];
                        counterB++;
                    }
                    if (filesInPath[i].Contains("d.scn"))
                    {
                        pathToFileSecond = filesInPath[i];
                        string[] nameFile = filesInPath[i].Split('\\');
                        pathToFileD.Text = nameFile[nameFile.Length - 1];
                        counterD++;
                    }
                }

                if(counterB > 1)
                {
                    MessageBox.Show("Файлов типа SCNB найдено более 1!");
                    logsWriter($"ОШИБКА: Файлов типа SCNB найдено более 1!");
                }
                if(counterD > 1)
                {
                    MessageBox.Show("Файлов типа SCND найдено более 1!");
                    logsWriter($"ОШИБКА: Файлов типа SCND найдено более 1!");
                }
                string[] res = { pathToFileFirst, pathToFileSecond }; 

                return res;
            }
            catch
            {
                return null;
            }
        }


        private BindingList<ListModel> readFile(string pathToFile)
        {
            if(pathToFile == null || pathToFile == "")
            {
                MessageBox.Show($"Файл не найден {pathToFile}");
                return null;
            }
            try
            {
                BindingList<ListModel> list = new BindingList<ListModel>();

                FileStream myStream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
                BinaryReader myReader = new BinaryReader(myStream);
                
                int id = 1;
                while (myReader.BaseStream.Position < myReader.BaseStream.Length - 8)
                {
                    byte[] d = myReader.ReadBytes(4);
                    int a1 = Decimal.ToByte(d[0]);
                    int a2 = Decimal.ToByte(d[1]);
                    int a3 = Decimal.ToByte(d[2]);
                    int a4 = Decimal.ToByte(d[3]);
                    int sum = ((256 * 256 * 256 * a1) + 256 * 256 * a2 + 256 * a3 + a4);
                    list.Add(new ListModel()
                    {
                        Id = id.ToString(),
                        Card = addNullToStartCardNumber(sum.ToString())
                    });
                    id++;
                    //Console.WriteLine($"{sum}");
                    //Console.WriteLine($"\t {d[0]} {d[1]} {d[2]} {d[3]}");
                }
                return list;
            }
            catch
            {
                MessageBox.Show($"Ошибка чтения файла {pathToFile}");
                logsWriter($"ОШИБКА: Не удалось прочитать файл - {pathToFile}");
                return null;
            }
        }

        private string addNullToStartCardNumber(string card)
        {
            while(card.Length < 10)
            {
                card = card.Insert(0, "0");
            }
            return card;
        }

        private void logsWriter(string log, bool isTime = true)
        {
            try
            {
                if (isTime)
                {
                    File.AppendAllText(pathToCurrentDirectory + "\\log.txt", $"[{DateTime.Now}] {log}\n");
                }
                else
                {
                    File.AppendAllText(pathToCurrentDirectory + "\\log.txt", log + "\n");
                }
            }
            catch
            {
                MessageBox.Show("Непредвиденная ошибка при записи лога. Файл удален/перемещен или нет прав на чтения файлов!");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            logsWriter("EXIT PROGRAM!");
            logsWriter("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -", false);
        }

        private void inputFindB_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private void inputFindD_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            checkerFavoriteFiles();
            mainWork();
            Title = "ReaderBin. Last update: " + DateTime.Now.ToString("HH:mm:ss tt");
        }
    }
}
