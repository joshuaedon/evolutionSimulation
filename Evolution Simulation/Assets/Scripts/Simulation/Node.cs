using UnityEngine;
using UnityEngine.UI;

public class Node {
    public float value;
    public Node[] nodes;
    public float[] weights;
    public GameObject nodeObject;
    public GameObject[] connectionObjects;
    public int layerNum;
    public int nodeNum;

    public Node(Node[] prevNodes, int layerNum, int nodeNum) {
        this.nodes = new Node[prevNodes.Length];
        for(int i = 0; i < prevNodes.Length; i++)
            nodes[i] = prevNodes[i];
        this.weights = new float[prevNodes.Length];
        for (int i = 0; i < weights.Length; i++)
            weights[i] = Random.Range(-1f, 1f);
        this.connectionObjects = new GameObject[prevNodes.Length];
        this.layerNum = layerNum;
        this.nodeNum = nodeNum;
    }

    public Node(int layerNum, int nodeNum) {
        this.layerNum = layerNum;
        this.nodeNum = nodeNum;
    }

    /*public Node(float[] a, float[] b, float aMutation, float bMutation) {
        weights = new float[a.Length];
        for (int i = 0; i < weights.Length; i++) {
            if(i < b.Length && Random.Range(0, 2) == 0)
                weights[i] = b[i] + Random.Range(-bMutation, bMutation);
            else
                weights[i] = a[i] + Random.Range(-aMutation, aMutation);
        }
    }

    public Node(float[] a, float aMutation) {
        weights = new float[a.Length];
        for (int i = 0; i < weights.Length; i++)
            weights[i] = a[i] + Random.Range(-aMutation, aMutation);
    }

    //Remove?
    public Node(int prevSize, int nodeNum) {
      	weights = new float[prevSize];
        for (int i = 0; i < weights.Length-1; i++) {
            weights[i] = 0;
        }
        weights[nodeNum] = 1;
    }*/

    public void calculateValue() {
        float sum = 0;
        for (int n = 0; n < nodes.Length; n++)
            sum += nodes[n].value*weights[n];
        //sum /= sqrt(currentNode.connections.length);
        // ^Suggested in https://www.youtube.com/watch?v=8bNIkfRJZpo but he makes connection weights smaller when assigned
        // - I think this will be better but may cause problems when mutations cause layer sizes to change
        // ACTIVATION FUNCTION
        sum = 1/(1 + Mathf.Exp(-sum));
        this.value = sum;
    }

    public void display() {
        if(nodeObject != null) {
            float c = 0.5f + value / 2;
            nodeObject.GetComponent<Image>().color = new Color(c, c, c);
            nodeObject.transform.GetChild(0).GetComponent<Text>().text = value.ToString("n2");
        }
    }
}
