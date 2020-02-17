using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class defaultVariables {
	// Setup Constants

	// Variables






	OpenSimplexNoise osn;
    //// Grid attributes
    [Range(0, 4)]
    public float ticksPerSec = 0;
    public float framesPerTick;
    public static string tickFrame;
    [Range(0, 10000)]
    public static int time = 0;
    // []
    public float terrainTimeStep = 0.0000001f;
    // []
    public int terrainTimeUpdate = 250;
    // Terrain
    [Range(0f, 10000f)]
    public float seed;
    [Range(0, 200)]
    public static int cols = 150;
    [Range(0, 200)]
    public static int rows = 75;
    [Range(0f, 100f)]
    public float noiseScale = 15f;
    [Range(0f, 1f)]
    public static float seaLevel = 0.4f;
    [Range(1f, 10f)]
    public static float yScale = 3f;
    [Range(0, 50)]
    public int seaBorder = 10;
    // Food
    [Range(0f, 10f)]
    public float maxFood = 10f;
    // []
    public float foodSpread = 0.05f;
    // [] Agents
    public int startingAgents = 50;
    //// Grid state
    public static Chunk[,] gridArray;
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    public static List<Agent> agents;
}
