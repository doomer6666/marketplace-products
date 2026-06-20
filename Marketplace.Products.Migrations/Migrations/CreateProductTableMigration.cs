using FluentMigrator;

namespace Marketplace.Products.Migrations.Migrations;

[Migration(2026061501, "Create Products Table")]
public class CreateProductTableMigration : Migration
{
    public override void Down()
    {
        Execute.Sql("DROP TRIGGER IF EXISTS update_products_updatedat ON products;");
        Execute.Sql("DROP FUNCTION IF EXISTS update_updatedat_column;");

        if (Schema.Table("Products").Exists())
        {
            Delete.Table("Products");
        }
    }

    public override void Up()
    {
        Create
            .Table("products")
            .WithColumn("id")
            .AsGuid()
            .PrimaryKey()
            .WithColumn("name")
            .AsString()
            .NotNullable()
            .WithColumn("description")
            .AsString(2000)
            .Nullable()
            .WithColumn("price")
            .AsDecimal()
            .NotNullable()
            .WithColumn("weight")
            .AsDouble()
            .NotNullable()
            .WithColumn("category")
            .AsInt32()
            .NotNullable()
            .WithColumn("createdat")
            .AsDateTime()
            .NotNullable()
            .WithColumn("updatedat")
            .AsDateTime()
            .NotNullable();
    }
}