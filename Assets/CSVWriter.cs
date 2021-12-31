using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVWriter {
    private StreamWriter fileWriter;

    public CSVWriter(string filePath) {
        try {
            fileWriter = new StreamWriter(filePath, true);
        }
        catch (System.Exception e) {
            Debug.LogError("Cannot write to file. Error: " + e.Message);
        }
    }

    public void Close() {
        fileWriter.Close();
    }

    public void WriteRow(string[] data, bool addNewline = true) {
        fileWriter.Write(string.Join(",", data));
        if (addNewline) fileWriter.Write("\n");
    }
}
