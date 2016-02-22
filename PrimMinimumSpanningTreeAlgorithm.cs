using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using QuickGraph.Collections;
using System.Diagnostics.Contracts;
using QuickGraph.Algorithms.Services;

namespace QuickGraph.Algorithms.MinimumSpanningTree
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public sealed class PrimMinimumSpanningTreeAlgorithm<TVertex, TEdge>
        : AlgorithmBase<IUndirectedGraph<TVertex, TEdge>>
        , IMinimumSpanningTreeAlgorithm<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        readonly Func<TEdge, double> edgeWeights;

        public PrimMinimumSpanningTreeAlgorithm(
            IUndirectedGraph<TVertex, TEdge> visitedGraph,
            Func<TEdge, double> edgeWeights
            )
            : this(null, visitedGraph, edgeWeights)
        { }

        public PrimMinimumSpanningTreeAlgorithm(
            IAlgorithmComponent host,
            IUndirectedGraph<TVertex, TEdge> visitedGraph,
            Func<TEdge, double> edgeWeights
            )
            : base(host, visitedGraph)
        {
            Contract.Requires(edgeWeights != null);

            this.edgeWeights = edgeWeights;
        }

        public event VertexAction<TVertex> TreeVertex;
        private void OnTreeVertex(TVertex vertex)
        {
            var eh = TreeVertex;
            if (eh != null)
                eh(vertex);
        }

        public event EdgeAction<TVertex, TEdge> TreeEdge;
        private void OnTreeEdge(TEdge edge)
        {
            var eh = this.TreeEdge;
            if (eh != null)
                eh(edge);
        }

        protected override void InternalCompute()
        {
            var cancelManager = this.Services.CancelManager;
            EdgeHeap edgeHeap = new EdgeHeap(this);
            //var edgeSortedList = new SortedList<double, TEdge>();
            TEdge nextEdge;
            TVertex nextVertex;
            VertexList<TVertex> visited = new VertexList<TVertex>();
            TVertex start = VisitedGraph.Vertices.GetEnumerator().Current;
            TreeVertex(start);
            visited.Add(start);
            // adding all the outgoing edges to the sorted list
            foreach (var edge in VisitedGraph.Edges.Where(e => e.Source.Equals(start)))
            {
                edgeHeap.Add(edge);
            }

            if (cancelManager.IsCancelling)
                return;

            nextEdge = edgeHeap.GetSmallest();
            nextVertex = nextEdge.Target;
            TreeVertex(nextVertex);
            TreeEdge(nextEdge);
            visited.Add(start);

            // adding one edge and one vertex untill tree is complete
            for (int i = 1; i <= VisitedGraph.VertexCount - 1; i++)
            {
                edgeHeap.RemoveAllTargetingVertex(nextVertex);

                var edges = VisitedGraph.Edges.Where(e => e.Source.Equals(nextVertex) && !visited.Contains(e.Target));
                foreach (var edge in edges)
                    edgeHeap.Add(edge);

                nextEdge = edgeHeap.GetSmallest();
                nextVertex = nextEdge.Target;
                TreeVertex(nextVertex);
                TreeEdge(nextEdge);
                visited.Add(start);
            }
        }

        private class EdgeHeap
        {
            private List<TEdge> heap = new List<TEdge>();
            private PrimMinimumSpanningTreeAlgorithm<TVertex,TEdge> algorithm;

            public EdgeHeap(PrimMinimumSpanningTreeAlgorithm<TVertex, TEdge> algorithm)
            {
                this.algorithm = algorithm;
                heap = new List<TEdge>();
            }

            private void Sink(int index)
            {
                var maxPosition = heap.Count();
                var bestPosition = index;
                if (index * 2 + 1 < maxPosition)
                {
                    if (algorithm.edgeWeights(heap[bestPosition]) > algorithm.edgeWeights(heap[index * 2 + 1]))
                        bestPosition = index * 2;
                }

                if (index * 2 + 2 < maxPosition)
                {
                    if (algorithm.edgeWeights(heap[bestPosition]) > algorithm.edgeWeights(heap[index * 2 - 1 + 2]))
                        bestPosition = index * 2 + 2;
                }
                if (bestPosition != index)
                {
                    TEdge aux = heap[index];
                    heap[index] = heap[bestPosition];
                    heap[bestPosition] = aux;
                    Sink(bestPosition);
                }
            }

            private void Lift(int index)
            {
                if (index > 0)
                {
                    if (algorithm.edgeWeights(heap[index]) < algorithm.edgeWeights(heap[(index - 1) / 2]))
                    {
                        TEdge aux = heap[index];
                        heap[index] = heap[(index - 1) / 2];
                        heap[(index - 1) / 2] = aux;
                        Lift((index - 1) / 2);
                    }
                }
            }

            public void Add(TEdge edge)
            {
                heap.Add(edge);
                Lift(heap.Count-1);
            }

            public TEdge GetSmallest()
            {
                TEdge result = heap[0];
                heap[0] = heap[heap.Count - 1];
                heap.RemoveAt(heap.Count - 1);
                Sink(0);
                return result;
            }
            public void Remove(int index)
            {
                heap[index] = heap[heap.Count - 1];
                heap.RemoveAt(heap.Count - 1);
                Sink(index);
                Lift(index);
            }
            public void RemoveAllTargetingVertex(TVertex target)
            {
                var edges = heap.Where(e => e.Target.Equals(target));
                foreach (var e in edges)
                    Remove(heap.IndexOf(e));
            }
        }
    }
}
