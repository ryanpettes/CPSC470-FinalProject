using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LightFlicker : MonoBehaviour
{
    // Variables correlating to light flickering
    public float minIntensity;
    public float maxIntensity;
    // Higher smoothing == "lantern-like"
    // Lower smoothing == "flickering"
    [Range(1, 50)]
    public int smoothing;
    
    // Reference to object's Light component
    public Light lightSource;

    private Queue<float> smoothQueue;
    private float lastSum = 0;

    void Start()
    {
        smoothQueue = new Queue<float>(smoothing);
        // If no light source attached, use scene's directional light
        if (lightSource == null)
        {
            lightSource = GetComponent<Light>();
        }
    }

    // Update light intensity each frame
    private void Update()
    {
        if (lightSource == null) return;

        // If queue is too large, pop an item off
        while (smoothQueue.Count >= smoothing)
        {
            lastSum -= smoothQueue.Dequeue();
        }
        
        // Generate new value in range [minIntensity, maxIntensity], add it to queue
        float newIntensity = Random.Range(minIntensity, maxIntensity);
        smoothQueue.Enqueue(newIntensity);
        lastSum += newIntensity;
        
        // Calculate average of all items currently held in queue to determine current frame's
        //   light intensity
        lightSource.intensity = lastSum / (float)smoothQueue.Count;
    }
}
