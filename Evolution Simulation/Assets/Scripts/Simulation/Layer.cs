public class Layer {
  public Node[] nodes;
  
  public Layer(int prevSize, int layerSize) {
    nodes = new Node[layerSize + 1];
    for (int i = 0; i < nodes.Length - 1; i++)
      nodes[i] = new Node(prevSize);
    nodes[nodes.Length - 1] = new Node(0);
    nodes[nodes.Length - 1].value = 1;
  }

  public Layer(Node[] a, Node[] b, float aMutation, float bMutation) {
    nodes = new Node[a.Length];
    for (int i = 0; i < nodes.Length - 1; i++) {
      if(i < b.Length)
        nodes[i] = new Node(a[i].connections, b[i].connections, aMutation, bMutation);
      else
        nodes[i] = new Node(a[i].connections, aMutation);
    }
    nodes[nodes.Length - 1] = new Node(0);
    nodes[nodes.Length - 1].value = 1;
  }

  public Layer(Node[] a, float aMutation) {
    nodes = new Node[a.Length];
    for (int i = 0; i < nodes.Length - 1; i++)
      nodes[i] = new Node(a[i].connections, aMutation);
    nodes[nodes.Length - 1] = new Node(0);
    nodes[nodes.Length - 1].value = 1;
  }

  //Remove?
  public Layer(int prevSize) {
  	nodes = new Node[prevSize];
    for (int i = 0; i < nodes.Length - 1; i++)
      nodes[i] = new Node(prevSize, i);
    nodes[nodes.Length - 1] = new Node(0);
    nodes[nodes.Length - 1].value = 1;
  }
}
