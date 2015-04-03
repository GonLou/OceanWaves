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

	public InputField Text01;
	public InputField Text02;
	public InputField Text03;
	
	public Text bText01;
	public Text bText02;
	
	public int buttonState; 			// 0 - stop; 1 - 10 secs; 2 - play.
	private float timeCount;
	private float timeStart;
	
	public int currentIndex;
	
	private bool isTangent;
	
	public struct WaveData {
		public	float wLength; 			// the crest-to-crest distance between waves in world space. wavelength, w = 2pi/L
		public	float wAmplitude;		// the height from the water plane to the wave crest
		public	float wSpeed;			//  the distance the crest moves forward per second. phi = speed x 2pi/L
	}
	/*
	• Deep water h > alpha/4;
	• Transitional depth alpha/25 < h < alpha/4;
	• Shallow water h < alpha/25.
	*/
		
	public List<WaveData> WaveDataItems = new List<WaveData>();
	
	Vector4 wDir = new Vector4(1,1,1,0);// direction of the wave, D
	Vector2 wDir2 = new Vector2(1,1);	// direction of the wave, D
	
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
		
		// Initializes the mode stop/10 secs/play
		buttonState = 0;
		
		// The index of wave selection
		currentIndex = 0;
		Text01.text = "0";
		Text02.text = "0";
		Text03.text = "0";
		
		isTangent = false;
	}
	
	/// <summary>
	/// magic happens
	/// </summary>	
	void Update () {
		//Mesh mesh = GetComponent<MeshFilter>().mesh;
		
		float k = 1.1f;
		
		if (buttonState != 0) {
	
			if (baseHeight == null)
				baseHeight = mesh.vertices;

			Vector3[] vertices = new Vector3[baseHeight.Length];
			for (int i=0;i<vertices.Length;i++)
			{
				Vector3 vertex = baseHeight[i];
				
				foreach(WaveData wd in WaveDataItems)
				{
					// vertex.y += wd.wAmplitude * 
								// //Vector2.Dot(wDir2, new Vector2(1, baseHeight[i].y)) * 
								// Mathf.Sin(	(Time.time * wd.wSpeed * 2 * Mathf.PI / wd.wLength) + 
								// //Vector4.Dot(wDir, new Vector4(baseHeight[i].x, baseHeight[i].y, baseHeight[i].z, 0)) * 
								// Vector2.Dot(wDir2, new Vector2(baseHeight[i].x, baseHeight[i].y)) * 
								// (2*Mathf.PI/wd.wLength));
					// vertex.y += wd.wAmplitude * 2 *
								// Mathf.Pow 	((Mathf.Sin(	(Time.time * wd.wSpeed * 2 * Mathf.PI / wd.wLength) + 
											// Vector2.Dot(wDir2, new Vector2(baseHeight[i].x, baseHeight[i].y)) * 
											// (2*Mathf.PI/wd.wLength)) + 1) / 2, k);
					vertex.y += k * Vector2.Dot(new Vector2(1, 0), new Vector2(baseHeight[i].x, 0)) * wd.wAmplitude * (2*Mathf.PI/wd.wLength) *
								Mathf.Pow 	((Mathf.Sin(	(Time.time * wd.wSpeed * 2 * Mathf.PI / wd.wLength) + 
															Vector2.Dot(wDir2, new Vector2(baseHeight[i].x, baseHeight[i].y)) * 
															(2*Mathf.PI/wd.wLength)) + 1) / 2, k-1) * 
											Mathf.Cos(	(Time.time * wd.wSpeed * 2 * Mathf.PI / wd.wLength) + 
														Vector2.Dot(wDir2, new Vector2(baseHeight[i].x, baseHeight[i].y)) * 
														(2*Mathf.PI/wd.wLength));
				}

				vertex.y = vertex.y / WaveDataItems.Count / 10;
				vertices[i] = vertex;
			}
			mesh.vertices = vertices;
			mesh.RecalculateNormals();
			timeCount = Mathf.Abs(Time.time);
				
			// Activate tangent
			{
			if ( Input.GetKeyDown(KeyCode.T) ) { // T key
				if (isTangent) isTangent = false; else isTangent = true;
			}	
			
			if (isTangent) {TangentSolver(mesh); Debug.Log("T Activated");}			
			}
		
			// checks if time for button 1 is already over
			if (buttonState == 1 && (timeCount-timeStart) > 10) {
				buttonState = 0;
				timeCount = 0;
				bText01.text = "Play 10 seconds";
			}
		}
		
		// Waves Controls
		if ( Input.GetKeyDown(KeyCode.Z) ) { // left
			if (Text01.text != WaveDataItems[currentIndex].wLength.ToString()) {
				float floatTmp = 0.0f;
				float.TryParse(Text01.text, out floatTmp);
//				WaveDataItems[currentIndex].wLength.value = floatTmp;
			}
			if (currentIndex > 0) currentIndex--; else currentIndex = 0;
			ChangeWaveDisplay();
		}
		if ( Input.GetKeyDown(KeyCode.X) ) { // right
			if (currentIndex < WaveDataItems.Count-1) currentIndex++; else currentIndex = WaveDataItems.Count-1;
			ChangeWaveDisplay();
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
	
	// Menu Controls
	public void ButtonStateOne () {
		if (buttonState == 1)  {
			buttonState = 0;
			bText01.text = "Play 10 seconds";
		}
		else if (buttonState == 0) {
			buttonState = 1;
			timeStart = Mathf.Abs(Time.time);
			bText01.text = "Stop animation";
		}
	}
	public void ButtonStateTwo () {
		if (buttonState == 2)  {
			buttonState = 0;
			bText02.text = "Play infinite loop";
		}
		else if (buttonState == 0) {
			buttonState = 2;
			bText02.text = "Stop animation";
		}
	}	
	
	// Change the materials Controls
	public void Toggle01On() {
			//go.renderer.material = material_01;
			// toggle01.isOn = true;
			// toggle02.isOn = false;
			// toggle03.isOn = false;
	}
	public void Toggle02On() {
			//go.renderer.material = material_02;
			// toggle01.isOn = false;
			// toggle02.isOn = true;
			// toggle03.isOn = false;
	}
	public void Toggle03On() {
			//go.renderer.material = material_03;
			// toggle01.isOn = false;
			// toggle02.isOn = false;
			// toggle03.isOn = true;
	}
		
	/// <summary>
	/// Change the waves display on the right side
	/// </summary>
	void ChangeWaveDisplay() {
		Text01.text = WaveDataItems[currentIndex].wLength.ToString();
		Text02.text = WaveDataItems[currentIndex].wAmplitude.ToString();
		Text03.text = WaveDataItems[currentIndex].wSpeed.ToString();	
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

	
	/// <summary>
	/// Calculates the tangent
	/// </summary>
	/// <param name="meshBuilder">Mesh builder.</param>
	private static void TangentSolver(Mesh theMesh)
     {
         int vertexCount = theMesh.vertexCount;
         Vector3[] vertices = theMesh.vertices;
         Vector3[] normals = theMesh.normals;
         Vector2[] texcoords = theMesh.uv;
         int[] triangles = theMesh.triangles;
         int triangleCount = triangles.Length / 3;
         Vector4[] tangents = new Vector4[vertexCount];
         Vector3[] tan1 = new Vector3[vertexCount];
         Vector3[] tan2 = new Vector3[vertexCount];
         int tri = 0;
         for (int i = 0; i < (triangleCount); i++)
         {
             int i1 = triangles[tri];
             int i2 = triangles[tri + 1];
             int i3 = triangles[tri + 2];
 
             Vector3 v1 = vertices[i1];
             Vector3 v2 = vertices[i2];
             Vector3 v3 = vertices[i3];
 
             Vector2 w1 = texcoords[i1];
             Vector2 w2 = texcoords[i2];
             Vector2 w3 = texcoords[i3];
 
             float x1 = v2.x - v1.x;
             float x2 = v3.x - v1.x;
             float y1 = v2.y - v1.y;
             float y2 = v3.y - v1.y;
             float z1 = v2.z - v1.z;
             float z2 = v3.z - v1.z;
 
             float s1 = w2.x - w1.x;
             float s2 = w3.x - w1.x;
             float t1 = w2.y - w1.y;
             float t2 = w3.y - w1.y;
 
             float r = 1.0f / (s1 * t2 - s2 * t1);
             Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
             Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
 
             tan1[i1] += sdir;
             tan1[i2] += sdir;
             tan1[i3] += sdir;
 
             tan2[i1] += tdir;
             tan2[i2] += tdir;
             tan2[i3] += tdir;
 
             tri += 3;
         }
 
         for (int i = 0; i < (vertexCount); i++)
         {
             Vector3 n = normals[i];
             Vector3 t = tan1[i];
 
             // Gram-Schmidt orthogonalize
             Vector3.OrthoNormalize(ref n, ref t);
 
             tangents[i].x = t.x;
             tangents[i].y = t.y;
             tangents[i].z = t.z;
 
             // Calculate handedness
             tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0) ? -1.0f : 1.0f;
         }
         theMesh.tangents = tangents;
     }
	
}