﻿using Algorithm.Sandbox.DataStructures;
using Algorithm.Sandbox.DataStructures.Graph.AdjacencyList;
using System;

namespace Algorithm.Sandbox.GraphAlgorithms.Flow
{
 
    /// <summary>
    /// A Edmond Karp max flox implementation on weighted directed graph using 
    /// adjacency list representation of graph & residual graph
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="W"></typeparam>
    public class EdmondKarpMaxFlow<T, W> where W : IComparable
    {
        IFlowOperators<W> operators;
        public EdmondKarpMaxFlow(IFlowOperators<W> operators)
        {
            this.operators = operators;
        }

        /// <summary>
        /// Compute max flow by searching a path
        /// And then augmenting the residual graph until
        /// no more path exists in residual graph with possible flow
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        /// <param name="sink"></param>
        /// <returns></returns>
        public W ComputeMaxFlow(AsWeightedDiGraph<T, W> graph,
            T source, T sink)
        {
            var residualGraph = createResidualGraph(graph);

            AsArrayList<T> path = BFS(residualGraph, source, sink);

            var result = operators.defaultWeight;

            while (path != null)
            {
                result = operators.AddWeights(result, AugmentResidualGraph(graph, residualGraph, path));
                path = BFS(residualGraph, source, sink);
            }

            return result;
        }

        /// <summary>
        /// Augment current Path to residual Graph
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="residualGraph"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private W AugmentResidualGraph(AsWeightedDiGraph<T, W> graph,
            AsWeightedDiGraph<T, W> residualGraph, AsArrayList<T> path)
        {
            var min = operators.MaxWeight;

            for (int i = 0; i < path.Length - 1; i++)
            {
                var vertex_1 = residualGraph.FindVertex(path[i]);
                var vertex_2 = residualGraph.FindVertex(path[i + 1]);

                var edgeValue = vertex_1.OutEdges[vertex_2];

                if (min.CompareTo(edgeValue) > 0)
                {
                    min = edgeValue;
                }

            }

            //augment path
            for (int i = 0; i < path.Length - 1; i++)
            {
                var vertex_1 = residualGraph.FindVertex(path[i]);
                var vertex_2 = residualGraph.FindVertex(path[i + 1]);

                //substract from forward paths
                vertex_1.OutEdges[vertex_2] = operators.SubstractWeights(vertex_1.OutEdges[vertex_2], min);

                //add for backward paths
                vertex_2.OutEdges[vertex_1] = operators.AddWeights(vertex_2.OutEdges[vertex_1], min);

            }

            return min;
        }

        /// <summary>
        /// bredth first search to find a path to sink in residual graph from source
        /// </summary>
        /// <param name="residualGraph"></param>
        /// <param name="source"></param>
        /// <param name="sink"></param>
        /// <returns></returns>
        private AsArrayList<T> BFS(AsWeightedDiGraph<T, W> residualGraph, T source, T sink)
        {
            //init parent lookup table to trace path
            var parentLookUp = new AsDictionary<AsWeightedDiGraphVertex<T, W>, AsWeightedDiGraphVertex<T, W>>();
            foreach (var vertex in residualGraph.Vertices)
            {
                parentLookUp.Add(vertex.Value, null);
            }

            //regular BFS stuff
            var queue = new AsQueue<AsWeightedDiGraphVertex<T, W>>();
            var visited = new AsHashSet<AsWeightedDiGraphVertex<T, W>>();
            queue.Enqueue(residualGraph.Vertices[source]);
            visited.Add(residualGraph.Vertices[source]);

            AsWeightedDiGraphVertex<T, W> currentVertex = null;

            while (queue.Count() > 0)
            {
                currentVertex = queue.Dequeue();
              
                //reached sink? then break otherwise dig in
                if (currentVertex.Value.Equals(sink))
                {
                    break;
                }
                else
                {
                    foreach (var edge in currentVertex.OutEdges)
                    {
                        //visit only if edge have available flow
                        if (!visited.Contains(edge.Key)
                            && edge.Value.CompareTo(operators.defaultWeight) > 0)
                        {
                            //keep track of this to trace out path once sink is found
                            parentLookUp[edge.Key] = currentVertex;
                            queue.Enqueue(edge.Key);
                            visited.Add(edge.Key);                       
                        }
                    }
                }
            }

            //could'nt find a path
            if (currentVertex == null || !currentVertex.Value.Equals(sink))
            {
                return null;
            }

            //traverse back from sink to find path to source
            var path = new AsStack<T>();

            path.Push(sink);

            while (currentVertex != null && !currentVertex.Value.Equals(source))
            {
                path.Push(parentLookUp[currentVertex].Value);
                currentVertex = parentLookUp[currentVertex];
            }

            //now reverse the stack to get the path from source to sink
            var result = new AsArrayList<T>();

            while (path.Count > 0)
            {
                result.Add(path.Pop());
            }

            return result;
        }

        /// <summary>
        /// clones this graph and creates a residual graph
        /// </summary>
        /// <param name="residualGraph"></param>
        /// <returns></returns>
        private AsWeightedDiGraph<T, W> createResidualGraph(AsWeightedDiGraph<T, W> residualGraph)
        {
            var newGraph = new AsWeightedDiGraph<T, W>();

            //clone graph vertices
            foreach (var vertex in residualGraph.Vertices)
            {
                newGraph.AddVertex(vertex.Key);
            }

            //clone edges
            foreach (var vertex in residualGraph.Vertices)
            {
                //Use either OutEdges or InEdges for cloning
                //here we use OutEdges
                foreach (var edge in vertex.Value.OutEdges)
                {
                    //original edge
                    newGraph.AddEdge(vertex.Key, edge.Key.Value, edge.Value);
                    //add a backward edge for residual graph with edge value as default(W)
                    newGraph.AddEdge(edge.Key.Value, vertex.Key, default(W));
                }
            }

            return newGraph;
        }
    }
}
