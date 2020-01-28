using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Collections;
using Narupa.Frame;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Properties.Collections;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    /// <summary>
    /// Locate cycles by looking at the bonding in a molecule, optionally limiting the search
    /// to only include cycles wholely within a single residue.
    /// </summary>
    [Serializable]
    public class CyclesCalculatorNode
    {
        [SerializeField]
        private BondArrayProperty bonds = new BondArrayProperty();

        public IProperty<BondPair[]> Bonds => bonds;

        [SerializeField]
        private IntProperty particleCount = new IntProperty();

        public IProperty<int> ParticleCount => particleCount;

        private readonly SelectionArrayProperty cycles = new SelectionArrayProperty();

        public IReadOnlyProperty<IReadOnlyList<int>[]> Cycles => cycles;

        private readonly IntProperty cyclesCount = new IntProperty();

        public IReadOnlyProperty<int> CyclesCount => cyclesCount;

        private readonly List<Cycle> cachedCycles = new List<Cycle>();

        protected virtual bool IsInputDirty => bonds.IsDirty || particleCount.IsDirty;

        protected virtual bool IsInputValid => bonds.HasNonEmptyValue();

        public void Refresh()
        {
            if (IsInputDirty)
            {
                cachedCycles.Clear();
                if (IsInputValid)
                {
                    var particleCount = this.particleCount.Value;
                    var neighbourList = new List<int>[particleCount];
                    for (var i = 0; i < particleCount; i++)
                        neighbourList[i] = new List<int>();
                    foreach (var bond in bonds)
                    {
                        neighbourList[bond.A].Add(bond.B);
                        neighbourList[bond.B].Add(bond.A);
                    }

                    cachedCycles.Clear();
                    cachedCycles.AddRange(ComputeChordlessCycles(particleCount, neighbourList));
                }

                cycles.Value = cachedCycles.Select(cycle => cycle.ToList())
                                           .ToArray();
                cyclesCount.Value = cachedCycles.Count;

                ClearDirty();
            }
        }

        private IEnumerable<Cycle> ComputeChordlessCycles(int count, List<int>[] adjacency)
        {
            var l = DegreeLabeling(count, adjacency);

            // Triplets(G) (Algorithm 4 of https://arxiv.org/pdf/1309.1051.pdf)
            var T = new Queue<List<int>>();
            for (var u = 0; u < count; u++)
                foreach (var x in adjacency[u])
                foreach (var y in adjacency[u])
                {
                    // Ensure u < x
                    if (u >= x)
                        continue;

                    // Ensure x < y and hence u < x < y
                    if (x >= y)
                        continue;

                    if (adjacency[x].Contains(y))
                    {
                        yield return new Cycle(x, u, y);
                    }
                    else
                    {
                        T.Enqueue(new List<int>
                        {
                            x,
                            u,
                            y
                        });
                    }
                }

            while (T.Any())
            {
                var p = T.Dequeue();

                Debug.Log(p.AsPretty());

                if (p.Count == 6)
                    continue;

                var u2 = p[1];
                var ut = p.Last();
                foreach (var v in adjacency[ut])
                {
                    if (v <= u2)
                        continue;
                    if (Enumerable.Range(1, p.Count - 2)
                                  .Any(i => adjacency[p[i]].Contains(v)))
                        continue;

                    var p2 = new List<int>();
                    p2.AddRange(p);
                    p2.Add(v);
                    
                    if (adjacency[p[0]].Contains(v))
                    {
                        yield return new Cycle(p2.ToArray());
                    }
                    else
                    {
                        T.Enqueue(p2);
                    }
                }
            }
        }

        private IEnumerable<Cycle> ChordlessCycles(int count, List<int>[] adjacency)
        {
            var l = DegreeLabeling(count, adjacency);
            var (T, C) = Triplets(l, adjacency);

            var blocked = new int[count];

            for (var u = 0; u < count; u++)
                blocked[u] = 0;

            foreach (var (x, u, y) in T)
            {
                BlockNeighbours(u, blocked, adjacency);
                foreach (var cycle in CC_Visit(l, new List<int>
                {
                    x,
                    u,
                    y
                }, l[u], blocked, adjacency))
                    yield return cycle;
                UnblockNeighbours(u, blocked, adjacency);
            }
        }

        private IEnumerable<Cycle> CC_Visit(int[] l,
                                            List<int> p,
                                            int key,
                                            int[] blocked,
                                            List<int>[] adjacency)
        {
            var ut = p.Last();
            BlockNeighbours(ut, blocked, adjacency);
            foreach (var v in adjacency[ut])
            {
                if (l[v] <= key)
                    continue;
                if (blocked[v] != 1)
                    continue;
                var p2 = new List<int>();
                p2.AddRange(p);
                p2.Add(v);
                if (adjacency[v].Contains(p[0]))
                {
                    yield return new Cycle(p2.ToArray());
                }
                else
                {
                    foreach (var cycle in CC_Visit(l, p2, key, blocked, adjacency))
                        yield return cycle;
                }
            }
        }

        private void BlockNeighbours(int v, int[] blocked, List<int>[] adjacency)
        {
            foreach (var u in adjacency[v])
                blocked[u] = blocked[u] + 1;
        }

        private void UnblockNeighbours(int v, int[] blocked, List<int>[] adjacency)
        {
            foreach (var u in adjacency[v])
                if (blocked[u] > 0)
                    blocked[u] = blocked[u] - 1;
        }

        private (List<(int x, int u, int y)> T, List<Cycle> C) Triplets(
            int[] graph,
            List<int>[] adjacency)
        {
            var T = new List<(int x, int u, int y)>();
            var C = new List<Cycle>();

            for (var u = 0; u < graph.Length; u++)
            {
                foreach (var x in adjacency[u])
                {
                    if (graph[u] >= graph[x])
                        continue;
                    foreach (var y in adjacency[u])
                    {
                        if (graph[x] >= graph[y])
                            continue;

                        if (adjacency[x].Contains(y))
                        {
                            C.Add(new Cycle(x, u, y));
                        }
                        else
                        {
                            T.Add((x, u, y));
                        }
                    }
                }
            }

            return (T, C);
        }

        private int[] DegreeLabeling(int count, List<int>[] adjacency)
        {
            var degree = new int[count];
            var color = new bool[count];
            var l = new int[count];
            for (var i = 0; i < count; i++)
            {
                degree[i] = adjacency[i].Count;
                color[i] = true; //white
                l[i] = -1;
            }

            for (var i = 0; i < count; i++)
            {
                int v = -1;
                var min_degree = count;
                for (var x = 0; x < count; x++)
                {
                    if (color[x] && degree[x] < min_degree)
                    {
                        v = x;
                        min_degree = degree[x];
                    }
                }

                l[v] = i;
                color[v] = false;
                foreach (var u in adjacency[v])
                {
                    if (color[u])
                        degree[u]--;
                }
            }

            return l;
        }

        protected virtual void ClearDirty()
        {
            bonds.IsDirty = false;
            particleCount.IsDirty = false;
        }
    }
}