using NanoVer.Core.Math;

namespace NanoVer.Frontend.Manipulation
{
    public interface IInteractableParticles
    {
        ActiveParticleGrab GetParticleGrab(Transformation grabber);
    }
}