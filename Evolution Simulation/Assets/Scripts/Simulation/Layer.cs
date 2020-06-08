// public class Layer {
//   	public Node[] nodes;
  
// 	  public Layer(Node[] prevNodes, int layerSize, int layerNum) {
// 	      nodes = new Node[layerSize + 1];
// 	      for (int i = 0; i < nodes.Length - 1; i++)
// 	          nodes[i] = new Node(prevNodes, layerNum, i);
// 	      nodes[nodes.Length - 1] = new Node(new Node[0], layerNum, nodes.Length - 1);
// 	      nodes[nodes.Length - 1].value = 1;
// 	  }

// 	public Layer() {
// 		nodes = new Node[1];
// 		nodes[0] = new Node(new Node[0], 0, 0);
//       	nodes[0].value = 1;
// 	}
// }
