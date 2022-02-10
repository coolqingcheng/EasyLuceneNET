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
        SearchResult<T> Search<T>(SearchRequest request) where T : class, new();
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

    public class SearchRequest
    {
        public string keyword { get; set; }
        public int index { get; set; } = 1;
        public int size { get; set; } = 15;
        public string[] fields { get; set; }

        /// <summary>
        /// 倒序排列字段
        /// </summary>
        public string OrderByDescField { get; set; }


        /// <summary>
        /// 顺序排序字段
        /// </summary>
        public string OrderByField { get; set; }
    }
}