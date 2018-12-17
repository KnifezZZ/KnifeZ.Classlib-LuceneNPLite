using System;
using System.Collections.Generic;
using System.Text;
namespace KnifeZ.ClassLib.LuceneNP
{
    public class LiteNewsModel
    {
        /// <summary>
        /// ҵ�����--Ψһ��ʶ��ɾ���������ж�����
        /// </summary>
        public  string BillCode { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// ����ժҪ
        /// </summary>
        public string Abstract { get; set; }
        /// <summary>
        /// �������
        /// </summary>
        public string TitleHighLighter { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime Time { get; set; }

    }
}
