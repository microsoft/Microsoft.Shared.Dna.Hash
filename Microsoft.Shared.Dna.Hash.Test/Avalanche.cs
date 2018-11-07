//-----------------------------------------------------------------------------
// <copyright file="Avalanche.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Hash.Test
{
    using System;

    /// <summary>
    /// Contains methods for performing various bit arithmetic operations.
    /// </summary>
    internal class Avalanche
    {
        /// <summary>
        /// The total number of changed bits across all hashes and patterns.
        /// </summary>
        private int grandTotal = 0;

        /// <summary>
        /// The maximum possible number of changed bits across all hashes and patterns.
        /// </summary>
        private int grandLimit = 0;

        /// <summary>
        /// The current hash algorithm.
        /// </summary>
        private AffineHash hash = null;

        /// <summary>
        /// The length of the <see cref="source"/> and <see cref="target"/> arrays.
        /// </summary>
        private int length = 0;

        /// <summary>
        /// The maximum possible number of changed bits for the current pattern.
        /// </summary>
        private int limit = 0;

        /// <summary>
        /// The hash value of the current pattern.
        /// </summary>
        private int reference = 0;

        /// <summary>
        /// The source array pattern.
        /// </summary>
        private byte[] source = null;

        /// <summary>
        /// The target array pattern.
        /// </summary>
        private byte[] target = null;

        /// <summary>
        /// The total number of bits in <see cref="source"/> and <see cref="target"/>.
        /// </summary>
        private int toggleBits = 0;

        /// <summary>
        /// Resets the hash algorithm.
        /// </summary>
        /// <param name="candidate">The new candidate algorithm.</param>
        public void Reset(AffineHash candidate)
        {
            this.hash = candidate;
            this.grandLimit = 0;
            this.grandTotal = 0;
        }

        /// <summary>
        /// Changes the target hash pattern.
        /// </summary>
        /// <param name="pattern">The new pattern to measure.</param>
        public void Next(byte[] pattern)
        {
            this.source = pattern;
            this.length = this.source.Length;
            this.toggleBits = 8 * this.length;
            this.reference = this.hash.GetHashCode(this.source);
            this.limit = this.toggleBits * 32;
            this.grandLimit += this.limit;
            if (this.target == null || this.length != this.target.Length)
            {
                this.target = new byte[this.length];
            }

            this.source.CopyTo(this.target, 0);
        }

        /// <summary>
        /// Measures the predictablity of hashes generated using a given algorithm and reference data.
        /// </summary>
        /// <returns>The distance the observed entropy is from perfect. Lower is better.</returns>
        public double Predictability()
        {
            int total = 0;
            for (int i = 0; i < this.toggleBits; i++)
            {
                this.Toggle(i);
                int delta = this.hash.GetHashCode(this.target);
                int diff = Avalanche.HammingDistance(this.reference, delta);
                total += diff;
            }

            this.grandTotal += total;
            return Avalanche.Predictability(total, this.limit);
        }

        /// <summary>
        /// Finishes the avalanche series for a given algorithm.
        /// </summary>
        /// <returns>The distance the observed entropy is from perfect. Lower is better.</returns>
        public double Finish()
        {
            return Avalanche.Predictability(this.grandTotal, this.grandLimit);
        }

        /// <summary>
        /// Measures the predictablity of two series of paired bits.
        /// </summary>
        /// <param name="total">The total number of bits changed in the series.</param>
        /// <param name="limit">The maximum number of bits that could have possibly changed.</param>
        /// <returns>The distance the observed entropy is from perfect. Lower is better.</returns>
        private static double Predictability(int total, int limit)
        {
            return Math.Abs(0.5D - ((double)total / limit));
        }

        /// <summary>
        /// Calculates the number of bits that are different between two hashes.
        /// </summary>
        /// <param name="left">The left hash.</param>
        /// <param name="right">The right hash.</param>
        /// <returns>The number of different bits.</returns>
        private static int HammingDistance(int left, int right)
        {
            uint diff = (uint)(left ^ right);
            int result = 0;
            while (diff != 0U)
            {
                diff &= diff - 1U;
                result++;
            }

            return result;
        }

        /// <summary>
        /// Toggles a single bit in the target array.
        /// </summary>
        /// <param name="bit">The bit to toggle.</param>
        private void Toggle(int bit)
        {
            int offset = bit / 8;
            int toggle = 1 << (bit % 8);
            this.target[offset] = (byte)(this.source[offset] ^ toggle);
            if (offset > 0 && toggle == 1)
            {
                int previous = offset - 1;
                this.target[previous] = this.source[previous];
            }
        }
    }
}
