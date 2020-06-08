using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputNode : Node {
	public int type;       // 0-constant, 1-random, 2-health, 3-hunger, 4-sense
	public int sensType;   // 0-food, 1-water, 2-agent // 3-agent colour, 4-scent
	public int sensePos;   // 0-front left, 1-front, 2-front right, 3-left, 4-below, 5-right, 6-back left, 7-back, 8-back right

	// Initiated network
	public InputNode(int id) {
		this.id = id;
		this.type = Random.Range(0, 5);
		if(this.type == 4) {
			this.senseType = Random.Range(0, 3);
			this.sensePos = Random.Range(0, 9);
		}
	}

	// Inherited network
	public InputNode(InputNode oldNode) {
		this.id = oldNode;
		this.type = oldNode.type;
		this.senseType = oldNode.senseType;
		this.sensePos = oldNode.sensePos;
	}

	public string returnType() {
		switch(type) {
			case 0: return "Constant";
			case 1: return "Random";
			case 2: return "Health";
			case 3: return "Hunger";
			case 4:
				string s = "";
				switch(senseType) {
					case 0: s += "Food "; break;
					case 1: s += "Water "; break;
					case 2: s += "Agent "; break;
				}
				switch(sensePos) {
					case 0: s += "Front Left"; break;
					case 1: s += "Front"; break;
					case 2: s += "Front Right"; break;
					case 3: s += "Left"; break;
					case 4: s += "Below"; break;
					case 5: s += "Right"; break;
					case 6: s += "Back Left"; break;
					case 7: s += "Back"; break;
					case 8: s += "Back Right"; break;
				}
				return s;
		}
		return "";
	}

	public override void display() {
        if(nodeObject != null) {
            float c = 0.5f + value / 2;
            Color col = new Color(c, c, c);
            switch(type) {
				case 0: break;
				case 1: break;
				case 2: break;
				case 3: break;
				case 4:
					string s = "";
					switch(senseType) {
						case 0: col = new Color(0.263f, c, 0.094f); break;
						case 1: col = new Color(0, 0.157f, c); break;
						case 2: col = new Color(c, 0.25f, 0.25f); break;
					}
			}
        	nodeObject.GetComponent<Image>().color = col;
            nodeObject.transform.GetChild(0).GetComponent<Text>().text = value.ToString("n1");
        }
    }
}