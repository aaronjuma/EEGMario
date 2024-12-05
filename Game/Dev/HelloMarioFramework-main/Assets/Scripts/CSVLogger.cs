using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class CSVLogger
{
    // File path for the CSV
    private string filePath;
    

    // Initialization
    public CSVLogger(string id)
    {
        string filename = DateTime.Now.ToString("yyyy-MM-dd-HHmm");
        // Set the file path to the persistent data path
        filePath = Path.Combine(Application.persistentDataPath, filename+"_"+id+".csv");
        Debug.Log(filePath);

        // Write headers to the CSV file
        WriteHeaders(new List<string> { "Time", "Engagement", "difficulty"});
    }

    // Writes headers to the CSV file
    private void WriteHeaders(List<string> headers)
    {
        using (StreamWriter writer = new StreamWriter(filePath, false)) // 'false' to overwrite the file
        {
            writer.WriteLine(string.Join(",", headers));
        }
    }

    // Update is called once per frame
    public void Update(float time, float engagement, int difficulty)
    {
        // Convert data to a CSV line
        List<string> rowData = new List<string>
        {
            time.ToString("F5"),
            engagement.ToString(),
            difficulty.ToString()
        };

        // Write data to the CSV
        AppendRow(rowData);
    }

    // Appends a row to the CSV file
    private void AppendRow(List<string> rowData)
    {
        using (StreamWriter writer = new StreamWriter(filePath, true)) // 'true' to append
        {
            writer.WriteLine(string.Join(",", rowData));
        }
    }
}

