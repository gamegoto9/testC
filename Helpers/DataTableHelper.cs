using System.Data;

namespace DotnetAPI.Helpers
{
    public static class DataTableHelper
    {
        public static DataTable ConvertListToDataTable(List<object> list)
        {
            DataTable dataTable = new DataTable();
            if (list.Count > 0)
            {
                // Convert the first object to a dictionary
                var firstObjectAsDictionary = (IDictionary<string, object>)list[0];

                // Print information about the first object
                // Console.WriteLine("First Object Type: " + firstObjectAsDictionary.GetType().FullName);

                // Get property names from the dictionary
                var propertyNames = firstObjectAsDictionary.Keys.ToList();

                // Print information about object properties
                // Console.WriteLine("\nObject Properties:");
                foreach (var propertyName in propertyNames)
                {
                    // Console.WriteLine($"  {propertyName}: {firstObjectAsDictionary[propertyName]?.GetType().FullName}");
                }

                // Create columns in DataTable based on object properties
                foreach (var propertyName in propertyNames)
                {
                    var propertyValue = firstObjectAsDictionary[propertyName];
                    Type columnType = (propertyValue != null) ? propertyValue.GetType() : typeof(object);

                    // Convert Byte to Int32
                    if (columnType == typeof(System.Byte))
                    {
                        columnType = typeof(int);
                    }

                    // Convert Int16 to Int32
                    if (columnType == typeof(System.Int16))
                    {
                        columnType = typeof(int);
                    }

                    dataTable.Columns.Add(propertyName, columnType);
                }

                // Print information about DataTable columns
                Console.WriteLine("\nDataTable Columns:");
                foreach (DataColumn column in dataTable.Columns)
                {
                    Console.WriteLine($"  {column.ColumnName}: {column.DataType.FullName}");
                }

                // Populate DataTable with data from the List of objects

                foreach (var item in list)
                {
                    var itemAsDictionary = (IDictionary<string, object>)item;
                    DataRow row = dataTable.NewRow();
                    foreach (var propertyName in propertyNames)
                    {
                        var value = itemAsDictionary[propertyName];

                        // Check if the value is null and use DBNull.Value if necessary
                        row[propertyName] = (value != null) ? value : DBNull.Value;
                    }
                    dataTable.Rows.Add(row);
                }

            }
            else
            {
                // Console.WriteLine("List of objects is empty.");
            }

            return dataTable;
        }


        public static void AddListToDataSet(DataSet dataSet, List<object> list)
        {
            if (list.Count > 0)
            {
                // Create a DataTable based on the first object
                var firstObjectAsDictionary = (IDictionary<string, object>)list[0];
                DataTable dataTable = CreateDataTable(firstObjectAsDictionary);

                // Populate the DataTable with data from the List of objects
                foreach (var item in list)
                {
                    var itemAsDictionary = (IDictionary<string, object>)item;
                    DataRow row = dataTable.NewRow();
                    foreach (var propertyName in itemAsDictionary.Keys)
                    {
                        var value = itemAsDictionary[propertyName];

                        // Check if the value is null and use DBNull.Value if necessary
                        row[propertyName] = (value != null) ? value : DBNull.Value;
                    }
                    dataTable.Rows.Add(row);
                }

                // Add the DataTable to the DataSet
                dataSet.Tables.Add(dataTable);
            }
            else
            {
                Console.WriteLine("List of objects is empty.");
            }
        }

        private static DataTable CreateDataTable(IDictionary<string, object> firstObjectAsDictionary)
        {
            DataTable dataTable = new DataTable();

            foreach (var propertyName in firstObjectAsDictionary.Keys)
            {
                if (!dataTable.Columns.Contains(propertyName))
                {
                    var propertyValue = firstObjectAsDictionary[propertyName];
                    Type columnType = (propertyValue != null) ? propertyValue.GetType() : typeof(object);

                    // Convert Byte or Int16 to Int32
                    if (columnType == typeof(System.Byte) || columnType == typeof(System.Int16))
                    {
                        columnType = typeof(int);
                    }

                    dataTable.Columns.Add(propertyName, columnType);
                }

            }

            return dataTable;
        }

    }
}

public class DataColumnDto
{
    public string ColumnName { get; set; }
    public string DataType { get; set; }
}

public class DataTableDto
{
    public List<DataColumnDto> Columns { get; set; } = new List<DataColumnDto>();
}