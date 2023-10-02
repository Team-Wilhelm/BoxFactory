-- Create a function that accepts a schema name as an argument
CREATE OR REPLACE FUNCTION create_tables_with_schema(schema_to_create text) RETURNS void AS $$
BEGIN
    -- Use the provided schema name in the script
    -- Check if the schema exists, and if it does, drop it
    PERFORM schema_name FROM information_schema.schemata WHERE schema_name = schema_to_create;
    IF FOUND THEN
        EXECUTE 'DROP SCHEMA ' || schema_to_create || ' CASCADE';
    END IF;
    
    EXECUTE 'CREATE SCHEMA ' || schema_to_create;
    
    -- Create tables and other operations using the schema name
    EXECUTE 'CREATE TABLE IF NOT EXISTS ' || schema_to_create || '."Dimensions" (' ||
            '"dimensions_id" uuid PRIMARY KEY, ' ||
            '"height" integer, ' ||
            '"width" integer, ' ||
            '"depth" integer, ' ||
            ')';
    
    EXECUTE 'CREATE TABLE IF NOT EXISTS ' || schema_to_create || '."Boxes" (' ||
            '"box_id" uuid PRIMARY KEY, ' ||
            '"weight" integer, ' ||
            '"colour" varchar(25), ' ||
            '"material" varchar(25), ' ||
            '"created_at" timestamp, ' ||
            '"dimensions_id" uuid REFERENCES "Dimensions" ("dimensions_id") ON DELETE CASCADE' ||
          ')';

    EXECUTE 'CREATE TABLE IF NOT EXISTS ' || schema_to_create || '."Customers" (' ||
            '"customer_id" uuid PRIMARY KEY, ' ||
            '"email" varchar, ' ||
            '"phone_number" varchar(25), ' ||
            '"first_name" varchar(25), ' ||
            '"last_name" varchar, ' ||
            ')';

    EXECUTE 'CREATE TABLE IF NOT EXISTS ' || schema_to_create || '."Addresses" (' ||
            '"address_id" uuid PRIMARY KEY, ' ||
            '"street_name" varchar(25), ' ||
            '"house_number" integer, ' ||
            '"house_number_addition" varchar(10), ' ||
            '"city" varchar, ' ||
            '"postal_code" varchar(10), ' ||
            '"country" varchar(25), ' ||
            ')';

    EXECUTE 'CREATE TABLE IF NOT EXISTS ' || schema_to_create || '."Orders" (' ||
            '"order_id" uuid PRIMARY KEY, ' ||
            '"status" varchar(25), ' ||
            '"created_at" timestamp, ' ||
            '"updated_at" timestamp, ' ||
            '"customer_id" uuid REFERENCES "Customers" ("customer_id"), ' ||
            '"address_id" uuid REFERENCES "Addresses" ("address_id")' ||
            ')';
    
    EXECUTE 'CREATE TABLE IF NOT EXISTS ' || schema_to_create || '."Stock" (' ||
            '"stock_id" uuid PRIMARY KEY, ' ||
            '"quantity" integer ' ||
            ')';
    
    EXECUTE 'CREATE TABLE IF NOT EXISTS ' || schema_to_create || '."Box_Order_Link" (' ||
            '"box_id" uuid, ' ||
            '"order_id" uuid, ' ||
            'PRIMARY KEY ("box_id", "order_id"), ' ||
            'FOREIGN KEY ("box_id") REFERENCES ' || schema_to_create || '."Boxes" ("box_id") ON DELETE CASCADE,' ||
            'FOREIGN KEY ("order_id") REFERENCES ' || schema_to_create || '."Orders" ("order_id") ON DELETE CASCADE' ||
            ')';
    
    EXECUTE 'CREATE TABLE IF NOT EXISTS ' || schema_to_create || '."Box_Stock_Link" (' ||
            '"box_id" uuid,' ||
            '"stock_id" uuid, ' ||
            'PRIMARY KEY ("box_id", "stock_id"), ' ||
            'FOREIGN KEY ("box_id") REFERENCES ' || schema_to_create || '."Boxes" ("box_id") ON DELETE CASCADE, ' ||
            'FOREIGN KEY ("stock_id") REFERENCES ' || schema_to_create || '."Stock" ("stock_id") ON DELETE CASCADE' ||
            ')';
    
    EXECUTE 'CREATE TABLE IF NOT EXISTS ' || schema_to_create || '."Customer_Address_Link" (' ||
            '"customer_id" uuid, ' ||
            '"address_id" uuid, ' ||
            'PRIMARY KEY ("customer_id", "address_id"), ' ||
            'FOREIGN KEY ("customer_id") REFERENCES ' || schema_to_create || '."Customers" ("customer_id") ON DELETE CASCADE, ' ||
            'FOREIGN KEY ("address_id") REFERENCES ' || schema_to_create || '."Addresses" ("address_id") ON DELETE CASCADE' ||
            ')';
    
    -- Add foreign key constraints using the schema name
    EXECUTE 'ALTER TABLE ' || schema_to_create || '."Orders" ADD FOREIGN KEY ("customer_id") REFERENCES ' || schema_to_create || '."Customers" ("customer_id")';
    EXECUTE 'ALTER TABLE ' || schema_to_create || '."Orders" ADD FOREIGN KEY ("address_id") REFERENCES ' || schema_to_create || '."Addresses" ("address_id")';
    -- Add other foreign key constraints in a similar manner

    EXECUTE 'ALTER TABLE ' || schema_to_create || '."Boxes" ADD FOREIGN KEY ("dimensions_id") REFERENCES ' || schema_to_create || '."Dimensions" ("dimensions_id") ON DELETE CASCADE';
END;
$$ LANGUAGE plpgsql;

SELECT create_tables_with_schema('testing');

DROP FUNCTION create_tables_with_schema(text);
