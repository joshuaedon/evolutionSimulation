using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Node {
	public int id;
    public float value;
    public GameObject nodeObject;

    public abstract void display();
}
