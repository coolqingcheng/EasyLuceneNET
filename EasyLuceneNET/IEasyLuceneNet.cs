using Lucene.Net.Search;
using System.Collections.Generic;

namespace EasyLuceneNET
{
    public interface IEasyLuceneNet
    {
        /// <summary>
        /// 检索
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        SearchResult<T> Search<T>(string keyword, int index, int size, string[] fields) where T : class, new();
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        void AddIndex<T>(List<T> list);
    }

    public class SearchResult<T> where T : class, new()
    {
        public int Total { get; set; }

        public List<string> cutKeys { get; set; } = new List<string>();

        public List<T> list { get; set; } = new List<T>();
    }
}