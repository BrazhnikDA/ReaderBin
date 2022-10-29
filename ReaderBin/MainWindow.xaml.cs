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
        // Путь то директории где запущена приложение
        string pathToCurrentDirectory = Environment.CurrentDirectory;
        // Создание списков для хранения данных отображаемых в таблицых
        private BindingList<ListModel> dataD = new BindingList<ListModel>() { };
        private BindingList<ListModel> dataB = new BindingList<ListModel>() { };
        private BindingList<ListModel> dataPCLB = new BindingList<ListModel>() { };

        public MainWindow()
        {
            InitializeComponent();
        }

        // Проверка на существование текстового файла с логами и фалйа конфигурации, если не созданы создать их
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

        // Проверка ввода символов в поисковик
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Только цифры, 9 символов, увеличивать в ширину а не в высоту
           
            string input = inputFind.Text;
            if(input == "") { 
                if(dataB.Count > 0)
                    dgPassList.ClearDetailsVisibilityForItem(dataB);
                    dgPassList.ItemsSource = dataB;
                if (dataD.Count > 0)
                    dgPassListTwo.ClearDetailsVisibilityForItem(dataD);
                    dgPassListTwo.ItemsSource = dataD;
                if (dataPCLB.Count > 0)
                    dgPassListThird.ClearDetailsVisibilityForItem(dataPCLB);
                    dgPassListThird.ItemsSource = dataPCLB;
                return; 
            }
            findCard(input);
        }

        private void findCard(string input)
        {
            BindingList<ListModel> tmp = new BindingList<ListModel>();
            BindingList<ListModel> tmp2 = new BindingList<ListModel>();
            BindingList<ListModel> tmp3 = new BindingList<ListModel>();


            for (int i = 0; i < dataB.Count; i++)
            {
                if (dataB[i].Card.Contains(input))
                {
                    tmp.Add(dataB[i]);
                }
            }
            dgPassList.ClearDetailsVisibilityForItem(tmp);
            dgPassList.ItemsSource = tmp;

            for (int i = 0; i < dataD.Count; i++)
            {
                if (dataD[i].Card.Contains(input))
                {
                    tmp2.Add(dataD[i]);
                }
            }
            dgPassListTwo.ClearDetailsVisibilityForItem(tmp2);
            dgPassListTwo.ItemsSource = tmp2;

            for (int i = 0; i < dataPCLB.Count; i++)
            {
                if (dataPCLB[i].Card.Contains(input))
                {
                    tmp3.Add(dataPCLB[i]);
                }
            }
            dgPassListThird.ClearDetailsVisibilityForItem(tmp3);
            dgPassListThird.ItemsSource = tmp3;
        }


        // Если окно загружено, вызов функций
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            checkerFavoriteFiles();
            mainWork();
        }

        // Функция вызывающая бизнес логику по порядку
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
                    logsWriter($"Ошибка найдено 2 файда. Файлы загружены успешно: 1 - {paths[0]}; 2 - {paths[1]}");
                }
                if (paths.Count() == 3)
                {
                    fillListFirst(readFile(paths[0]));
                    fillListSecond(readFile(paths[1]));
                    fillListThird(readFilePCLB(paths[2]));
                    logsWriter($"Файлы загружены успешно: 1 - {paths[0]}; 2 - {paths[1]}; 3- {paths[2]}");
                }
            } else
            {
                logsWriter($"ОШИБКА: Файл с конфигом пуст!!!");
                MessageBox.Show("Путь до папки с файлами не задан!");
            }
        }

        // Заполнение первой таблицы
        private void fillListFirst(BindingList<ListModel> list)
        {
            if (list != null)
            {
                dataB.Clear();
                dgPassList.ClearDetailsVisibilityForItem(list);
                dataB = list;
                dgPassList.ItemsSource = dataB;
            }
        }

        // Заполнение второй таблицы
        private void fillListSecond(BindingList<ListModel> list)
        {
            if (list != null)
            {
                dataD.Clear();
                dgPassListTwo.ClearDetailsVisibilityForItem(list);
                dataD = list;
                dgPassListTwo.ItemsSource = dataD;
            }
        }

        // Заполнение третьей таблицы
        private void fillListThird(BindingList<ListModel> list)
        {
            if (list != null)
            {
                dataPCLB.Clear();
                dgPassListThird.ClearDetailsVisibilityForItem(list);
                dataPCLB = list;
                dgPassListThird.ItemsSource = dataPCLB;
            }
        }

        // Чтение конфигурационного файла и возвращаения пути указанного внутри файла
        private String[] readConfigFileAndReturnPathsToFiles()
        {
            try
            {
                string pathToFileFirst = "";
                string pathToFileSecond = "";
                string pathToFileThird = "";

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
                int counterPCLB = 0;

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
                    if (filesInPath[i].Contains("PCLB"))
                    {
                        pathToFileThird = filesInPath[i];
                        string[] nameFile = filesInPath[i].Split('\\');
                        pathToFilePCLB.Text = nameFile[nameFile.Length - 1];
                        counterPCLB++;
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
                if (counterPCLB > 1)
                {
                    MessageBox.Show("Файлов типа PCLB найдено более 1!");
                    logsWriter($"ОШИБКА: Файлов типа PCLB найдено более 1!");
                }
                string[] res = { pathToFileFirst, pathToFileSecond, pathToFileThird}; 

                return res;
            }
            catch
            {
                return null;
            }
        }

        // Чтение файлов из папки типа SCNB/SCND - файл каждого типа всегда 1!
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

        private BindingList<ListModel> readFilePCLB(string pathToFile)
        {
            if (pathToFile == null || pathToFile == "")
            {
                MessageBox.Show($"Файл не найден {pathToFile}");
                return null;
            }
            try
            {
                BindingList<ListModel> list = new BindingList<ListModel>();

                FileStream myStream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
                BinaryReader myReader = new BinaryReader(myStream);

                myReader.ReadBytes(9);

                string val = "";
                int sum = 0;
                int id = 0;
                int counter = 1;
                while (myReader.BaseStream.Position < myReader.BaseStream.Length)
                {
                    if (id % 2 == 0)
                    {
                        byte[] d = myReader.ReadBytes(4);
                        int a1 = Decimal.ToByte(d[0]);
                        int a2 = Decimal.ToByte(d[1]);
                        int a3 = Decimal.ToByte(d[2]);
                        int a4 = Decimal.ToByte(d[3]);
                        sum = ((256 * 256 * 256 * a1) + 256 * 256 * a2 + 256 * a3 + a4);
                        val = addNullToStartCardNumber(sum.ToString());
                    }else
                    {
                        byte[] d = myReader.ReadBytes(3);
                        int a1 = Decimal.ToByte(d[0]);
                        int a2 = Decimal.ToByte(d[1]);
                        int a3 = Decimal.ToByte(d[2]);
                        list.Add(new ListModel()
                        {
                            Id = counter.ToString(),
                            Card = val + " " + addNullToDate(a1.ToString()) + "." + addNullToDate(a2.ToString()) + "." + a3.ToString()
                        });
                        counter++;
                    }
                    sum = 0;
                    id++;
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

        private string addNullToDate(string data)
        {
            if(data.Length == 1)
            {
                return "0" + data;
            } else
            {
                return data;
            }
        }

        // Добавление нулей в начало номера карты до 10 символов
        private string addNullToStartCardNumber(string card)
        {
            while(card.Length < 10)
            {
                card = card.Insert(0, "0");
            }
            return card;
        }

        // Функция для записи логов
        // log - текст лога
        // isTime - Записать время лога? true = да/false = нет; По умолчанию время записывается при передаче одного параметра в функцию
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

        // Записать в лог сообщение о закрытии программы
        private void Window_Closed(object sender, EventArgs e)
        {
            logsWriter("EXIT PROGRAM!");
            logsWriter("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -", false);
        }
        
        // Проверка на то что в поиск (для первой таблицы) вводится ТОЛЬКО ЦИФРЫ
        private void inputFindB_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        // Проверка на то что в поиск (для второй таблицы) вводится ТОЛЬКО ЦИФРЫ
        private void inputFindD_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        // Обработка нажатия на кнопку обновить
        // Выполняет заново весь функционал
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mainWork();
            if(inputFind.Text.Length > 0)
            {
                findCard(inputFind.Text);
            }
            Title = "ReaderBin. Last update: " + DateTime.Now.ToString("HH:mm:ss tt");
        }
    }
}
