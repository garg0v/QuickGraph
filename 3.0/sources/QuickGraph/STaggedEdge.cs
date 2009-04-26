﻿using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace QuickGraph
{
    /// <summary>
    /// A tagged edge as value type.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TTag"></typeparam>
#if !SILVERLIGHT
    [Serializable]
#endif
    [StructLayout(LayoutKind.Auto)]
    [DebuggerDisplay("{Source}->{Target}:{Tag}")]
    public struct STaggedEdge<TVertex, TTag>
        : IEdge<TVertex>
        , ITagged<TTag>
    {
        readonly TVertex source;
        readonly TVertex target;
        TTag tag;

        public STaggedEdge(TVertex source, TVertex target, TTag tag)
        {
            Contract.Requires(source != null);
            Contract.Requires(target != null);

            this.source = source;
            this.target = target;
            this.tag = tag;
            this.TagChanged = null;
        }

        public TVertex Source
        {
            get { return this.source; }
        }

        public TVertex Target
        {
            get { return this.target; }
        }

        public event EventHandler TagChanged;

        void OnTagChanged(EventArgs e)
        {
            var eh = this.TagChanged;
            if (eh != null)
                eh(this, e);
        }

        public TTag Tag
        {
            get { return this.tag; }
            set
            {
                if (!object.Equals(this.tag, value))
                {
                    this.tag = value;
                    this.OnTagChanged(EventArgs.Empty);
                }
            }
        }
    }
}