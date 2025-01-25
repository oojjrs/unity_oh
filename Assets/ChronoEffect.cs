using System.Collections.Generic;
using UnityEngine;

namespace oojjrs.oh
{
    public class ChronoEffect : MonoBehaviour, ChronoInterface
    {
        private Dictionary<Animator, float> Speeds { get; } = new();

        private void OnDisable()
        {
            ChronoInterfaceMachine.Remove(this);
        }

        private void OnEnable()
        {
            ChronoInterfaceMachine.Add(this);
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
    }
}
