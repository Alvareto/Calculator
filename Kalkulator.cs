using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


namespace PrvaDomacaZadaca_Kalkulator
{
    public class Factory
    {
        public static ICalculator CreateCalculator()
        {
            // vratiti kalkulator
            return new Calculator();
        }
    }

    /// <summary>
    /// A record to store configuration options
    /// </summary>
    public static class Configuration
    {
        public static string DECIMAL_SEPARATOR;
        public static string ERROR_MESSAGE;
        public static int MAX_DISPLAY_LENGTH;

        static Configuration()
        {
            DECIMAL_SEPARATOR = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator =
                ",";
            ERROR_MESSAGE = "_E_";
            MAX_DISPLAY_LENGTH = 10;
        }
    }


    public class CalculatorServices : ICalculatorServices
    {
        private string appendToAccumulator(string digitAccumulator, object appendCh)
        {
            // ignore new input if there are too many digits
            if (digitAccumulator.Length > Configuration.MAX_DISPLAY_LENGTH)
            {
                return digitAccumulator; // ignore new input
            }
            else
            {
                // append the new char
                return (digitAccumulator + appendCh);
            }
        }

        public string AccumulateNonZeroDigit(char nonZeroDigit, string digitAccumulator)
        {
            return appendToAccumulator(digitAccumulator, nonZeroDigit);
        }

        public string AccumulateZero(string digitAccumulator)
        {
            return appendToAccumulator(digitAccumulator, '0');
        }

        public string AccumulateSeparator(string digitAccumulator)
        {
            string appendCh = (digitAccumulator == String.Empty)
                ? "0" + Configuration.DECIMAL_SEPARATOR
                : Configuration.DECIMAL_SEPARATOR;

            return appendToAccumulator(digitAccumulator, appendCh);
        }

        public double DoMathOperation(IBinaryOperation calculatorMathOp, double n1, double n2)
        {
            return calculatorMathOp.Compute(n1, n2);
        }

        public double DoMathOperation(IUnaryOperation calculatorMathOp, double n1)
        {
            return calculatorMathOp.Compute(n1);
        }

        public double GetNumberFromAccumulator(string accumulatorStateData)
        {
            double result;
            if (Double.TryParse(accumulatorStateData, out result))
                return result;
            return 0.0;
        }
    }

    public interface ICalculatorServices
    {
        /// <summary>
        /// type AccumulateNonZeroDigit = NonZeroDigit * DigitAccumulator -> DigitAccumulator
        /// </summary>
        /// <param name="nonZeroDigit"></param>
        /// <param name="digitAccumulator"></param>
        /// <returns></returns>
        string AccumulateNonZeroDigit(char nonZeroDigit, string digitAccumulator);

        /// <summary>
        /// type AccumulateZero = DigitAccumulator->DigitAccumulator
        /// </summary>
        /// <param name="digitAccumulator"></param>
        /// <returns></returns>
        string AccumulateZero(string digitAccumulator);

        /// <summary>
        /// type AccumulateSeparator = DigitAccumulator->DigitAccumulator
        /// </summary>
        /// <param name="digitAccumulator"></param>
        /// <returns></returns>
        string AccumulateSeparator(string digitAccumulator);

        /// <summary>
        /// type DoMathOperation = CalculatorMathOp * Number * Number->MathOperationResult
        /// </summary>
        /// <param name="calculatorMathOp"></param>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <returns></returns>
        double DoMathOperation(IBinaryOperation calculatorMathOp, double n1, double n2);
        /// <summary>
        /// type DoMathOperation = CalculatorMathOp * Number->MathOperationResult
        /// </summary>
        /// <param name="calculatorMathOp"></param>
        /// <param name="n1"></param>
        /// <returns></returns>
        double DoMathOperation(IUnaryOperation calculatorMathOp, double n1);

        /// <summary>
        /// type GetNumberFromAccumulator = AccumulatorStateData->Number
        /// </summary>
        /// <param name="accumulatorStateData"></param>
        /// <returns></returns>
        double GetNumberFromAccumulator(string accumulatorStateData);

        //string GetPendingOpFromState(State calculatorState);
        //string GetDisplayFromState(State calculatorState);
    }

    /// <summary>
    /// The 'Context' class
    /// </summary>
    public class Calculator : ICalculator
    {
        private State _state;
        //private string _display;

        // Constructor
        public Calculator()
        {
            // New calculator is 'ZeroState' by default
            //this._display = start;
            this._state = new ZeroState(new Display(), this);
        }

        // Properties
        public State State
        {
            get { return _state; }
            set { _state = value; }
        }

        //public Display Display
        //{
        //    get { return (Display)_state.Display; }
        //}

        public void Press(char inPressedDigit)
        {
            _state.Press(inPressedDigit);
        }

        public string GetCurrentDisplayState()
        {
            return _state.GetCurrentDisplayState();
        }
    }

    /// <summary>
    /// There are six possible inputs
    /// </summary>
    public enum InputType
    {
        Zero,
        NonZeroDigit,
        DecimalSeparator,
        MathOp,
        Equals,
        Clear
    }

    public class Input
    {
        protected internal InputType Type;
        protected internal char Value;

        public Input(char inPressedDigit)
        {
            this.Value = inPressedDigit;
            this.Type = fauxSwitch(inPressedDigit);

        }

        private InputType fauxSwitch(char caseSwitch)
        {
            if (caseSwitch == '0')
            {
                return InputType.Zero;
            }
            if (Char.IsDigit(caseSwitch))
            {
                return InputType.NonZeroDigit;
            }
            if (caseSwitch == ',')
            {
                return InputType.DecimalSeparator;
            }
            if (caseSwitch == '=')
            {
                return InputType.Equals;
            }
            if (caseSwitch == 'C')
            {
                return InputType.Clear;
            }

            return InputType.MathOp;
        }
    }

    /// <summary>
    /// The 'State' abstract class
    /// </summary>
    public abstract class State
    {
        protected Calculator calculator;
        protected IDisplay display;

        private double _accumulated;

        public string DigitAccumulator; // string
        public Operation PendingOperation; // (CalculatorMathOp * Number)
        public double Number; // float


        //private IOperation _pendingOp;
        //private IOperation _noOperation;

        // Properties
        public Calculator Calculator
        {
            get { return calculator; }
            set { calculator = value; }
        }

        public IDisplay Display
        {
            get { return display; }
            set { display = value; }
        }

        public abstract void Press(char inPressedDigit);

        public string GetCurrentDisplayState()
        {
            return Display.Format();
        }
    }

    /// <summary>
    /// A 'ConcreteState' class
    /// <remarks>
    /// Zero indicates that calculator is 
    /// </remarks>
    /// <para>Data associated with state: (optional) pending op</para>
    /// <para>Special behavior: ignores all Zero input</para>
    /// </summary>
    class ZeroState : State
    {
        // (optional) pending op
        //private IBinaryOperation _pendingOperation;
        private IOperation _pendingOp;

        // Overloaded constructors
        public ZeroState(State state) : this(state.Display, state.Calculator)
        { }

        public ZeroState(IDisplay display, Calculator calculator)
        {
            this.calculator = calculator;
            this.display = display;
            Initialize();
        }

        private void Initialize()
        {
            this.display.Set("0");
        }

        public override void Press(char inPressedDigit)
        {
            Input input = new Input(inPressedDigit);
            switch (input.Type)
            {
                case InputType.Zero:
                    // ACTION: (ignore)
                    // NEW_STATE: ZeroState
                    break;
                case InputType.NonZeroDigit:
                    // ACTION: start a new accumulator with the digit
                    // NEW_STATE: AccumulatorState
                    calculator.State = new AccumulatorState(this);
                    break;
                case InputType.DecimalSeparator:
                    // ACTION: start a new accumulator with "0."
                    // NEW_STATE: AccumulatorDecimalState
                    this.display.Set("0,");
                    calculator.State = new AccumulatorDecimalState(this);
                    break;
                case InputType.MathOp:
                    // ACTION: go to Computed or ErrorState state.
                    // If there is pending op, update the display based on the result of the calculation (or error).
                    // Also, if calculation was successful, push a new pending op, built from the event, using a current number of "0".
                    // NEW STATE: ComputedState

                    calculator.State = new ComputedState(this);
                    break;
                case InputType.Equals:
                    // ACTION: As with MathOp, but without any pending op
                    // NEW STATE: ComputedState

                    calculator.State = new ComputedState(this);
                    break;
                case InputType.Clear:
                    // ACTION: (ignore)
                    // NEW_STATE: ZeroState
                    break;
                default:
                    calculator.State = new ErrorState(this);
                    break;
            }
        }

        private void StateChangeCheck()
        {

        }
    }

    /// <summary>
    /// A 'ConcreteState' class
    /// <remarks>
    /// Zero indicates that 
    /// </remarks>
    /// <para>Data associated with state: Buffer and (optional) pending op</para>
    /// <para>Special behavior: Accumulates digits in buffer</para>
    /// </summary>
    class AccumulatorState : State
    {
        /// <summary>
        /// Buffer
        /// </summary>
        private char[] digits;
        // (optional) pending op
        //IBinaryOperation _pendingOperation;
        private IOperation _pendingOp;

        public AccumulatorState(State state)
        {
            this.calculator = state.Calculator;
            this.display = state.Display;
            Initialize();
        }

        private void Initialize()
        {

        }

        public override void Press(char inPressedDigit)
        {
            Input input = new Input(inPressedDigit);
            switch (input.Type)
            {
                case InputType.Zero:
                case InputType.NonZeroDigit:
                    // ACTION: Append the digit or "0" to the buffer.
                    // NEW_STATE: AccumulatorState
                    this.display.Append(input);
                    calculator.State = new AccumulatorState(this);
                    break;
                case InputType.DecimalSeparator:
                    // ACTION: Append the separator to the buffer, and transition to new state.
                    // NEW_STATE: AccumulatorDecimalState
                    this.display.Append(input);
                    calculator.State = new AccumulatorDecimalState(this);
                    break;
                case InputType.MathOp:
                    // ACTION: go to Computed or ErrorState state.
                    // If there is pending op, update the display based on the result of the calculation (or error).
                    // Also, if calculation was successful, push a new pending op, built from the event, using a current number of "0".
                    // NEW STATE: ComputedState

                    calculator.State = new ComputedState(this);
                    break;
                case InputType.Equals:
                    // ACTION: As with MathOp, but without any pending op
                    // NEW STATE: ComputedState

                    calculator.State = new ComputedState(this);
                    break;
                case InputType.Clear:
                    // ACTION: Go to Zero state. Clear any pending op.
                    // NEW_STATE: ZeroState

                    calculator.State = new ZeroState(this);
                    break;
                default:
                    calculator.State = new ErrorState(this);
                    break;
            }
        }

        private void StateChangeCheck()
        {

        }
    }

    /// <summary>
    /// A 'ConcreteState' class
    /// <remarks>
    /// Zero indicates that 
    /// <para>Data associated with state: Buffer and (optional) pending op</para>
    /// <para>Special behavior: Accumulates digits in buffer, but ignores decimal separators</para>
    /// </remarks>
    /// </summary>
    class AccumulatorDecimalState : State
    {
        /// <summary>
        /// Buffer
        /// </summary>
        private string _buffer;
        // (optional) pending op
        //IBinaryOperation _pendingOperation;

        public AccumulatorDecimalState(State state)
        {
            this.calculator = state.Calculator;
            this.display = state.Display;
            Initialize();
        }

        private void Initialize()
        {

        }

        public override void Press(char inPressedDigit)
        {
            Input input = new Input(inPressedDigit);
            switch (input.Type)
            {
                case InputType.Zero:
                case InputType.NonZeroDigit:
                    // ACTION: Append the digit or "0" to the buffer.
                    // NEW_STATE: AccumulatorState
                    this.display.Append(input);
                    calculator.State = new AccumulatorState(this);
                    break;
                case InputType.DecimalSeparator:
                    // ACTION: (ignore)
                    // NEW_STATE: AccumulatorDecimalState
                    break;
                case InputType.MathOp:
                    // ACTION: go to Computed or ErrorState state.
                    // If there is pending op, update the display based on the result of the calculation (or error).
                    // Also, if calculation was successful, push a new pending op, built from the event, using a current number of "0".
                    // NEW STATE: ComputedState

                    calculator.State = new ComputedState(this);
                    break;
                case InputType.Equals:
                    // ACTION: As with MathOp, but without any pending op
                    // NEW STATE: ComputedState

                    calculator.State = new ComputedState(this);
                    break;
                case InputType.Clear:
                    // ACTION: Go to Zero state. Clear any pending op.
                    // NEW_STATE: ZeroState

                    calculator.State = new ZeroState(this);
                    break;
                default:
                    calculator.State = new ErrorState(this);
                    break;
            }
        }

        private void StateChangeCheck()
        {

        }
    }

    /// <summary>
    /// A 'ConcreteState' class
    /// <remarks>
    /// Zero indicates that 
    /// <para>Data associated with state: Calculated number and (optional) pending op</para>
    /// <para>Special behavior: -</para>
    /// </remarks>
    /// </summary>
    class ComputedState : State
    {
        /// <summary>
        /// Calculated number
        /// </summary>
        private double _calculatedNumber;
        // (optional) pending op

        public ComputedState(State state)
        {
            this.calculator = state.Calculator;
            this.display = state.Display;
            Initialize();
        }

        private void Initialize()
        {

        }

        public override void Press(char inPressedDigit)
        {
            Input input = new Input(inPressedDigit);
            switch (input.Type)
            {
                case InputType.Zero:
                    // ACTION: Go to ZeroState state, but preserve any pending op
                    // NEW_STATE: ZeroState

                    break;
                case InputType.NonZeroDigit:
                    // ACTION: Start a new accumulator, preserving any pending op
                    // NEW_STATE: AccumulatorState
                    this.display.Append(input);
                    calculator.State = new AccumulatorState(this);
                    break;
                case InputType.DecimalSeparator:
                    // ACTION: Start a new decimal accumulator, preserving any pending op
                    // NEW_STATE: AccumulatorDecimalState
                    break;
                case InputType.MathOp:
                    // ACTION: Stay in Computed state. Replace any pending op with a new one built from the input event
                    // NEW STATE: ComputedState

                    calculator.State = new ComputedState(this);
                    break;
                case InputType.Equals:
                    // ACTION: Stay in Computed state. Clear any pending op
                    // NEW STATE: ComputedState

                    calculator.State = new ComputedState(this);
                    break;
                case InputType.Clear:
                    // ACTION: Go to Zero state. Clear any pending op.
                    // NEW_STATE: ZeroState

                    calculator.State = new ZeroState(this);
                    break;
                default:
                    calculator.State = new ErrorState(this);
                    break;
            }
        }
        private void StateChangeCheck()
        {

        }
    }


    /// <summary>
    /// A 'ConcreteState' class
    /// <remarks>
    /// Zero indicates that 
    /// <para>Data associated with state: Error message</para>
    /// <para>Special behavior: Ignores all input other than Clear</para>
    /// </remarks>
    /// </summary>
    class ErrorState : State
    {
        /// <summary>
        /// Error message
        /// </summary>
        private string _errorMessage = Configuration.ERROR_MESSAGE;

        public ErrorState(State state)
        {
            this.calculator = state.Calculator;
            this.display = state.Display;
            Initialize();
        }

        private void Initialize()
        {
            display.Set(_errorMessage);
        }

        public override void Press(char inPressedDigit)
        {
            Input input = new Input(inPressedDigit);
            switch (input.Type)
            {
                case InputType.Clear:
                    // ACTION: Go to Zero state. Clear any pending op.
                    // NEW_STATE: ZeroState
                    calculator.State = new ZeroState(this);
                    break;
            }
        }

        private void StateChangeCheck()
        {

        }
    }

    public interface IDisplay
    {
        void Append(string text);
        string Format();
        void Clear();
    }

    /// <summary>
    /// Handle everything related to display - formatting
    /// </summary>
    public class Display : IDisplay
    {
        private string buffer = String.Empty;

        public Display() : this(String.Empty)
        {
        }

        public Display(string text)
        {
            this.buffer = text;
        }

        public override string ToString()
        {
            return buffer;
        }

        public void Append(string text)
        {
            buffer += text;
            //Format();
        }

        public string Format()
        {
            return buffer;
        }

        public void Clear()
        {
            buffer = String.Empty;
        }
    }

    public interface IOperation
    {
        double Compute(params double[] operands);
    }

    public abstract class Operation : IOperation
    {
        public double FirstOperand;
        public double SecondOperand;
        public double Result;

        public abstract string KEYWORD { get; }

        /// <summary>
        /// Generic implemenation of the generic interface IOperation -- 
        /// <remarks>Instead of implementing this in every interface derived class, we implement it here and then derive from this class.</remarks>
        /// </summary>
        /// <param name="operands"></param>
        /// <exception cref="ArgumentNullException">Argument shouldn't be null</exception>
        /// <exception cref="ArgumentException">Number of parameters should be 1 or 2</exception>
        /// <returns></returns>
        public virtual double Compute(params double[] operands)
        {
            if (operands == null)
            {
                throw new ArgumentNullException();
            }
            if (operands.Length < 1)
            {
                throw new ArgumentException("Not enough numbers for operation");
            }
            if (operands.Length > 2)
            {
                throw new ArgumentException("Too many numbers for supported operations");
            }

            return 0;
        }
    }

    /// <summary>
    /// In mathematics, a unary operation is an operation with only one operand
    /// , i.e. a single input. An example is the function f : A → A,
    ///  where A is a set. The function f is a unary operation on A.
    /// </summary>
    public interface IUnaryOperation : IOperation
    {
        double Compute(double operand0);
    }

    /// <summary>
    /// In mathematics, a binary operation on a set is a calculation that combines two elements of the set (called operands) to produce another element of the set.
    /// </summary>
    public interface IBinaryOperation : IOperation
    {
        double Compute(double operand0, double operand1);
    }

    public interface IMemoryOperation : IOperation
    {
        void Store(double d);
        double Recall();
    }

    public class AdditionOperation : Operation, IBinaryOperation
    {
        public double Compute(double operand0, double operand1)
        {
            return operand0 + operand1;
        }

        public override string KEYWORD { get { return "+"; } }

        public sealed override double Compute(params double[] operands)
        {
            base.Compute(operands);
            return Compute(operands[0], operands[1]);
        }
    }

    public class SubtractionOperation : Operation, IBinaryOperation
    {
        public double Compute(double operand0, double operand1)
        {
            return operand0 - operand1;
        }

        public override string KEYWORD { get { return "-"; } }

        public sealed override double Compute(params double[] operands)
        {
            base.Compute(operands);
            return Compute(operands[0], operands[1]);
        }
    }

    public class MultiplicationOperation : Operation, IBinaryOperation
    {
        public double Compute(double operand0, double operand1)
        {
            return operand0 * operand1;
        }

        public override string KEYWORD { get { return "*"; } }

        public sealed override double Compute(params double[] operands)
        {
            base.Compute(operands);
            return Compute(operands[0], operands[1]);
        }
    }

    public class DivisionOperation : Operation, IBinaryOperation
    {
        public double Compute(double operand0, double operand1)
        {
            return operand0 / operand1;
        }

        public override string KEYWORD { get { return "/"; } }

        public sealed override double Compute(params double[] operands)
        {
            base.Compute(operands);
            return Compute(operands[0], operands[1]);
        }
    }

    public class MinusOperation : Operation, IUnaryOperation
    {
        public double Compute(double operand0)
        {
            return -operand0;
        }

        public override string KEYWORD { get { return "M"; } }

        public sealed override double Compute(params double[] operands)
        {
            base.Compute(operands);
            return Compute(operands[0]);
        }
    }

    public class SinusOperation : Operation, IUnaryOperation
    {
        public double Compute(double operand0)
        {
            return Math.Sin(operand0);
        }

        public override string KEYWORD { get { return "S"; } }

        public sealed override double Compute(params double[] operands)
        {
            base.Compute(operands);
            return Compute(operands[0]);
        }
    }

    public class CosinusOperation : Operation, IUnaryOperation
    {
        public double Compute(double operand0)
        {
            return Math.Cos(operand0);
        }

        public override string KEYWORD { get { return "C"; } }

        public sealed override double Compute(params double[] operands)
        {
            base.Compute(operands);
            return Compute(operands[0]);
        }
    }

    public class TangensOperation : Operation, IUnaryOperation
    {
        public double Compute(double operand0)
        {
            return Math.Tan(operand0);
        }

        public override string KEYWORD { get { return "T"; } }

        public sealed override double Compute(params double[] operands)
        {
            base.Compute(operands);
            return Compute(operands[0]);
        }
    }

    public class QuadratOperation : Operation, IUnaryOperation
    {
        public double Compute(double operand0)
        {
            return operand0 * operand0;
        }

        public override string KEYWORD { get { return "Q"; } }

        public sealed override double Compute(params double[] operands)
        {
            base.Compute(operands);
            return Compute(operands[0]);
        }
    }

    public class RootOperation : Operation, IUnaryOperation
    {
        public double Compute(double operand0)
        {
            return Math.Sqrt(operand0);
        }

        public override string KEYWORD { get { return "R"; } }

        public sealed override double Compute(params double[] operands)
        {
            base.Compute(operands);
            return Compute(operands[0]);
        }
    }

    public class InversOperation : Operation, IUnaryOperation
    {
        public double Compute(double operand0)
        {
            return 1.0 / operand0;
        }

        public override string KEYWORD { get { return "I"; } }

        public sealed override double Compute(params double[] operands)
        {
            base.Compute(operands);
            return Compute(operands[0]);
        }
    }

    public static class Extensions
    {
        public static IDisplay Trim(this IDisplay display, int length = 10)
        {
            string text = display.ToString();
            if (text.Length <= length)
            {
                return display;
            }
            else
            {
                return text.TakeFromEnd(length).ToDisplay();

            }
        }

        public static IEnumerable<T> TakeFromEnd<T>(this IEnumerable<T> sequence, int count)
        {
            return sequence.Reverse().Take(count).Reverse();
        }

        public static IDisplay ToDisplay(this string text)
        {
            return new Display(text);
        }

        public static IDisplay ToDisplay(this IEnumerable<char> text)
        {
            return new Display(text.ToString());
        }

        public static IOperation ToOperation(this Input input)
        {
            if (input.Type != InputType.MathOp)
            {
                throw new ArgumentException("Only mathematical operators can be turned into operation");
            }

            switch (input.Value)
            {
                case '+':
                    return new AdditionOperation();
                case '-':
                    return new SubtractionOperation();
                case '*':
                    return new MultiplicationOperation();
                case '/':
                    return new DivisionOperation();
                case 'M':
                    return new MinusOperation();
                case 'S':
                    return new SinusOperation();
                case 'K':
                    return new CosinusOperation();
                case 'T':
                    return new TangensOperation();
                case 'Q':
                    return new QuadratOperation();
                case 'R':
                    return new RootOperation();
                case 'I':
                    return new InversOperation();
                //case 'P':
                //    return IMemoryOperation();
                //    break;
                //case 'G':
                //    break;
                default:
                    return null;
            }
        }

        public static string ToString(this IOperation operation)
        {
            var operation1 = operation as Operation;
            return operation1 != null ? operation1.KEYWORD : String.Empty;
        }

        public static IEnumerable<char> ToCharArray(this IDisplay display)
        {
            return display.ToString().ToCharArray();
        }

        public static IDisplay Set(this IDisplay display, string text)
        {
            display.Clear();
            display.Append(text);
            display.Format();

            return display;
        }

        public static IDisplay Append(this IDisplay display, char digit)
        {
            display.Append(digit.ToString());
            return display;
        }

        public static IDisplay Append(this IDisplay display, Input input)
        {
            return display.Append(input.Value);
        }

        public static IDisplay MergeWith(this IDisplay display, IDisplay text)
        {
            display.Append(text.ToString());
            return display;
        }

        public static IDisplay Format(this IDisplay display)
        {
            display.Format();
            return display;
        }

        public static IDisplay Clear(this IDisplay display)
        {
            display.Clear();
            return display;
        }
    }



    /*

    public class Display
    {
        public Key[] keys;
        // ekran može sadržavati 10 UNESENIH znamenki
        private const int LIMIT_DIGITS_ENTERED = 10;
        // => uz decimalni znak i predznak = max 12 PRIKAZANIH znakova

        // nakon paljenja kalkulatora, na ekranu se prikazuje znamenka 0
        // nakon svakog brisanja ekrana na ekranu se prikazuje 0


        private const char CHAR_OPERATION_PLUS = '+';
    }

    public class Key
    {
        public KeyType Type;
        public char Value;
    }

    public enum KeyType
    {
        Digit,
        Operator,
        Separator
    }

    public enum Operator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Equals,
        Comma,
        Minus, // promjena predznaka  - naziv prema Minus
        Sinus,
        Cosinus,
        Tangens,
        Quadrat,
        Root,
        Invers,
        Put,
        Get,
        Clear,
        Reset
    }

    public static class Operation
    {
        const char CHAR_OPERATION_PLUS = '+';

        static Operation()
        {
            Context c = new Context(new StartState());
        }
    }

    public class Context
    {
        private State _state;

        // Constructor
        public Context(State state)
        {
            this.State = state;
        }

        // Gets or sets the state
        public State State
        {
            get { return _state; }
            set
            {
                _state = value;
                Console.WriteLine("State: " + _state.GetType().Name);
            }
        }

        public void Request()
        {
            _state.Handle(this);
        }
    }

    public abstract class State
    {
        public abstract void Handle(Context context);
    }

    class StartState : State
    {
        public override void Handle(Context context)
        {
            context.State = new StartState();
        }
    }

    public enum OperationType
    {

    }

    public interface IOperation
    {
        string HandleOperation(Func<Operator, Key, Key> handler);
        string HandleUnarOperation(Func<Operator, Key> handler);
        string HandleSimpleOperation(Func<Operator> handler);
    }



    */
}