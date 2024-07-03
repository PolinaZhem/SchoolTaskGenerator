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
        private List<TextBox> textBoxes_tasks = new List<TextBox>();
        private List<TextBox> textBoxes_answers = new List<TextBox>();

        private int count { get; set; }
        public Task()
        {
            InitializeComponent();
        }
        public void SetFormula(string f)
        {
            formula = f;
        }
        public string MakeTaskAndAnswer(Random rand, int variant)
        {
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

            double result = 0;
            try
            {
                result = solver.Calculate();
            }
            catch (Exception e)
            {
                string s = e.Message;
                s += "\r\n";
                foreach (var v in formula_vars)
                    s += v.Key + "=" + v.Value + "; ";
                return s;
            }
                

            var rd = new RowDefinition();
            rd.MinHeight = 38;
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
            delete_button.Click += DeleteTaskButton_Click;
            Grid.SetRow(delete_button, ExerciseGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(delete_button, 4);
            ExerciseGrid.Children.Add(delete_button);

            return "";
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int index = Grid.GetRow(button);

            var result = MessageBox.Show($"Вы точно хотите удалить строку {index}?", "Вы нажали Удалить задачу", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                ExerciseGrid.Children.Remove(button);
                TextBox t1 = new TextBox();
                t1 = textBoxes_tasks[index - 1];
                ExerciseGrid.Children.Remove(t1);
                t1 = textBoxes_answers[index - 1];
                ExerciseGrid.Children.Remove(t1);
                ExerciseGrid.RowDefinitions.RemoveAt(index);

                textBoxes_tasks.Remove(textBoxes_tasks[index - 1]);
                textBoxes_answers.Remove(textBoxes_answers[index - 1]);

                Label labelToDelete = null;

                foreach(UIElement child in ExerciseGrid.Children)
                {
                    int row = Grid.GetRow(child);
                    if (row > index && !(child is Label))
                        Grid.SetRow(child, row-1);
                    if (child is Label && row == this.count)
                        labelToDelete = child as Label;
                }

                ExerciseGrid.Children.Remove(labelToDelete);

                Grid.SetRowSpan(TaskSplitter, count);
                this.count--;
            }
            // удалить текстбоксы из чайлдов грида
            // удалить текстбоксы из списков текстбоксов (-1 индекс)
            // удалить кнопку из чайлдов грида
            // удалить RowDefinition из грида
            // сократить rowspan сплиттера
        }

        private void CountButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Int32.TryParse(CountTextBox.Text, out var count))
            {
                string error = "Введите целое число.";
                MessageBox.Show(error);
                CountTextBox.Text = "";
                return;
            }

            List<UIElement> gridElements = new List<UIElement>();
            // удаление всех строк грида кроме первой(нулевой)
            foreach (UIElement x in ExerciseGrid.Children)
            {
                int row = Grid.GetRow(x);
                if (row == 0)
                    gridElements.Add(x);
            }
            ExerciseGrid.Children.Clear();
            foreach (UIElement x in gridElements)
                ExerciseGrid.Children.Add(x);
            gridElements.Clear();

            textBoxes_tasks.Clear();
            textBoxes_answers.Clear();

            if (ExerciseGrid.RowDefinitions.Count() > 1)
                ExerciseGrid.RowDefinitions.RemoveRange(1, ExerciseGrid.RowDefinitions.Count() - 1);
            // в последней строке после этого не тот номер варианта (не удаляется предыдущий)

            Random rand = new Random();
            this.count = count;
            for (int j = 0; j < count; j++)
            {
                int max_tries = 10;
                int tries = 0;
                string error = "";
                while (tries < max_tries)
                {
                    string s = MakeTaskAndAnswer(rand, j + 1);
                    if (s == "")
                        break;
                    error += s + "\r\n";
                    tries++;
                }
                if (tries >= max_tries)
                {
                    MessageBox.Show("Не получилось создать задачи. В вычислениях формулы слишком часто возникают ошибки:\r\n" + error);
                    this.count = j;
                    break;
                }
            }
            Grid.SetRowSpan(TaskSplitter, count + 1);
            //string s = "";
            //foreach (var task in textBoxes_tasks)
            //{
            //    s += task.Text;
            //}
            //MessageBox.Show(s);
            SaveButton.IsEnabled = true;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Текстовые файлы|*.txt";
            dialog.Title = "Сохранение задачи";
            if (dialog.ShowDialog() != true)
                return;

            string answersfilename = dialog.FileName;
            int index = answersfilename.LastIndexOf(".");
            string type = answersfilename.Substring(index);

            answersfilename = answersfilename.Substring(0, answersfilename.LastIndexOf("."));
            answersfilename += "_ответы" + type;
            FileStream fs = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                for (int i = 0; i < count; i++)
                {
                    sw.WriteLine($"Вариант {i + 1}. ");
                    sw.WriteLine(textBoxes_tasks[i].Text + "\r\n\r\n");
                }
            }
            fs.Close();

            FileStream fs2 = new FileStream(answersfilename, FileMode.Create, FileAccess.Write);
            using (StreamWriter sw = new StreamWriter(fs2))
            {
                for (int i = 0; i < count; i++)
                {
                    sw.WriteLine($"Вариант {i + 1}. ");
                    sw.WriteLine(textBoxes_answers[i].Text + "\r\n\r\n");
                }
            }
            fs2.Close();
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
