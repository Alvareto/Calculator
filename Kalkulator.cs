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
        public static string _DEFAULT_;

        public const char ADDITION = '+';
        public const char SUBTRACTION = '-';
        public const char MULTIPLICATION = '*';
        public const char DIVISION = '/';

        public const char EQUALS = '=';

        public const char SINUS = 'S';
        public const char KOSINUS = 'K';
        public const char TANGENS = 'T';
        public const char QUADRAT = 'Q';
        public const char ROOT = 'R';
        public const char INVERS = 'I';

        public const char PUT = 'P';
        public const char GET = 'G';

        public const char MINUS = 'M';
        public const char CLEAR = 'C';
        public const char RESET = 'O';

        static Configuration()
        {
            DECIMAL_SEPARATOR = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator =
                ",";
            ERROR_MESSAGE = "-E-";
            MAX_DISPLAY_LENGTH = 10;
            _DEFAULT_ = "0";
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
        private IMemoryOperation _memory;


        public ICalculatorServices Service { get; set; }

        // Constructor
        public Calculator()
        {
            // New calculator is 'ZeroState' by default
            //this._display = start;
            this._memory = new Memory();
            this._state = new ZeroState(new Display(), this);
            this.Service = new CalculatorServices();
        }

        // Properties
        public State State
        {
            get { return _state; }
            set { _state = value; }
        }

        public double Memory
        {
            get { return _memory.Recall(); }
            set { _memory.Store(value); }
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

    #region INPUT
    /// <summary>
    /// There are six possible inputs
    /// </summary>
    public enum InputType
    {
        Zero,
        NonZeroDigit,
        DecimalSeparator,
        UnaryMathOp,
        BinaryMathOp,
        MemoryOp,
        Equals,
        Clear,
        Reset,
        _INVALID_
    }

    public class Input
    {
        protected internal InputType Type;
        protected internal char Value;

        public Input(char inPressedDigit)
        {
            this.Value = inPressedDigit;
            this.Type = inPressedDigit.GetInputType();
        }
    }
    #endregion

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
        // Overloaded constructors
        public ZeroState(State state) : this(state.Display, state.Calculator, state.PendingOperation)
        { }

        public ZeroState(IDisplay display, Calculator calculator, Operation pendingOperation = null)
        {
            this.calculator = calculator;
            this.display = display;
            this.PendingOperation = pendingOperation;
            Initialize();
        }

        private void Initialize()
        {
            //this.display.Set("0");
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
                case InputType.BinaryMathOp:
                    // ACTION: go to Computed or ErrorState state.
                    // If there is pending op, update the display based on the result of the calculation (or error).
                    // Also, if calculation was successful, push a new pending op, built from the event, using a current number of "0".
                    // NEW STATE: ComputedState

                    calculator.State = new ComputedState(this);
                    break;
                case InputType.UnaryMathOp:

                    break;
                case InputType.MemoryOp:

                    break;
                case InputType.Reset:
                    // ACTION: reset
                    // NEW_STATE: ZeroState
                    calculator.Reset();
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
                //case InputType._INVALID_:
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
        public AccumulatorState(State state) : this(state.Display, state.Calculator, state.PendingOperation)
        { }

        public AccumulatorState(IDisplay display, Calculator calculator, Operation pendingOperation = null)
        {
            this.calculator = calculator;
            this.display = display;
            this.PendingOperation = pendingOperation;
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
                    // ACTION: Append "0" to the buffer.
                    // NEW_STATE: AccumulatorState
                    this.DigitAccumulator = calculator.Service.AccumulateZero(DigitAccumulator);
                    this.display.Append(input);
                    calculator.State = new AccumulatorState(this);
                    break;
                case InputType.NonZeroDigit:
                    // ACTION: Append the digit to the buffer.
                    // NEW_STATE: AccumulatorState
                    this.DigitAccumulator = calculator.Service.AccumulateNonZeroDigit(input.Value, DigitAccumulator);
                    this.display.Append(input);
                    calculator.State = new AccumulatorState(this);
                    break;
                case InputType.DecimalSeparator:
                    // ACTION: Append the separator to the buffer, and transition to new state.
                    // NEW_STATE: AccumulatorDecimalState
                    this.DigitAccumulator = calculator.Service.AccumulateSeparator(DigitAccumulator);
                    this.display.Append(input);
                    calculator.State = new AccumulatorDecimalState(this);
                    break;
                case InputType.BinaryMathOp:
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

        public AccumulatorDecimalState(State state) : this(state.Display, state.Calculator, state.PendingOperation)
        { }

        public AccumulatorDecimalState(IDisplay display, Calculator calculator, Operation pendingOperation = null)
        {
            this.calculator = calculator;
            this.display = display;
            this.PendingOperation = pendingOperation;
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
                    // ACTION: Append "0" to the buffer.
                    // NEW_STATE: AccumulatorState
                    this.DigitAccumulator = calculator.Service.AccumulateZero(DigitAccumulator);
                    this.display.Append(input);
                    calculator.State = new AccumulatorDecimalState(this);
                    break;
                case InputType.NonZeroDigit:
                    // ACTION: Append the digit to the buffer.
                    // NEW_STATE: AccumulatorState
                    this.DigitAccumulator = calculator.Service.AccumulateNonZeroDigit(input.Value, DigitAccumulator);
                    this.display.Append(input);
                    calculator.State = new AccumulatorDecimalState(this);
                    break;
                case InputType.DecimalSeparator:
                    // ACTION: (ignore)
                    // NEW_STATE: AccumulatorDecimalState
                    break;
                case InputType.BinaryMathOp:
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

        public ComputedState(State state) : this(state.Display, state.Calculator, state.PendingOperation)
        { }

        public ComputedState(IDisplay display, Calculator calculator, Operation pendingOperation = null)
        {
            this.calculator = calculator;
            this.display = display;
            this.PendingOperation = pendingOperation;
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
                case InputType.BinaryMathOp:
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
        private readonly string _errorMessage = Configuration.ERROR_MESSAGE;

        public ErrorState(State state)
        {
            this.calculator = state.Calculator;
            this.display = state.Display;
            Initialize();
        }

        private void Initialize()
        {
            display.Set(_errorMessage);
            //this.PendingOperation = null;
        }

        public override void Press(char inPressedDigit)
        {
            Input input = new Input(inPressedDigit);
            switch (input.Type)
            {
                case InputType.Clear:
                    // ACTION: Go to Zero state. Clear any pending op.
                    // NEW_STATE: ZeroState
                    PendingOperation = null;
                    calculator.State = new ZeroState(this);
                    break;
            }
        }

        private void StateChangeCheck()
        {

        }
    }

    #region DISPLAY
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
        private string buffer; // = String.Empty;

        public Display() : this(Configuration._DEFAULT_)
        {
        }

        public Display(string text)
        {
            this.buffer = text;
        }

        public override string ToString()
        {
            return Format();
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
    #endregion

    #region OPERATION
    public interface IOperation
    {
        double Compute(params double[] operands);
        double Result { get; set; }
        double[] Operands { get; }
    }

    public abstract class Operation : IOperation
    {
        public double FirstOperand;
        public double SecondOperand;

        public abstract double Result { get; set; }
        public double[] Operands { get { return new double[] { FirstOperand, SecondOperand }; } }

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
            var count = operands.Length;
            if (count < 1)
            {
                throw new ArgumentException("Not enough numbers for operation");
            }
            if (count > 2)
            {
                throw new ArgumentException("Too many numbers for supported operations");
            }

            FirstOperand = operands[0];
            SecondOperand = (count > 1) ? operands[1] : 0.0;

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

        void Clear();
    }

    public class Memory : IMemoryOperation
    {
        private static double _MEMORY_; // = 0.0;
        private const double _NO_VALUE_ = 0.0;

        public double Compute(params double[] operands)
        {
            int count = operands.Length;

            if (count == 0)
            {
                return Recall();
            }
            if (count == 1)
            {
                double number = operands[0];
                Store(number);
                return number;
            }

            // else
            throw new ArgumentException("Too many numbers for supported operations");
        }

        public double Result
        {
            get { return Recall(); }
            set { Store(value); }
        }

        // no operands
        public double[] Operands { get { return new double[] { }; } }

        public void Store(double d)
        {
            // spremanjem broja u memoriju briše se prethodno spremljeni broj
            _MEMORY_ = d;
        }

        public double Recall()
        {
            return _MEMORY_;
        }

        public void Clear()
        {
            _MEMORY_ = _NO_VALUE_;
        }
    }

    public class AdditionOperation : Operation, IBinaryOperation
    {
        public double Compute(double operand0, double operand1)
        {
            //Result = operand0 + operand1;
            return Result = operand0 + operand1;
        }

        public override double Result { get; set; }

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
            return Result = operand0 - operand1;
        }

        public override double Result { get; set; }
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
            return Result = operand0 * operand1;
        }

        public override double Result { get; set; }
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
            return Result = operand0 / operand1;
        }

        public override double Result { get; set; }
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
            return Result = -operand0;
        }

        public override double Result { get; set; }
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
            return Result = Math.Sin(operand0);
        }

        public override double Result { get; set; }
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
            return Result = Math.Cos(operand0);
        }

        public override double Result { get; set; }
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
            return Result = Math.Tan(operand0);
        }

        public override double Result { get; set; }
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
            return Result = operand0 * operand0;
        }

        public override double Result { get; set; }
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
            return Result = Math.Sqrt(operand0);
        }

        public override double Result { get; set; }
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
            return Result = 1.0 / operand0;
        }

        public override double Result { get; set; }
        public override string KEYWORD { get { return "I"; } }

        public sealed override double Compute(params double[] operands)
        {
            base.Compute(operands);
            return Compute(operands[0]);
        }
    }


    public static class OperationServices
    {
        public static Operation NextOperation(this Operation current, Operation next)
        {
            next.FirstOperand = current.Result;

            return next;
        }
    }
    #endregion

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

        public static InputType GetInputType(this char inputChar)
        {
            if (inputChar == '0')
            {
                return InputType.Zero;
            }
            if (Char.IsDigit(inputChar))
            {
                return InputType.NonZeroDigit;
            }
            if (inputChar == ',')
            {
                return InputType.DecimalSeparator;
            }
            if (inputChar == Configuration.EQUALS)
            {
                return InputType.Equals;
            }
            if (inputChar == Configuration.CLEAR)
            {
                return InputType.Clear;
            }
            if (inputChar == Configuration.RESET)
            {
                return InputType.Reset;
            }

            var binChars = new char[]
            {
                Configuration.ADDITION
                , Configuration.SUBTRACTION
                , Configuration.MULTIPLICATION
                , Configuration.DIVISION
            };
            var unChars = new char[]
            {
                Configuration.SINUS
                , Configuration.KOSINUS
                , Configuration.TANGENS
                , Configuration.QUADRAT
                , Configuration.ROOT
                , Configuration.INVERS
            };
            var memChars = new char[]
            {
                Configuration.PUT
                , Configuration.GET
            };
            if (binChars.Contains(inputChar))
            {
                return InputType.BinaryMathOp;
            }
            if (unChars.Contains(inputChar))
            {
                return InputType.UnaryMathOp;
            }
            if (memChars.Contains(inputChar))
            {
                return InputType.MemoryOp;
            }

            return InputType._INVALID_;
        }

        public static IOperation ToOperation(this Input input)
        {
            if (input.Type == InputType._INVALID_ || (input.Type != InputType.BinaryMathOp && input.Type != InputType.UnaryMathOp && input.Type != InputType.MemoryOp))
            {
                throw new ArgumentException("Only mathematical operators can be turned into operation");
            }

            switch (input.Value)
            {
                case Configuration.ADDITION:
                    return new AdditionOperation();
                case Configuration.SUBTRACTION:
                    return new SubtractionOperation();
                case Configuration.MULTIPLICATION:
                    return new MultiplicationOperation();
                case Configuration.DIVISION:
                    return new DivisionOperation();
                case Configuration.MINUS:
                    return new MinusOperation();
                case Configuration.SINUS:
                    return new SinusOperation();
                case Configuration.KOSINUS:
                    return new CosinusOperation();
                case Configuration.TANGENS:
                    return new TangensOperation();
                case Configuration.QUADRAT:
                    return new QuadratOperation();
                case Configuration.ROOT:
                    return new RootOperation();
                case Configuration.INVERS:
                    return new InversOperation();
                case Configuration.PUT:
                case Configuration.GET:
                    return new Memory();
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

        public static IDisplay Append(this IDisplay display, Operation input)
        {
            display.Append(input.KEYWORD);
            return display;
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

        //public static double Clear(this double value)
        //{
        //    value = 0.0;
        //    return value;
        //}

        public static Calculator Reset(this Calculator calculator)
        {
            calculator.State = new ZeroState(new Display("0"), calculator);
            calculator.Memory = 0.0;

            calculator = new Calculator();

            return calculator;
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
