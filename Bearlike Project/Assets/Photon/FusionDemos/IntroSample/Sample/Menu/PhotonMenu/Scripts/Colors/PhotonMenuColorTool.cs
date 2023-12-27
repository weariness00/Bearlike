using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Photon/Menu/Color Control")]
public class PhotonMenuColorTool : ScriptableObject {
  public List<Object> Prefabs;
  public string[] Paths;
  public PhotonMenuColorPalette Palette;

}
