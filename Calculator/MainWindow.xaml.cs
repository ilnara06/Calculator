using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Calculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DigitButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Display.Text += button.Content.ToString();
        }

        private void OperatorButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Display.Text += " " + button.Content.ToString() + " ";
        }

        private void FunctionButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string function = button.Content.ToString();
            Display.Text += function + "(";
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Display.Text))
            {
                Display.Text = Display.Text.Remove(Display.Text.Length - 1);
            }
        }
        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string expression = Display.Text.Replace(" ", ""); // Удаляем пробелы
                expression = expression.Replace(",", "."); // Заменяем запятую на точку

                // Проверяем баланс скобок
                if (!AreBracketsBalanced(expression))
                {
                    Display.Text = "Error: Unbalanced brackets";
                    return;
                }

                // Обработка функций sin, cos, tan, cot, log10, log2
                while (expression.Contains("sin") || expression.Contains("cos") || expression.Contains("tan") ||
                       expression.Contains("cot") || expression.Contains("log10") || expression.Contains("log2"))
                {
                    if (expression.Contains("sin"))
                    {
                        expression = ProcessFunction(expression, "sin", x => Math.Sin(DegreesToRadians(double.Parse(x))));
                    }
                    else if (expression.Contains("cos"))
                    {
                        expression = ProcessFunction(expression, "cos", x => Math.Cos(DegreesToRadians(double.Parse(x))));
                    }
                    else if (expression.Contains("tan"))
                    {
                        expression = ProcessFunction(expression, "tan", x => Math.Tan(DegreesToRadians(double.Parse(x))));
                    }
                    else if (expression.Contains("cot"))
                    {
                        expression = ProcessFunction(expression, "cot", x => 1 / Math.Tan(DegreesToRadians(double.Parse(x))));
                    }
                    else if (expression.Contains("log10"))
                    {
                        expression = ProcessFunction(expression, "log10", x => Math.Log10(double.Parse(x)));
                    }
                    else if (expression.Contains("log2"))
                    {
                        expression = ProcessFunction(expression, "log2", x => Math.Log(double.Parse(x), 2));
                    }
                }

                // Вычисление результата с помощью собственного парсера
                double result = EvaluateExpression(expression);
                Display.Text = result.ToString(CultureInfo.InvariantCulture); // Форматируем результат
            }
            catch (Exception ex)
            {
                Display.Text = "Error";
            }
        }

        // Метод для вычисления выражения без использования DataTable.Compute
        private double EvaluateExpression(string expression)
        {
            expression = expression.Replace(" ", ""); // Убираем лишние пробелы
            double result = ParseExpression(expression, out int _);
            return result;
        }

        // Рекурсивный парсер для обработки операторов
        private double ParseExpression(string expression, out int position)
        {
            position = 0;
            double result = ParseTerm(expression, ref position);

            while (position < expression.Length)
            {
                char op = expression[position];
                if (op == '+' || op == '-')
                {
                    position++;
                    double term = ParseTerm(expression, ref position);
                    result = op == '+' ? result + term : result - term;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        // Парсер для обработки умножения и деления
        private double ParseTerm(string expression, ref int position)
        {
            double result = ParseFactor(expression, ref position);

            while (position < expression.Length)
            {
                char op = expression[position];
                if (op == '*' || op == '/')
                {
                    position++;
                    double factor = ParseFactor(expression, ref position);
                    result = op == '*' ? result * factor : result / factor;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        // Парсер для обработки чисел и скобок
        private double ParseFactor(string expression, ref int position)
        {
            if (position >= expression.Length)
            {
                throw new ArgumentException("Неполное выражение");
            }

            if (expression[position] == '(')
            {
                position++; // Пропускаем '('
                double result = ParseExpression(expression, out int innerPosition);
                position = innerPosition + 1; // Пропускаем ')'
                return result;
            }

            // Парсим число
            int start = position;
            while (position < expression.Length && (char.IsDigit(expression[position]) || expression[position] == '.'))
            {
                position++;
            }

            string number = expression.Substring(start, position - start);
            return double.Parse(number, CultureInfo.InvariantCulture);
        }

        // Вспомогательный метод для обработки функций
        private string ProcessFunction(string expression, string functionName, Func<string, double> func)
        {
            int startIndex = expression.IndexOf(functionName + "(");
            if (startIndex == -1)
            {
                return expression;
            }

            int endIndex = expression.IndexOf(')', startIndex);
            if (endIndex == -1 || endIndex <= startIndex + functionName.Length + 1)
            {
                throw new ArgumentException($"Некорректное выражение в функции {functionName}");
            }

            string argument = expression.Substring(startIndex + functionName.Length + 1, endIndex - startIndex - functionName.Length - 1);

            // Проверяем, что аргумент является числом
            if (!double.TryParse(argument, out double argValue))
            {
                throw new ArgumentException($"Аргумент функции {functionName} должен быть числом");
            }

            double value = func(argument);
            string replacement = value.ToString("F15", CultureInfo.InvariantCulture); // Форматируем число

            return expression.Substring(0, startIndex) + replacement + expression.Substring(endIndex + 1);
        }

        // Метод для преобразования градусов в радианы
        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        // Метод для проверки балансировки скобок
        private bool AreBracketsBalanced(string expression)
        {
            int balance = 0;
            foreach (char c in expression)
            {
                if (c == '(') balance++;
                if (c == ')') balance--;
                if (balance < 0) return false; // Если закрывающая скобка появилась раньше открывающей
            }
            return balance == 0;
        }
    }
}