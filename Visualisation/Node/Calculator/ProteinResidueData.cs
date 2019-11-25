using UnityEngine;

namespace Narupa.Visualisation.Node.Calculator
{
    /// <summary>
    /// Useful utility class for storing information about protein residues.
    /// </summary>
    public class ProteinResidueData
    {
        public int AlphaCarbonIndex  { get; set; }
        public Vector3 AlphaCarbonPosition { get; set; }
        public int CarbonIndex  { get; set; }
        public Vector3 CarbonPosition  { get; set; }
        public int HydrogenIndex  { get; set; }
        public Vector3 HydrogenPosition  { get; set; }
        public int NitrogenIndex  { get; set; }
        public Vector3 NitrogenPosition  { get; set; }
        public int OxygenIndex  { get; set; }
        public Vector3 OxygenPosition  { get; set; }

        public SecondaryStructurePattern Pattern = SecondaryStructurePattern.None;
        public SecondaryStructureAssignment SecondaryStructure { get; set; }
        public double AcceptorHydrogenBondEnergy { get; set; } = 1e10;
        public double DonorHydrogenBondEnergy { get; set; } = 1e10;
        public ProteinResidueData AcceptorHydrogenBondResidue { get; set; }
        public ProteinResidueData DonorHydrogenBondResidue { get; set; }
        public int ordinal { get; set; }
        public int ResidueIndex { get; set; }
    }
}