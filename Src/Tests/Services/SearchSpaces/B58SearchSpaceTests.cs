﻿// The FinderOuter
// Copyright (c) 2020 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using FinderOuter.Backend;
using FinderOuter.Services;
using FinderOuter.Services.SearchSpaces;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace Tests.Services.SearchSpaces
{
    public class B58SearchSpaceTests
    {
        public static IEnumerable<object[]> GetShiftedMultCases()
        {
            for (int i = 0; i <= 24; i++)
            {
                yield return new object[] { 35, 7, i }; // Address
                yield return new object[] { 51, 10, i }; // Uncompressed WIF
                yield return new object[] { 52, 10, i }; // Compressed WIF
                yield return new object[] { 58, 11, i }; // BIP38
            }
        }
        [Theory]
        [MemberData(nameof(GetShiftedMultCases))]
        public void GetShiftedMultPow58Test(int maxPow, int uLen, int shift)
        {
            ulong[] shiftedPowers = B58SearchSpace.GetShiftedMultPow58(maxPow, uLen, shift);

            ulong mask = (1U << shift) - 1;
            int index = 0;
            for (int i = 0; i < 58; i++)
            {
                for (int j = 0; j < maxPow; j++)
                {
                    byte[] ba = new byte[4 * uLen];
                    for (int k = 0; k < ba.Length; k += 4, index++)
                    {
                        // Make sure values are shifted correctly
                        Assert.Equal(0U, shiftedPowers[index] & mask);
                        ulong val = shiftedPowers[index] >> shift;
                        // Make sure each unshifted value fits in a UInt32
                        Assert.True(val <= uint.MaxValue);

                        ba[k] = (byte)val;
                        ba[k + 1] = (byte)(val >> 8);
                        ba[k + 2] = (byte)(val >> 16);
                        ba[k + 3] = (byte)(val >> 24);
                    }

                    BigInteger actual = new(ba, true, false);
                    BigInteger expected = BigInteger.Pow(58, j) * i;
                    Assert.Equal(expected, actual);
                }
            }

            Assert.Equal(index, shiftedPowers.Length);
        }


        public static IEnumerable<object[]> GetProcessCases()
        {
            ulong[] compWIfMultPow = B58SearchSpace.GetShiftedMultPow58(ConstantsFO.PrivKeyCompWifLen, 10, 16);
            ulong[] uncompWifMultPow = B58SearchSpace.GetShiftedMultPow58(ConstantsFO.PrivKeyUncompWifLen, 10, 24);
            ulong[] addrMultPow = B58SearchSpace.GetShiftedMultPow58(0, 7, 24);
            ulong[] bipMultPow = B58SearchSpace.GetShiftedMultPow58(ConstantsFO.Bip38Base58Len, 11, 8);

            // Invalid inputs
            yield return new object[]
            {
                string.Empty, 'z', Base58Service.InputType.Address, false, "Missing character is not accepted.", 0,
                false, null, null, null
            };
            yield return new object[]
            {
                string.Empty, '*', Base58Service.InputType.Address, false, "Input contains invalid base-58 character(s).", 0,
                false, null, null, null
            };
            yield return new object[]
            {
                null, '*', Base58Service.InputType.Address, false, "Input contains invalid base-58 character(s).", 0,
                false, null, null, null
            };
            yield return new object[]
            {
                " ", '*', Base58Service.InputType.Address, false, "Input contains invalid base-58 character(s).", 0,
                false, null, null, null
            };
            yield return new object[]
            {
                "0", '*', Base58Service.InputType.Address, false, "Input contains invalid base-58 character(s).", 0,
                false, null, null, null
            };
            yield return new object[]
            {
                "a", '*', (Base58Service.InputType)1000, false, "Given input type is not defined.", 0, false, null, null, null
            };

            // Process private keys:
            yield return new object[]
            {
                "7HueCGU8rMjxEXxiPuD5BDku4MkFqeZyd4dZ1jvhTVqvbTLvyTJ",
                '*', Base58Service.InputType.PrivateKey, false, "The given key has an invalid first character.",
                0, false, null, null, null
            };
            yield return new object[]
            {
                "5HueCGU8rMjxEXxiPuD5BDku4MkFqeZyd4dZ1jvhTVqvbTLvyTJ",
                '*', Base58Service.InputType.PrivateKey, true, null,
                0, false, null, null, null
            };
            yield return new object[]
            {
                "5HueCGU8rMjxEXxiPuD5BDku4MkFqeZyd4dZ1jvhTVqvbT",
                '*', Base58Service.InputType.PrivateKey, true, null,
                0, false, null, null, null
            };
            yield return new object[]
            {
                "L53fCHmQhbNp1B4JipfBtfeHZH7cAibzG9oK19XfiFzxHgAkz6JK",
                '*', Base58Service.InputType.PrivateKey, true, null,
                0, false, null, null, null
            };
            yield return new object[]
            {
                "KwdMAjGmerYanjeui5SHS7JkmpZvVipYvB2LJGU1ZxJwYvP98617",
                '*', Base58Service.InputType.PrivateKey, true, null,
                0, false, null, null, null
            };
            yield return new object[]
            {
                "kwdMAjGmerYanjeui5SHS7JkmpZvVipYvB2LJGU1ZxJwYvP98617",
                '*', Base58Service.InputType.PrivateKey, false, "The given key has an invalid first character.",
                0, false, null, null, null
            };
            yield return new object[]
            {
                "5HueCGU8rMjxEXxiPuD5BDku4MkFqeZyd4dZ1jvhTVqvbTL*yTJ",
                '?', Base58Service.InputType.PrivateKey, false, "Input contains invalid base-58 character(s).",
                0, false, null, null, null
            };
            yield return new object[]
            {
                "5HueCGU8rMjxEXxiPuD5BDku4MkFqeZyd4dZ1jvhTVqvbTL-yTJ",
                '-', Base58Service.InputType.PrivateKey, true, null,
                1, false, new int[] { 47 }, new int[] { 30 }, uncompWifMultPow
            };
            yield return new object[]
            {
                "K*dMAjGmerYanjeui5SHS7JkmpZvVipYvB2LJGU1*xJw*vP9861*",
                '*', Base58Service.InputType.PrivateKey, true, null,
                4, true, new int[] { 51, 44, 40, 1 }, new int[] { 0, 70, 110, 500 }, compWIfMultPow
            };

            // Process addresses:
            yield return new object[]
            {
                "1BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2",
                '*', Base58Service.InputType.Address, true, null,
                0, false, null, null, null
            };
            yield return new object[]
            {
                "2BvBMSEYstWetqTFn5Au4m4GFg7xJaNVN2",
                '*', Base58Service.InputType.Address, false, "The given address has an invalid first character.",
                0, false, null, null, null
            };
        }
        [Theory]
        [MemberData(nameof(GetProcessCases))]
        public void ProcessTest(string input, char missChar, Base58Service.InputType t, bool expB, string expErr, int expMisCount,
                                bool isComp, int[] misIndex, int[] multMisIndex, ulong[] multPow58)
        {
            B58SearchSpace ss = new();
            bool actualB = ss.Process(input, missChar, t, out string actualErr);

            Assert.Equal(expB, actualB);
            Assert.Equal(expErr, actualErr);
            Assert.Equal(expMisCount, ss.MissCount);
            Assert.Equal(t, ss.inputType);
            if (expB)
            {
                Assert.Equal(input, ss.Input);
                Assert.Equal(isComp, ss.isComp);
                Assert.Equal(misIndex, ss.MissingIndexes);
                Assert.Equal(multMisIndex, ss.multMissingIndexes);
                Assert.Equal(multPow58, ss.multPow58);
            }
        }
    }
}
