# Datefolders
This package add the functionality to synchronize the title property of a Document in Umbraco with the nodename field.  

# Behavior
- When synchronization is enabled, the title field contents will automatically change if the node name is entered or altered in Umbraco
- Click the lock to disable the synchronization

## Configuration
Create a datatype in Umbraco of type NodeNameSync and change the title property from textstring to the NodeNameSync Datatype