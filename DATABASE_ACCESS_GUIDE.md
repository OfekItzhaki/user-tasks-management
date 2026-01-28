# Database Access Guide

This guide explains how to access and open the TaskManagementDb database.

## 📍 Database Location

### Option 1: SQL Server LocalDB (Default - Current Setup)

**Connection String:**
```
Server=(localdb)\mssqllocaldb;Database=TaskManagementDb;Trusted_Connection=True
```

**Database File Location:**
LocalDB stores database files in your user profile:
```
C:\Users\<YourUsername>\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB\
```

**Database Files:**
- `TaskManagementDb.mdf` - Main database file
- `TaskManagementDb_log.ldf` - Transaction log file

**Note:** These files are created automatically when you run migrations.

---

### Option 2: Docker SQL Server

If using Docker Compose:
- **Server:** `localhost,1433` or `localhost`
- **Database:** `TaskManagementDb`
- **Username:** `sa`
- **Password:** `YourStrong@Passw0rd123`
- **Port:** `1433`

---

## 🔧 How to Open the Database

### Method 1: SQL Server Management Studio (SSMS) - Recommended

1. **Download and Install SSMS:**
   - Download from: https://aka.ms/ssmsfullsetup
   - Free tool from Microsoft

2. **Connect to LocalDB:**
   - Open SSMS
   - **Server name:** `(localdb)\mssqllocaldb`
   - **Authentication:** Windows Authentication
   - Click **Connect**

3. **Navigate to Database:**
   - Expand **Databases** in Object Explorer
   - Find **TaskManagementDb**
   - Expand to see tables, views, etc.

---

### Method 2: Visual Studio Server Explorer

1. **Open Visual Studio**
2. **View → Server Explorer** (or **Ctrl+Alt+S**)
3. **Right-click "Data Connections" → Add Connection**
4. **Server name:** `(localdb)\mssqllocaldb`
5. **Select or enter database name:** `TaskManagementDb`
6. **Test Connection** → **OK**

---

### Method 3: Azure Data Studio (Cross-platform)

1. **Download:** https://aka.ms/azuredatastudio
2. **New Connection:**
   - **Server:** `(localdb)\mssqllocaldb`
   - **Authentication:** Windows Authentication
   - **Database:** `TaskManagementDb`
   - **Connect**

---

### Method 4: Command Line (sqlcmd)

1. **Open PowerShell or Command Prompt**
2. **Connect:**
   ```powershell
   sqlcmd -S "(localdb)\mssqllocaldb" -d TaskManagementDb
   ```
3. **Run SQL queries:**
   ```sql
   SELECT * FROM Tasks;
   GO
   ```
4. **Exit:** Type `EXIT`

---

### Method 5: Using .NET EF Core Tools

**View database schema:**
```powershell
cd src/TaskManagement.API
dotnet ef dbcontext info --project ../TaskManagement.Infrastructure
```

**List all tables:**
```powershell
sqlcmd -S "(localdb)\mssqllocaldb" -d TaskManagementDb -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
```

---

## 🔍 Quick Database Queries

### Check if database exists:
```sql
SELECT name FROM sys.databases WHERE name = 'TaskManagementDb';
```

### List all tables:
```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

### Count tasks:
```sql
SELECT COUNT(*) AS TaskCount FROM Tasks;
```

### View all tasks:
```sql
SELECT * FROM Tasks;
```

### Check indexes:
```sql
SELECT 
    i.name AS IndexName,
    t.name AS TableName,
    c.name AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name = 'Tasks'
ORDER BY i.name, ic.key_ordinal;
```

### Check RowVersion column:
```sql
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Tasks' AND COLUMN_NAME = 'RowVersion';
```

---

## 🐳 Docker SQL Server Access

If using Docker Compose:

**Connection String:**
```
Server=localhost,1433;Database=TaskManagementDb;User Id=sa;Password=YourStrong@Passw0rd123;TrustServerCertificate=True
```

**Using sqlcmd:**
```powershell
sqlcmd -S localhost,1433 -U sa -P "YourStrong@Passw0rd123" -d TaskManagementDb -C
```

**Using SSMS:**
- **Server name:** `localhost,1433`
- **Authentication:** SQL Server Authentication
- **Login:** `sa`
- **Password:** `YourStrong@Passw0rd123`

---

## 📊 Database Schema

### Main Tables:
- **Tasks** - Task information
- **Users** - User information
- **Tags** - Tag definitions
- **TaskTags** - Many-to-many relationship (Tasks ↔ Tags)
- **UserTasks** - Many-to-many relationship (Tasks ↔ Users)

### Key Columns:
- **Tasks.Id** - Primary key
- **Tasks.RowVersion** - For optimistic concurrency (timestamp)
- **Tasks.DueDate** - Indexed for performance
- **Tasks.Priority** - Indexed for performance
- **Tasks.CreatedAt** - Indexed for performance

---

## 🛠️ Troubleshooting

### "Cannot connect to (localdb)\mssqllocaldb"

**Solution:**
```powershell
# Start LocalDB
sqllocaldb start mssqllocaldb

# Check status
sqllocaldb info mssqllocaldb
```

### "Database TaskManagementDb does not exist"

**Solution:**
```powershell
# Run migrations to create database
cd src/TaskManagement.API
dotnet ef database update --project ../TaskManagement.Infrastructure
```

### "Access Denied"

**Solution:**
- Make sure you're using Windows Authentication
- Run PowerShell/SSMS as Administrator if needed
- Check that LocalDB is running: `sqllocaldb info mssqllocaldb`

---

## 📝 Useful Commands

### Check LocalDB status:
```powershell
sqllocaldb info mssqllocaldb
```

### Start LocalDB:
```powershell
sqllocaldb start mssqllocaldb
```

### Stop LocalDB:
```powershell
sqllocaldb stop mssqllocaldb
```

### List all LocalDB instances:
```powershell
sqllocaldb info
```

### Backup database:
```sql
BACKUP DATABASE TaskManagementDb 
TO DISK = 'C:\Backup\TaskManagementDb.bak';
```

### Restore database:
```sql
RESTORE DATABASE TaskManagementDb 
FROM DISK = 'C:\Backup\TaskManagementDb.bak';
```

---

## 🔗 Quick Links

- **SSMS Download:** https://aka.ms/ssmsfullsetup
- **Azure Data Studio:** https://aka.ms/azuredatastudio
- **SQL Server Express:** https://www.microsoft.com/sql-server/sql-server-downloads

---

## 💡 Tips

1. **SSMS is the easiest** for visual database exploration
2. **Azure Data Studio** is cross-platform and lightweight
3. **sqlcmd** is great for quick queries from command line
4. **LocalDB files** are in your user profile (hidden by default)
5. **Always backup** before making manual changes
