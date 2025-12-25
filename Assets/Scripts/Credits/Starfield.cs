using UnityEngine;

namespace UnityJam.Credits
{
    public class Starfield : MonoBehaviour
    {
        private ParticleSystem ps;
        private int starCount = 200;
        private float starSpeed = 30f; // Much faster

        private void Start()
        {
            ps = gameObject.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 5f;
            main.startSpeed = starSpeed;
            main.startSize = 0.1f;
            main.maxParticles = starCount;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            var emission = ps.emission;
            emission.rateOverTime = 40f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(20, 1, 1); // Wide X, thin Y
            
            // Align to spawn from top
            transform.position = new Vector3(0, 12, 0); 
            transform.rotation = Quaternion.Euler(90, 0, 0); // Point Down

            var renderer = GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit")); 
        }

        private void Update()
        {
            // Optional dynamic speed?
        }
    }
}
