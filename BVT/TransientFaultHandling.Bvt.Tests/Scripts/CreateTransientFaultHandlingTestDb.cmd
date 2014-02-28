sqlcmd -S (localdb)\v11.0 -E -i CreateTransientFaultHandlingTestDatabase.sql
sqlcmd -S (localdb)\v11.0 -E -i CreateTransientFaultHandlingTestDatabaseObjects.sql -d TransientFaultHandlingTest