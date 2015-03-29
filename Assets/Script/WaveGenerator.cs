 using UnityEngine;
 using System.Collections;
 using System.Collections.Generic;
 using System.IO;
 using UnityEngine.UI;
 
 public class WaveGenerator : MonoBehaviour
 {
	public Mesh mesh;
 	private float widthLength = 30.0f;
	private int meshSegmentCount = 100;	
	
	public Material material_01;
	public Material material_02;
	public Material material_03;
	public GameObject go;
	
	public Toggle toggle01;
	public Toggle toggle02;
	public Toggle toggle03;
	
	public Button Button01;	
	public Button Button02;	
	
	public int buttonState;
	private float timeCount;
	private float timeStart;
	
	public struct WaveData {
		public	float wLength; 			// the crest-to-crest distance between waves in world space. wavelength, w = 2pi/L
		public	float wAmplitude;		// the height from the water plane to the wave crest
		public	float wSpeed;			//  the distance the crest moves forward per second. phi = speed x 2pi/L
	}
	/*
	� Deep water h > alpha/4;
	� Transitional depth alpha/25 < h < alpha/4;
	� Shallow water h < alpha/25.
	*/
		
	public List<WaveData> WaveDataItems = new List<WaveData>();
	
	Vector4 wDir = new Vector4(1,1,0,0);// direction of the wave, D
	
    private Vector3[] baseHeight;
  
  	protected virtual void Start()
	{
		MeshBuilder meshBuilder = new MeshBuilder();
		
		float segmentSize = widthLength / meshSegmentCount;
		
		for (int i = 0; i <= meshSegmentCount; i++)
		{
			float z = segmentSize * i;
			float v = (1.0f / meshSegmentCount) * i;
			
			for (int j = 0; j <= meshSegmentCount; j++)
			{
				float x = segmentSize * j;
				float u = (1.0f / meshSegmentCount) * j;
				
				Vector3 offset = new Vector3(x, 0, z);
				
				Vector2 uv = new Vector2(u, v);
				bool buildTriangles = i > 0 && j > 0;
				
				BuildQuadForGrid(meshBuilder, offset, uv, buildTriangles, meshSegmentCount + 1);
			}
		}

		// Creates the mesh
		mesh = meshBuilder.CreateMesh();
		// Recalculates the normals of the mesh from the triangles and vertices
		mesh.RecalculateNormals();
		
		// Search for a MeshFilter component attached to this GameObject
		MeshFilter filter = GetComponent<MeshFilter>();
		// Render the Mesh created
		if (filter != null)	filter.sharedMesh = mesh;
		
		// Populates the struct with the file values
		string line;
		int counter = 3;
		WaveData wd  = new WaveData();
		System.IO.StreamReader file = new System.IO.StreamReader(@"StringFile.txt");
		while((line = file.ReadLine()) != null) {
			switch (counter) {
				case 3 : 	wd = new WaveData(); 
							if (line != null) float.TryParse(line, out wd.wLength);
							break;
				case 2 : 	if (line != null) float.TryParse(line, out wd.wAmplitude);
							break;
				case 1 : 	if (line != null) float.TryParse(line, out wd.wSpeed);
							WaveDataItems.Add(wd);
							break;

			}
			//line == null && 
			if (counter == 0) counter = 3;
			else counter--;
			//Debug.Log(counter);
		}
		file.Close();

		// Just to know how many parameters are loaded
		Debug.Log("List contains " + WaveDataItems.Count + " entries.");
		
		buttonState = 0;
		
	}
	
	/// <summary>
	/// magic happens
	/// </summary>	
	void Update () {
		//Mesh mesh = GetComponent<MeshFilter>().mesh;
		
		if (buttonState != 0) {
	
			if (baseHeight == null)
				baseHeight = mesh.vertices;

			Vector3[] vertices = new Vector3[baseHeight.Length];
			for (int i=0;i<vertices.Length;i++)
			{
				Vector3 vertex = baseHeight[i];
				
				foreach(WaveData wd in WaveDataItems)
				{
					vertex.y += wd.wAmplitude * 
								Mathf.Sin(	(Time.time * wd.wSpeed * 2 * Mathf.PI / wd.wLength) + 
								Vector4.Dot(wDir, new Vector4(baseHeight[i].x, baseHeight[i].y, baseHeight[i].z, 0)) * 
								(2*Mathf.PI/wd.wLength));
					// vertex.x += wd.wAmplitude * 
								// Mathf.Cos(	(Time.time * wd.wSpeed * 2 * Mathf.PI / wd.wLength) + 
								// Vector4.Dot(wDir, new Vector4(baseHeight[i].x, baseHeight[i].y, baseHeight[i].z, 0)) * 
								// (2*Mathf.PI/wd.wLength));							
				}

				vertex.y = vertex.y / WaveDataItems.Count;
				//vertex.x = vertex.x / WaveDataItems.Count;
				vertices[i] = vertex;
			}
			mesh.vertices = vertices;
			mesh.RecalculateNormals();
			timeCount = Mathf.Abs(Time.time);
			
			if (buttonState == 1 && (timeCount-timeStart) > 10) {
				buttonState = 0;
				timeCount = 0;
			}
		}

		// Menu Controls
		if ( Input.GetKeyDown(KeyCode.P) ) {
			buttonState = 1;
			timeStart = Mathf.Abs(Time.time);
		}
		if ( Input.GetKeyDown(KeyCode.L) ) {
			if (buttonState == 2) buttonState = 0;
			else buttonState = 2;
		}
		
		// Change the materials
		if ( Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1) ) {
			go.renderer.material = material_01;
			toggle01.isOn = true;
			toggle02.isOn = false;
			toggle03.isOn = false;
		}
		if ( Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2) ) {
			go.renderer.material = material_02;
			toggle01.isOn = false;
			toggle02.isOn = true;
			toggle03.isOn = false;
		}
		if ( Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3) ) {
			go.renderer.material = material_03;
			toggle01.isOn = false;
			toggle02.isOn = false;
			toggle03.isOn = true;
		}

	}
		
	/// <summary>
	/// Saves the parameters of the wave on a file
	/// </summary>
	void SaveFile() {
	    using (System.IO.StreamWriter file = new StreamWriter(@"StringFile.txt")) {
			foreach(WaveData wd in WaveDataItems) {
				file.WriteLine(wd.wLength.ToString());
				file.WriteLine(wd.wAmplitude.ToString());
				file.WriteLine(wd.wSpeed.ToString());
				file.WriteLine();
			}
        }		
	}
	
	/// <summary>
	/// Builds the mesh
	/// </summary>
	/// <param name="meshBuilder">Mesh builder.</param>
	/// <param name="position">Position.</param>
	/// <param name="uv">Uv.</param>
	/// <param name="buildTriangles">If set to <c>true</c> build triangles.</param>
	/// <param name="vertsPerRow">Verts per row.</param>
	private void BuildQuadForGrid(MeshBuilder meshBuilder, Vector3 position, Vector2 uv, bool buildTriangles, int vertsPerRow)
	{
		meshBuilder.Vertices.Add(position);
		meshBuilder.UVs.Add(uv);
		
		if (buildTriangles)
		{
			int baseIndex = meshBuilder.Vertices.Count - 1;
			
			int index0 = baseIndex;
			int index1 = baseIndex - 1;
			int index2 = baseIndex - vertsPerRow;
			int index3 = baseIndex - vertsPerRow - 1;
			
			meshBuilder.AddTriangle(index0, index2, index1);
			meshBuilder.AddTriangle(index2, index3, index1);
		}
	}	

}