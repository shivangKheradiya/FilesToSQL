using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System;
using Microsoft.Win32;
using System.Windows.Forms;

namespace FilesToSQL
{
    public class SQLiteDBStore
    {
        public List<string> filesToStore { get; internal set; }
        public List<string> ErrorFiles { get; set; } = new List<string>();

        public static string SQLiteDatabasePath = "C:\\db\\TestDb.db";
        public static string ConnectionString { get; set; }

        public bool SetConnectionString()
        {
            // Create a SaveFileDialog instance
            using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog())
            {
                // Configure the dialog box properties
                saveFileDialog.Filter = "Database Files (*.db)|*.db|All Files (*.*)|*.*";
                saveFileDialog.Title = "Save Database File";
                saveFileDialog.DefaultExt = "db";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = "myDatabase.db"; // Default file name

                // Show the dialog and get the result
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    SQLiteDatabasePath = saveFileDialog.FileName;
                    Console.WriteLine($"File will be saved to: {SQLiteDatabasePath}");

                    // Here, you can implement the logic to save the DB file
                    // Example (create an empty file for demonstration):
                    try
                    {
                        ConnectionString = $"Data Source={SQLiteDatabasePath};Version=3;";
                        Console.WriteLine("File saved successfully.");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while saving the file: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Save operation was canceled.");
                }
            }
            return false;
        }

        public bool SetConnectionStringForRetrival()
        {
            // Create a SaveFileDialog instance
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                // Configure the dialog box properties
                openFileDialog.Filter = "Database Files (*.db)|*.db|All Files (*.*)|*.*";
                openFileDialog.Title = "Open Database File";
                openFileDialog.DefaultExt = "db";
                openFileDialog.AddExtension = true;
                openFileDialog.FileName = "myDatabase.db"; // Default file name

                // Show the dialog and get the result
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    SQLiteDatabasePath = openFileDialog.FileName;
                    Console.WriteLine($"File will be saved to: {SQLiteDatabasePath}");

                    // Here, you can implement the logic to save the DB file
                    // Example (create an empty file for demonstration):
                    try
                    {
                        ConnectionString = $"Data Source={SQLiteDatabasePath};Version=3;";
                        Console.WriteLine("File saved successfully.");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while saving the file: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Save operation was canceled.");
                }
            }
            return false;
        }

        public void CreateSQLiteTable()
        {
            // SQLite creates the database if it does not exist
            SQLiteConnection.CreateFile(SQLiteDatabasePath);

            string createTableQuery = @"CREATE TABLE IF NOT EXISTS FileData (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FileName TEXT NOT NULL,
                ChunkOrder INTEGER NOT NULL,
                ChunkData BLOB NOT NULL
                );";

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void ExcuteAction()
        {

            foreach (string file in filesToStore)
            {
                try
                {
                    UploadFileToDatabase(file, file);
                }
                catch (Exception ex)
                {
                    ErrorFiles.Add(file);
                }
            }
            // Define the output file path (in the current application directory)
            string outputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorFiles.txt");

            // Save the error files list to the file
            SaveErrorFilesToFile(ErrorFiles, outputFilePath);
        }

        public static void SaveErrorFilesToFile(List<string> errorFiles, string outputFilePath)
        {
            if (errorFiles == null || errorFiles.Count == 0)
            {
                Console.WriteLine("ErrorFiles list is empty or null. Nothing to save.");
                return;
            }

            try
            {
                // Write each error file to the text file
                File.WriteAllLines(outputFilePath, errorFiles);
                Console.WriteLine($"Error files have been successfully saved to {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while saving the file: {ex.Message}");
            }
        }

        public void RetrieveAllFilesFromDatabase()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    string query = "SELECT DISTINCT FileName FROM FileData";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string fileName = reader.GetString(0);
                            RetrieveFileFromDatabase(fileName, connection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving files: {ex.Message}");
            }
        }

        static void UploadFileToDatabase(string filePath, string fileName)
        {
            const int chunkSize = 8000; // 8 KB chunks to optimize database operations

            try
            {
                byte[] fileContent = File.ReadAllBytes(filePath); // Read file as binary
                int totalChunks = (int)Math.Ceiling((double)fileContent.Length / chunkSize);

                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    for (int i = 0; i < totalChunks; i++)
                    {
                        int offset = i * chunkSize;
                        byte[] chunkData = new byte[Math.Min(chunkSize, fileContent.Length - offset)];
                        Array.Copy(fileContent, offset, chunkData, 0, chunkData.Length);

                        string query = "INSERT INTO FileData (FileName, ChunkOrder, ChunkData) VALUES (@FileName, @ChunkOrder, @ChunkData)";
                        using (SQLiteCommand command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@FileName", fileName);
                            command.Parameters.AddWithValue("@ChunkOrder", i);
                            command.Parameters.AddWithValue("@ChunkData", chunkData);
                            command.ExecuteNonQuery();
                        }
                    }
                }

                Console.WriteLine("File uploaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while uploading file: {ex.Message}");
            }
        }

        public void RetrieveFileFromDatabase(string fileName, SQLiteConnection connection)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(fileName);

                // Check if the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    // Create the directory if it does not exist
                    Directory.CreateDirectory(directoryPath);
                    Console.WriteLine($"Directory created: {directoryPath}");
                }

                string query = "SELECT ChunkData FROM FileData WHERE FileName = @FileName ORDER BY ChunkOrder";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FileName", fileName);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                        {
                            while (reader.Read())
                            {
                                byte[] chunkData = (byte[])reader["ChunkData"];
                                fileStream.Write(chunkData, 0, chunkData.Length);
                            }
                        }
                    }
                }
                Console.WriteLine("File retrieved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while retrieving file: {ex.Message}");
            }
        }
    }
}