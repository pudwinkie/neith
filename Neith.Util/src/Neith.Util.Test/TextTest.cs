using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Neith.Util.Text;

namespace Neith.Util.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class TextTest
    {
        [Test]
        public void LevenshteinDistanceTest()
        {
            Assert.AreEqual(0, LevenshteinDistance.Compute("123", "123"));
            Assert.AreEqual(0, LevenshteinDistance.Compute("‚P‚Q‚R", "‚P‚Q‚R"));
            Assert.AreEqual(1, LevenshteinDistance.Compute("123", "1234"));
            Assert.AreEqual(1, LevenshteinDistance.Compute("1243", "123"));
            Assert.AreEqual(4, LevenshteinDistance.Compute("‰F’ˆ‚ğ‚©‚¯‚é­—", "‰F’ˆiƒ\ƒ‰j‚ğ‚©‚¯‚é­—"));
        }

        [Test]
        public void NgramDistanceTest()
        {
            Debug.WriteLine(string.Format("Ngram: {0:0.00}", Ngram.CompareBigram("abcdefg", "abcdefg")));
            Debug.WriteLine(string.Format("Ngram: {0:0.00}", Ngram.CompareBigram("abcdefg", "wwabcdefgss")));
            Debug.WriteLine(string.Format("Ngram: {0:0.00}", Ngram.CompareBigram("abcdefg", "abcdddefg")));
            Debug.WriteLine(string.Format("Ngram: {0:0.00}", Ngram.CompareBigram("abcdddefg", "abcdefg")));
            Debug.WriteLine(string.Format("Ngram: {0:0.00}", Ngram.CompareBigram("abce", "abcd")));
            Debug.WriteLine(string.Format("Ngram: {0:0.00}", Ngram.CompareBigram("rabrt", "ttshabfhs")));
            Debug.WriteLine(string.Format("Ngram: {0:0.00}", Ngram.CompareBigram("‰F’ˆ‚ğ‚©‚¯‚é­—", "‰F’ˆiƒ\ƒ‰j‚ğ‚©‚¯‚é­—")));
            Assert.Less(0.8, Ngram.CompareBigram("‰F’ˆ‚ğ‚©‚¯‚é­—", "‰F’ˆiƒ\ƒ‰j‚ğ‚©‚¯‚é­—"));
        }
    }
}
