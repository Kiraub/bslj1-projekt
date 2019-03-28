using System;

namespace Bitgraph
{
    /// <summary>
    /// Repräsentiert eine polynomiale Funktion zweiten Grades der Form f(x) = a*x^2 + b*x + c
    /// </summary>
    public struct QuadPolynomial
    {
        /// <summary>
        /// Faktor a(x) in a*x^2
        /// </summary>
        public Func<float, float> two;
        /// <summary>
        /// Faktor b(x) in b*x
        /// </summary>
        public Func<float, float> one;
        /// <summary>
        /// Summand c(x) in c
        /// </summary>
        public Func<float, float> zero;
        /// <summary>
        /// Funktionswert für geg. Eingabe
        /// </summary>
        /// <param name="xValue">Eingabewert</param>
        /// <returns>Funktionswert</returns>
        public float FunctionValue(float xValue) => (float)Math.Pow(xValue, 2.0) * two(xValue) + one(xValue) * xValue + zero(xValue);
    }
}
