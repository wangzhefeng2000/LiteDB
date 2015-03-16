﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LiteDB
{
    internal class QueryGreater : Query
    {
        private BsonValue _value;
        private bool _equals;

        public QueryGreater(string field, BsonValue value, bool equals)
            : base(field)
        {
            _value = value;
            _equals = equals;
        }

        internal override IEnumerable<IndexNode> ExecuteIndex(IndexService indexer, CollectionIndex index)
        {
            // find first indexNode
            var value = _value.Normalize(index.Options);
            var node = indexer.Find(index, value, true, Query.Ascending);

            if (node == null) yield break;

            // move until next is last
            while (node != null)
            {
                var diff = node.Value.CompareTo(value);

                if (diff == 1 || (_equals && diff == 0))
                {
                    if (node.IsHeadTail) yield break;

                    yield return node;
                }

                node = indexer.GetNode(node.Next[0]);
            }
        }

        internal override bool ExecuteFullScan(BsonDocument doc)
        {
            return doc.Get(this.Field).CompareTo(_value) >= (_equals ? 0 : 1);
        }
    }
}
