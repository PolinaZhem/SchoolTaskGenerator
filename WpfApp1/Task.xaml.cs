using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        private RPN_Solver solver = null;
        private List<string> text_of_task = null;
        private List<VariableSetter> text_vars = null;
        private Dictionary<string, double> formula_vars = null;
        private string formula = "";

        private int count { get; set; }
        public Task()
        {
            InitializeComponent();
        }
        public int Count
        {
            get { return count; }
        }
        public void SetSolver(RPN_Solver s)
        {
            solver = s;
        }
        public void SetText(List<string> t)
        {
            text_of_task = t;
        }
        public void SetVar(List<VariableSetter> v)
        {
            text_vars = v;
        }
        public void SetForVar(Dictionary<string, double> f)
        {
            formula_vars = f;
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

            string t = "Вариант " + variant.ToString() + ".";
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
            rd = new RowDefinition();
            AnswersGrid.RowDefinitions.Add(rd);
            TextBox textt = new TextBox();
            textt.Text = t;
            textBoxes_tasks.Add(textt);
            Grid.SetRow(textt, ExerciseGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(textt, 0);
            ExerciseGrid.Children.Add(textt);
            textt = new TextBox();
            textt.Text = result.ToString();
            textBoxes_answers.Add(textt);
            Grid.SetRow(textt, AnswersGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(textt, 2);
            AnswersGrid.Children.Add(textt);

        }

        private void CountButton_Click(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            this.count = CountComboBox.SelectedIndex + 1;
            for (int j = 0; j < count; j++)
            {
                MakeTaskAndAnswer(rand, j+1);
            }
            SaveButton.IsEnabled = true;
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
    }
}
