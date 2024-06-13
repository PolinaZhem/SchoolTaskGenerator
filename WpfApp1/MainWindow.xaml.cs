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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime;

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
            text_vars = new List<VariableSetter>
            {
                new VariableSetter { Name="Speed", RangeFrom=100, RangeTo=200, Step = 5, DigitsToRound = 1},
                new VariableSetter { Name="Time", RangeFrom=200, RangeTo=400, Step = 15, DigitsToRound = 2},
                new VariableSetter { Name="Dist", RangeFrom=300, RangeTo=500, Step = 25, DigitsToRound = 3}
            };
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

            string t = "";
            foreach (var i in text_of_task)
            {
                t += "|" + i + "|";
            }
            t += "\r\n";
            foreach (var i in text_vars)
            {
                t += "|" + i.Name + "|, ";
            }
            //TaskTextBox.Text = t;
            MessageBox.Show(t);
            VariablesGrid.ItemsSource = text_vars;
            VariablesGrid.Items.Refresh();


            /*
             * "Бла бла бла скорость равна {Speed} м/с, бла бла время было {t} сек."
             * Разобрать текст на список строк без переменных в фигурных скобках
             * Закинуть переменные в таблицу --------
             * 
             * По кнопке анализ формулы построить RPN и сверить список переменных *я не знаю как передать список переменных в диалоговое окно и обратно
             * 
             * По кнопке Сгенерировать задачу:
             *      - создать словарь переменных
             *      - задать переменным целые значения по формуле (rand()%((b-a)/step+1))*step+a
             *      - совместить текст и значения
             *      - рассчитать формулу со значениями переменных
             *      - составить текст задачи и ответ к ней, показать в MessageBox
             *      
             *      formula_vars
             *      генерировать ответ к задаче. hhbhh
             */

            // var_list.Clear();
            // var_list.Add(new VariableSetter { Name = "ABCD", RangeFrom = 0, RangeTo = 100, Step = 1, DigitsToRound = 0 });
            // VariablesGrid.Items.Refresh();
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
            //Random rand = new Random();
            //string t = "";
            //foreach (var v in text_vars)
            //{
            //    int diff = ((int)v.RangeTo - (int)v.RangeFrom) / (int)v.Step;
            //    int value = rand.Next(diff + 1) * (int)v.Step + (int)v.RangeFrom;
            //    //t += $"{v.Name} = {value} ({diff + 1})\r\n";
            //    formula_vars[v.Name] = value;
            //    v.Value = value;
            //}

            //for (int i =0; i<text_of_task.Count-1; i++)
            //{
            //    t+=text_of_task[i];
            //    t+=text_vars[i].Value;
            //}
            //t += text_of_task.Last();

            //MessageBox.Show(t);

            task = new Task();
            task.SetSolver(solver);
            task.SetText(text_of_task);
            task.SetVar(text_vars);
            task.SetForVar(formula_vars);

            var res = task.ShowDialog();
            //formula_vars
        }
    }
}
