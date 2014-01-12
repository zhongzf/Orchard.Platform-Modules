using Orchard.Data.Migration;

namespace RaisingStudio.SessionFactory
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable("ConnectionsRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name")
                    .Column<string>("Provider")
                    .Column<string>("ConnectionString")
                );

            return 1;
        }
    }
}