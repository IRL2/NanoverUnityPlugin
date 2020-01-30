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

        /// <summary>
        /// Chordless cycle computation using https://arxiv.org/pdf/1309.1051.pdf
        /// </summary>
        private IEnumerable<Cycle> ComputeChordlessCycles(int count, List<int>[] adjacency)
        {
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

        protected virtual void ClearDirty()
        {
            bonds.IsDirty = false;
            particleCount.IsDirty = false;
        }
    }
}