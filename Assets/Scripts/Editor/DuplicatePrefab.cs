using UnityEngine;
using UnityEditor;

public class DuplicatePrefab : ScriptableWizard
{
	[MenuItem("GameObject/Create Other/Duplicate Prefab...")]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard("Duplicate Prefab",typeof(DuplicatePrefab));
	}
	
	void OnWizardUpdate()
	{
	}
	
	UnityEngine.Object duplicatePrefab( UnityEngine.GameObject go )
	{
		// FYI:  Don't need to call this if go is already a prefab:
		//UnityEngine.Object prefab = UnityEditor.PrefabUtility.GetPrefabObject( go );
		return UnityEditor.PrefabUtility.InstantiatePrefab( go );
	}
	
	void OnWizardCreate()
	{
		UnityEngine.Object prefab = duplicatePrefab( go );
		UnityEngine.GameObject dupGO = (UnityEngine.GameObject)prefab;
		dupGO.name = "new name";
		dupGO.transform.position = new Vector3(1,1,1);
	}
	
	public UnityEngine.GameObject go;
}