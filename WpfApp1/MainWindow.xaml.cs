using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RPN_Solver solver = new RPN_Solver();
        private List<string> text_of_task = null;
        private List<VariableSetter> text_vars = null;
        private Dictionary<string, double> formula_vars = null;

        public MainWindow()
        {
            InitializeComponent();
            text_vars = new List<VariableSetter>();
            //{
            //    new VariableSetter { Name="Speed", RangeFrom=100, RangeTo=200, Step = 5, DigitsToRound = 1},
            //    new VariableSetter { Name="Time", RangeFrom=200, RangeTo=400, Step = 15, DigitsToRound = 2},
            //    new VariableSetter { Name="Dist", RangeFrom=300, RangeTo=500, Step = 25, DigitsToRound = 3}
            //};
            VariablesGrid.ItemsSource = text_vars;
        }

        private void TextAnalysisButton_Click(object sender, RoutedEventArgs e)
        { 
            string text = TaskTextBox.Text;
            text_of_task = new List<string>();
            text_vars.Clear();
            while (text.Contains("{"))
            {
                int i1 = text.IndexOf("{");
                int i2 = text.IndexOf("}");
                text_of_task.Add(text.Substring(0, i1));
                text_vars.Add(new VariableSetter { Name = text.Substring(i1 + 1, i2 - i1 - 1).ToLower(), RangeFrom = 0, RangeTo = 100, Step = 5, DigitsToRound = 1 });
                text = text.Substring(i2 + 1);
            }
            text_of_task.Add(text);

            //string t = "";
            //foreach (var i in text_of_task)
            //{
            //    t += "|" + i + "|";
            //}
            //t += "\r\n";
            //foreach (var i in text_vars)
            //{
            //    t += "|" + i.Name + "|, ";
            //}
            //TaskTextBox.Text = t;
            //MessageBox.Show(t);
            VariablesGrid.ItemsSource = text_vars;
            VariablesGrid.Items.Refresh();
        }

        private void FormulaAnalysisButton_Click(object sender, RoutedEventArgs e)
        {
            string formula = FormulaTextBox.Text;
            solver.LoadFormula(formula);
            formula_vars = solver.GetVars();

            bool everything_ok = true;

            foreach (var k in formula_vars.Keys)
            {
                if (!text_vars.Any(x => x.Name == k))
                {
                    everything_ok = false;
                    break;
                }
            }

            GenerateTaskButton.IsEnabled = everything_ok;
        }

        private Task task;
        private void GenerateTaskButton_Click(object sender, RoutedEventArgs e)
        {
            task = new Task();
            task.Solver = solver;
            task.TextOfTask = text_of_task;
            task.TextVars = text_vars;
            task.FormulaVars = formula_vars;

            var res = task.ShowDialog();
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Файлы задач (*.sctask)|*.sctask|Все файлы|*.*";
            dialog.Title = "Сохранение задачи";
            if (dialog.ShowDialog() != true)
                return;

            FileStream fs = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(TaskTextBox.Text);
                sw.WriteLine(FormulaTextBox.Text);
                sw.WriteLine(VariablesGrid.Items.Count);
                foreach (VariableSetter item in VariablesGrid.Items)
                {
                    sw.WriteLine(item.ToString());
                }
            }
            fs.Close();
        }
        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Файлы задач (*.sctask)|*.sctask|Все файлы|*.*";
            dialog.Title = "Открытие задачи";
            if (dialog.ShowDialog() != true)
                return;

            FileStream fc = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read);
            using (StreamReader sr = new StreamReader(fc))
            {
                TaskTextBox.Text = sr.ReadLine();
                FormulaTextBox.Text = sr.ReadLine();
                int n = Int32.Parse(sr.ReadLine());
                TextAnalysisButton_Click(null, null);
                for (int i = 0; i < n; i++)
                {
                    string t = sr.ReadLine();
                    var text = t.Split(' ');
                    var v = text_vars.Find(x => x.Name == text[0]);

                    //VariableSetter variableSetter = new VariableSetter();
                    //variableSetter.Name = text[0];
                    v.RangeFrom = Int32.Parse(text[1]);
                    v.RangeTo = Int32.Parse(text[2]);
                    v.Step = Int32.Parse(text[3]);
                    v.DigitsToRound = Int32.Parse(text[4]);
                }
                VariablesGrid.Items.Refresh();
            }
        }
        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {
            TaskTextBox.Clear();
            FormulaTextBox.Clear();
            text_vars.Clear();
            VariablesGrid.ItemsSource = text_vars;
            VariablesGrid.Items.Refresh();
        }
        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
