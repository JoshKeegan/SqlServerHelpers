$sqlInstance = "(local)\SQL2016"
$dbName = "SqlServerHelpers"

# Create the DB
sqlcmd -S "$sqlInstance" -Q "USE master; CREATE DATABASE $dbName;"
# Add the Table Types
sqlcmd -S "$sqlInstance" -d "$dbName" -i "$($env:appveyor_build_folder)\Queries\TableTypes.sql"