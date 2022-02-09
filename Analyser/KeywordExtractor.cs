﻿using JiebaNet.Segmenter.Common;
using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace JiebaNet.Analyser
{
    public abstract class KeywordExtractor
    {
        protected static readonly List<string> DefaultStopWords = new List<string>()
        {
            "the", "of", "is", "and", "to", "in", "that", "we", "for", "an", "are",
            "by", "be", "as", "on", "with", "can", "if", "from", "which", "you", "it",
            "this", "then", "at", "have", "all", "not", "one", "has", "or", "that"
        };

        protected virtual ISet<string> StopWords { get; set; }

        public void SetStopWords(string stopWordsFile)
        {
            StopWords = new HashSet<string>();
            var lines = FileExtension.ReadEmbeddedAllLines(stopWordsFile);
            foreach (var line in lines)
            {
                StopWords.Add(line.Trim());
            }
        }

        public abstract IEnumerable<string> ExtractTags(string text, int count = 20, IEnumerable<string> allowPos = null);
        public abstract IEnumerable<WordWeightPair> ExtractTagsWithWeight(string text, int count = 20, IEnumerable<string> allowPos = null);
    }
}