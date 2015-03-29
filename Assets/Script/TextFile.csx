///////////////////////////////////////////////////////////////////
/// 
/// author:
/// Goncalo Lourenco
/// 
/// 
/// <summary>
/// File I/O operations for Ocean Waves
/// </summary>
/// 
///////////////////////////////////////////////////////////////////

	// public float wLength = 10; 			// the crest-to-crest distance between waves in world space. wavelength, w = 2pi/L
	// public float wAmplitude = 1.5f;		// the height from the water plane to the wave crest
    // public float wSpeed = 1.0f;			//  the distance the crest moves forward per second. phi = speed x 2pi/L
 
using UnityEngine;
using System.Collections;
using System.IO;

public class StringFile {

	private int group_line = 3;

	/// Initialization
	public StringFile () {
	}

	/// destructor
	~StringFile() {
	}
	
	/// <summary>
	/// To get the total number of records
	/// </summary>
	/// <returns>The total records.</returns>
	public int getTotalRecords() {
		int counter = 0;
		string line;
		
		System.IO.StreamReader file = new System.IO.StreamReader(@"StringFile.txt");
		while((line = file.ReadLine()) != null)
		{
			counter++;
		}
		file.Close();
		return (int)(counter/group_line);
	}

	/// <summary>
	/// Write into the file
	/// </summary>
	/// <param name="wLength">Symbol.</param>
	/// <param name="wAmplitude">Rule.</param>
	/// <param name="wSpeed">Axiom.</param>
	public void write(float wLength, float wAmplitude, float wSpeed) {
		if (!System.IO.File.Exists(@"StringFile.txt")) {  // if files does not exist create one
			string[] allLines = { 	"wLength:", wLength.ToString(), 
									"wAmplitude:", wAmplitude.ToString(),
									"wSpeed:", wSpeed.ToString()
								""};
			//System.IO.File.WriteAllLines(@"StringFile.txt", allLines);
		}
		else { // append to the end of the file
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"StringFile.txt", true)) {
				file.WriteLine("wLength:");
				file.WriteLine(wLength.ToString());
				file.WriteLine("wAmplitude:");
				file.WriteLine(wAmplitude.ToString());
				file.WriteLine("wSpeed:");
				file.WriteLine(wSpeed.ToString());
				file.WriteLine();
			}
		}			
	}
	
	/// <summary>
	/// load from file
	/// </summary>
	/// <param name="example_num">Example_num.</param>
	public void LoadFile(int example_num) {

		// Read the file and display it line by line.
		System.IO.StreamReader file = new System.IO.StreamReader(@"StringFile.txt");
		while(line = file.ReadLine())
		{

		
			counter++;
		}

		myString = new StringCreator(alphabet, rule, axiom, interaction, angle, unit_size);
		file.Close();

	}

}