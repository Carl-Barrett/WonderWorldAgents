using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Text;
using MarchingCubes;
using UnityEngine.Rendering;
using Unity.Mathematics;

public class ObjExporter : MonoBehaviour
{
	public Mesh mesh;
	public Transform root;
	public string m_name;
	

    private void Update()
    {
       
    }

	

	public void DoExportWSubmeshes()
	{
		DoExport(true);
	}

	
	public void DoExportWOSubmeshes()
	{
		DoExport(false);
	}


	public void DoExport(bool makeSubmeshes)
	{
		

		string meshName = m_name;
		string fileName = ""; //EditorUtility.SaveFilePanel("Export .obj file", "", meshName, "obj");

		ObjExporterScript.Start();

		StringBuilder meshString = new StringBuilder();

		meshString.Append("#" + meshName + ".obj"
						  + "\n#" + System.DateTime.Now.ToLongDateString()
						  + "\n#" + System.DateTime.Now.ToLongTimeString()
						  + "\n#-------"
						  + "\n\n");

		Transform t = root;

		Vector3 originalPosition = t.position;
		t.position = Vector3.zero;

		if (!makeSubmeshes)
		{
			meshString.Append("g ").Append(t.name).Append("\n");
		}
		meshString.Append(ProcessTransform(t, makeSubmeshes, mesh));

		WriteToFile(meshString.ToString(), fileName);

		t.position = originalPosition;

		ObjExporterScript.End();
		Debug.Log("Exported Mesh: " + fileName);
	}

	static string ProcessTransform(Transform t, bool makeSubmeshes, Mesh mesh)
	{
		StringBuilder meshString = new StringBuilder();

		meshString.Append("#" + t.name
						  + "\n#-------"
						  + "\n");

		if (makeSubmeshes)
		{
			meshString.Append("g ").Append(t.name).Append("\n");
		}

        if (mesh != null)
        {
			meshString.Append(ObjExporterScript.MeshToStringDirect(mesh, t));
		}

		/*MeshFilter mf = t.GetComponent<MeshFilter>();
		if (mf != null)
		{
			meshString.Append(ObjExporterScript.MeshToString(mf, t));
		}*/

		/*for (int i = 0; i < t.childCount; i++)
		{
			meshString.Append(ProcessTransform(t.GetChild(i), makeSubmeshes));
		}*/

		return meshString.ToString();
	}

	static void WriteToFile(string s, string filename)
	{
		using (StreamWriter sw = new StreamWriter(filename))
		{
			sw.Write(s);
		}
	}

	
}