using System;
using System.Collections.Generic;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using PanGu;
using PanGu.HighLight;
using LN = Lucene.Net;
using System.Diagnostics;
using System.IO;
using Lucene.Net.Analysis.PanGu;

namespace KnifeZ.ClassLib.LuceneNP
{
    public class LNPIndex
    {
        /// <summary>
        /// 索引存放目录
        /// </summary>
        protected string IndexDic => PanGu.Framework.Path.GetAssemblyPath() + @"IndexDic";

        public LN.Store.Directory Direcotry
        {
            get
            { //创建索引目录
                if (!System.IO.Directory.Exists(IndexDic))
                {
                    System.IO.Directory.CreateDirectory(IndexDic);
                }

                LN.Store.Directory direcotry = FSDirectory.Open(IndexDic);
                return direcotry;
            }
        }
        /// <summary>
        /// 盘古分词的配置文件
        /// </summary>
        protected string PanGuXmlPath => PanGu.Framework.Path.GetAssemblyPath() + "/PanGu/PanGu.xml";

        /// <summary>
        /// 盘古分词器
        /// </summary>
        protected Analyzer PanGuAnalyzer => new PanGuAnalyzer();

        public string CreatedIndex(List<LiteNewsModel> list)
        {
            //IndexWriter第三个参数:true指重新创建索引,false指从当前索引追加....此处为新建索引所以为true,后续应该建立的索引应采用追加
            IndexWriter writer = new IndexWriter(Direcotry, PanGuAnalyzer, true, IndexWriter.MaxFieldLength.LIMITED);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 1; i < list.Count; i++)
            {
                AddIndex(writer, list[i]);
            }
            //释放资源
            writer.Optimize();
            writer.Dispose();
            string time = ((double)sw.ElapsedMilliseconds / 1000).ToString();
            sw.Stop();
            return time;
        }
        public void AddSingleIndex(LiteNewsModel model)
        {
            IndexWriter writer = new IndexWriter(Direcotry, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.LIMITED);
            AddIndex(writer, model);
            //释放资源
            writer.Optimize();
            writer.Dispose();
        }
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="analyzer"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        internal static void AddIndex(IndexWriter writer,LiteNewsModel model)
        {
            try
            {
                Document doc = new Document();
                //只有对需要全文检索的字段才ANALYZED
                doc.Add(new Field("billCode", model.BillCode, Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc.Add(new Field("title", model.Title, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
                doc.Add(new Field("abstract", model.Abstract, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
                doc.Add(new Field("content", model.Content, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
                doc.Add(new Field("url", model.Url, Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc.Add(new Field("time", model.Time.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                writer.AddDocument(doc);
            }
            catch (FileNotFoundException fnfe)
            {
                throw fnfe;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 查询多个字段
        /// </summary>
        internal List<LiteNewsModel> SearchIndex(string searchKey)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            BooleanQuery bQuery = new BooleanQuery();

            #region 一个字段查询 
            //if (!string.IsNullOrEmpty(title))
            //{
            //    title = GetKeyWordsSplitBySpace(title);
            //    QueryParser parse = new QueryParser(LN.Util.Version.LUCENE_30, "title", PanGuAnalyzer);//一个字段查询  
            //    Query query = parse.Parse(title);
            //    parse.DefaultOperator = QueryParser.Operator.OR;
            //    bQuery.Add(query, new Occur());
            //    dic.Add("title", title);
            //}

            #endregion

            string[] fileds = { "title","abstract", "content" };//查询字段  
            searchKey = GetKeyWordsSplitBySpace(searchKey);
            QueryParser parse = new MultiFieldQueryParser(LN.Util.Version.LUCENE_30, fileds, PanGuAnalyzer);//多个字段查询
            Query query = parse.Parse(searchKey);
            bQuery.Add(query, new Occur());
            dic.Add("title", searchKey);
            dic.Add("abstract", searchKey);
            dic.Add("content", searchKey);

            if (bQuery != null && bQuery.GetClauses().Length > 0)
            {
                return GetSearchResult(bQuery, dic);
            }
            return new List<LiteNewsModel>();
        }


        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="bQuery"></param>
        private List<LiteNewsModel> GetSearchResult(BooleanQuery bQuery, Dictionary<string, string> dicKeywords)
        {
            var list = new List<LiteNewsModel>();
            IndexSearcher search = new IndexSearcher(Direcotry, true);
            // Stopwatch stopwatch = Stopwatch.StartNew();
            //SortField构造函数第三个字段true为降序,false为升序
            Sort sort = new Sort(new SortField[] {
                SortField.FIELD_SCORE, new SortField("title", SortField.SCORE, true),
                SortField.FIELD_SCORE,new SortField("abstract", SortField.SCORE, true),
                SortField.FIELD_DOC,new SortField("content", SortField.SCORE, true),
            }
                //, new SortField("content", SortField.STRING_VAL, true)
                );

            int maxNum = 100;//查询条数
            TopDocs docs = search.Search(bQuery, (Filter)null, maxNum, sort);
            if (docs != null)
            {

                for (int i = 0; i < docs.TotalHits && i < maxNum; i++)
                {
                    Document doc = search.Doc(docs.ScoreDocs[i].Doc);
                    LiteNewsModel model = new LiteNewsModel()
                    {
                        Title = doc.Get("title").ToString(),
                        Abstract = doc.Get("abstract").ToString(),
                        BillCode = doc.Get("billCode").ToString(),
                        Url = doc.Get("url").ToString(),
                        Content = doc.Get("content").ToString(),
                        Time =Convert.ToDateTime(doc.Get("addtime")),
                    };
                    list.Add(SetHighlighter(dicKeywords, model));
                }
            }
            return list;
        }
        /// <summary>
        /// 处理关键字为索引格式
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        private string GetKeyWordsSplitBySpace(string keywords)
        {
            PanGuTokenizer ktTokenizer = new PanGuTokenizer();
            StringBuilder result = new StringBuilder();
            ICollection<WordInfo> words = ktTokenizer.SegmentToWordInfos(keywords);

            foreach (WordInfo word in words)
            {
                if (word == null)
                {
                    continue;
                }
                result.AppendFormat("{0}^{1}.0 ", word.Word, (int)Math.Pow(2, word.Rank));
            }
            return result.ToString().Trim();
        }

        /// <summary>
        /// 设置关键字高亮
        /// </summary>
        /// <param name="dicKeywords">关键字列表</param>
        /// <param name="model">返回的数据模型</param>
        /// <returns></returns>
        private LiteNewsModel SetHighlighter(Dictionary<string, string> dicKeywords, LiteNewsModel model)
        {
            SimpleHTMLFormatter simpleHTMLFormatter = new PanGu.HighLight.SimpleHTMLFormatter("<font color=\"red\">", "</font>");
            Highlighter highlighter = new PanGu.HighLight.Highlighter(simpleHTMLFormatter, new Segment())
            {
                FragmentSize = 50
            };
            string strTitle = string.Empty;
            string strContent = string.Empty;
            string strAbstract = string.Empty;
            dicKeywords.TryGetValue("title", out strTitle);
            dicKeywords.TryGetValue("abstract", out strAbstract);
            dicKeywords.TryGetValue("content", out strContent);
            if (!string.IsNullOrEmpty(strTitle))
            {
                var transStr = highlighter.GetBestFragment(strTitle, model.Title);
                model.Title = string.IsNullOrEmpty(transStr) ? model.Title : transStr;
            }
            if (!string.IsNullOrEmpty(strContent))
            {
                var transStr = highlighter.GetBestFragment(strContent, model.Content);
                model.Content = string.IsNullOrEmpty(transStr) ? model.Content : transStr;
            }
            if (!string.IsNullOrEmpty(strAbstract))
            {
                var transStr = highlighter.GetBestFragment(strAbstract, model.Abstract);
                model.Abstract = string.IsNullOrEmpty(transStr) ? model.Content : transStr;
            }
            return model;
        }
        #region 删除索引数据
        /// <summary>  
        /// 删除索引数据（根据billCode）  
        /// </summary>  
        /// <param name="billCode"></param>  
        /// <returns></returns>  
        public bool Delete(string billCode)
        {
            bool IsSuccess = false;
            Term term = new Term("billCode", billCode);
            IndexWriter writer = new IndexWriter(Direcotry, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.LIMITED);
            writer.DeleteDocuments(term);
            writer.Commit();
            //writer.Optimize();
            IsSuccess = writer.HasDeletions();
            writer.Dispose();
            return IsSuccess;
        }
        #endregion

        #region 删除全部索引数据  
        /// <summary>  
        /// 删除全部索引数据  
        /// </summary>  
        /// <returns></returns>  
        public bool DeleteAll()
        {
            bool IsSuccess = true;
            try
            {

                IndexWriter writer = new IndexWriter(Direcotry, PanGuAnalyzer, false, IndexWriter.MaxFieldLength.LIMITED);
                writer.DeleteAll();
                writer.Commit();
                //writer.Optimize();//  
                IsSuccess = writer.HasDeletions();
                writer.Dispose();
            }
            catch
            {
                IsSuccess = false;
            }
            return IsSuccess;
        }
        #endregion
    }
}
