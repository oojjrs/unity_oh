using System.Collections.Generic;
using UnityEngine;

namespace oojjrs.oh
{
    public class ChronoEffect : MonoBehaviour, ChronoInterface
    {
        private Dictionary<Animator, float> OriginalAnimatorSpeeds { get; } = new();
        private Dictionary<ParticleSystem, float> OriginalParticleSystemSpeeds { get; } = new();
        private Dictionary<Animator, float> Speeds { get; } = new();

        private void OnDisable()
        {
            ChronoInterfaceMachine.Remove(this);
        }

        private void OnEnable()
        {
            ChronoInterfaceMachine.Add(this);
        }

        private void Start()
        {
            if (ChronoInterfaceMachine.Pausing)
                ((ChronoInterface)this).Pause();
        }

        void ChronoInterface.Pause()
        {
            foreach (var ps in GetComponentsInChildren<ParticleSystem>())
                ps.Pause();

            foreach (var animator in GetComponentsInChildren<Animator>())
            {
                Speeds[animator] = animator.speed;

                animator.speed = 0;
            }
        }

        void ChronoInterface.Resume()
        {
            foreach (var ps in GetComponentsInChildren<ParticleSystem>())
                ps.Play();

            foreach (var animator in GetComponentsInChildren<Animator>())
            {
                if (Speeds.TryGetValue(animator, out var speed))
                    animator.speed = speed;
            }
        }

        void ChronoInterface.SetSpeed(float speed)
        {
            foreach (var ps in GetComponentsInChildren<ParticleSystem>())
            {
                var main = ps.main;

                if (OriginalParticleSystemSpeeds.TryGetValue(ps, out var psSpeed) == false)
                {
                    psSpeed = main.simulationSpeed;
                    OriginalParticleSystemSpeeds[ps] = psSpeed;
                }

                main.simulationSpeed = psSpeed * speed;
            }

            foreach (var animator in GetComponentsInChildren<Animator>())
            {
                if (OriginalAnimatorSpeeds.TryGetValue(animator, out var animSpeed) == false)
                {
                    animSpeed = animator.speed;
                    OriginalAnimatorSpeeds[animator] = animSpeed;
                }

                animator.speed = animSpeed * speed;
            }
        }
    }
}
