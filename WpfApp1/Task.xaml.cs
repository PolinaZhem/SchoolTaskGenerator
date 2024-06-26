﻿using Microsoft.Win32;
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
            string error_text = "";
            bool is_error = false;
            double result = 0;
            try
            {
                result = solver.Calculate();
            }
            catch (ArgumentException e)
            {
                error_text = e.Message;
                is_error = true;
            }
            if (is_error)
                MessageBox.Show(error_text);
            //double result = solver.Calculate();
            //AnswersTextBox.Text += result.ToString() + "\r\n\r\n";

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
            Grid.SetRow(delete_button, ExerciseGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(delete_button, 4);
            ExerciseGrid.Children.Add(delete_button);
            delete_button.Click += DeleteTaskButton_Click;
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int index = Grid.GetRow(button);

            var result = MessageBox.Show($"Вы точно хотите удалить строку {index}?", "Вы нажали Удалить задачу", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                //ExerciseGrid.Children.Remove(button);
                //ExerciseGrid.Children.RemoveAt(index);
                TextBox t1 = new TextBox();
                t1 = textBoxes_tasks[index - 1];
                ExerciseGrid.Children.Remove(t1);
                t1 = textBoxes_answers[index - 1];
                ExerciseGrid.Children.Remove(t1);
                //ExerciseGrid.RowDefinitions.Remove(ExerciseGrid.RowDefinitions[index]);
                
                
                textBoxes_tasks.Remove(textBoxes_tasks[index-1]);
                textBoxes_answers.Remove(textBoxes_answers[index-1]);
                string s = "";
                foreach (var x in textBoxes_tasks)
                {
                    s += x.Text + "|";
                }
                MessageBox.Show(s);

                Grid.SetRowSpan(TaskSplitter, count);
                this.count--;
                //правильно удаляется только предпоследняя строка
            }
            // удалить текстбоксы из чайлдов грида
            // удалить текстбоксы из списков текстбоксов (-1 индекс)
            // удалить кнопку из чайлдов грида
            // удалить RowDefinition из грида
            // сократить rowspan сплиттера
        }

        private void CountButton_Click(object sender, RoutedEventArgs e)
        {
            // удаление всех строк грида кроме первой(нулевой)
            int rows = ExerciseGrid.RowDefinitions.Count();
            for (int i = rows-1; i > 0; i--)
            {
                ExerciseGrid.RowDefinitions.Remove(ExerciseGrid.RowDefinitions[i]);
            }
            // в последней строке после этого не тот номер варианта (не удаляется предыдущий)

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
                //string s = "";
                //foreach (var task in textBoxes_tasks)
                //{
                //    s += task.Text;
                //}
                //MessageBox.Show(s);
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

            string answersfilename = dialog.FileName;
            string type = "";
            int index = answersfilename.LastIndexOf(".");
            for (int i = index; i < answersfilename.Length; i++)
            {
                type += answersfilename[i];
            }
            answersfilename = answersfilename.Substring(0, answersfilename.LastIndexOf("."));
            answersfilename += "_answers" + type;
            FileStream fs = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write);
            FileStream fs2 = new FileStream(answersfilename, FileMode.Create, FileAccess.Write);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                for (int i = 0; i < count; i++)
                {
                    sw.WriteLine($"Вариант {i+1}. ");
                    sw.WriteLine(textBoxes_tasks[i].Text + "\r\n\r\n");
                }
            }
            fs.Close();
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
