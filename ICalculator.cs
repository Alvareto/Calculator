namespace PrvaDomacaZadaca_Kalkulator
{
    public interface ICalculator
    {
        /// <summary>
        /// Preko ovoga se kalkulatoru zadaje koja je tipka pritisnuta
        /// </summary>
        /// <param name="inPressedDigit">pritisnuta tipka</param>
        void Press(char inPressedDigit);
        /// <summary>
        /// Vraća trenutno stanje ekrana kalkulatora
        /// </summary>
        /// <returns></returns>
        string GetCurrentDisplayState();
    }
}