using System;
using System.Collections.Generic;
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
            ExerciseTextBox.Clear();
            AnswersTextBox.Clear();
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
        public void MakeTaskAndAnswer()
        {
            solver.LoadFormula(formula);
            Random rand = new Random();
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
            ExerciseTextBox.Text += t;
            double result = solver.Calculate();
            AnswersTextBox.Text = result.ToString();
        }

        private void CountButton_Click(object sender, RoutedEventArgs e)
        {
            this.count = CountComboBox.SelectedIndex + 1;
            for (int j = 0; j < count; j++)
            {
                MakeTaskAndAnswer();
            }
        }
    }
}
