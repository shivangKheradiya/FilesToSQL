using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FilesToSQL
{
    public class MSSQLDBStore
    {
        public static string ConnectionString { get; set; } = "your_connection_string";
        public static string ServerName { get; set; } = "your_server_name";
        public static string UserID { get; set; } = "sa";
        public static string Password { get; set; }
        public static string MasterConnectionString { get; set; }
        public static string NewDatabaseName{ get; set; } = "YourDatabaseName";
        public static string NewDatabaseConnectionString { get; set; } = $"Server={ServerName};Database={NewDatabaseName};User Id={UserID};Password={Password};";

        public List<string> filesToStore { get; internal set; }
        public List<string> ErrorFiles { get; set; } = new List<string>();

        public void SetConnectionString()
        {
            GetPropertiesFromConfigFile();
            MasterConnectionString = $"Server={ServerName};Database=master;User Id={UserID};Password={Password};";
            NewDatabaseConnectionString = $"Server={ServerName};Database={NewDatabaseName};User Id={UserID};Password={Password};";
        }

        private void GetPropertiesFromConfigFile()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(appDirectory, "SQLCredentials.txt");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The file SQLCredentials.txt was not found in the application directory.");

            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains("="))
                    continue;

                var parts = line.Split(new[] { '=' }, 2);
                string property = parts[0].Trim();
                string value = parts[1].Trim();

                switch (property)
                {
                    case nameof(ServerName):
                        ServerName = value;
                        break;
                    case nameof(NewDatabaseName):
                        NewDatabaseName = value;
                        break;
                    case nameof(UserID):
                        UserID = value;
                        break;
                    case nameof(Password):
                        Password = value;
                        break;
                    default:
                        MessageBox.Show($"Unrecognized property name '{property}' in the file.");
                        break;
                }
            }
        }

        public void CreateMSSQLDatabaseAndTable()
        {
            // Step 1: Create the database
            string createDatabaseQuery = $@"
                        IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{NewDatabaseName}')
                        CREATE DATABASE [{NewDatabaseName}];
                        ";

            using (SqlConnection masterConnection = new SqlConnection(MasterConnectionString))
            {
                masterConnection.Open();
                using (SqlCommand command = new SqlCommand(createDatabaseQuery, masterConnection))
                {
                    command.ExecuteNonQuery();
                }
            }

            // Step 2: Create the table in the new database
            string createTableQuery = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FileData' AND xtype='U')
                        CREATE TABLE FileData (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        FileName NVARCHAR(2000) NOT NULL,
                        ChunkOrder INT NOT NULL,
                        ChunkData VARBINARY(MAX) NOT NULL
                    );";

            using (SqlConnection databaseConnection = new SqlConnection(NewDatabaseConnectionString))
            {
                databaseConnection.Open();
                using (SqlCommand command = new SqlCommand(createTableQuery, databaseConnection))
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

        static void UploadFileToDatabase(string filePath, string fileName)
        {
            const int chunkSize = 8000; // 8 KB chunks to optimize database operations

            try
            {
                byte[] fileContent = File.ReadAllBytes(filePath); // Read file as binary
                int totalChunks = (int)Math.Ceiling((double)fileContent.Length / chunkSize);

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    for (int i = 0; i < totalChunks; i++)
                    {
                        int offset = i * chunkSize;
                        byte[] chunkData = new byte[Math.Min(chunkSize, fileContent.Length - offset)];
                        Array.Copy(fileContent, offset, chunkData, 0, chunkData.Length);

                        string query = "INSERT INTO FileData (FileName, ChunkOrder, ChunkData) VALUES (@FileName, @ChunkOrder, @ChunkData)";
                        using (SqlCommand command = new SqlCommand(query, connection))
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

        public void RetrieveAllFilesFromDatabase(string outputFolder)
        {
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(NewDatabaseConnectionString))
                {
                    connection.Open();

                    string query = "SELECT DISTINCT FileName FROM FileData";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
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
                Console.WriteLine($"Error retrieving files: {ex.Message}");
            }
        }

        static void RetrieveFileFromDatabase(string fileName, SqlConnection connection)
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
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FileName", fileName);

                    using (SqlDataReader reader = command.ExecuteReader())
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

