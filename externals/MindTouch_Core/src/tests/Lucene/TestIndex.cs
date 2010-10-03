/*
 * MindTouch Core - open source enterprise collaborative networking
 * Copyright (c) 2006-2010 MindTouch Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * For community documentation and downloads visit www.opengarden.org;
 * please review the licensing section.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 * http://www.gnu.org/copyleft/gpl.html
 */
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace MindTouch.Lucene.Tests {
    public class TestIndex {

        //--- Fields ---
        public readonly string Default;
        private readonly IndexWriter _writer;
        private readonly QueryParser _parser;
        private readonly RAMDirectory _rd;

        //--- Constructors ---
        public TestIndex() : this("content", new StandardAnalyzer()) { }
        public TestIndex(Analyzer analyzer) : this("content", analyzer) { }

        public TestIndex(string def, Analyzer analyzer) {
            Default = def;
            _rd = new RAMDirectory();
            _writer = new IndexWriter(_rd, analyzer, true, IndexWriter.MaxFieldLength.LIMITED);
            _parser = new QueryParser(def, analyzer);
        }

        //--- Methods ---
        public void Add(Document d) {
            _writer.AddDocument(d);
            _writer.Commit();
        }

        public Query Parse(string query) {
            return _parser.Parse(query);
        }

        public Hits Search(string query) {
            var searcher = new IndexSearcher(_rd);
            return searcher.Search(_parser.Parse(query));
        }

        public Hits Search(Query query) {
            var searcher = new IndexSearcher(_rd);
            return searcher.Search(query);
        }

        public Hits Search(string query, Sort sort) {
            var searcher = new IndexSearcher(_rd);
            return searcher.Search(_parser.Parse(query),sort);
        }
    }
}