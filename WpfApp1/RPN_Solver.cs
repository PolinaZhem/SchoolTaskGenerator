using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace WpfApp1
{
    public class RPN_Solver
    {
        private Stack<Token> _RPN = null;
        private Dictionary<string, double> value_of_variable = null;

        private void copy_of_RPN(Stack<Token> copyofr, Stack<Token> r)
        {
            foreach (Token token in r)
            {
                Token t = new Token(token);
                copyofr.Push(t);
            }
        }

        private void PrintStack(Stack<Token> given_st, string name)
        {
            Stack<Token> copy_stack = new Stack<Token>(given_st);

            Console.Write(name + ": ");
            foreach (var x in copy_stack)
            {
                Console.Write(x.str);
                Console.Write(" ");
            }
            Console.WriteLine(" ");
        }

        private Queue<Token> TokenizeString(string s)
        {
            Queue<Token> tokens = new Queue<Token>();
            bool isTokenFinished = false;
            TokenType currentType = TokenType.None;

            double number = 0.0;
            double fraction = 0.0;
            int digits_after_point = 0;
            bool on_integer_part = true;
            string str = "";

            Token token = new Token(TokenType.None, "???");

            for (int i = 0; i <= s.Length; i++)
            {
                if (isTokenFinished || i == s.Length)
                {
                    switch (currentType)
                    {
                        case TokenType.Oper:
                            {
                                OperType t;
                                str = str.ToLower();
                                if (Operation.oper_by_name.ContainsKey(str))
                                {
                                    t = Operation.oper_by_name[str];

                                    if (t == OperType.Sub)
                                    {
                                        if (tokens.Count == 0 ||
                                            tokens.Last().toktype == TokenType.Bracket && (tokens.Last() as Bracket).is_opening ||
                                            tokens.Last().toktype == TokenType.Oper)
                                        {
                                            t = OperType.Minus;
                                            str = "~";
                                        }
                                    }

                                    token = new Operation(str, t);
                                }
                                else
                                {
                                    token = new Variable(str);
                                    currentType = TokenType.Variable;
                                }
                                str = "";
                            }
                            break;

                        case TokenType.Number:
                            {
                                token = new Number(str, number + fraction / Math.Pow(10, digits_after_point));
                                number = 0;
                                fraction = 0;
                                digits_after_point = 0;
                                on_integer_part = true;
                                str = "";
                            }
                            break;

                        case TokenType.Bracket:
                            {
                                token = new Bracket(str);
                                str = "";
                            }
                            break;
                    }
                    tokens.Enqueue(token);
                    isTokenFinished = false;
                    currentType = TokenType.None;

                    if (i == s.Length)
                        continue;
                }

                if (Operation.isdigit(s[i]) && (currentType == TokenType.Number || currentType == TokenType.None))
                {
                    if (s[i] == '.')
                    {
                        on_integer_part = false;
                    }
                    else if (on_integer_part)
                    {
                        number = number * 10 + Operation.char2digit(s[i]);
                    }
                    else
                    {
                        fraction = fraction * 10 + Operation.char2digit(s[i]);
                        digits_after_point++;
                    }
                    str += s[i];
                    currentType = TokenType.Number;
                }
                else if (Operation.isopersym(s[i]) && (currentType == TokenType.Oper || currentType == TokenType.None))
                {
                    if (Operation.oper_symbols.Contains(s[i]) && currentType == TokenType.Oper)
                    {
                        isTokenFinished = true;
                        i--;
                        continue;
                    }
                    str += s[i];
                    currentType = TokenType.Oper;
                    if (Operation.oper_by_name.ContainsKey(str))
                    {
                        isTokenFinished = true;
                    }
                }
                else if ((s[i] == '(' || s[i] == ')') && currentType == TokenType.None)
                {
                    str += s[i];
                    currentType = TokenType.Bracket;
                }
                else if (currentType != TokenType.None)
                {
                    isTokenFinished = true;
                    i--;
                }
            }

            return tokens;
        }

        private Stack<Token> Convert2RPN(Queue<Token> infix_tokens)
        {
            Stack<Token> RPN = new Stack<Token>();
            Stack<Token> OpStack = new Stack<Token>();

            while (infix_tokens.Count > 0)
            {
                //Console.WriteLine("************************************************************");
                //PrintStack(RPN, "RPN");
                //PrintStack(OpStack, "OpS");
                Token token = infix_tokens.Dequeue();

                switch (token.toktype)
                {
                    case TokenType.Number:
                        RPN.Push(token);
                        break;

                    case TokenType.Variable:
                        RPN.Push(token);
                        value_of_variable[token.str] = 0;
                        break;

                    case TokenType.Bracket:
                        if ((token as Bracket).is_opening)
                            OpStack.Push(token);
                        else
                        {
                            while (OpStack.Peek().toktype != TokenType.Bracket)
                            {
                                RPN.Push(OpStack.Pop());
                                if (OpStack.Count == 0)
                                {
                                    throw new Exception("Неправильные скобки! Лишняя закрывающая скобка.");
                                }
                            }
                            OpStack.Pop();
                        }
                        break;

                    case TokenType.Oper:
                        Operation op = token as Operation;
                        if (op.oper_counter == 0)
                        {
                            RPN.Push(token);
                            break;
                        }
                        if (op.type == OperType.RoundStart)
                        {
                            OpStack.Push(token);
                            break;
                        }
                        if (op.type == OperType.RoundEnd)
                        {
                            while (OpStack.Count > 0 &&
                                (OpStack.First() as Operation).type != OperType.RoundStart)
                            {
                                RPN.Push(OpStack.Pop());
                            }
                            RPN.Push(new Operation("round", OperType.Round));
                            OpStack.Pop();
                            break;
                        }
                        while (OpStack.Count > 0 &&
                            OpStack.Peek().toktype == TokenType.Oper &&
                            (OpStack.Peek() as Operation).priority >= op.priority &&
                            ((OpStack.Peek() as Operation).oper_counter == 2 || op.oper_counter == 2))
                        {
                            RPN.Push(OpStack.Pop());
                        }
                        OpStack.Push(token);
                        break;
                        // (b+c)*(a-sin(30*pi/180))
                }
            }
            while (OpStack.Count > 0)
            {
                if (OpStack.Last().toktype == TokenType.Bracket)
                {
                    throw new Exception("Неправильные скобки! Лишняя открывающая скобка.");
                }
                if (OpStack.Last().toktype == TokenType.Oper &&
                (OpStack.Last() as Operation).type == OperType.RoundStart)
                {
                    throw new Exception("Неправильная операция! Неполная операция выделения целой части.");
                }
                RPN.Push(OpStack.Pop());
            }

            return RPN;
        }

        private double CalculateRPN(Stack<Token> RPN, Dictionary<string, double> value_of_variable)
        {
            double result = 0;

            Token token = RPN.Pop();

            double a = 0, b = 0;

            switch (token.toktype)
            {
                case TokenType.Number:
                    return (token as Number).data;

                case TokenType.Variable:
                    if (value_of_variable.ContainsKey(token.str))
                        return value_of_variable[token.str];
                    else
                        throw new Exception("Значение переменной " + token.str + " отсутствует!");

                case TokenType.Oper:
                    {
                        Operation oper = token as Operation;
                        if (oper.type == OperType.None)
                        {
                            throw new Exception("Неизвестный тип операции!");
                            return 0;
                        }
                        if (oper.oper_counter == 1)
                        {
                            if (RPN.Count() == 0) throw new Exception("Не хватает операнда для операции!");
                            b = CalculateRPN(RPN, value_of_variable);
                        }
                        else if (oper.oper_counter == 2)
                        {
                            if (RPN.Count() == 0) throw new Exception("Not enough operands for operation!");
                            b = CalculateRPN(RPN, value_of_variable);

                            if (RPN.Count() == 0) throw new Exception("Not enough operands for operation!");
                            a = CalculateRPN(RPN, value_of_variable);
                        }
                        switch (oper.type)
                        {
                            case OperType.Add: return a + b;
                            case OperType.Sub: return a - b;
                            case OperType.Mul: return a * b;
                            case OperType.Div:
                                {
                                    if (b == 0)
                                    {
                                        throw new Exception("Operation error! Dividing by zero.");
                                    }
                                    else
                                        return a / b;
                                }
                            case OperType.Pow:
                                {
                                    if (a == 0 && b < 0)
                                        throw new Exception("Operation error! Negative power of zero.");

                                    else if (a < 0)
                                        throw new Exception("Operation error! Power of negative number.");
                                    else
                                        return Math.Pow(a, b);
                                }
                            case OperType.Mod:
                                {
                                    int a1 = (int)Math.Round(a, 0);
                                    int b1 = (int)Math.Round(b, 0);
                                    if (b1 == 0)
                                        throw new Exception("Operation error! Dividing by zero.");
                                    return a1 % b1;
                                }
                            case OperType.Fac:
                                {
                                    if ((int)Math.Round(b, 0) < 0)
                                        throw new Exception("Operation error! The factorial of a negative number.");

                                    int b1 = 1;
                                    int b_max = (int)Math.Round(b, 0);
                                    for (int i = 1; i <= b_max; i++)
                                        b1 *= i;
                                    return b1;
                                }
                            case OperType.Round: return (int)Math.Round(b, 0);
                            case OperType.Minus: return -b;
                            case OperType.Sin: return Math.Sin(b);
                            case OperType.Cos: return Math.Cos(b);
                            case OperType.Tan: return Math.Tan(b);
                            case OperType.Arcs:
                                {
                                    if (b < -1 || b > 1)
                                        throw new Exception("Wrong operation!");
                                    else
                                        return Math.Asin(b);
                                }
                            case OperType.Arcc:
                                {
                                    if (b < -1 || b > 1)
                                        throw new Exception("Wrong operation!");
                                    else
                                        return Math.Acos(b);
                                }
                            case OperType.Arct: return Math.Atan(b);
                            case OperType.Ln:
                                {
                                    if (b < 0)
                                        throw new Exception("Wrong operation!");
                                    else
                                        return Math.Log(b);
                                }
                            case OperType.Log:
                                {
                                    if (b < 0 || a < 0)
                                        throw new Exception("Wrong operation!");
                                    else
                                        return Math.Log(b, a);
                                }
                            case OperType.Pi: return Math.PI;
                            case OperType.E: return Math.E;
                        }
                    }
                    break;
            }
            return result;
        }

        private double RPNCalcString(string s, Dictionary<string, double> value_of_variable)
        {
            string error_text;
            bool is_error = false;
            double result = 0;
            try
            {
                Queue<Token> tokens = TokenizeString(s);
                foreach (var x in tokens)
                {
                    Console.Write(x.str);
                    Console.Write(" ");
                }
                Console.WriteLine(" ");
                Stack<Token> RPN = Convert2RPN(tokens);
                result = CalculateRPN(RPN, value_of_variable);
            }
            catch (Exception e)
            {
                error_text = e.Message;
                is_error = true;
            }
            return result;
        }

        public void LoadFormula(string formula_string)
        {
            value_of_variable = new Dictionary<string, double>();
            Queue<Token> tokens = TokenizeString(formula_string);
            _RPN = Convert2RPN(tokens);
        }

        public Dictionary<string, double> GetVars()
        {
            return value_of_variable;
        }

        public double Calculate()
        {
            Stack<Token> tmp_toks = new Stack<Token>(_RPN);
            Stack<Token> tokens = new Stack<Token>(tmp_toks);
            if (tokens.Count() == 0)
                throw new Exception("No loaded formula!");
            double result = CalculateRPN(tokens, value_of_variable);
            if (tokens.Count() > 0)
                throw new Exception("Fromula error! Too much operands.");

            return result;
            //добавить копию RPN
        }
    }
}

