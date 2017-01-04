$sqlInstance = "(local)\SQL2016"
$dbName = "SqlServerHelpers"

# Create the DB
sqlcmd -S "$sqlInstance" -Q "USE master; CREATE DATABASE $dbName;"
# Add the Table Types
sqlcmd -S "$sqlInstance" -d "$dbName" -i "$($env:appveyor_build_folder)\Queries\TableTypes.sql"

# Replace the db connection string with the one for this SQL Server instance in AppVeyor
$configPath = join-path $($env:appveyor_build_folder) "UnitTests\App.config"
$doc = (gc $configPath) -as [xml]
$doc.SelectSingleNode('//connectionStrings/add[@name="db"]').connectionString = "Server=(local)\SQL2016; Database=$dbName; Trusted_connection=true"
$doc.Save($configPath)