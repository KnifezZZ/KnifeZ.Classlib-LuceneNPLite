using System;
using System.Collections.Generic;
using System.Text;
namespace KnifeZ.ClassLib.LuceneNP
{
    public class LiteNewsModel
    {
        /// <summary>
        /// 业务编码--唯一标识，删除索引的判断依据
        /// </summary>
        public  string BillCode { get; set; }
        /// <summary>
        /// 链接
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 正文
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 搜索摘要
        /// </summary>
        public string Abstract { get; set; }
        /// <summary>
        /// 标题高亮
        /// </summary>
        public string TitleHighLighter { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime Time { get; set; }

    }
}
