//-----------------------------------------------------------------------------
// <copyright file="AffineHashTests.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
//     Licensed under the MIT license. See license file in the project root for full license information.
// </copyright>
//-----------------------------------------------------------------------------

namespace Microsoft.Shared.Dna.Hash.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test methods for <see cref="AffineHash"/>.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AffineHashTests
    {
        /// <summary>
        /// Gets or sets the test context which provides information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Tests <see cref="AffineHash.AffineHash(long, long)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Constructor_Rejects_Invalid_Arguments()
        {
            var cases = new[]
            {
                new { scale = 0L, shift = 0L, expected = typeof(ArgumentOutOfRangeException) },
                new { scale = 1L, shift = -1L, expected = typeof(ArgumentOutOfRangeException) },
                new { scale = 1L << 32, shift = 0L, expected = typeof(ArgumentOutOfRangeException) },
                new { scale = 1L, shift = 1L << 32, expected = typeof(ArgumentOutOfRangeException) },
                new { scale = 2L, shift = 0L, expected = typeof(ArgumentException) },
            };

            for (int i = 0; i < cases.Length; i++)
            {
                var c = cases[i];
                try
                {
                    AffineHash target = new AffineHash(c.scale, c.shift);
                    Assert.Fail("index:{0}, target:{1}", i, target);
                }
                catch (Exception ex)
                {
                    Assert.AreEqual(c.expected, ex.GetType(), "index: {0}", i);
                }
            }
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(byte)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_Byte()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<byte> values = Enumerable.Range(byte.MinValue, byte.MaxValue - byte.MinValue + 1)
                .Select(n => (byte)n);
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(byte[])"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_Byte_Array()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<byte[]> values = AffineHashTests.RandomByteArrays(random, 101, 10000);
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(char)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_Char()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<char> values = Enumerable.Range(char.MinValue, char.MaxValue - char.MinValue + 1)
                .Select(n => (char)n);
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(char[])"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_Char_Array()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<char[]> values = AffineHashTests.RandomCharArrays(random, 101, 10000);
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(DateTime)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_DateTime()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<DateTime> values = AffineHashTests.RandomByteArrays(random, sizeof(ulong), 10000)
                .Select(b => new DateTime((long)(BitConverter.ToUInt64(b, 0) % (ulong)DateTime.MaxValue.Ticks)));
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(DateTime)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_DateTimeOffset()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<DateTimeOffset> values = AffineHashTests.RandomByteArrays(random, sizeof(ulong), 10000)
                .Select(b => new DateTimeOffset((long)(BitConverter.ToUInt64(b, 0) % (ulong)DateTime.MaxValue.Ticks), TimeSpan.FromMinutes(random.Next(-840, 840))));
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(decimal)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_Decimal()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<decimal> values = Enumerable.Range(0, 10000)
                .Select(n => new decimal(random.Next(), random.Next(), random.Next(), random.NextDouble() > 0.5D, (byte)random.Next(29)));
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(double)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_Double()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<double> values = AffineHashTests.RandomByteArrays(random, sizeof(double), 10000)
                .Select(b => BitConverter.ToDouble(b, 0));
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(Guid)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_Guid()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<Guid> values = AffineHashTests.RandomByteArrays(random, 16, 10000)
                .Select(b => new Guid(b));
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(short)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_Int16()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<short> values = Enumerable.Range(short.MinValue, short.MaxValue - short.MinValue + 1)
                .Select(n => (short)n);
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(int)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_Int32()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<int> values = AffineHashTests.RandomByteArrays(random, sizeof(int), 10000)
                .Select(b => BitConverter.ToInt32(b, 0));
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(long)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_Int64()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<long> values = AffineHashTests.RandomByteArrays(random, sizeof(long), 10000)
                .Select(b => BitConverter.ToInt64(b, 0));
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(sbyte)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_SByte()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<sbyte> values = Enumerable.Range(sbyte.MinValue, sbyte.MaxValue - sbyte.MinValue + 1)
                .Select(n => (sbyte)n);
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(float)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_Single()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<float> values = AffineHashTests.RandomByteArrays(random, sizeof(float), 10000)
                .Select(b => BitConverter.ToSingle(b, 0));
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(string)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_String()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<string> values = AffineHashTests.RandomCharArrays(random, 101, 10000)
                .Select(b => new string(b));
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(ushort)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_UInt16()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<ushort> values = Enumerable.Range(ushort.MinValue, ushort.MaxValue - ushort.MinValue + 1)
                .Select(n => (ushort)n);
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(uint)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_UInt32()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<uint> values = AffineHashTests.RandomByteArrays(random, sizeof(uint), 10000)
                .Select(b => BitConverter.ToUInt32(b, 0));
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(ulong)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Distributes_UInt64()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            IEnumerable<ulong> values = AffineHashTests.RandomByteArrays(random, sizeof(ulong), 10000)
                .Select(b => BitConverter.ToUInt64(b, 0));
            AffineHashTests.AssertHashDistribution(target, values);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(byte[], int, int)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Matches_Identical_Byte_Array_Segments()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            byte[] value = { 0, 1, 2, 3, 0, 1, 2, 3 };
            int expected = target.GetHashCode(value, 0, 4);
            int actual = target.GetHashCode(value, 4, 4);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(char[], int, int)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Matches_Identical_Char_Array_Segments()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            char[] value = { 'T', 'e', 's', 't', 'T', 'e', 's', 't' };
            int expected = target.GetHashCode(value, 0, 4);
            int actual = target.GetHashCode(value, 4, 4);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(string, int, int)"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Matches_Identical_String_Segments()
        {
            Random random = AffineHashTests.Random();
            AffineHash target = new AffineHash(random);
            string value = "TestTest";
            int expected = target.GetHashCode(value, 0, 4);
            int actual = target.GetHashCode(value, 4, 4);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(byte[])"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Measure_Avalanche()
        {
            Random random = new Random();
            var cases = new[]
            {
                new { name = "zeroes", source = Enumerable.Repeat((byte)0x00, 100).ToArray() },
                new { name = "ones", source = Enumerable.Repeat((byte)0xFF, 100).ToArray() },
                new { name = "alternating-right", source = Enumerable.Repeat((byte)0x55, 100).ToArray() },
                new { name = "alternating-left", source = Enumerable.Repeat((byte)0xAA, 100).ToArray() },
                new { name = "random", source = Enumerable.Repeat(default(byte), 100).Select(b => (byte)random.Next()).ToArray() },
            };
            Avalanche avalanche = new Avalanche();
            this.TestContext.WriteLine("power,predictability,scale,shift");
            for (int trial = 0; trial < 10000; trial++)
            {
                AffineHash target = new AffineHash(random);
                avalanche.Reset(target);
                bool strong = true;
                bool weak = false;
                foreach (var c in cases)
                {
                    avalanche.Next(c.source);
                    double predictability = avalanche.Predictability();
                    if (predictability > 0.1)
                    {
                        strong = false;
                    }

                    if (predictability > 0.2)
                    {
                        weak = true;
                    }
                }

                if (strong)
                {
                    double predictability = avalanche.Finish();
                    this.TestContext.WriteLine("strong,{0},{1},{2}", predictability, target.Scale, target.Shift);
                }

                if (weak)
                {
                    double predictability = avalanche.Finish();
                    this.TestContext.WriteLine("weak,{0},{1},{2}", predictability, target.Scale, target.Shift);
                }
            }
        }

        /// <summary>
        /// Tests <see cref="AffineHash.GetHashCode(byte[])"/>.
        /// </summary>
        [TestMethod]
        public void AffineHash_Hash_Rejects_Invalid_Arguments()
        {
            var cases = new[]
            {
                new { arguments = new object[] { null }, types = new Type[] { typeof(byte[]) }, expected = typeof(ArgumentNullException) },
                new { arguments = new object[] { null }, types = new Type[] { typeof(char[]) }, expected = typeof(ArgumentNullException) },
                new { arguments = new object[] { null }, types = new Type[] { typeof(string) }, expected = typeof(ArgumentNullException) },
                new { arguments = new object[] { null, 0, 0 }, types = new Type[] { typeof(byte[]), typeof(int), typeof(int) }, expected = typeof(ArgumentNullException) },
                new { arguments = new object[] { null, 0, 0 }, types = new Type[] { typeof(char[]), typeof(int), typeof(int) }, expected = typeof(ArgumentNullException) },
                new { arguments = new object[] { null, 0, 0 }, types = new Type[] { typeof(string), typeof(int), typeof(int) }, expected = typeof(ArgumentNullException) },
                new { arguments = new object[] { new byte[10], -1, 5 }, types = (Type[])null, expected = typeof(ArgumentOutOfRangeException) },
                new { arguments = new object[] { new byte[10], 10, 5 }, types = (Type[])null, expected = typeof(ArgumentOutOfRangeException) },
                new { arguments = new object[] { new byte[10], 5, -1 }, types = (Type[])null, expected = typeof(ArgumentOutOfRangeException) },
                new { arguments = new object[] { new byte[10], 5, 6 }, types = (Type[])null, expected = typeof(ArgumentOutOfRangeException) },
                new { arguments = new object[] { new char[10], -1, 5 }, types = (Type[])null, expected = typeof(ArgumentOutOfRangeException) },
                new { arguments = new object[] { new char[10], 10, 5 }, types = (Type[])null, expected = typeof(ArgumentOutOfRangeException) },
                new { arguments = new object[] { new char[10], 5, -1 }, types = (Type[])null, expected = typeof(ArgumentOutOfRangeException) },
                new { arguments = new object[] { new char[10], 5, 6 }, types = (Type[])null, expected = typeof(ArgumentOutOfRangeException) },
                new { arguments = new object[] { "1234567890", -1, 5 }, types = (Type[])null, expected = typeof(ArgumentOutOfRangeException) },
                new { arguments = new object[] { "1234567890", 10, 5 }, types = (Type[])null, expected = typeof(ArgumentOutOfRangeException) },
                new { arguments = new object[] { "1234567890", 5, -1 }, types = (Type[])null, expected = typeof(ArgumentOutOfRangeException) },
                new { arguments = new object[] { "1234567890", 5, 6 }, types = (Type[])null, expected = typeof(ArgumentOutOfRangeException) },
            };
            AffineHash target = new AffineHash(1L, 0L);
            for (int i = 0; i < cases.Length; i++)
            {
                var c = cases[i];
                Type[] types = c.types ?? c.arguments.Select(a => a.GetType()).ToArray();
                Delegate method = AffineHashTests.HashDelegate(target, types);
                try
                {
                    object result = method.DynamicInvoke(c.arguments);
                    Assert.Fail("index: {0}, return: {1}", i, result);
                }
                catch (TargetInvocationException ex)
                {
                    Assert.AreEqual(c.expected, ex.InnerException.GetType(), "index: {0}", i);
                }
                catch (Exception ex)
                {
                    Assert.Fail("index: {0}, exception: {1}", i, ex);
                }
            }
        }

        /// <summary>
        /// Asserts that the distribution of hash values is uniform for a given set of data.
        /// </summary>
        /// <typeparam name="T">The type of data to hash.</typeparam>
        /// <param name="target">The target hash algorithm.</param>
        /// <param name="values">The values to hash.</param>
        private static void AssertHashDistribution<T>(AffineHash target, IEnumerable<T> values)
        {
            Distribution distribution = new Distribution();
            Dictionary<int, T> hashes = new Dictionary<int, T>();
            Func<T, int> method = (Func<T, int>)AffineHashTests.HashDelegate(target, typeof(T));
            foreach (T value in values)
            {
                int hash = method(value);
                distribution.Observe(hash);
                if (hashes.ContainsKey(hash))
                {
                    Assert.Fail("scale:{0}, shift:{1}, hash:{2}, original:{3}, collision:{4}", target.Scale, target.Shift, hash, hashes[hash], value);
                }
                else
                {
                    hashes.Add(hash, value);
                }
            }

            double ks = distribution.Finish();
            Assert.IsTrue(ks < 0.1D, "scale:{0}, shift:{1}, ks:{2}", target.Scale, target.Shift, ks);
        }

        /// <summary>
        /// Gets the hash method delegate for the specified arguments and return type.
        /// </summary>
        /// <param name="target">The target hash instance.</param>
        /// <param name="args">The argument types of the hash method.</param>
        /// <returns>A delegate that will call the hash method on the target.</returns>
        private static Delegate HashDelegate(AffineHash target, params Type[] args)
        {
            Type[] delegateTypeArgs = new Type[args.Length + 1];
            args.CopyTo(delegateTypeArgs, 0);
            delegateTypeArgs[args.Length] = typeof(int);
            Type delegateType = null;
            switch (args.Length)
            {
                case 1:
                    delegateType = typeof(Func<,>);
                    break;
                case 2:
                    delegateType = typeof(Func<,,>);
                    break;
                case 3:
                    delegateType = typeof(Func<,,,>);
                    break;
            }

            delegateType = delegateType.MakeGenericType(delegateTypeArgs);
            return typeof(AffineHash).GetMethod(nameof(AffineHash.GetHashCode), args).CreateDelegate(delegateType, target);
        }

        /// <summary>
        /// Creates a random number generator with a predictable seed unique to the caller.
        /// </summary>
        /// <param name="caller">The caller of this method.</param>
        /// <returns>A random number generator.</returns>
        private static Random Random([CallerMemberName] string caller = "")
        {
            return new Random(caller.GetHashCode() + 1);
        }

        /// <summary>
        /// Generates random byte arrays for test data.
        /// </summary>
        /// <param name="random">The random number generator to use.</param>
        /// <param name="length">The length of each byte array.</param>
        /// <param name="count">The total number of byte arrays to generate.</param>
        /// <returns>A list of random byte arrays.</returns>
        private static IEnumerable<byte[]> RandomByteArrays(Random random, int length, int count)
        {
            byte[] buffer = new byte[length];
            for (int i = 0; i < count; i++)
            {
                random.NextBytes(buffer);
                yield return buffer;
            }
        }

        /// <summary>
        /// Generates random character arrays for test data.
        /// </summary>
        /// <param name="random">The random number generator to use.</param>
        /// <param name="length">The length of each character array.</param>
        /// <param name="count">The total number of character arrays to generate.</param>
        /// <returns>A list of random character arrays.</returns>
        private static IEnumerable<char[]> RandomCharArrays(Random random, int length, int count)
        {
            char[] buffer = new char[length];
            foreach (byte[] bytes in AffineHashTests.RandomByteArrays(random, length * sizeof(char), count))
            {
                int o = 0;
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = BitConverter.ToChar(bytes, o);
                    o += sizeof(char);
                }

                yield return buffer;
            }
        }
    }
}
