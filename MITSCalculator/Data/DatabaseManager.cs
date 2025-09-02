using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace MITSCalculator.Data;

public class DatabaseManager : IDisposable
{
    private readonly string _connectionString;
    private readonly string _encryptionKey;
    private SqliteConnection? _connection;

    public DatabaseManager()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MITSCalculator");
        Directory.CreateDirectory(appDataPath);
        
        var dbPath = Path.Combine(appDataPath, "user_data.db");
        _connectionString = $"Data Source={dbPath}";
        _encryptionKey = GenerateOrLoadEncryptionKey(appDataPath);
        
        InitializeDatabase();
    }

    private string GenerateOrLoadEncryptionKey(string appDataPath)
    {
        var keyPath = Path.Combine(appDataPath, "encryption.key");
        
        if (File.Exists(keyPath))
        {
            return File.ReadAllText(keyPath);
        }
        
        // Generate new 256-bit AES key
        using var aes = Aes.Create();
        aes.GenerateKey();
        var key = Convert.ToBase64String(aes.Key);
        
        File.WriteAllText(keyPath, key);
        return key;
    }

    private void InitializeDatabase()
    {
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();

        // Create tables
        CreateTables();
    }

    private void CreateTables()
    {
        var createTablesScript = @"
            CREATE TABLE IF NOT EXISTS History (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Expression TEXT NOT NULL,
                Result TEXT NOT NULL,
                Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                Mode TEXT DEFAULT 'Standard'
            );

            CREATE TABLE IF NOT EXISTS Variables (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT UNIQUE NOT NULL,
                Value TEXT NOT NULL,
                IsCustomFunction INTEGER DEFAULT 0,
                Description TEXT,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS Formulas (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT UNIQUE NOT NULL,
                Expression TEXT NOT NULL,
                Variables TEXT,
                Category TEXT,
                Description TEXT,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS Settings (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL,
                UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS Projects (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT UNIQUE NOT NULL,
                ProjectData TEXT NOT NULL,
                Description TEXT,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS GraphData (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId INTEGER,
                FunctionExpression TEXT NOT NULL,
                XMin REAL,
                XMax REAL,
                DataPoints TEXT,
                PlotType TEXT DEFAULT 'Line',
                Color TEXT,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (ProjectId) REFERENCES Projects(Id)
            );

            CREATE INDEX IF NOT EXISTS idx_history_timestamp ON History(Timestamp);
            CREATE INDEX IF NOT EXISTS idx_variables_name ON Variables(Name);
            CREATE INDEX IF NOT EXISTS idx_formulas_category ON Formulas(Category);
            CREATE INDEX IF NOT EXISTS idx_projects_name ON Projects(Name);
        ";

        using var command = new SqliteCommand(createTablesScript, _connection);
        command.ExecuteNonQuery();

        // Initialize default settings
        InitializeDefaultSettings();
    }

    private void InitializeDefaultSettings()
    {
        var defaultSettings = new Dictionary<string, string>
        {
            ["theme"] = "Dark",
            ["precision"] = "10",
            ["angle_unit"] = "Degrees",
            ["number_format"] = "Auto",
            ["auto_save"] = "true",
            ["privacy_mode"] = "false"
        };

        foreach (var setting in defaultSettings)
        {
            SaveSetting(setting.Key, setting.Value, false);
        }
    }

    // History operations
    public void SaveCalculation(string expression, string result, string mode = "Standard")
    {
        var encryptedExpression = EncryptData(expression);
        var encryptedResult = EncryptData(result);

        using var command = new SqliteCommand(
            "INSERT INTO History (Expression, Result, Mode) VALUES (@expression, @result, @mode)", 
            _connection);
        
        command.Parameters.AddWithValue("@expression", encryptedExpression);
        command.Parameters.AddWithValue("@result", encryptedResult);
        command.Parameters.AddWithValue("@mode", mode);
        command.ExecuteNonQuery();
    }

    public List<HistoryEntry> GetHistory(int limit = 100)
    {
        var history = new List<HistoryEntry>();
        
        using var command = new SqliteCommand(
            "SELECT Id, Expression, Result, Timestamp, Mode FROM History ORDER BY Timestamp DESC LIMIT @limit", 
            _connection);
        
        command.Parameters.AddWithValue("@limit", limit);
        
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            history.Add(new HistoryEntry
            {
                Id = reader.GetInt32(0),
                Expression = DecryptData(reader.GetString(1)),
                Result = DecryptData(reader.GetString(2)),
                Timestamp = DateTime.Parse(reader.GetString(3)),
                Mode = reader.GetString(4)
            });
        }
        
        return history;
    }

    public void ClearHistory()
    {
        using var command = new SqliteCommand("DELETE FROM History", _connection);
        command.ExecuteNonQuery();
    }

    // Variable operations
    public void SaveVariable(string name, double value, bool isCustomFunction = false, string description = "")
    {
        var encryptedValue = EncryptData(value.ToString());
        var encryptedDescription = EncryptData(description);

        using var command = new SqliteCommand(@"
            INSERT OR REPLACE INTO Variables (Name, Value, IsCustomFunction, Description) 
            VALUES (@name, @value, @isFunction, @description)", _connection);
        
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@value", encryptedValue);
        command.Parameters.AddWithValue("@isFunction", isCustomFunction ? 1 : 0);
        command.Parameters.AddWithValue("@description", encryptedDescription);
        command.ExecuteNonQuery();
    }

    public Dictionary<string, double> GetVariables()
    {
        var variables = new Dictionary<string, double>();
        
        using var command = new SqliteCommand("SELECT Name, Value FROM Variables WHERE IsCustomFunction = 0", _connection);
        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            var name = reader.GetString(0);
            var encryptedValue = reader.GetString(1);
            var value = DecryptData(encryptedValue);
            
            if (double.TryParse(value, out double numValue))
            {
                variables[name] = numValue;
            }
        }
        
        return variables;
    }

    // Formula operations
    public void SaveFormula(string name, string expression, string variables, string category = "", string description = "")
    {
        var encryptedExpression = EncryptData(expression);
        var encryptedVariables = EncryptData(variables);
        var encryptedDescription = EncryptData(description);

        using var command = new SqliteCommand(@"
            INSERT OR REPLACE INTO Formulas (Name, Expression, Variables, Category, Description) 
            VALUES (@name, @expression, @variables, @category, @description)", _connection);
        
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@expression", encryptedExpression);
        command.Parameters.AddWithValue("@variables", encryptedVariables);
        command.Parameters.AddWithValue("@category", category);
        command.Parameters.AddWithValue("@description", encryptedDescription);
        command.ExecuteNonQuery();
    }

    public List<Formula> GetFormulas(string category = "")
    {
        var formulas = new List<Formula>();
        var sql = string.IsNullOrEmpty(category) 
            ? "SELECT * FROM Formulas ORDER BY Category, Name"
            : "SELECT * FROM Formulas WHERE Category = @category ORDER BY Name";

        using var command = new SqliteCommand(sql, _connection);
        if (!string.IsNullOrEmpty(category))
        {
            command.Parameters.AddWithValue("@category", category);
        }

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            formulas.Add(new Formula
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Expression = DecryptData(reader.GetString(2)),
                Variables = DecryptData(reader.GetString(3)),
                Category = reader.GetString(4),
                Description = DecryptData(reader.GetString(5)),
                CreatedAt = DateTime.Parse(reader.GetString(6))
            });
        }

        return formulas;
    }

    // Settings operations
    public void SaveSetting(string key, string value, bool updateTimestamp = true)
    {
        var encryptedValue = EncryptData(value);
        
        var sql = updateTimestamp 
            ? "INSERT OR REPLACE INTO Settings (Key, Value, UpdatedAt) VALUES (@key, @value, CURRENT_TIMESTAMP)"
            : "INSERT OR IGNORE INTO Settings (Key, Value) VALUES (@key, @value)";

        using var command = new SqliteCommand(sql, _connection);
        command.Parameters.AddWithValue("@key", key);
        command.Parameters.AddWithValue("@value", encryptedValue);
        command.ExecuteNonQuery();
    }

    public string GetSetting(string key, string defaultValue = "")
    {
        using var command = new SqliteCommand("SELECT Value FROM Settings WHERE Key = @key", _connection);
        command.Parameters.AddWithValue("@key", key);
        
        var result = command.ExecuteScalar();
        if (result != null)
        {
            return DecryptData(result.ToString() ?? "");
        }
        
        return defaultValue;
    }

    // Project operations
    public void SaveProject(string name, object projectData, string description = "")
    {
        var encryptedData = EncryptData(JsonSerializer.Serialize(projectData));
        var encryptedDescription = EncryptData(description);

        using var command = new SqliteCommand(@"
            INSERT OR REPLACE INTO Projects (Name, ProjectData, Description, UpdatedAt) 
            VALUES (@name, @data, @description, CURRENT_TIMESTAMP)", _connection);
        
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@data", encryptedData);
        command.Parameters.AddWithValue("@description", encryptedDescription);
        command.ExecuteNonQuery();
    }

    // Encryption/Decryption
    private string EncryptData(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext)) return "";

        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(_encryptionKey);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var encryptedBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

        // Combine IV and encrypted data
        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
        Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    private string DecryptData(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext)) return "";

        try
        {
            var ciphertextBytes = Convert.FromBase64String(ciphertext);
            
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(_encryptionKey);

            // Extract IV from the beginning
            var iv = new byte[aes.IV.Length];
            var encrypted = new byte[ciphertextBytes.Length - iv.Length];
            
            Array.Copy(ciphertextBytes, 0, iv, 0, iv.Length);
            Array.Copy(ciphertextBytes, iv.Length, encrypted, 0, encrypted.Length);
            
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
            
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return ciphertext; // Return as-is if decryption fails
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}

// Data models
public class HistoryEntry
{
    public int Id { get; set; }
    public string Expression { get; set; } = "";
    public string Result { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string Mode { get; set; } = "Standard";
}

public class Formula
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Expression { get; set; } = "";
    public string Variables { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}