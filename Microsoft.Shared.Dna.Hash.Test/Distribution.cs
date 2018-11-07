//-----------------------------------------------------------------------------
// <copyright file="Distribution.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Hash.Test
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Measures how uniformly distributed a set of hashes are.
    /// </summary>
    internal sealed class Distribution
    {
        /// <summary>
        /// Scaling factor to convert a discrete 32-bit value to a continuous value between zero and one.
        /// </summary>
        private const double Scale = 4294967296D;

        /// <summary>
        /// The list of emperically observed values.
        /// </summary>
        private List<double> observations;

        /// <summary>
        /// Initializes a new instance of the <see cref="Distribution"/> class.
        /// </summary>
        public Distribution()
        {
            this.observations = new List<double>();
        }

        /// <summary>
        /// Records an observed value.
        /// </summary>
        /// <param name="value">The value to record.</param>
        public void Observe(int value)
        {
            uint discrete = (uint)value;
            this.observations.Add(discrete / Distribution.Scale);
        }

        /// <summary>
        /// Resets the distribution for a new series.
        /// </summary>
        public void Reset()
        {
            this.observations.Clear();
        }

        /// <summary>
        /// Computes the Kolmogorov–Smirnov statistic for the distribution of the observed hashes compared to a uniform distribution.
        /// </summary>
        /// <returns>The K-S statistic. The closer this is to zero the better the distribution.</returns>
        public double Finish()
        {
            this.observations.Sort();
            double n = this.observations.Count;
            double s = 1D / n;
            double result = 0D;
            for (double u = s; u <= 1D; u += s)
            {
                double e = this.EmpericalDistribution(u);
                double absolute = Math.Abs(e - u);
                if (absolute > result)
                {
                    result = absolute;
                }
            }

            return result;
        }

        /// <summary>
        /// Computes the cumulative emperical distribution at a given value.
        /// </summary>
        /// <param name="value">The target value.</param>
        /// <returns>The fraction of observations less than the target value.</returns>
        private double EmpericalDistribution(double value)
        {
            int length = this.observations.Count;
            int lower = 0;
            int upper = length - 1;
            while (lower < upper)
            {
                int i = ((upper - lower) / 2) + lower;
                int n = i + 1;
                if (value < this.observations[i])
                {
                    upper = i - 1;
                }
                else
                {
                    lower = n;
                }
            }

            return (double)lower / length;
        }
    }
}
