using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal enum TokenType { None, Number, Oper, Bracket, Variable };

    internal class Token
    {
        public string str;
        public TokenType toktype;

        public Token(TokenType t, string s)
        {
            toktype = t;
            str = s;
        }
        public Token(Token t)
        {
            str = t.str;
            toktype = t.toktype;
        }

        public override string ToString()
        {
            return str;
        }
    }

    internal class Number : Token
    {
        public double data;

        public Number(string s, double num) : base(TokenType.Number, s)
        {
            data = num;
        }
    };

    internal class Variable : Token
    {
        public Variable(string s) : base(TokenType.Variable, s)
        {
        }
    };

    internal enum OperType
    {
        None = 0, Add, Sub, Mul, Div, Mod, Minus,
        Sin, Cos, Tan, Arcs, Arcc, Arct,
        Pow, Ln, Log, Fac, Sqrt,
        RoundStart, RoundEnd, Round,
        Pi, E
    };

    internal class Operation : Token
    {
        public static bool isdigit(char c) { return c >= '0' && c <= '9' || c == '.'; }
        public static int char2digit(char c) { return c - '0'; }
        public const string oper_symbols = "+-*/%^!";
        public static bool isopersym(char c)
        {
            if (c == 0) return false;
            return (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' ||
                oper_symbols.Contains(c) || c == '[' || c == ']' );
        }

        public static Dictionary<string, OperType> oper_by_name = new Dictionary<string, OperType>()
        {
            {"+", OperType.Add}, {"-", OperType.Sub},
            {"*", OperType.Mul}, {"/", OperType.Div}, {"%", OperType.Mod},
            {"sin", OperType.Sin}, {"cos", OperType.Cos}, {"tan", OperType.Tan},
            {"arcs", OperType.Arcs}, {"arcc", OperType.Arcc}, {"arct", OperType.Arct},
            {"^", OperType.Pow}, {"ln", OperType.Ln}, {"log", OperType.Log},
            {"!", OperType.Fac}, {"[", OperType.RoundStart}, {"]", OperType.RoundEnd},
            {"round", OperType.Round}, {"pi", OperType.Pi}, {"e", OperType.E},
            {"sqrt", OperType.Sqrt }
        };

        public OperType type;
        public int priority;
        public int oper_counter;

        public Operation(string s, OperType t) : base(TokenType.Oper, s)
        {
            type = t;
            oper_counter = 2;
            switch (type)
            {
                case OperType.Add:
                case OperType.Sub:
                    priority = 1;
                    break;
                case OperType.Mul:
                case OperType.Div:
                case OperType.Mod:
                    priority = 2;
                    break;
                case OperType.Fac:
                    priority = 3;
                    oper_counter = 1;
                    break;
                case OperType.Pow:
                    priority = 3;
                    break;
                case OperType.Sqrt:
                    priority = 3;
                    oper_counter = 1;
                    break;
                case OperType.Log:
                    priority = 10;
                    break;
                case OperType.Minus:
                case OperType.Sin:
                case OperType.Cos:
                case OperType.Tan:
                case OperType.Arcs:
                case OperType.Arcc:
                case OperType.Arct:
                case OperType.Ln:
                    oper_counter = 1;
                    priority = 10;
                    break;
                case OperType.RoundStart:
                case OperType.RoundEnd:
                case OperType.Round:
                    priority = 0;
                    oper_counter = 1;
                    break;
                case OperType.Pi:
                case OperType.E:
                    oper_counter = 0;
                    priority = 100;
                    break;
                default:
                    priority = 0;
                    break;
            }
        }
    }

    internal class Bracket : Token
    {
        public bool is_opening;

        public Bracket(string s) : base(TokenType.Bracket, s)
        {
            is_opening = (s[0] == '(');
        }
    };
}
