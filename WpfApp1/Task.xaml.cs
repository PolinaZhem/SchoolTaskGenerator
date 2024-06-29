using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для Task.xaml
    /// </summary>
    public partial class Task : Window
    {
        private RPN_Solver solver;
        public RPN_Solver Solver
        {
            get => solver;
            set => solver = value;
        }
        private List<string> text_of_task = null;
        public List<string> TextOfTask
        {
            get => text_of_task;
            set => text_of_task = value;
        }
        private List<VariableSetter> text_vars = null;
        public List<VariableSetter> TextVars
        {
            get => text_vars;
            set => text_vars = value;
        }
        private Dictionary<string, double> formula_vars = null;
        public Dictionary<string, double> FormulaVars
        {
            get => formula_vars;
            set => formula_vars = value;
        }
        private string formula = "";

        private int count { get; set; }
        public Task()
        {
            InitializeComponent();
        }
        public void SetFormula(string f)
        {
            formula = f;
        }
        public void MakeTaskAndAnswer(Random rand, int variant)
        {
           // solver.LoadFormula(formula);
            foreach (var v in text_vars)
            {
                int diff = ((int)v.RangeTo - (int)v.RangeFrom) / (int)v.Step;
                int value = rand.Next(diff + 1) * (int)v.Step + (int)v.RangeFrom;
                formula_vars[v.Name] = value;
                v.Value = value;
            }

            string t = "";
            for (int i = 0; i < text_of_task.Count - 1; i++)
            {
                t += text_of_task[i];
                t += text_vars[i].Value;
            }
            t += text_of_task.Last();
            //ExerciseTextBox.Text += t;
            double result = solver.Calculate();
            //AnswersTextBox.Text += result.ToString() + "\r\n\r\n";

            List<TextBox> textBoxes_tasks = new List<TextBox>();
            List<TextBox> textBoxes_answers = new List<TextBox>();
            var rd = new RowDefinition();
            ExerciseGrid.RowDefinitions.Add(rd);

            Label number = new Label();
            number.Content = variant.ToString();
            Grid.SetRow(number, ExerciseGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(number, 0);
            ExerciseGrid.Children.Add(number);

            TextBox textt = new TextBox();
            textt.TextWrapping = TextWrapping.Wrap;
            textt.Text = t;
            textBoxes_tasks.Add(textt);
            Grid.SetRow(textt, ExerciseGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(textt, 1);
            ExerciseGrid.Children.Add(textt);

            textt = new TextBox();
            textt.TextWrapping = TextWrapping.Wrap;
            textt.Text = result.ToString();
            textBoxes_answers.Add(textt);
            Grid.SetRow(textt, ExerciseGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(textt, 3);
            ExerciseGrid.Children.Add(textt);

            Image delete_icon = new Image();
            delete_icon.Source = new BitmapImage(
                new Uri("pack://application:,,,/WpfApp1;component/icons/delete.png"));
            delete_icon.Width = 32;
            delete_icon.Height = 32;
            Button delete_button = new Button();
            delete_button.Content = delete_icon;
            var margin = delete_button.Margin;
            margin.Top = 2;
            margin.Left = 2;
            margin.Right = 2;
            margin.Bottom = 2;
            delete_button.Margin = margin;
            Grid.SetRow(delete_button, ExerciseGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(delete_button, 4);
            ExerciseGrid.Children.Add(delete_button);
            delete_button.Click += DeleteTaskButton_Click;
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int index = Grid.GetRow(button);

            MessageBox.Show($"Вы нажали удалить строку {index}");
            // удалить текстбоксы из чайлдов грида
            // удалить текстбоксы из списков текстбоксов (-1 индекс)
            // удалить кнопку из чайлдов грида
            // удалить RowDefinition из грида
            // сократить rowspan сплиттера
        }

        private void CountButton_Click(object sender, RoutedEventArgs e)
        {
            // удаление всех строк грида кроме первой(нулевой)
            Random rand = new Random();
            if (!Int32.TryParse(CountTextBox.Text, out var count))
            {
                string error = "Введите целое число.";
                MessageBox.Show(error);
                CountTextBox.Text = "";
            }
            else
            {
                this.count = Int32.Parse(CountTextBox.Text);
                for (int j = 0; j < count; j++)
                {
                    MakeTaskAndAnswer(rand, j + 1);
                }
                Grid.SetRowSpan(TaskSplitter, count+1);
                SaveButton.IsEnabled = true;
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Файлы задач (*.sctask)|*.sctask|Все файлы|*.*";
            dialog.Title = "Сохранение задачи";
            if (dialog.ShowDialog() != true)
                return;

            FileStream fs = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                for (int i = 0; i < count; i++)
                {
                    sw.WriteLine();
                    //Я не понимаю как считывать текст из определенного TextBox 
                }
            }
            fs.Close();
        }
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
