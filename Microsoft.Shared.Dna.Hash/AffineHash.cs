//-----------------------------------------------------------------------------
// <copyright file="AffineHash.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Hash
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Hashes data using an affine hash.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Performance",
        "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "You don't have to use everything in an in-line code share.")]
    internal sealed class AffineHash
    {
        /// <summary>
        /// The exclusive maximum value for <see cref="Scale"/> and <see cref="Shift"/>;
        /// </summary>
        public const long MaxFactor = 1L << AffineHash.BlockBits;

        /// <summary>
        /// The number of bits in a hash block.
        /// </summary>
        private const int BlockBits = 32;

        /// <summary>
        /// The mask value for block bits.
        /// </summary>
        private const ulong BlockMask = (1UL << AffineHash.BlockBits) - 1UL;

        /// <summary>
        /// The scale factor for the hash.
        /// </summary>
        private readonly ulong scale;

        /// <summary>
        /// The shift factor for the hash.
        /// </summary>
        private readonly ulong shift;

        /// <summary>
        /// Initializes a new instance of the <see cref="AffineHash"/> class.
        /// </summary>
        /// <param name="random">The random number generator that will select a scale and shift factor.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public AffineHash(Random random) : this(random, AffineHash.NextScale(random))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AffineHash"/> class.
        /// </summary>
        /// <param name="scale">The scale factor for the hash.</param>
        /// <param name="shift">The shift factor for the hash.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        public AffineHash(long scale, long shift)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentOutOfRangeException>(scale > 0L);
            Contract.Requires<ArgumentOutOfRangeException>(scale < AffineHash.MaxFactor);
            Contract.Requires<ArgumentException>(scale % 2 != 0);
            Contract.Requires<ArgumentOutOfRangeException>(shift >= 0L);
            Contract.Requires<ArgumentOutOfRangeException>(shift < AffineHash.MaxFactor);
#endif
            this.scale = (ulong)scale;
            this.shift = (ulong)shift;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="AffineHash"/> class from being created.
        /// </summary>
        private AffineHash()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AffineHash"/> class.
        /// </summary>
        /// <param name="random">The random number generator that will select a shift factor.</param>
        /// <param name="scale">The scale factor for the hash.</param>
        private AffineHash(Random random, long scale) : this(scale, AffineHash.NextShift(random, scale))
        {
        }

        /// <summary>
        /// Gets the scale factor for the hash.
        /// </summary>
        public long Scale => (long)this.scale;

        /// <summary>
        /// Gets the shift factor for the hash.
        /// </summary>
        public long Shift => (long)this.shift;

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(byte value)
        {
            return (int)this.ComputeSmallBlock((ulong)value);
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(byte[] value)
        {
            return this.GetHashCode(value, 0, value?.Length ?? 0);
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <param name="offset">The starting index of the segment.</param>
        /// <param name="length">The length of the segment.</param>
        /// <returns>The hash code corresponding to the value segment.</returns>
        public int GetHashCode(byte[] value, int offset, int length)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentNullException>(value != null);
            Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(offset + length <= value.Length);
#endif
            unsafe
            {
                fixed (byte* valuePointer = value)
                {
                    byte* offsetPointer = valuePointer + offset;
                    if (BitConverter.IsLittleEndian)
                    {
                        return this.GetHashCodeLittleEndian(offsetPointer, length);
                    }
                    else
                    {
                        return this.GetHashCodeBigEndian(offsetPointer, length);
                    }
                }
            }
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(char value)
        {
            return (int)this.ComputeSmallBlock((ulong)value);
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(char[] value)
        {
            return this.GetHashCode(value, 0, value?.Length ?? 0);
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <param name="offset">The starting index of the segment.</param>
        /// <param name="length">The length of the segment.</param>
        /// <returns>The hash code corresponding to the value segment.</returns>
        public int GetHashCode(char[] value, int offset, int length)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentNullException>(value != null);
            Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(offset + length <= value.Length);
#endif
            unsafe
            {
                fixed (char* valuePointer = value)
                {
                    char* offsetPointer = valuePointer + offset;
                    if (BitConverter.IsLittleEndian)
                    {
                        return this.GetHashCodeLittleEndian(offsetPointer, length);
                    }
                    else
                    {
                        return this.GetHashCodeBigEndian(offsetPointer, length);
                    }
                }
            }
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(DateTime value)
        {
            return (int)this.ComputeBigBlock((ulong)value.Ticks);
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(DateTimeOffset value)
        {
            return (int)this.ComputeBigBlock((ulong)value.UtcTicks);
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(decimal value)
        {
            unsafe
            {
                /* Crack the layout of the structure. https://referencesource.microsoft.com/#mscorlib/system/decimal.cs */
                byte* valuePointer = (byte*)&value;
                ulong flags = (ulong)(*(int*)valuePointer);
                ulong hi = (ulong)(*(int*)(valuePointer + sizeof(int)));
                ulong lo = (ulong)(*(int*)(valuePointer + sizeof(int) + sizeof(int)));
                ulong mid = (ulong)(*(int*)(valuePointer + sizeof(int) + sizeof(int) + sizeof(int)));
                return (int)this.ComputeSmallBlock(this.ComputeSmallBlock(this.ComputeSmallBlock(this.ComputeSmallBlock(flags) + hi) + lo) + mid);
            }
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(double value)
        {
            return (int)this.ComputeBigBlock((ulong)BitConverter.DoubleToInt64Bits(value));
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(float value)
        {
            return (int)this.ComputeBigBlock((ulong)BitConverter.DoubleToInt64Bits(value));
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(Guid value)
        {
            unsafe
            {
                /* Crack the layout of the structure. https://referencesource.microsoft.com/#mscorlib/system/guid.cs */
                byte* valuePointer = (byte*)&value;
                ulong a = (ulong)(*(int*)valuePointer);
                ulong b = (ulong)(*(short*)(valuePointer + sizeof(int)));
                ulong c = (ulong)(*(short*)(valuePointer + sizeof(int) + sizeof(short)));
                ulong d = (ulong)(*(valuePointer + sizeof(int) + sizeof(short) + sizeof(short)));
                ulong e = (ulong)(*(valuePointer + sizeof(int) + sizeof(short) + sizeof(short) + sizeof(byte)));
                ulong f = (ulong)(*(valuePointer + sizeof(int) + sizeof(short) + sizeof(short) + sizeof(byte) + sizeof(byte)));
                ulong g = (ulong)(*(valuePointer + sizeof(int) + sizeof(short) + sizeof(short) + sizeof(byte) + sizeof(byte) + sizeof(byte)));
                ulong h = (ulong)(*(valuePointer + sizeof(int) + sizeof(short) + sizeof(short) + sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte)));
                ulong i = (ulong)(*(valuePointer + sizeof(int) + sizeof(short) + sizeof(short) + sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte)));
                ulong j = (ulong)(*(valuePointer + sizeof(int) + sizeof(short) + sizeof(short) + sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte)));
                ulong k = (ulong)(*(valuePointer + sizeof(int) + sizeof(short) + sizeof(short) + sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte)));
                ulong bc = (b << 16) + c;
                ulong dg = (d << 24) + (e << 16) + (f << 8) + g;
                ulong hk = (h << 24) + (i << 16) + (j << 8) + k;
                return (int)this.ComputeSmallBlock(this.ComputeSmallBlock(this.ComputeSmallBlock(this.ComputeSmallBlock(a) + bc) + dg) + hk);
            }
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(int value)
        {
            return (int)this.ComputeSmallBlock((ulong)value);
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(long value)
        {
            return (int)this.ComputeBigBlock((ulong)value);
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(sbyte value)
        {
            return (int)this.ComputeSmallBlock((ulong)value);
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(short value)
        {
            return (int)this.ComputeSmallBlock((ulong)value);
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(string value)
        {
            return this.GetHashCode(value, 0, value?.Length ?? 0);
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <param name="offset">The starting index of the segment.</param>
        /// <param name="length">The length of the segment.</param>
        /// <returns>The hash code corresponding to the value segment.</returns>
        public int GetHashCode(string value, int offset, int length)
        {
#if CONTRACTS_FULL // Work around the implicit rewrite requirements of Contract.Requires<T>
            Contract.Requires<ArgumentNullException>(value != null);
            Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(offset + length <= value.Length);
#endif
            unsafe
            {
                fixed (char* valuePointer = value)
                {
                    char* offsetPointer = valuePointer + offset;
                    if (BitConverter.IsLittleEndian)
                    {
                        return this.GetHashCodeLittleEndian(offsetPointer, length);
                    }
                    else
                    {
                        return this.GetHashCodeBigEndian(offsetPointer, length);
                    }
                }
            }
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(uint value)
        {
            return (int)this.ComputeSmallBlock((ulong)value);
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(ulong value)
        {
            return (int)this.ComputeBigBlock(value);
        }

        /// <summary>
        /// Computes a hash code for a value.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The hash code corresponding to the value.</returns>
        public int GetHashCode(ushort value)
        {
            return (int)this.ComputeSmallBlock((ulong)value);
        }

        /// <summary>
        /// Determines if two integers have any common factors other than one.
        /// </summary>
        /// <param name="left">The left integer.</param>
        /// <param name="right">The right integer.</param>
        /// <returns>A value indicating whether the two integers are coprime.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private static bool Coprime(long left, long right)
        {
            long dividend = left;
            long divisor = right;
            while (dividend != 0L && divisor != 0L)
            {
                long remainder = dividend % divisor;
                if (remainder == 0L)
                {
                    return divisor == 1L;
                }

                dividend = divisor;
                divisor = remainder;
            }

            return false;
        }

        /// <summary>
        /// Generates a random affine factor value.
        /// </summary>
        /// <param name="random">A random number generator.</param>
        /// <param name="minValue">The inclusive minimum value to select.</param>
        /// <returns>A random factor value.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private static long NextFactor(Random random, long minValue)
        {
            ulong result = 0UL;
            byte[] buffer = new byte[sizeof(ulong)];
            random.NextBytes(buffer);
            result = BitConverter.ToUInt64(buffer, 0);

            ulong m = (ulong)minValue;
            if (m > result)
            {
                result += m;
            }

            result &= AffineHash.BlockMask & ~1UL;
            result++;
            return (long)result;
        }

        /// <summary>
        /// Generates a random affine scale factor for a given domain.
        /// </summary>
        /// <param name="random">A random number generator.</param>
        /// <returns>A random scale factor.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private static long NextScale(Random random)
        {
            return AffineHash.NextFactor(random, 1L);
        }

        /// <summary>
        /// Generates a random affine shift factor for a given domain.
        /// </summary>
        /// <param name="random">A random number generator.</param>
        /// <param name="scale">The scale factor for the hash.</param>
        /// <returns>A random shift factor.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance",
            "CA1811:AvoidUncalledPrivateCode",
            Justification = "You don't have to use everything in an in-line code share.")]
        private static long NextShift(Random random, long scale)
        {
            long result = scale;
            while (!AffineHash.Coprime(result, scale))
            {
                result = AffineHash.NextFactor(random, 0L);
            }

            return result;
        }

        /// <summary>
        /// Computes a partial hash code for a value that could overflow the affine function.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The partial hash code corresponding to the value.</returns>
        private ulong ComputeBigBlock(ulong value)
        {
            return this.ComputeSmallBlock(this.ComputeSmallBlock(value >> AffineHash.BlockBits) + (value & AffineHash.BlockMask));
        }

        /// <summary>
        /// Computes a partial hash code for a value that will not overflow the affine function.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <returns>The partial hash code corresponding to the value.</returns>
        private ulong ComputeSmallBlock(ulong value)
        {
            ulong affine = (this.scale * value) + this.shift;
            return (affine >> AffineHash.BlockBits) ^ (affine & AffineHash.BlockMask);
        }

        /// <summary>
        /// Computes a hash code for a binary segment on big-endian architectures.
        /// </summary>
        /// <param name="valuePointer">The pointer to the binary segment.</param>
        /// <param name="length">The length of the binary segment.</param>
        /// <returns>The hash code corresponding to the binary segment.</returns>
        private unsafe int GetHashCodeBigEndian(byte* valuePointer, int length)
        {
            ulong result = 0UL;
            ulong block = 0U;
            int remainder = length & 3; // mod 4
            int cutoff = length - remainder;
            int i = 0;
            while (i < cutoff)
            {
                block =
                    ((ulong)(*(valuePointer + i++)) << 24) |
                    ((ulong)(*(valuePointer + i++)) << 16) |
                    ((ulong)(*(valuePointer + i++)) << 8) |
                    ((ulong)(*(valuePointer + i++)));
                result = this.ComputeSmallBlock(result + block);
            }

            if (remainder == 0)
            {
                return (int)result;
            }

            byte* remainderPointer = valuePointer + cutoff;
            int offset = 24;
            block = 0UL;
            for (i = 0; i < remainder; i++)
            {
                block |= (ulong)(*(remainderPointer + i)) << offset;
                offset -= 8;
            }

            return (int)this.ComputeSmallBlock(result + block);
        }

        /// <summary>
        /// Computes a hash code for a text segment on big-endian architectures.
        /// </summary>
        /// <param name="valuePointer">The pointer to the text segment.</param>
        /// <param name="length">The length of the text segment.</param>
        /// <returns>The hash code corresponding to the text segment.</returns>
        private unsafe int GetHashCodeBigEndian(char* valuePointer, int length)
        {
            ulong result = 0UL;
            ulong block = 0UL;
            int remainder = length & 1; // mod 2
            int cutoff = length - remainder;
            int i = 0;
            while (i < cutoff)
            {
                block =
                    ((ulong)(*(valuePointer + i++)) << 16) |
                    ((ulong)(*(valuePointer + i++)));
                result = this.ComputeSmallBlock(result + block);
            }

            if (remainder == 0)
            {
                return (int)result;
            }

            block = (ulong)(*(valuePointer + cutoff)) << 16;
            return (int)this.ComputeBigBlock(result + block);
        }

        /// <summary>
        /// Computes a hash code for a binary segment on little-endian architectures.
        /// </summary>
        /// <param name="valuePointer">The pointer to the binary segment.</param>
        /// <param name="length">The length of the binary segment.</param>
        /// <returns>The hash code corresponding to the binary segment.</returns>
        private unsafe int GetHashCodeLittleEndian(byte* valuePointer, int length)
        {
            ulong result = 0UL;
            ulong block = 0UL;
            int blocks = length >> 2; // div 4
            uint* blockPointer = (uint*)valuePointer;
            for (int i = 0; i < blocks; i++)
            {
                block = *(blockPointer + i);
                result = this.ComputeSmallBlock(result + block);
            }

            int remainder = length & 3; // mod 4
            if (remainder == 0)
            {
                return (int)result;
            }

            byte* remainderPointer = valuePointer + length - remainder;
            int offset = 0;
            block = 0U;
            for (int i = 0; i < remainder; i++)
            {
                block |= (ulong)(*(remainderPointer + i)) << offset;
                offset += 8;
            }

            return (int)this.ComputeSmallBlock(result + block);
        }

        /// <summary>
        /// Computes a hash code for a text segment on little-endian architectures.
        /// </summary>
        /// <param name="valuePointer">The pointer to the text segment.</param>
        /// <param name="length">The length of the text segment.</param>
        /// <returns>The hash code corresponding to the text segment.</returns>
        private unsafe int GetHashCodeLittleEndian(char* valuePointer, int length)
        {
            return this.GetHashCodeLittleEndian((byte*)valuePointer, length * sizeof(char));
        }
    }
}
