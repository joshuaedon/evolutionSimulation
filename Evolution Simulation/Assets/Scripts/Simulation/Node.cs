using UnityEngine;

public class Node {
  public float value;
  public float[] connections;
  
  public Node(int prevSize) {
    connections = new float[prevSize];
    for (int i = 0; i < connections.Length-1; i++)
      connections[i] = Random.Range(-1f, 1f);
  }

  public Node(float[] a, float[] b, float aMutation, float bMutation) {
    connections = new float[a.Length];
    for (int i = 0; i < connections.Length; i++) {
      if(i < b.Length && Random.Range(0, 2) == 0)
        connections[i] = b[i] + Random.Range(-bMutation, bMutation);
      else
        connections[i] = a[i] + Random.Range(-aMutation, aMutation);
    }
  }

  public Node(float[] a, float aMutation) {
    connections = new float[a.Length];
    for (int i = 0; i < connections.Length; i++)
      connections[i] = a[i] + Random.Range(-aMutation, aMutation);
  }

  //Remove?
  public Node(int prevSize, int nodeNum) {
  	connections = new float[prevSize];
    for (int i = 0; i < connections.Length-1; i++) {
      connections[i] = 0;
    }
    connections[nodeNum] = 1;
  }
}
