﻿//==============================================================================
// Project:     Trading Simulator
// Name:        IndicatorsMomentum
// Description: collection of momentum-based indicators
// History:     2018ix15, FUB, created
//------------------------------------------------------------------------------
// Copyright:   (c) 2017-2018, Bertram Solutions LLC
//              http://www.bertram.solutions
// License:     this code is licensed under GPL-3.0-or-later
//==============================================================================

#region libraries
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace TuringTrader.Simulator
{
    /// <summary>
    /// Collection of momentum-based indicators.
    /// </summary>
    public static class IndicatorsMomentum
    {
        #region public static ITimeSeries<double> CCI(this Instrument series, int n = 20)
        /// <summary>
        /// Calculate Commodity Channel Index of input time series, as described here:
        /// <see href="https://en.wikipedia.org/wiki/Commodity_channel_index"/>
        /// </summary>
        /// <param name="series">input time series (OHLC)</param>
        /// <param name="n">averaging length</param>
        /// <returns>CCI time series</returns>
        public static ITimeSeries<double> CCI(this Instrument series, int n = 20)
        {
            return series.TypicalPrice().CCI(n);
        }
        #endregion
        #region public static ITimeSeries<double> CCI(this ITimeSeries<double> series, int n = 20)
        /// <summary>
        /// Calculate Commodity Channel Index of input time series, as described here:
        /// <see href="https://en.wikipedia.org/wiki/Commodity_channel_index"/>
        /// </summary>
        /// <param name="series">input time series</param>
        /// <param name="n">averaging length</param>
        /// <returns>CCI time series</returns>
        public static ITimeSeries<double> CCI(this ITimeSeries<double> series, int n = 20)
        {
            return IndicatorsBasic.BufferedLambda(
                (v) =>
                {
                    ITimeSeries<double> delta = series.Subtract(series.SMA(n));
                    ITimeSeries<double> meanDeviation = delta.AbsValue().SMA(n);

                    return delta[0] / (0.15 * meanDeviation[0]);
                },
                0.5,
                series.GetHashCode(), n);
        }
        #endregion
        #region public static ITimeSeries<double> TSI(this ITimeSeries<double> series, int r = 25, int s = 13)
        /// <summary>
        /// Calculate True Strength Index of input time series, as described here:
        /// <see href="https://en.wikipedia.org/wiki/True_strength_index"/>
        /// </summary>
        /// <param name="series">input time series</param>
        /// <param name="r">smoothing period for momentum</param>
        /// <param name="s">smoothing period for smoothed momentum</param>
        /// <returns>TSI time series</returns>
        public static ITimeSeries<double> TSI(this ITimeSeries<double> series, int r = 25, int s = 13)
        {
            return IndicatorsBasic.BufferedLambda(
                (v) =>
                {
                    ITimeSeries<double> momentum = series.Return();
                    double numerator = momentum.EMA(r).EMA(s)[0];
                    double denominator = momentum.AbsValue().EMA(r).EMA(s)[0];
                    return 100.0 * numerator / denominator;
                },
                0.5,
                series.GetHashCode(), r, s);
        }
        #endregion
        #region public static ITimeSeries<double> RSI(this ITimeSeries<double> series, int n)
        /// <summary>
        /// Calculate Relative Strength Index, as described here:
        /// <see href="https://en.wikipedia.org/wiki/Relative_strength_index"/>
        /// </summary>
        /// <param name="series">input time series</param>
        /// <param name="n">smoothing period</param>
        /// <returns>RSI time series</returns>
        public static ITimeSeries<double> RSI(this ITimeSeries<double> series, int n = 14)
        {
            return IndicatorsBasic.BufferedLambda(
                (v) =>
                {
                    double avgUp = IndicatorsBasic.Lambda(
                        (t) => Math.Max(0.0, series.Return()[t]),
                        series.GetHashCode(), n).EMA(n)[0];

                    double avgDown = IndicatorsBasic.Lambda(
                        (t) => Math.Max(0.0, -series.Return()[t]),
                        series.GetHashCode(), n).EMA(n)[0];

                    double rs = avgUp / Math.Max(1e-10, avgDown);
                    return 100.0 - 100.0 / (1 + rs);
                },
                50.0,
                series.GetHashCode(), n);
        }
        #endregion
        #region public static ITimeSeries<double> WilliamsPercentR(this Instrument series, int n = 10)
        /// <summary>
        /// Calculate Williams %R, as described here:
        /// <see href="https://en.wikipedia.org/wiki/Williams_%25R"/>
        /// </summary>
        /// <param name="series">input time series (OHLC)</param>
        /// <param name="n">period</param>
        /// <returns>Williams %R as time series</returns>
        public static ITimeSeries<double> WilliamsPercentR(this Instrument series, int n = 10)
        {
            return IndicatorsBasic.BufferedLambda(
                (v) =>
                {
                    double hh = series.High.Highest(n)[0];
                    double ll = series.Low.Lowest(n)[0];

                    //return -100.0 * (hh - series.Close[0]) / (hh - ll);
                    double denom = hh - ll;
                    return denom != 0.0
                        ? -100.0 * (hh - series.Close[0]) / denom
                        : -50.0;
                },
                -50.0,
                series.GetHashCode(), n);
        }
        #endregion
        #region public static ITimeSeries<double> WilliamsPercentR(this ITimeSeries<double> series, int n = 10)
        /// <summary>
        /// Calculate Williams %R, as described here:
        /// <see href="https://en.wikipedia.org/wiki/Williams_%25R"/>
        /// </summary>
        /// <param name="series">input time series</param>
        /// <param name="n">period</param>
        /// <returns>Williams %R as time series</returns>
        public static ITimeSeries<double> WilliamsPercentR(this ITimeSeries<double> series, int n = 10)
        {
            return IndicatorsBasic.BufferedLambda(
                (v) =>
                {
                    double hh = series.Highest(n)[0];
                    double ll = series.Lowest(n)[0];

                    //return -100.0 * (hh - series[0]) / (hh - ll);
                    double denom = hh - ll;
                    return denom != 0.0
                        ? -100.0 * (hh - series[0]) / denom
                        : -50.0;
                },
                -50.0,
                series.GetHashCode(), n);
        }
        #endregion
        #region public static StochasticOscillatorResult StochasticOscillator(this Instrument series, int n = 14)
        /// <summary>
        /// Calculate Stochastic Oscillator, as described here:
        /// <see href="https://en.wikipedia.org/wiki/Stochastic_oscillator"/>
        /// </summary>
        /// <param name="series">input time series (OHLC)</param>
        /// <param name="n">oscillator period</param>
        /// <returns>Stochastic Oscillator as time series</returns>
        public static StochasticOscillatorResult StochasticOscillator(this Instrument series, int n = 14)
        {
            var container = Cache<StochasticOscillatorResult>.GetData(
                    Cache.UniqueId(series.GetHashCode(), n),
                    () => new StochasticOscillatorResult());

            container.PercentK = IndicatorsBasic.BufferedLambda(
                (v) =>
                {
                    double hh = series.High.Highest(n)[0];
                    double ll = series.Low.Lowest(n)[0];

                    //return 100.0 * (hh - series[0].Close) / (hh - ll);
                    double denom = hh - ll;
                    return denom != 0.0
                        ? 100.0 * (hh - series[0].Close) / denom
                        : 50.0;
                },
                50.0,
                series.GetHashCode(), n);

            container.PercentD = container.PercentK.SMA(3);

            return container;
        }
        #endregion
        #region public static StochasticOscillatorResult StochasticOscillator(this ITimeSeries<double> series, int n = 14)
        /// <summary>
        /// Calculate Stochastic Oscillator, as described here:
        /// <see href="https://en.wikipedia.org/wiki/Stochastic_oscillator"/>
        /// </summary>
        /// <param name="series">input time series</param>
        /// <param name="n">oscillator period</param>
        /// <returns>Stochastic Oscillator as time series</returns>
        public static StochasticOscillatorResult StochasticOscillator(this ITimeSeries<double> series, int n = 14)
        {
            var container = Cache<StochasticOscillatorResult>.GetData(
                    Cache.UniqueId(series.GetHashCode(), n),
                    () => new StochasticOscillatorResult());

            container.PercentK = IndicatorsBasic.BufferedLambda(
                (v) =>
                {
                    double hh = series.Highest(n)[0];
                    double ll = series.Lowest(n)[0];

                    //return 100.0 * (hh - series[0]) / (hh - ll);
                    double denom = hh - ll;
                    return denom != 0.0
                        ? 100.0 * (hh - series[0]) / denom
                        : 50.0;
                },
                50.0,
                series.GetHashCode(), n);

            container.PercentD = container.PercentK.SMA(3);

            return container;
        }

        /// <summary>
        /// Container for Stochastic Oscillator result.
        /// </summary>
        public class StochasticOscillatorResult
        {
            /// <summary>
            /// %K
            /// </summary>
            public ITimeSeries<double> PercentK;

            /// <summary>
            /// %D
            /// </summary>
            public ITimeSeries<double> PercentD;
        }
        #endregion

        #region public static ITimeSeries<double> LinRegression(this ITimeSeries<double> series, int n)
        /// <summary>
        /// Calculate linear regression of time series.
        /// </summary>
        /// <param name="series">input time series</param>
        /// <param name="n">number of bars for regression</param>
        /// <returns>regression parameters as time series</returns>
        public static _Regression LinRegression(this ITimeSeries<double> series, int n)
        {
            var functor = Cache<LinRegressionFunctor>.GetData(
                    Cache.UniqueId(series.GetHashCode(), n),
                    () => new LinRegressionFunctor(series, n));

            functor.Calc();

            return functor;
        }

        /// <summary>
        /// Container holding regression parameters.
        /// </summary>
        public class _Regression
        {
            /// <summary>
            /// Slope as time series.
            /// </summary>
            public ITimeSeries<double> Slope = new TimeSeries<double>();
            /// <summary>
            /// Y-axis intercept as time series.
            /// </summary>
            public ITimeSeries<double> Intercept = new TimeSeries<double>();
            /// <summary>
            /// Coefficient of determination, R2, as time series.
            /// </summary>
            public ITimeSeries<double> R2 = new TimeSeries<double>();
        }
        private class LinRegressionFunctor : _Regression
        {
            private ITimeSeries<double> _series;
            private int _n;

            public LinRegressionFunctor(ITimeSeries<double> series, int n)
            {
                _series = series;
                _n = n;
            }

            public void Calc()
            {
                // simple linear regression
                // https://en.wikipedia.org/wiki/Simple_linear_regression
                // b = sum((x - avg(x)) * (y - avg(y)) / sum((x - avg(x))^2)
                // a = avg(y) - b * avg(x)

                double avgX = Enumerable.Range(-_n + 1, _n)
                    .Average(x => x);
                double avgY = Enumerable.Range(-_n + 1, _n)
                    .Average(x => _series[-x]);
                double sumXx = Enumerable.Range(-_n + 1, _n)
                    .Sum(x => Math.Pow(x - avgX, 2));
                double sumXy = Enumerable.Range(-_n + 1, _n)
                    .Sum(x => (x - avgX) * (_series[-x] - avgY));
                double b = sumXy / sumXx;
                double a = avgY - b * avgX;

                // coefficient of determination
                // https://en.wikipedia.org/wiki/Coefficient_of_determination
                // f = a + b * x
                // SSreg = sum((f - avg(y))^2)
                // SSres = sum((y - f)^2)

                double regressionSumOfSquares = Enumerable.Range(-_n + 1, _n)
                    .Sum(x => Math.Pow(a + b * x - avgY, 2));
                double residualSumOfSquares = Enumerable.Range(-_n + 1, _n)
                    .Sum(x => Math.Pow(a + b * x - _series[-x], 2));
                double r2 = regressionSumOfSquares != 0.0
                    ? 1.0 - residualSumOfSquares / regressionSumOfSquares
                    : 0.0;

                (Slope as TimeSeries<double>).Value = b;
                (Intercept as TimeSeries<double>).Value = a;
                (R2 as TimeSeries<double>).Value = r2;
            }
        }
        #endregion
        #region public static ITimeSeries<double> LogRegression(this ITimeSeries<double> series, int n)
        /// <summary>
        /// Calculate logarithmic regression of time series.
        /// </summary>
        /// <param name="series">input time series</param>
        /// <param name="n">number of bars for regression</param>
        /// <returns>regression parameters as time series</returns>
        public static _Regression LogRegression(this ITimeSeries<double> series, int n)
        {
            return series.Log().LinRegression(n);
        }
        #endregion
    }
}

//==============================================================================
// end of file