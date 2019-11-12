using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Frame;
using Narupa.Visualisation.Property;
using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    /// <summary>
    /// Locate cycles by looking at the bonding in a molecule, optionally limiting the search
    /// to only include cycles wholely within a single residue.
    /// </summary>
    [Serializable]
    public class CyclesCalculator
    {
        [SerializeField]
        private BondArrayProperty bonds = new BondArrayProperty();

        public IProperty<BondPair[]> Bonds => bonds;

        [SerializeField]
        private IntArrayProperty particleResidues = new IntArrayProperty();

        public IProperty<int[]> ParticleResidues => particleResidues;

        private readonly SelectionArrayProperty cycles = new SelectionArrayProperty();

        public IReadOnlyProperty<IReadOnlyList<int>[]> Cycles => cycles;

        private readonly IntProperty cyclesCount = new IntProperty();

        public IReadOnlyProperty<int> CyclesCount => cyclesCount;

        private readonly List<Cycle> cachedCycles = new List<Cycle>();

        protected virtual bool IsInputDirty => bonds.IsDirty || particleResidues.IsDirty;

        protected virtual bool IsInputValid => bonds.HasNonEmptyValue();

        public void Refresh()
        {
            if (IsInputDirty)
            {
                cachedCycles.Clear();
                if (IsInputValid)
                {
                    FindRings();
                }

                cycles.Value = cachedCycles.Select(cycle => cycle.ToList()).ToArray();
                cyclesCount.Value = cachedCycles.Count;

                ClearDirty();
            }
        }

        protected virtual void ClearDirty()
        {
            bonds.IsDirty = false;
            particleResidues.IsDirty = false;
        }

        /// <summary>
        ///     Finds all cycles, given a set of bond pairs
        /// </summary>
        private void FindRings()
        {
            if (particleResidues.HasNonEmptyValue())
            {
                var residueIds = particleResidues.Value;
                var bondsPerResidueId = new List<BondPair>[residueIds.Max() + 1];
                foreach (var bond in bonds.Value)
                {
                    var resId = residueIds[bond.A];
                    if (resId == residueIds[bond.B])
                    {
                        if (bondsPerResidueId[resId] == null)
                            bondsPerResidueId[resId] = new List<BondPair>();
                        bondsPerResidueId[resId].Add(bond);
                    }
                }

                foreach (var list in bondsPerResidueId)
                    if (list != null)
                        cachedCycles.AddRange(FindCycles(list));
            }
            else
            {
                cachedCycles.AddRange(FindCycles(bonds.Value));
            }
        }

        /// <summary>
        ///     Finds a set of cycles in a graph. Copied from https://stackoverflow.com/a/14115627
        /// </summary>
        private IEnumerable<Cycle> FindCycles(IReadOnlyList<BondPair> graph)
        {
            if (graph.Count > 48)
                return new Cycle[0];

            var list = new List<Cycle>();
            for (var i = 0; i < graph.Count; i++)
            for (var j = 0; j < 2; j++)
            {
                FindNewCycles(graph, list, new[] { (int) graph[i][j] });
            }

            return list;
        }

        private void FindNewCycles(IReadOnlyList<BondPair> graph,
                                   ICollection<Cycle> cyclesList,
                                   int[] path)
        {
            if (path.Length > 6)
                return;
            var n = path[0];
            var sub = new int[path.Length + 1];

            for (var i = 0; i < graph.Count; i++)
            for (var y = 0; y <= 1; y++)
                if ((int) graph[i][y] == n)
                    //  edge refers to our current node
                {
                    var x = (int) graph[i][(y + 1) % 2];
                    if (!Visited(x, path))
                        //  neighbor node not on path yet
                    {
                        sub[0] = x;
                        Array.Copy(path, 0, sub, 1, path.Length);
                        //  explore extended path
                        FindNewCycles(graph, cyclesList, sub);
                    }
                    else if (path.Length > 2 && x == path[path.Length - 1])
                        //  cycle found
                    {
                        var p = Normalize(path);
                        var inv = Invert(p);
                        if (IsNew(cyclesList, p) && IsNew(cyclesList, inv))
                            cyclesList.Add(new Cycle(p));
                    }
                }
        }

        private static bool ListEquals(IReadOnlyList<int> a, IReadOnlyList<int> b)
        {
            if (a.Count != b.Count)
                return false;
            for (var i = 0; i < a.Count; i++)
                if (a[i] != b[i])
                    return false;
            return true;
        }

        private static int[] Invert(IReadOnlyList<int> path)
        {
            var p = new int[path.Count];

            for (var i = 0; i < path.Count; i++)
                p[i] = path[path.Count - 1 - i];

            return Normalize(p);
        }

        //  rotate cycle path such that it begins with the smallest node
        private static int[] Normalize(int[] path)
        {
            var p = new int[path.Length];
            var x = Smallest(path);

            Array.Copy(path, 0, p, 0, path.Length);

            while (p[0] != x)
            {
                var n = p[0];
                Array.Copy(p, 1, p, 0, p.Length - 1);
                p[p.Length - 1] = n;
            }

            return p;
        }

        private static bool IsNew(IEnumerable<Cycle> cyclesList, IReadOnlyList<int> path)
        {
            return !cyclesList.Any(p => ListEquals(p.Indices, path));
        }

        private static int Smallest(IEnumerable<int> path)
        {
            return path.Min();
        }

        private static bool Visited(int n, IEnumerable<int> path)
        {
            return path.Any(p => p == n);
        }
    }
}